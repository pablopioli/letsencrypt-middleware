using Certes.Acme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LetsEncrypt
{
    public class LetsEncryptOptions
    {
        /// <summary>
        /// Initialize an instance of <see cref="LetsEncryptOptions" />
        /// </summary>
        public LetsEncryptOptions()
        {
            // Default to the production server.
            AcmeServer = WellKnownServers.LetsEncryptV2;
        }

        /// <summary>
        /// Indicate that you agree with Let's Encrypt's terms of service.
        /// <para>
        /// See <see href="https://letsencrypt.org">https://letsencrypt.org</see> for details.
        /// </para>
        /// </summary>
        public bool AcceptTermsOfService { get; set; }

        /// <summary>
        /// The email address used to register with letsencrypt.org.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Use Let's Encrypt staging server.
        /// <para>
        /// This is recommended during development of the application.
        /// </para>
        /// </summary>
        public bool UseStagingServer
        {
            get => AcmeServer == WellKnownServers.LetsEncryptStagingV2;
            set => AcmeServer = value
                    ? WellKnownServers.LetsEncryptStagingV2
                    : WellKnownServers.LetsEncryptV2;
        }

        /// <summary>
        /// A folder to store obtained certificates
        /// </summary>
        public string CacheFolder { get; set; }

        /// <summary>
        /// The uri to the server that implements thE ACME protocol for certificate generation.
        /// </summary>
        internal Uri AcmeServer { get; set; }

        /// <summary>
        /// The private key corresponding to the Let´s Encrypt account
        /// If not set another account will be created
        /// If a CacheFolder is provided and the key is not set, the account key will be stored in this folder
        /// </summary>
        internal string AccountKey { get; set; }

        /// <summary>
        /// The hosts for wich you can request certificates
        /// </summary>
        public string[] Hosts
        {
            get
            {
                return _hosts.Select(x => x.HostName).ToArray();
            }
            set
            {
                foreach (var hostName in value)
                {
                    _hosts.Add(new HostInfo
                    {
                        HostName = hostName
                    });
                }
            }
        }

        public int DaysBefore { get; set; } = 15;

        /// <summary>
        /// The password used to protect the generated certificate
        /// </summary>
        private string _encryptionPassword = string.Empty;
        public string EncryptionPassword
        {
            get => _encryptionPassword;
            set => _encryptionPassword = value ?? throw new ArgumentNullException(nameof(value));
        }
        internal List<HostInfo> ConfiguredHosts => _hosts;
        private readonly List<HostInfo> _hosts = new List<HostInfo>();

        public void AddHostName(string hostName, X509Certificate2 fallbackCertificate)
        {
            _hosts.Add(new HostInfo
            {
                HostName = hostName,
                FallBackCertificate = fallbackCertificate
            });
        }

        internal class HostInfo
        {
            public string HostName { get; set; }
            public X509Certificate2 FallBackCertificate { get; set; }
        }
    }
}
