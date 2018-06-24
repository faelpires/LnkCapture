using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
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
        private string _purpose;
        private readonly IDataProtectionProvider _provider;

        public UpdateController(IConfiguration configuration, IDataProtectionProvider provider, ILinkBO linkBO)
        {
            _configuration = configuration;
            _provider = provider;
            _linkBO = linkBO;
            _purpose = _configuration.GetSection("AppConfiguration")["Purpose"];
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
                var protector = _provider.CreateProtector(_purpose);
                var chatId = protector.Protect(update.Message.Chat.Id.ToString());

                await _linkBO.SendLinksRecoverMessageAsync(update, chatId);
            }
            else
                await _linkBO.SaveLinkAsync(update);

            return Ok();
        }
    }
}