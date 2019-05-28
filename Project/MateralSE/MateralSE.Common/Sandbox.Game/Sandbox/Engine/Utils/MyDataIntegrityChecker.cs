namespace Sandbox.Engine.Utils
{
    using Sandbox;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using VRage.FileSystem;

    internal static class MyDataIntegrityChecker
    {
        public const int HASH_SIZE = 20;
        private static byte[] m_combinedData = new byte[40];
        private static byte[] m_hash = new byte[20];
        private static StringBuilder m_stringBuilder = new StringBuilder(8);

        public static string GetHashBase64() => 
            Convert.ToBase64String(m_hash);

        public static string GetHashHex()
        {
            uint num = 0;
            m_stringBuilder.Clear();
            foreach (byte num3 in m_hash)
            {
                m_stringBuilder.AppendFormat("{0:x2}", num3);
                num += num3;
            }
            return m_stringBuilder.ToString();
        }

        public static unsafe void HashInData(string dataName, Stream data)
        {
            using (HashAlgorithm algorithm = new SHA1Managed())
            {
                byte[] sourceArray = algorithm.ComputeHash(data);
                Array.Copy(sourceArray, m_combinedData, 20);
                Array.Copy(algorithm.ComputeHash(Encoding.Unicode.GetBytes(dataName.ToCharArray())), 0, m_combinedData, 20, 20);
                byte[] buffer2 = algorithm.ComputeHash(m_combinedData);
                for (int i = 0; i < 20; i++)
                {
                    byte* numPtr1 = (byte*) ref m_hash[i];
                    numPtr1[0] = (byte) (numPtr1[0] ^ buffer2[i]);
                }
            }
        }

        public static void HashInFile(string fileName)
        {
            using (Stream stream = MyFileSystem.OpenRead(fileName).UnwrapGZip())
            {
                HashInData(fileName.ToLower(), stream);
            }
            MySandboxGame.Log.WriteLine(GetHashHex());
        }

        public static void ResetHash()
        {
            Array.Clear(m_hash, 0, 20);
        }
    }
}

