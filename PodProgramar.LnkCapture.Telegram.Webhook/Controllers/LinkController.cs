using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using PodProgramar.Utils.Cryptography;
using System;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Controllers
{
    [Route("api/[controller]")]
    public class LinkController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILinkBO _linkBO;
        private readonly string _encryptionKey;

        public LinkController(IConfiguration configuration, ILinkBO linkBO)
        {
            _configuration = configuration;
            _linkBO = linkBO;
            _encryptionKey = _configuration.GetSection("AppConfiguration")["EncryptionKey"];
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, [FromQuery(Name = "search")] string search = null,
                                                        [FromQuery(Name = "user")]string user = null,
                                                        [FromQuery(Name = "startDate")]DateTime? startDate = null,
                                                        [FromQuery(Name = "endDate")] DateTime? endDate = null,
                                                        [FromQuery(Name = "pageIndex")] int? pageIndex = null,
                                                        [FromQuery(Name = "pageSize")] int? pageSize = null)
        {
            var chatId = Encryptor.DecryptString(Uri.EscapeDataString(id) != id ? Uri.UnescapeDataString(id) : id, _encryptionKey);

            object result = null;

            if (HttpContext.Request.ContentType != null && HttpContext.Request.ContentType.ToLowerInvariant() == "application/json")
                result = await _linkBO.GetChatLinksAsync(long.Parse(chatId), search, user, startDate, endDate, pageIndex, pageSize);
            else
            {
                return RedirectToPage("/Index", new { id });
            }

            return Ok(result);
        }
    }
}