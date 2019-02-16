using Certes;
using Certes.Acme;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace LetsEncrypt
{
    public class CertificateBuilderService
    {
        private readonly ILogger _logger;
        private readonly LetsEncryptOptions _options;
        private readonly CertificateSelector _certificateSelector;

        public CertificateBuilderService(ILogger<CertificateBuilderService> logger,
                                         IOptions<LetsEncryptOptions> options,
                                         CertificateSelector certificateSelector)
        {
            _logger = logger;
            _options = options.Value;
            _certificateSelector = certificateSelector;
        }

        public async Task BuildCertificate(IOrderContext order, string hostName)
        {
            try
            {
                var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);

                var cert = await order.Generate(new CsrInfo
                {
                    CommonName = hostName
                }, privateKey);

                var pfxBuilder = cert.ToPfx(privateKey);
                var pfx = pfxBuilder.Build(hostName, _options.EncryptionPassword);

                var x509Cert = new X509Certificate2(pfx, _options.EncryptionPassword);

                _certificateSelector.Use(hostName, x509Cert);

                if (!string.IsNullOrEmpty(_options.CacheFolder))
                {
                    var fileName = Path.Combine(_options.CacheFolder, hostName + ".pfx");

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    File.WriteAllBytes(fileName, pfx);
                }

                _logger.LogInformation($"New certificate generated for {hostName}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}
