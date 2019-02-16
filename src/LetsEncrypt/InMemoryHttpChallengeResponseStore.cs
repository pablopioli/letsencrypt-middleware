using System.Collections.Concurrent;

namespace LetsEncrypt
{
    internal class InMemoryHttpChallengeResponseStore : IHttpChallengeResponseStore
    {
        private ConcurrentDictionary<string, OrderInfo> _values = new ConcurrentDictionary<string, OrderInfo>();

        public void AddChallengeResponse(string token, OrderInfo orderInfo)
            => _values.AddOrUpdate(token, orderInfo, (_, __) => orderInfo);

        public bool TryGetResponse(string token, out OrderInfo orderInfo)
            => _values.TryGetValue(token, out orderInfo);

        public void ClearPendingOrders()
        {
            _values.Clear();
        }
    }
}
