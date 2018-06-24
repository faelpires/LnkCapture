﻿using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MihaZupan;
using PodProgramar.LnkCapture.Data.BusinessObjects.Resources;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.DTO;
using PodProgramar.LnkCapture.Data.Models;
using PodProgramar.Utils.Text.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class LinkBO : BaseBO, ILinkBO
    {
        private readonly ILogger _logger;
        private readonly TelegramBotClient _telegramBotClient;

        public LinkBO(IConfiguration configuration, LnkCaptureContext lnkCaptureContext, ILogger<LinkBO> logger) : base(lnkCaptureContext, configuration)
        {
            var botConfiguration = Configuration.GetSection("BotConfiguration");

            _logger = logger;
            _telegramBotClient = string.IsNullOrEmpty(botConfiguration["Socks5Host"])
                ? new TelegramBotClient(botConfiguration["BotToken"])
                : new TelegramBotClient(
                    botConfiguration["BotToken"],
                    new HttpToSocks5Proxy(botConfiguration["Socks5Host"], int.Parse(botConfiguration["Socks5Port"])));
        }

        public async Task<LinkResultDTO> GetChatLinksAsync(long id)
        {
            try
            {
                var result = Context.Link.Where(p => p.ChatId == id).OrderByDescending(p => p.CreateDate);
                var chat = await _telegramBotClient.GetChatAsync(new ChatId(id));

                var linkResultDTO = new LinkResultDTO
                {
                    ChatId = id,
                    ChatTitle = chat?.Title,
                    CreateDate = DateTime.Now,
                    Items = new List<LinkDTO>()
                };

                foreach (var link in result)
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
                        await _telegramBotClient.SendPhotoAsync(update.Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile($"{rootImagesUri}/link_already_exists_{new Random().Next(1, 3)}.jpg"), $"{uri}\n{LinkResources.LinkAlreadyExists.GetRandomText()}", ParseMode.Default, true, update.Message.MessageId);
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
                            Title = title.Length > 300 ? title.Substring(0, 300) : title,
                            Username = update.Message.From.Username,
                            UserId = update.Message.From.Id
                        };

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

        public async Task UpdateTitlesAsync()
        {
            foreach (var link in Context.Link)
            {
                string title = null;

                if (UriIsValid(link.Uri, out title))
                {
                    if (title != null)
                        link.Title = title.Length > 300 ? title.Substring(0, 300) : title;
                }
            }

            Context.SaveChanges();
        }

        public async Task SendLinksRecoverMessageAsync(Update update, string chatId)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type == MessageType.Text)
            {
                try
                {
                    var chatListUri = Configuration.GetSection("AppConfiguration")["ChatListUri"];

                    await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, $"{LinkResources.LinksRecover} {chatListUri}/{chatId}", ParseMode.Default, true, true, update.Message.MessageId);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception.Message);

                    throw;
                }
            }
        }

        private bool UriExistsInDatabase(string uri, double chatId)
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
                    wc.Headers.Add("User-Agent: Other");
                    string htmlContent = DownloadStringAwareOfEncoding(wc, new Uri(uri));

                    var document = new HtmlDocument();
                    document.LoadHtml(htmlContent);

                    var title = document.DocumentNode.Descendants("title").FirstOrDefault();

                    if (title != null)
                        uriTitle = WebUtility.HtmlDecode(title.InnerText).Trim();
                    else
                        uriTitle = null;

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