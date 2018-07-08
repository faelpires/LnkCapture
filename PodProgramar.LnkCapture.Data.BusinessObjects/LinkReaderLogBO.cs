using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.DAL;
using PodProgramar.LnkCapture.Data.Models;
using System;
using System.Threading.Tasks;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public class LinkReaderLogBO : BaseDataBO, ILinkReaderLogBO
    {
        #region Constructors

        public LinkReaderLogBO(IConfiguration configuration, LnkCaptureContext lnkCaptureContext, ILogger<LinkReaderLogBO> logger) : base(lnkCaptureContext, configuration, logger)
        {
            Logger = logger;
        }

        #endregion Constructors

        #region Methods

        public async Task SaveLogAsync(Guid linkReaderId, bool IsAPIRequest, int rowCount)
        {
            try
            {
                var linkReaderLog = new LinkReaderLog()
                {
                    AccessDate = DateTime.Now,
                    IsAPIRequest = IsAPIRequest,
                    LinkReaderId = linkReaderId,
                    LinkReaderLogId = Guid.NewGuid(),
                    RowCount = rowCount
                };

                Context.LinkReaderLog.Add(linkReaderLog);
                await Context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message);

                throw;
            }
        }

        #endregion Methods
    }
}