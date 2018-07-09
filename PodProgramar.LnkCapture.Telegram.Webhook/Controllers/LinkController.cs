using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using System;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Controllers
{
    [Route("api/[controller]")]
    public class LinkController : Controller
    {
        private readonly ILinkBO _linkBO;
        private readonly ILinkReaderBO _linkReaderBO;

        public LinkController(ILinkBO linkBO, ILinkReaderBO linkReaderBO)
        {
            _linkBO = linkBO;
            _linkReaderBO = linkReaderBO;
        }

        [HttpGet("/api/[controller]/{id}")]
        [EnableCors("AllowAllOrigins")]
        public async Task<IActionResult> Get([FromRoute] string id,
                                             [FromQuery(Name = "search")] string search = null,
                                             [FromQuery(Name = "user")]string user = null,
                                             [FromQuery(Name = "startDate")]DateTime? startDate = null,
                                             [FromQuery(Name = "endDate")] DateTime? endDate = null,
                                             [FromQuery(Name = "pageIndex")] int? pageIndex = null,
                                             [FromQuery(Name = "pageSize")] int? pageSize = null)
        {
            var linkReaderId = Guid.Parse(id);
            var linkReader = await _linkReaderBO.GetAsync(linkReaderId);
            var isAPIRequest = Request.Headers.ContainsKey("IsAPIRequest") ? bool.Parse(Request.Headers["IsAPIRequest"]) : true;

            object result = null;

            if (HttpContext.Request.ContentType != null && HttpContext.Request.ContentType.ToLowerInvariant().StartsWith("application/json"))
            {
                result = await _linkBO.GetAsync(linkReader, isAPIRequest, search, user, startDate, endDate, pageIndex, pageSize);
                return Ok(result);
            }
            else
            {
                return RedirectToPage($"/Index/{linkReaderId}");
            }
        }
    }
}