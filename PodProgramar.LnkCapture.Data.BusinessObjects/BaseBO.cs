using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PodProgramar.LnkCapture.Data.BusinessObjects
{
    public abstract class BaseBO : IBO
    {
        public IConfiguration Configuration { get; }
        public ILogger Logger { get; set; }

        public BaseBO(IConfiguration configuration, ILogger<IBO> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }
    }
}