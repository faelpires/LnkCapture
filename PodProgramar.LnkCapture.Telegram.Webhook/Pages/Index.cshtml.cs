using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Telegram.Webhook.Pages
{
    public class IndexModel : PageModel
    {
        #region Fields

        private readonly IChatBO _chatBO;
        private readonly ILinkReaderBO _linkReaderBO;

        #endregion Fields

        #region Properties

        public Guid ChatId { get; private set; }

        public IDictionary<Guid, string> Chats { get; private set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DefaultStartDate { get; private set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DefaultEndDate { get; private set; }

        #endregion Properties

        public IndexModel(IChatBO chatBO, ILinkReaderBO linkReaderBO)
        {
            _chatBO = chatBO;
            _linkReaderBO = linkReaderBO;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var linkReader = await _linkReaderBO.GetAsync(id);

                ChatId = id;
                Chats = await _linkReaderBO.GetRelatedLinkReadersAsync(linkReader.LinkReaderId);
                DefaultStartDate = DateTime.Now.AddMonths(-1);
                DefaultEndDate = DateTime.Now;

                return Page();
            }
            catch (Exception)
            {
                return RedirectToPage("Error");
            }
        }
    }
}