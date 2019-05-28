namespace VRage.Cryptography
{
    using System;
    using System.Security.Cryptography;

    public static class MySHA256
    {
        private static bool m_supportsFips = true;

        public static SHA256 Create()
        {
            try
            {
                return CreateInternal();
            }
            catch
            {
                m_supportsFips = false;
                return CreateInternal();
            }
        }

        private static SHA256 CreateInternal() => 
            (!m_supportsFips ? ((SHA256) new SHA256Managed()) : ((SHA256) new SHA256CryptoServiceProvider()));
    }
}

