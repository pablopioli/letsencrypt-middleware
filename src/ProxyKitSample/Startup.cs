using LetsEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProxyKit;
using System.IO;

namespace ProxyKitSample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.AddLetsEncrypt(leOptions);

            services.AddProxy();
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
                    appBuilder.RunProxy(context => context
                           .ForwardTo("http://localhost:6001")
                           .Send());
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
