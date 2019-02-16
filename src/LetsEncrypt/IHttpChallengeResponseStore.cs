namespace LetsEncrypt
{
    public interface IHttpChallengeResponseStore
    {
        void AddChallengeResponse(string token, OrderInfo orderInfo);

        bool TryGetResponse(string token, out OrderInfo orderInfo);
        void ClearPendingOrders();
    }
}
