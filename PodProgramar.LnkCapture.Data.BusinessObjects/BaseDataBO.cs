using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodProgramar.LnkCapture.Data.DAL;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public abstract class BaseDataBO : BaseBO
    {
        public LnkCaptureContext Context { get; }

        public BaseDataBO(LnkCaptureContext lnkCaptureContext, IConfiguration configuration, ILogger<BaseDataBO> logger) : base(configuration, logger)
        {
            Context = lnkCaptureContext;
        }
    }
}