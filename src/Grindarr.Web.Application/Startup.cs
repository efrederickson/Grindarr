using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grindarr.Core;
using Grindarr.Web.Api.Authorization;
using Grindarr.Web.Frontend;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Grindarr.Web.Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllers().AddApplicationPart(typeof(Grindarr.Web.Api.Controllers.DownloadController).Assembly);
            services.AddHostedService<ApplicationPartsLogger>();
            services.AddGrindarrFrontend();

            // Here we initialize the Grindarr Config values to ensure the defaults exist
            Config.Instance.RegisterDefaultCoreConfigurationValues();
            ApiKeyWrapper.RegisterDefaultConfiguration();

            new Core.Scrapers.ApacheOpenDirectoryScraper.ApacheOpenDirectoryScraper(new Uri("http://dummy.com"));
            new Core.Scrapers.GetComicsDotInfo.GetComicsScraper();
            new Core.Scrapers.NginxOpenDirectoryScraper.NginxOpenDirectoryScraper("http://dummy.com");
            new Grindarr.Soulseek.SoulseekScraper();
            Grindarr.Core.Downloaders.DownloaderFactory.Register("zippyshare.com", new Grindarr.Core.Downloaders.Zippyshare.ZippyshareDownloader());
            Grindarr.Core.Downloaders.DownloaderFactory.Register("soulseek", new Grindarr.Soulseek.SoulSeekDownloader());

            Core.Logging.Log.WriteLine("Grindarr Web Application loading...");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseMiddleware(typeof(ErrorHandlingMiddleware));
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }
            
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseApiKeyMiddleware("/api");
            
            app.UseGrindarrFrontend(redirectRoot: true);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
