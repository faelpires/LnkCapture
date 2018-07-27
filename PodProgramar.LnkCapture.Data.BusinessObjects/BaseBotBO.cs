using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MihaZupan;
using Telegram.Bot;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public abstract class BaseBotBO : BaseBO
    {
        public TelegramBotClient BotClient { get; }

        public BaseBotBO(IConfiguration configuration, ILogger<BaseBotBO> logger) : base(configuration, logger)
        {
            var botConfiguration = Configuration.GetSection("BotConfiguration");

            BotClient = string.IsNullOrEmpty(botConfiguration["Socks5Host"])
                ? new TelegramBotClient(botConfiguration["BotToken"])
                : new TelegramBotClient(
                    botConfiguration["BotToken"],
                    new HttpToSocks5Proxy(botConfiguration["Socks5Host"], int.Parse(botConfiguration["Socks5Port"])));
        }
    }
}