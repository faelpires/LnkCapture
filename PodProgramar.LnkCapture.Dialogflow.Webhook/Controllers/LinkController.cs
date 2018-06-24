﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Controllers
{
    [Route("api/[controller]")]
    public class LinkController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILinkBO _linkBO;
        private string _purpose;
        private readonly IDataProtectionProvider _provider;

        public LinkController(IConfiguration configuration, IDataProtectionProvider provider, ILinkBO linkBO)
        {
            _configuration = configuration;
            _provider = provider;
            _linkBO = linkBO;
            _purpose = _configuration.GetSection("AppConfiguration")["Purpose"];
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var protector = _provider.CreateProtector(_purpose);
            var chatId = protector.Unprotect(id);

            object result = null;

            if (HttpContext.Request.ContentType != null && HttpContext.Request.ContentType.ToLowerInvariant() == "application/json")
                result = await _linkBO.GetChatLinksAsync(long.Parse(chatId));
            else
            {
                return RedirectToPage("/Index", new { id = id });
            }

            return Ok(result);
        }
    }
}