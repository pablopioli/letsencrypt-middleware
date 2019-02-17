using LetsEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace BasicSample
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var cacheFolder = "certcache";
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            var leOptions = new LetsEncryptOptions
            {
                EmailAddress = "testmail4642256@outlook.com",
                AcceptTermsOfService = true,
                UseStagingServer = true,
                CacheFolder = cacheFolder,
                Hosts = new string[] { "d73a1d56.ngrok.io" },
                EncryptionPassword = "FBD2690B-63B2-43FB-B331-78004B505D86",
            };

            services.AddLetsEncrypt(leOptions);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.MapWhen(
                httpContext => !httpContext.Request.Path.StartsWithSegments(LetsEncrypt.Constants.ChallengePath),
                appBuilder =>
                {
                    appBuilder.UseHttpsRedirection();
                    app.UseMvc();
                    app.UseStaticFiles();
                    app.UseCookiePolicy();
                }
            );


            app.UseLetsEncrypt();

        }
    }
}
