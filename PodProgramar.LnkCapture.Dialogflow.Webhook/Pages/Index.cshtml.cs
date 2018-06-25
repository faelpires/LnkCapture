using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using PodProgramar.LnkCapture.Data.DTO;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILinkBO _linkBO;
        private readonly string _purpose;
        private readonly IDataProtectionProvider _provider;

        public string ChatIdEncrypted { get; private set; }

        public IndexModel(IConfiguration configuration, IDataProtectionProvider provider, ILinkBO linkBO)
        {
            _configuration = configuration;
            _provider = provider;
            _linkBO = linkBO;
            _purpose = _configuration.GetSection("AppConfiguration")["Purpose"];
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            ChatIdEncrypted = id;

            return Page();
        }
    }
}