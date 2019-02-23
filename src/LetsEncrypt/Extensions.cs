using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace LetsEncrypt
{
    public static class Extensions
    {
        public static void UseLetsEncrypt(this IApplicationBuilder builder)
        {
            builder.Map(LetsEncrypt.Constants.ChallengePath, mapped =>
            {
                mapped.UseMiddleware<HttpChallengeResponseMiddleware>();
            });
        }

        public static void AddLetsEncrypt(this IServiceCollection services, LetsEncryptOptions options)
        {
            if (!options.AcceptTermsOfService)
            {
                throw new Exception("You must accept Let’s Encrypt terms of service");
            }

            services.Configure<LetsEncryptOptions>(x =>
            {
                x.EmailAddress = options.EmailAddress;
                x.CacheFolder = options.CacheFolder;
                x.AccountKey = options.AccountKey;
                x.EncryptionPassword = options.EncryptionPassword;
                x.DaysBefore = options.DaysBefore;
            });

            var selector = new CertificateSelector(options);

            foreach (var host in options.ConfiguredHosts)
            {
                var cert = host.FallBackCertificate;
                if (host.FallBackCertificate == null && !string.IsNullOrEmpty(options.CacheFolder))
                {
                    var fileName = Path.Combine(options.CacheFolder, host.HostName + ".pfx");
                    if (File.Exists(fileName))
                    {
                        cert = new X509Certificate2(fileName, options.EncryptionPassword);
                    }
                }

                selector.Use(host.HostName, cert);
            }

            ServiceLocator.SetCertificateSelector(selector);

            services.AddSingleton<CertificateSelector>(x => selector);
            services.AddSingleton<AccountManager>();
            services.AddSingleton<HttpChallengeResponseMiddleware>();
            services.AddSingleton<IHttpChallengeResponseStore, InMemoryHttpChallengeResponseStore>();

            services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelOptionsSetup>();
            services.AddTransient<CertificateBuilderService>();

            services.AddHostedService<CertificateRequestService>();
        }
    }
}
