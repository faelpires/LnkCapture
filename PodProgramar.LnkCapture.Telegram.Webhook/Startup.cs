using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using PodProgramar.LnkCapture.Data.BusinessObjects;
using PodProgramar.LnkCapture.Data.DAL;
using System.Globalization;

namespace PodProgramar.LnkCapture.Telegram.Webhook
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

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

            services.AddTransient<IConfigBO, ConfigBO>();
            services.AddTransient<IBotCommandsBO, BotCommandBO>();
            services.AddTransient<IChatBO, ChatBO>();
            services.AddTransient<IMessageBO, MessageBO>();
            services.AddTransient<ILinkReaderLogBO, LinkReaderLogBO>();
            services.AddTransient<ILinkReaderBO, LinkReaderBO>();
            services.AddTransient<ILinkBO, LinkBO>();
            services.AddTransient<ICrawlerBO, CrawlerBO>();

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

            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("pt-BR")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });
        }
    }
}