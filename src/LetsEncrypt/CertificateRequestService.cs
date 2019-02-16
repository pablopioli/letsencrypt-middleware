using Certes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LetsEncrypt
{
    public class CertificateRequestService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly LetsEncryptOptions _options;
        private readonly CertificateSelector _certificateSelector;
        private readonly AccountManager _accountManager;
        private readonly IHttpChallengeResponseStore _httpChallengeResponseStore;
        private Timer _timer;

        public CertificateRequestService(ILogger<CertificateRequestService> logger,
            IOptions<LetsEncryptOptions> options,
            CertificateSelector certificateSelector,
            AccountManager accountManager,
            IHttpChallengeResponseStore httpChallengeResponseStore)
        {
            _logger = logger;
            _options = options.Value;
            _certificateSelector = certificateSelector;
            _accountManager = accountManager;
            _httpChallengeResponseStore = httpChallengeResponseStore;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LetsEncrypt request service has started");

            _timer = new Timer(RefreshCertificates, null, TimeSpan.Zero, TimeSpan.FromHours(12));

            return Task.CompletedTask;
        }

        private async void RefreshCertificates(object state)
        {
            _httpChallengeResponseStore.ClearPendingOrders();

            var hostNames = _certificateSelector.GetCertificatesAboutToExpire();

            if (hostNames.Length > 0)
            {
                var account = await _accountManager.GetAccountKey();

                if (string.IsNullOrEmpty(account))
                {
                    _logger.LogError("Canot get Let´s Encrpyt account");
                }
                else
                {
                    try
                    {
                        foreach (var host in hostNames)
                        {
                            var acme = new AcmeContext(_options.AcmeServer, KeyFactory.FromPem(account));

                            _logger.LogInformation("LetsEncrypt: Creating order");

                            var order = await acme.NewOrder(new[] { host });
                            var authz = (await order.Authorizations()).First();
                            var httpChallenge = await authz.Http();

                            var orderInfo = new OrderInfo
                            {
                                Order = order,
                                Challenge = httpChallenge,
                                HostName = host
                            };

                            _httpChallengeResponseStore.AddChallengeResponse(httpChallenge.Token, orderInfo);
                            await httpChallenge.Validate();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        throw;
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LetsEncrypt request service is stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
