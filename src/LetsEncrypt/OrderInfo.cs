using Certes.Acme;

namespace LetsEncrypt
{
    public class OrderInfo
    {
        public IOrderContext Order { get; set; }
        public IChallengeContext Challenge { get; set; }
        public string HostName { get; set; }
    }
}
