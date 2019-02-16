using Certes;
using Certes.Acme;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LetsEncrypt
{
    public class AccountManager
    {
        private readonly ILogger _logger;
        private readonly LetsEncryptOptions _options;
        private string _accountKey;
        private string _keyFile;

        public AccountManager(ILogger<CertificateRequestService> logger, IOptions<LetsEncryptOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            if (string.IsNullOrEmpty(options.Value.AccountKey))
            {
                if (!string.IsNullOrEmpty(options.Value.CacheFolder))
                {
                    _keyFile = Path.Combine(options.Value.CacheFolder, "account");

                    if (File.Exists(_keyFile))
                    {
                        _accountKey = File.ReadAllText(_keyFile);
                    }
                }
            }
            else
            {
                _accountKey = options.Value.AccountKey;
            }
        }

        public async Task<string> GetAccountKey()
        {
            if (!string.IsNullOrEmpty(_accountKey))
            {
                return _accountKey;
            }

            _logger.LogInformation("Getting a new account key");

            var acme = new AcmeContext(_options.AcmeServer);
            IAccountContext account;

            try
            {
                account = await acme.NewAccount(_options.EmailAddress, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }

            var pemKey = acme.AccountKey.ToPem();

            if (!string.IsNullOrEmpty(_keyFile))
            {
                File.WriteAllText(_keyFile, pemKey);
            }

            return pemKey;
        }
    }
}
