using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace BasicSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            builder.UseStartup<Startup>();

            builder.UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 80);
                options.Listen(IPAddress.Any, 443, listenOptions =>
                {
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ServerCertificateSelector = (features, name) =>
                        {
                            var certSelector = LetsEncrypt.ServiceLocator.GetCertificateSelector();
                            return certSelector.Select(features, name);
                        };
                    });
                });
            });

            return builder;
        }
    }
}
