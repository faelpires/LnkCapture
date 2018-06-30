using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using PodProgramar.Utils.Cryptography;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILinkBO _linkBO;
        private readonly string _encryptionKey;

        public UpdateController(IConfiguration configuration, ILinkBO linkBO)
        {
            _configuration = configuration;
            _linkBO = linkBO;
            _encryptionKey = _configuration.GetSection("AppConfiguration")["EncryptionKey"];
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            if (
                    (update.Type == UpdateType.Message && (update.Message.Text == "/linksurl@LnkCaptureBot" || update.Message.Text == "/linksurl"))
                    ||
                    (update.Type == UpdateType.EditedMessage && (update.EditedMessage.Text == "/linksurl@LnkCaptureBot" || update.EditedMessage.Text == "/linksurl"))
                )
            {
                var chatId = Uri.EscapeDataString(Encryptor.EncryptString(update.Message.Chat.Id.ToString(), _encryptionKey));
                var userId = update.Message.From.Id;

                await _linkBO.SendLinksRecoverMessageAsync(update, chatId, userId);
            }
            else
                await _linkBO.SaveLinkAsync(update);

            return Ok();
        }
    }
}