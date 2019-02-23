namespace LetsEncrypt
{
    public static class ServiceLocator
    {
        private static CertificateSelector _certificateSelector;

        public static CertificateSelector GetCertificateSelector()
        {
            return _certificateSelector;
        }
        internal static void SetCertificateSelector(CertificateSelector certificateSelector)
        {
            _certificateSelector = certificateSelector;
        }
    }
}
