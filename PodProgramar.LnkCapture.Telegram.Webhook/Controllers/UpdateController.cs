using Microsoft.AspNetCore.Mvc;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly IBotCommandsBO _botCommandBO;
        private readonly ILinkBO _linkBO;
        private readonly IMessageBO _messageBO;

        public UpdateController(ILinkBO linkBO, IMessageBO messageBO, IBotCommandsBO botCommandBO)
        {
            _botCommandBO = botCommandBO;
            _linkBO = linkBO;
            _messageBO = messageBO;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            if (_botCommandBO.IsBotCommand(update))
                await _botCommandBO.ExecuteCommandAsync(update);
            else
                await _linkBO.SaveAsync(update);

            return Ok();
        }
    }
}