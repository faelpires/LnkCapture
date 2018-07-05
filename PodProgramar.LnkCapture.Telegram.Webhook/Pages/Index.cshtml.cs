using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MihaZupan;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using PodProgramar.Utils.Cryptography;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILinkBO _linkBO;
        private readonly string _encryptionKey;
        private readonly TelegramBotClient _telegramBotClient;

        public string ChatIdEncrypted { get; private set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DefaultStartDate { get; private set; }

        public DateTime DefaultEndDate { get; private set; }

        public string ChatTitle { get; private set; }

        public IndexModel(IConfiguration configuration, ILinkBO linkBO)
        {
            var botConfiguration = configuration.GetSection("BotConfiguration");
            _configuration = configuration;
            _linkBO = linkBO;
            _encryptionKey = _configuration.GetSection("AppConfiguration")["EncryptionKey"];

            _telegramBotClient = string.IsNullOrEmpty(botConfiguration["Socks5Host"])
                ? new TelegramBotClient(botConfiguration["BotToken"])
                : new TelegramBotClient(
                    botConfiguration["BotToken"],
                    new HttpToSocks5Proxy(botConfiguration["Socks5Host"], int.Parse(botConfiguration["Socks5Port"])));
        }

        public async Task<IActionResult> OnGetAsync([FromQuery]string id)
        {
            var chatId = long.Parse(Encryptor.DecryptString(Uri.EscapeDataString(id) != id ? Uri.UnescapeDataString(id) : id, _encryptionKey));
            var chat = await _telegramBotClient.GetChatAsync(chatId);

            ChatTitle = chat?.Title;
            ChatIdEncrypted = id;
            DefaultStartDate = DateTime.Now.AddMonths(-1);
            DefaultEndDate = DateTime.Now;

            return Page();
        }
    }
}