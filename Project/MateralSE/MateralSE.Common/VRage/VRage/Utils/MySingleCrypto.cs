namespace VRage.Utils
{
    using System;

    public class MySingleCrypto
    {
        private readonly byte[] m_password;

        private MySingleCrypto()
        {
        }

        public MySingleCrypto(byte[] password)
        {
            this.m_password = (byte[]) password.Clone();
        }

        public void Decrypt(byte[] data, int length)
        {
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte) (data[i] - this.m_password[index]);
                index = (index + 1) % this.m_password.Length;
            }
        }

        public void Encrypt(byte[] data, int length)
        {
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte) (data[i] + this.m_password[index]);
                index = (index + 1) % this.m_password.Length;
            }
        }
    }
}

