using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILinkBO _linkBO;

        public string ChatIdEncrypted { get; private set; }

        public IndexModel(IConfiguration configuration, ILinkBO linkBO)
        {
            _configuration = configuration;
            _linkBO = linkBO;
        }

        public async Task<IActionResult> OnGetAsync([FromRoute]string id)
        {
            ChatIdEncrypted = id;

            return Page();
        }
    }
}