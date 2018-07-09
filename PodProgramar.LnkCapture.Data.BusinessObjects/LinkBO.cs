using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.DTO;
using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class LinkBO : BaseDataBO, ILinkBO
    {
        #region Fields

        private readonly IMessageBO _messageBO;
        private readonly IChatBO _chatBO;
        private readonly ILinkReaderLogBO _linkReaderLogBO;

        #endregion Fields

        #region Constructors

        public LinkBO(IConfiguration configuration, LnkCaptureContext lnkCaptureContext, ILogger<LinkBO> logger, IMessageBO messageBO, IChatBO chatBO, ILinkReaderLogBO linkReaderLogBO) : base(lnkCaptureContext, configuration, logger)
        {
            Logger = logger;
            _messageBO = messageBO;
            _chatBO = chatBO;
            _linkReaderLogBO = linkReaderLogBO;
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

                var linkResultDTO = new LinkResultDTO
                {
                    ChatId = linkReader.ChatId,
                    ChatTitle = chat?.Title,
                    CreateDate = DateTime.Now,
                    TotalItems = totalItems,
                    TotalSearchItems = totalSearchItems,
                    Items = new List<LinkDTO>()
                };

                foreach (var link in searchItems)
                {
                    var linkDTO = new LinkDTO
                    {
                        Title = link.Title,
                        CreateDate = link.CreateDate,
                        Uri = link.Uri
                    };

                    if (!string.IsNullOrEmpty(link.Username))
                        linkDTO.Username = $"@{link.Username}";
                    else
                        linkDTO.UserId = link.UserId;

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
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type != MessageType.Text)
                return;

            if (update.Message.From.IsBot)
                return;

            var uris = GetUris(update);

            if (uris == null || uris.Length == 0)
                return;

            var rootUri = Configuration.GetSection("AppConfiguration")["RootUri"];

            foreach (var uri in uris)
            {
                if (uri.StartsWith(rootUri))
                    continue;

                if (UriExistsInDatabase(uri, update.Message.Chat.Id))
                {
                    await _messageBO.SendLinkAlreadyExistsMessageAsync(update.Message.Chat.Id, uri, uris.Length, update.Message.MessageId);
                }
                else
                {
                    var uriIsValid = await UriIsValidAsync(uri);

                    if (uriIsValid.IsValid)
                    {
                        try
                        {
                            var link = new Link
                            {
                                ChatId = update.Message.Chat.Id,
                                CreateDate = DateTime.Now,
                                LinkId = Guid.NewGuid(),
                                Message = update.Message.Text,
                                Uri = uri,
                                UserId = update.Message.From.Id
                            };

                            if (!string.IsNullOrEmpty(uriIsValid.UriTitle))
                                link.Title = uriIsValid.UriTitle.Length > 300 ? uriIsValid.UriTitle.Substring(0, 300) : uriIsValid.UriTitle;

                            if (!string.IsNullOrEmpty(update.Message.From.Username))
                                link.Username = update.Message.From.Username;

                            Context.Link.Add(link);
                            await Context.SaveChangesAsync();

                            await _messageBO.SendLinkSavedMessageAsync(update.Message.Chat.Id, uri, uris.Length, update.Message.MessageId);
                        }
                        catch (Exception exception)
                        {
                            Logger.LogError(exception.Message);

                            throw;
                        }
                    }
                    else
                    {
                        await _messageBO.SendInvalidLinkMessageAsync(update.Message.Chat.Id, uri, uris.Length, update.Message.MessageId);
                    }
                }
            }
        }

        #endregion Public

        #region Private

        private bool UriExistsInDatabase(string uri, long chatId)
        {
            try
            {
                return (from c in Context.Link
                        where c.ChatId == chatId
                            && c.Uri == uri
                        select c.LinkId).Count() > 0;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        private string[] GetUris(Update update)
        {
            if (update.Message.Entities == null || update.Message.EntityValues == null)
                return null;

            var result = new List<string>();
            var values = update.Message.EntityValues.ToArray();

            for (int i = 0; i < values.Length; i++)
            {
                if (update.Message.Entities[i].Type == MessageEntityType.Url)
                {
                    if ((Uri.TryCreate(values[i], UriKind.Absolute, out Uri uri) || Uri.TryCreate("http://" + values[i], UriKind.Absolute, out uri)) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        result.Add(uri.AbsoluteUri);
                    }
                }
            }

            return result.ToArray();
        }

        private async Task<UriValidResult> UriIsValidAsync(string uri)
        {
            var result = new UriValidResult { IsValid = false, UriTitle = null };

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
                    string htmlContent = await DownloadStringAwareOfEncodingAsync(wc, new Uri(uri));

                    var document = new HtmlDocument();
                    document.LoadHtml(htmlContent);
                    result.IsValid = true;

                    try
                    {
                        var title = document.DocumentNode.Descendants("title").FirstOrDefault();

                        if (title != null)
                            result.UriTitle = WebUtility.HtmlDecode(title.InnerText).Trim();
                    }
                    catch (Exception)
                    {
                        result.UriTitle = null;
                    }

                    return result;
                }
            }
            catch (Exception)
            {
                result.UriTitle = null;

                return result;
            }
        }

        private async Task<string> DownloadStringAwareOfEncodingAsync(WebClient webClient, Uri uri)
        {
            try
            {
                webClient.Headers.Add("User-Agent: Other");

                var rawData = await webClient.DownloadDataTaskAsync(uri);
                var encoding = GetEncodingFrom(webClient.ResponseHeaders, Encoding.UTF8);

                return encoding.GetString(rawData);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        private Encoding GetEncodingFrom(NameValueCollection responseHeaders, Encoding defaultEncoding = null)
        {
            try
            {
                if (responseHeaders == null)
                    throw new ArgumentNullException("responseHeaders");

                var contentType = responseHeaders["Content-Type"];

                if (contentType == null)
                    return defaultEncoding;

                var contentTypeParts = contentType.Split(';');

                if (contentTypeParts.Length <= 1)
                    return defaultEncoding;

                var charsetPart = contentTypeParts.Skip(1).FirstOrDefault(p => p.TrimStart().StartsWith("charset", StringComparison.InvariantCultureIgnoreCase));

                if (charsetPart == null)
                    return defaultEncoding;

                var charsetPartParts = charsetPart.Split('=');

                if (charsetPartParts.Length != 2)
                    return defaultEncoding;

                var charsetName = charsetPartParts[1].Trim();

                if (charsetName == "")
                    return defaultEncoding;

                return Encoding.GetEncoding(charsetName);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                return defaultEncoding ?? Encoding.UTF8;
            }
        }

        #endregion Private

        #endregion Methods
    }
}