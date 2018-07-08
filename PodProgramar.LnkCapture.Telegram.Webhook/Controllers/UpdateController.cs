using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly ILinkBO _linkBO;
        private readonly IMessageBO _messageBO;

        public UpdateController(ILinkBO linkBO, IMessageBO messageBO)
        {
            _linkBO = linkBO;
            _messageBO = messageBO;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {
                var validBotCommands = new[] { "/linksurl@LnkCaptureBot", "/linksurl" };
                MessageEntity entity = null;
                string entityValue = null;

                if (update.Message.Entities != null && update.Message.EntityValues != null)
                {
                    entity = update.Message.Entities.FirstOrDefault();
                    entityValue = update.Message.EntityValues.FirstOrDefault();
                }

                if (entity != null && entityValue != null && entity.Type == MessageEntityType.BotCommand && validBotCommands.Contains(entityValue))
                    await _messageBO.SendLinksRecoverMessageAsync(update);
                else
                    await _linkBO.SaveAsync(update);
            }

            return Ok();
        }
    }
}