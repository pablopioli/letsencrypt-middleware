using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LetsEncrypt
{
    public class HttpChallengeResponseMiddleware : IMiddleware
    {
        private readonly ILogger<HttpChallengeResponseMiddleware> _logger;
        private readonly IHttpChallengeResponseStore _httpChallengeResponseStore;
        private readonly CertificateBuilderService _certificateBuilderService;

        public HttpChallengeResponseMiddleware(ILogger<HttpChallengeResponseMiddleware> logger,
            IHttpChallengeResponseStore httpChallengeResponseStore,
            CertificateBuilderService certificateBuilderService)
        {
            _logger = logger;
            _httpChallengeResponseStore = httpChallengeResponseStore;
            _certificateBuilderService = certificateBuilderService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Path.ToString();

            _logger.LogDebug("Requested challenge request for {token}", token);

            // assumes that this middleware has been mapped
            if (token.StartsWith("/"))
            {
                token = token.Substring(1);
            }

            if (!_httpChallengeResponseStore.TryGetResponse(token, out var orderInfo))
            {
                await next(context);
                return;
            }

            _logger.LogDebug("Confirmed challenge request for {token}", token);

            context.Response.ContentLength = orderInfo.Challenge.KeyAuthz.Length;
            context.Response.ContentType = "application/octet-stream";
            await context.Response.WriteAsync(orderInfo.Challenge.KeyAuthz, context.RequestAborted);

            _ = Task.Run(async () =>
              {
                  // Give some time to Let´s Encrypt to process our response
                  await Task.Delay(30 * 1000);
                  await _certificateBuilderService.BuildCertificate(orderInfo.Order, orderInfo.HostName);
              });
        }
    }
}
