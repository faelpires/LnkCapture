using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MihaZupan;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class LinkReaderBO : BaseDataBO, ILinkReaderBO
    {
        #region Fields

        private readonly TelegramBotClient _telegramBotClient;
        private readonly IChatBO _chatBO;

        #endregion Fields

        #region Constructors

        public LinkReaderBO(IConfiguration configuration, LnkCaptureContext lnkCaptureContext, ILogger<LinkReaderBO> logger, IChatBO chatBO) : base(lnkCaptureContext, configuration, logger)
        {
            var botConfiguration = Configuration.GetSection("BotConfiguration");

            Logger = logger;
            _chatBO = chatBO;

            _telegramBotClient = string.IsNullOrEmpty(botConfiguration["Socks5Host"])
                ? new TelegramBotClient(botConfiguration["BotToken"])
                : new TelegramBotClient(
                    botConfiguration["BotToken"],
                    new HttpToSocks5Proxy(botConfiguration["Socks5Host"], int.Parse(botConfiguration["Socks5Port"])));
        }

        #endregion Constructors

        #region Methods

        public async Task<LinkReader> SaveAsync(int userId, long chatId)
        {
            try
            {
                var linkReaderId = Guid.NewGuid();

                var newlinkReader = new LinkReader
                {
                    ChatId = chatId,
                    CreateDate = DateTime.Now,
                    LinkReaderId = linkReaderId,
                    UserId = userId
                };

                Context.LinkReader.Add(newlinkReader);
                await Context.SaveChangesAsync();

                return newlinkReader;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task<LinkReader> GetAsync(int userId, long chatId)
        {
            try
            {
                return await Context.LinkReader.FirstOrDefaultAsync(p => p.UserId == userId && p.ChatId == chatId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task<LinkReader> GetAsync(Guid linkReaderId)
        {
            try
            {
                var linkReader = await Context.LinkReader.FirstOrDefaultAsync(p => p.LinkReaderId == linkReaderId);

                if (linkReader != null)
                    return linkReader;
                else
                    throw new Exception("Invalid Id");
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        public async Task<IDictionary<Guid, string>> GetRelatedLinkReadersAsync(Guid linkReaderId)
        {
            try
            {
                var result = new Dictionary<Guid, string>();
                var linkReader = await Context.LinkReader.FirstOrDefaultAsync(p => p.LinkReaderId == linkReaderId);

                if (linkReader != null)
                {
                    var linkReaderList = await Context.LinkReader.Where(p => p.UserId == linkReader.UserId)
                                            .Select(p => new { p.LinkReaderId, p.ChatId })
                                            .ToListAsync();

                    foreach (var item in linkReaderList)
                    {
                        var chat = await _chatBO.GetChatAsync(item.ChatId);
                        result.Add(item.LinkReaderId, chat.Title != null ? chat.Title : "LnkCapture");
                    }

                    return result;
                }
                else
                    throw new Exception("Invalid Id");
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        #endregion Methods
    }
}