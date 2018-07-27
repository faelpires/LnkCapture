using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.DTO;
using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class LinkBO : BaseDataBO, ILinkBO
    {
        #region Fields

        private readonly IConfigBO _configBO;
        private readonly IMessageBO _messageBO;
        private readonly IChatBO _chatBO;
        private readonly ILinkReaderLogBO _linkReaderLogBO;
        private readonly ICrawlerBO _crawlerBO;

        #endregion Fields

        #region Constructors

        public LinkBO(IConfiguration configuration, LnkCaptureContext lnkCaptureContext, ILogger<LinkBO> logger, IMessageBO messageBO, IChatBO chatBO, ILinkReaderLogBO linkReaderLogBO, IConfigBO configBO, ICrawlerBO crawlerBO) : base(lnkCaptureContext, configuration, logger)
        {
            Logger = logger;
            _configBO = configBO;
            _messageBO = messageBO;
            _chatBO = chatBO;
            _linkReaderLogBO = linkReaderLogBO;
            _crawlerBO = crawlerBO;
        }

        #endregion Constructors

        #region Classes

        private class UriValidResult
        {
            public bool IsValid { get; set; }
            public string UriTitle { get; set; }
        }

        #endregion Classes

        #region Methods

        #region Public

        public async Task<LinkResultDTO> GetAsync(LinkReader linkReader, bool isAPIRequest, string search, string user, DateTime? startDate, DateTime? endDate, int? pageIndex, int? pageSize)
        {
            try
            {
                if (!pageIndex.HasValue)
                    pageIndex = 0;

                if (!pageSize.HasValue)
                    pageSize = 10;

                if (!startDate.HasValue)
                    startDate = DateTime.Now.AddMonths(-1);

                if (!endDate.HasValue)
                    endDate = DateTime.Now.AddDays(1).AddMilliseconds(-1);
                else
                    endDate = endDate.Value.AddDays(1).AddMilliseconds(-1);

                var searchQuery = Context.Link.Where(p => p.ChatId == linkReader.ChatId
                                                        && (string.IsNullOrEmpty(search) || p.Message.Contains(search))
                                                        && (string.IsNullOrEmpty(user) || p.Username.Contains(user))
                                                        && (p.CreateDate >= startDate.Value)
                                                        && (p.CreateDate <= endDate.Value)
                                                    ).OrderByDescending(p => p.CreateDate);

                var totalItems = await Context.Link.Where(p => p.ChatId == linkReader.ChatId).CountAsync();
                var totalSearchItems = await searchQuery.CountAsync();
                var searchItems = await searchQuery.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value).ToListAsync();

                var chat = await _chatBO.GetChatAsync(linkReader.ChatId);

                var linkResultDTO = new LinkResultDTO {
                    ChatId = linkReader.ChatId,
                    ChatTitle = chat.Title,
                    CreateDate = DateTime.Now,
                    TotalItems = totalItems,
                    TotalSearchItems = totalSearchItems,
                    Items = new List<LinkDTO>()
                };

                var userDataList = new List<Tuple<int, string, string>>();

                foreach (var link in searchItems)
                {
                    var linkDTO = new LinkDTO {
                        Title = link.Title,
                        CreateDate = link.CreateDate,
                        Uri = link.Uri,
                        Description = link.Description,
                        Keywords = link.Keywords,
                        ThumbnailUri = link.ThumbnailUri,
                        UserId = link.UserId
                    };

                    if (!string.IsNullOrEmpty(link.Username))
                        linkDTO.Username = link.Username;

                    var linkUserData = userDataList.FirstOrDefault(p => p.Item1 == link.UserId);

                    if (linkUserData != null)
                    {
                        if (!string.IsNullOrWhiteSpace(linkUserData.Item2))
                            linkDTO.FirstName = linkUserData.Item2;

                        if (!string.IsNullOrWhiteSpace(linkUserData.Item3))
                            linkDTO.LastName = linkUserData.Item3;
                    }
                    else
                    {
                        var chatMember = await _chatBO.GetChatMemberAsync(linkReader.ChatId, link.UserId);

                        if (chatMember != null && chatMember.User != null)
                        {
                            if (!string.IsNullOrWhiteSpace(chatMember.User.FirstName))
                                linkDTO.FirstName = chatMember.User.FirstName;

                            if (!string.IsNullOrWhiteSpace(chatMember.User.LastName))
                                linkDTO.LastName = chatMember.User.LastName;

                            var userData = new Tuple<int, string, string>(link.UserId, linkDTO.FirstName, linkDTO.LastName);
                            userDataList.Add(userData);
                        }
                    }

                    linkResultDTO.Items.Add(linkDTO);
                }

                await _linkReaderLogBO.SaveLogAsync(linkReader.LinkReaderId, isAPIRequest, searchItems.Count());

                return linkResultDTO;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task SaveAsync(Update update)
        {
            if (update == null)
                return;

            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type != MessageType.Text)
                return;

            if (update.Message == null)
                return;

            if (string.IsNullOrWhiteSpace(update.Message.Text))
                return;

            if (update.Message.From.IsBot)
                return;

            var uris = GetUris(update);

            if (uris == null || uris.Length == 0)
                return;

            var rootUri = Configuration.GetSection("AppConfiguration")["RootUri"];
            var config = await _configBO.GetAsync(update.Message.Chat.Id);

            foreach (var uri in uris)
            {
                try
                {
                    if (uri.AbsoluteUri.StartsWith(rootUri))
                        continue;

                    if (await UriExistsInDatabase(uri, update.Message.Chat.Id))
                    {
                        if (config.EnableLinkAlreadyExistsMessage)
                            await _messageBO.SendLinkAlreadyExistsMessageAsync(update.Message.Chat.Id, uri.AbsoluteUri, uris.Length, update.Message.MessageId);
                    }
                    else
                    {
                        var uriData = await _crawlerBO.GetUriDataAsync(uri);

                        if (uriData.IsValid)
                        {
                            try
                            {
                                var link = new Link {
                                    LinkId = Guid.NewGuid(),
                                    ChatId = update.Message.Chat.Id,
                                    CreateDate = update.Message.Date,
                                    Message = update.Message.Text,
                                    Uri = uriData.Uri.AbsoluteUri,
                                    UserId = update.Message.From.Id
                                };

                                if (uriData.HasTitle)
                                    link.Title = uriData.Title.Length > 300 ? uriData.Title.Substring(0, 300) : uriData.Title;

                                if (uriData.HasDescription)
                                    link.Description = uriData.Description;

                                if (uriData.HasKeywords)
                                    link.Keywords = uriData.Keywords;

                                if (uriData.HasTumbnailUri)
                                    link.ThumbnailUri = uriData.ThumbnailUri.AbsoluteUri.Length <= 2048 ? uriData.ThumbnailUri.AbsoluteUri : null;

                                if (!string.IsNullOrEmpty(update.Message.From.Username))
                                    link.Username = update.Message.From.Username;

                                Context.Link.Add(link);
                                await Context.SaveChangesAsync();

                                if (config.EnableSavedMessage)
                                    await _messageBO.SendLinkSavedMessageAsync(update.Message.Chat.Id, uri.AbsoluteUri, uris.Length, update.Message.MessageId);
                            }
                            catch (Exception exception)
                            {
                                Logger.LogError(exception.Message);

                                throw;
                            }
                        }
                        else
                        {
                            if (config.EnableInvalidLinkMessage)
                                await _messageBO.SendInvalidLinkMessageAsync(update.Message.Chat.Id, uri.AbsoluteUri, uris.Length, update.Message.MessageId);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception.Message);

                    throw;
                }
            }
        }

        #endregion Public

        #region Private

        private async Task<bool> UriExistsInDatabase(Uri uri, long chatId)
        {
            try
            {
                return await (from c in Context.Link
                              where c.ChatId == chatId
                                  && c.Uri == uri.AbsoluteUri
                              select c.LinkId).AnyAsync();
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        private Uri[] GetUris(Update update)
        {
            if (update.Message.Entities == null || update.Message.EntityValues == null)
                return null;

            var result = new List<Uri>();
            var values = update.Message.EntityValues.ToArray();

            for (var i = 0; i < values.Length; i++)
            {
                if (update.Message.Entities[i].Type == MessageEntityType.Url)
                {
                    if ((Uri.TryCreate(values[i], UriKind.Absolute, out Uri uri) || Uri.TryCreate("http://" + values[i], UriKind.Absolute, out uri)) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        result.Add(uri);
                    }
                }
            }

            return result.ToArray();
        }

        public void UpdateAsync()
        {
            {
                try
                {
                    var result = Context.Link.Where(p => !string.IsNullOrEmpty(p.Title) && !string.IsNullOrEmpty(p.Description) && !string.IsNullOrEmpty(p.Keywords) && !string.IsNullOrEmpty(p.ThumbnailUri)).ToList();

                    foreach (var link in result)
                    {
                        try
                        {
                            var uriData = _crawlerBO.GetUriDataAsync(new Uri(link.Uri)).Result;

                            if (uriData.HasTitle)
                                link.Title = uriData.Title.Length > 300 ? uriData.Title.Substring(0, 300) : uriData.Title;

                            if (uriData.HasDescription)
                                link.Description = uriData.Description;

                            if (uriData.HasKeywords)
                                link.Keywords = uriData.Keywords;

                            if (uriData.HasTumbnailUri)
                                link.ThumbnailUri = uriData.ThumbnailUri.AbsoluteUri.Length <= 2048 ? uriData.ThumbnailUri.AbsoluteUri : null;

                            Context.Link.Update(link);
                            Context.SaveChanges();
                        }
                        catch (Exception exception)
                        {
                            Logger.LogError(exception.Message);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception.Message);

                    throw;
                }
            }
        }

        #endregion Private

        #endregion Methods
    }
}