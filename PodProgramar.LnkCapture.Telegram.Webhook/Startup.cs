using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using PodProgramar.LnkCapture.Data.DAL;
using System.IO;

namespace PodProgramar.LnkCapture.Telegram.Webhook
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .AddRazorPagesOptions(options =>
                    {
                        options.Conventions.AddPageRoute("/Index", "/{id}");
                    })
                    .AddJsonOptions(o =>
                    {
                        o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                        o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                                    builder =>
                                    {
                                        builder.AllowAnyHeader();
                                        builder.AllowAnyMethod();
                                        builder.AllowAnyOrigin();
                                        builder.AllowCredentials();
                                    });
            });

            services.AddSingleton(_ => Configuration);

            services.AddDbContext<LnkCaptureContext>(options => options.UseSqlServer(Configuration.GetConnectionString("LnkCaptureDatabase")));

            services.AddTransient<IChatBO, ChatBO>();
            services.AddTransient<IMessageBO, MessageBO>();
            services.AddTransient<ILinkReaderLogBO, LinkReaderLogBO>();
            services.AddTransient<ILinkReaderBO, LinkReaderBO>();
            services.AddTransient<ILinkBO, LinkBO>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAllOrigins");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddAzureWebAppDiagnostics(new AzureAppServicesDiagnosticsSettings
            {
                OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level}] {RequestId}-{SourceContext}: {Message}{NewLine}{Exception}"
            });

            app.UseMvc();

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });
        }
    }
}