using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace LetsEncrypt
{
    public class CertificateSelector
    {
        private ConcurrentDictionary<string, X509Certificate2> _certs = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

        private readonly LetsEncryptOptions _options;

        public CertificateSelector(LetsEncryptOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string[] GetCertificatesAboutToExpire()
        {
            var certs = _certs.ToArray();
            var result = new List<string>();

            foreach (var cert in certs)
            {

                bool mustRequest;
                if (cert.Value == null)
                {
                    mustRequest = true;
                }
                else
                {
                    mustRequest = DateTime.UtcNow.AddDays(_options.DaysBefore) > cert.Value.NotAfter;
                }

                if (mustRequest)
                {
                    result.Add(cert.Key);
                }
            }

            return result.ToArray();
        }

        public void Use(string hostName, X509Certificate2 certificate)
        {
            _certs.AddOrUpdate(hostName, certificate, (_, __) => certificate);
        }

        public X509Certificate2 Select(ConnectionContext features, string hostName)
        {
            if (!_certs.TryGetValue(hostName, out var retVal))
            {
                return null;
            }

            return retVal;
        }
    }
}
