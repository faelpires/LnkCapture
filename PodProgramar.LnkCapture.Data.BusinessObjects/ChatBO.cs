using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class ChatBO : BaseBotBO, IChatBO
    {
        public ChatBO(IConfiguration configuration, ILogger<ChatBO> logger) : base(configuration, logger)
        {
            Logger = logger;
        }

        public async Task<Chat> GetChatAsync(long chatId)
        {
            try
            {
                return await BotClient.GetChatAsync(chatId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }
    }
}
