namespace VRage.Common.Utils
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public class MyRSA
    {
        private HashAlgorithm m_hasher = MySHA256.Create();

        public MyRSA()
        {
            this.m_hasher.Initialize();
        }

        public void GenerateKeys(string publicKeyFileName, string privateKeyFileName)
        {
            byte[] buffer;
            byte[] buffer2;
            this.GenerateKeys(out buffer, out buffer2);
            if ((buffer != null) && (buffer2 != null))
            {
                File.WriteAllText(publicKeyFileName, Convert.ToBase64String(buffer));
                File.WriteAllText(privateKeyFileName, Convert.ToBase64String(buffer2));
            }
        }

        public void GenerateKeys(out byte[] publicKey, out byte[] privateKey)
        {
            RSACryptoServiceProvider provider = null;
            try
            {
                CspParameters parameters = new CspParameters();
                parameters.ProviderType = 1;
                parameters.Flags = CspProviderFlags.UseArchivableKey;
                parameters.KeyNumber = 1;
                provider = new RSACryptoServiceProvider(parameters) {
                    PersistKeyInCsp = false
                };
                publicKey = provider.ExportCspBlob(false);
                privateKey = provider.ExportCspBlob(true);
            }
            catch (Exception)
            {
                publicKey = null;
                privateKey = null;
            }
            finally
            {
                if (provider != null)
                {
                    provider.PersistKeyInCsp = false;
                }
            }
        }

        public string SignData(string data, string privateKey)
        {
            byte[] buffer;
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.PersistKeyInCsp = false;
                byte[] bytes = new UTF8Encoding().GetBytes(data);
                try
                {
                    provider.ImportCspBlob(Convert.FromBase64String(privateKey));
                    buffer = provider.SignData(bytes, this.m_hasher);
                }
                catch (CryptographicException)
                {
                    return null;
                }
                finally
                {
                    provider.PersistKeyInCsp = false;
                }
            }
            return Convert.ToBase64String(buffer);
        }

        public string SignHash(byte[] hash, byte[] privateKey)
        {
            byte[] buffer;
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.PersistKeyInCsp = false;
                try
                {
                    provider.ImportCspBlob(privateKey);
                    buffer = provider.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
                }
                catch (CryptographicException)
                {
                    return null;
                }
                finally
                {
                    provider.PersistKeyInCsp = false;
                }
            }
            return Convert.ToBase64String(buffer);
        }

        public string SignHash(string hash, string privateKey)
        {
            byte[] buffer;
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.PersistKeyInCsp = false;
                byte[] bytes = new UTF8Encoding().GetBytes(hash);
                try
                {
                    provider.ImportCspBlob(Convert.FromBase64String(privateKey));
                    buffer = provider.SignHash(bytes, CryptoConfig.MapNameToOID("SHA256"));
                }
                catch (CryptographicException)
                {
                    return null;
                }
                finally
                {
                    provider.PersistKeyInCsp = false;
                }
            }
            return Convert.ToBase64String(buffer);
        }

        public bool VerifyData(string originalMessage, string signedMessage, string publicKey)
        {
            bool flag;
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                byte[] bytes = new UTF8Encoding().GetBytes(originalMessage);
                byte[] signature = Convert.FromBase64String(signedMessage);
                try
                {
                    provider.ImportCspBlob(Convert.FromBase64String(publicKey));
                    flag = provider.VerifyData(bytes, this.m_hasher, signature);
                }
                catch (CryptographicException)
                {
                    flag = false;
                }
                finally
                {
                    provider.PersistKeyInCsp = false;
                }
            }
            return flag;
        }

        public bool VerifyHash(byte[] hash, byte[] signedHash, byte[] publicKey)
        {
            bool flag;
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                try
                {
                    provider.ImportCspBlob(publicKey);
                    flag = provider.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signedHash);
                }
                catch (CryptographicException)
                {
                    flag = false;
                }
                finally
                {
                    provider.PersistKeyInCsp = false;
                }
            }
            return flag;
        }

        public bool VerifyHash(string hash, string signedHash, string publicKey)
        {
            bool flag;
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                byte[] bytes = new UTF8Encoding().GetBytes(hash);
                byte[] rgbSignature = Convert.FromBase64String(signedHash);
                try
                {
                    provider.ImportCspBlob(Convert.FromBase64String(publicKey));
                    flag = provider.VerifyHash(bytes, CryptoConfig.MapNameToOID("SHA256"), rgbSignature);
                }
                catch (CryptographicException)
                {
                    flag = false;
                }
                finally
                {
                    provider.PersistKeyInCsp = false;
                }
            }
            return flag;
        }

        public HashAlgorithm HashObject =>
            this.m_hasher;
    }
}

