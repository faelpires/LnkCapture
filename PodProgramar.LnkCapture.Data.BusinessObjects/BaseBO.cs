using Microsoft.Extensions.Configuration;
using PodProgramar.LnkCapture.Data.DAL;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public abstract class BaseBO : IBO
    {
        public LnkCaptureContext Context { get; }
        public IConfiguration Configuration { get; }

        public BaseBO(LnkCaptureContext lnkcaptureContext, IConfiguration configuration)
        {
            this.Context = lnkcaptureContext;
            this.Configuration = configuration;
        }
    }
}