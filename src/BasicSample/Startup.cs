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
                EmailAddress = "hostmaster@example.com",
                AcceptTermsOfService = true,
                CacheFolder = cacheFolder,
                Hosts = new string[] { "example.com" },
                EncryptionPassword = "4570F8BA-0DC7-42AB-9FC2-246EA841453C",
            };

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
            });

            services.AddLetsEncrypt(leOptions);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapWhen(
                httpContext => !httpContext.Request.Path.StartsWithSegments(LetsEncrypt.Constants.ChallengePath),
                appBuilder =>
                {
                    appBuilder.UseExceptionHandler("/Error");
                    appBuilder.UseHsts();
                    appBuilder.UseHttpsRedirection();
                    appBuilder.UseMvc();
                    appBuilder.UseStaticFiles();
                    appBuilder.UseCookiePolicy();
                }
            );

            app.MapWhen(
                httpContext => httpContext.Request.Path.StartsWithSegments(LetsEncrypt.Constants.ChallengePath),
                appBuilder =>
                {
                    appBuilder.UseLetsEncrypt();
                }
            );
        }
    }
}
