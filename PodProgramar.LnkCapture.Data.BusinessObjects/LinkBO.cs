using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MihaZupan;
using PodProgramar.LnkCapture.Data.BusinessObjects.Resources;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.DTO;
using PodProgramar.LnkCapture.Data.Models;
using PodProgramar.Utils.Cryptography;
using PodProgramar.Utils.Text.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class LinkBO : BaseBO, ILinkBO
    {
        private readonly ILogger _logger;
        private readonly TelegramBotClient _telegramBotClient;
        private readonly string _encryptionKey;

        public LinkBO(IConfiguration configuration, LnkCaptureContext lnkCaptureContext, ILogger<LinkBO> logger) : base(lnkCaptureContext, configuration)
        {
            var botConfiguration = Configuration.GetSection("BotConfiguration");
            _encryptionKey = Configuration.GetSection("AppConfiguration")["EncryptionKey"];

            _logger = logger;
            _telegramBotClient = string.IsNullOrEmpty(botConfiguration["Socks5Host"])
                ? new TelegramBotClient(botConfiguration["BotToken"])
                : new TelegramBotClient(
                    botConfiguration["BotToken"],
                    new HttpToSocks5Proxy(botConfiguration["Socks5Host"], int.Parse(botConfiguration["Socks5Port"])));
        }

        public async Task<LinkResultDTO> GetChatLinksAsync(long id, string search, string user, DateTime? startDate, DateTime? endDate, int? pageIndex, int? pageSize)
        {
            try
            {
                if (!pageIndex.HasValue)
                    pageIndex = 0;

                if (!pageSize.HasValue)
                    pageSize = 10;

                var searchQuery = Context.Link.Where(p => p.ChatId == id
                                                        && (string.IsNullOrEmpty(search) || p.Message.Contains(search))
                                                        && (string.IsNullOrEmpty(user) || p.Username.Contains(user))
                                                        && (!startDate.HasValue || p.CreateDate >= startDate.Value)
                                                        && (!endDate.HasValue || p.CreateDate <= endDate.Value.AddDays(1).AddMilliseconds(-1))
                                                    ).OrderByDescending(p => p.CreateDate);

                var totalItems = Context.Link.Where(p => p.ChatId == id).Count();
                var searchTotalItems = searchQuery.Count();

                var chat = await _telegramBotClient.GetChatAsync(new ChatId(id));

                var linkResultDTO = new LinkResultDTO
                {
                    ChatId = id,
                    ChatTitle = chat?.Title,
                    CreateDate = DateTime.Now,
                    TotalItems = totalItems,
                    TotalSearchItems = searchTotalItems,
                    Items = new List<LinkDTO>()
                };

                foreach (var link in searchQuery.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value))
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

                return linkResultDTO;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task SaveLinkAsync(Update update)
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
            var rootImagesUri = Configuration.GetSection("AppConfiguration")["ImagesUri"];

            foreach (var uri in uris)
            {
                if (uri.StartsWith(rootUri))
                    continue;

                if (UriExistsInDatabase(uri, update.Message.Chat.Id))
                {
                    try
                    {
                        var message = new StringBuilder();

                        if (uris.Length > 1)
                            message.Append(uri).Append("\n");

                        message.Append(LinkResources.LinkAlreadyExists.GetRandomText());

                        await _telegramBotClient.SendPhotoAsync(update.Message.Chat.Id, new InputOnlineFile($"{rootImagesUri}/link_already_exists_{new Random().Next(1, 3)}.jpg"), message.ToString(), ParseMode.Default, true, update.Message.MessageId);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception.Message);
                    }
                }
                else
                {
                    string title = null;

                    if (UriIsValid(uri, out title))
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

                        if (!string.IsNullOrEmpty(title))
                            link.Title = title.Length > 300 ? title.Substring(0, 300) : title;

                        if (!string.IsNullOrEmpty(update.Message.From.Username))
                            link.Username = update.Message.From.Username;

                        Context.Link.Add(link);
                        Context.SaveChanges();

                        try
                        {
                            await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{uri}\n{LinkResources.LinkSaved.GetRandomText()}", ParseMode.Default, true, true, update.Message.MessageId);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception.Message);

                            throw;
                        }
                    }
                    else
                    {
                        try
                        {
                            await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{uri}\n{LinkResources.LinkInvalid.GetRandomText()}", ParseMode.Default, true, true, update.Message.MessageId);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception.Message);

                            throw;
                        }
                    }
                }
            }
        }

        public async Task SendLinksRecoverMessageAsync(Update update)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type == MessageType.Text)
            {
                try
                {
                    var rootUri = Configuration.GetSection("AppConfiguration")["RootUri"];
                    var chatId = Uri.EscapeDataString(Encryptor.EncryptString(update.Message.Chat.Id.ToString(), _encryptionKey));

                    await _telegramBotClient.SendTextMessageAsync(update.Message.From.Id,
                                                                  $"{string.Format(LinkResources.LinksRecover, string.IsNullOrEmpty(update.Message.Chat.Title) ? "" : $" {update.Message.Chat.Title}")} {rootUri}?id={chatId}",
                                                                  ParseMode.Default, true, true);
                }
                catch (ChatNotInitiatedException)
                {
                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{LinkResources.ChatNotInitiatedException}", ParseMode.Default, true, true, update.Message.MessageId);
                }
                catch (ForbiddenException)
                {
                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{LinkResources.ForbiddenException}", ParseMode.Default, true, true, update.Message.MessageId);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception.Message);

                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{LinkResources.UnknownException}", ParseMode.Default, true, true, update.Message.MessageId);
                }
            }
        }

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
                _logger.LogError(exception.Message);

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
                    Uri uri;

                    if ((Uri.TryCreate(values[i], UriKind.Absolute, out uri) || Uri.TryCreate("http://" + values[i], UriKind.Absolute, out uri)) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        result.Add(uri.AbsoluteUri);
                    }
                }
            }

            return result.ToArray();
        }

        private bool UriIsValid(string uri, out string uriTitle)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
                    string htmlContent = DownloadStringAwareOfEncoding(wc, new Uri(uri));

                    var document = new HtmlDocument();
                    document.LoadHtml(htmlContent);

                    try
                    {
                        var title = document.DocumentNode.Descendants("title").FirstOrDefault();

                        if (title != null)
                            uriTitle = WebUtility.HtmlDecode(title.InnerText).Trim();
                        else
                            uriTitle = null;
                    }
                    catch (Exception)
                    {
                        uriTitle = null;
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                uriTitle = null;

                return false;
            }
        }

        private string DownloadStringAwareOfEncoding(WebClient webClient, Uri uri)
        {
            try
            {
                webClient.Headers.Add("User-Agent: Other");

                var rawData = webClient.DownloadData(uri);
                var encoding = GetEncodingFrom(webClient.ResponseHeaders, Encoding.UTF8);

                return encoding.GetString(rawData);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);

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
                _logger.LogError(exception.Message);

                return defaultEncoding ?? Encoding.UTF8;
            }
        }
    }
}