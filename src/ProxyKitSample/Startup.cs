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
                EmailAddress = "testmail4642256@outlook.com",
                AcceptTermsOfService = true,
                UseStagingServer = true,
                CacheFolder = cacheFolder,
                Hosts = new string[] { "d73a1d56.ngrok.io" },
                EncryptionPassword = "FBD2690B-63B2-43FB-B331-78004B505D86",
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

            app.UseLetsEncrypt();

            app.RunProxy(context => context
                   .ForwardTo("http://localhost:6001")
                   .Send());
        }
    }
}
