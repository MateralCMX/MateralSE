namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public static class MySerialKey
    {
        private static int m_dataSize = 14;
        private static int m_hashSize = 4;

        public static string AddDashes(string key)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                if (((i % 5) == 0) && (i > 0))
                {
                    builder.Append('-');
                }
                builder.Append(key[i]);
            }
            return builder.ToString();
        }

        public static string[] Generate(short productTypeId, short distributorId, int keyCount)
        {
            string[] strArray;
            byte[] bytes = BitConverter.GetBytes(productTypeId);
            byte[] buffer2 = BitConverter.GetBytes(distributorId);
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                using (SHA1Managed managed = new SHA1Managed())
                {
                    List<string> list = new List<string>(keyCount);
                    byte[] data = new byte[m_dataSize + m_hashSize];
                    int num = 0;
                    while (true)
                    {
                        if (num >= keyCount)
                        {
                            strArray = list.ToArray();
                            break;
                        }
                        provider.GetBytes(data);
                        data[0] = bytes[0];
                        data[1] = bytes[1];
                        data[2] = buffer2[0];
                        data[3] = buffer2[1];
                        int index = 0;
                        while (true)
                        {
                            if (index >= 4)
                            {
                                byte[] buffer4 = managed.ComputeHash(data, 0, m_dataSize);
                                int num3 = 0;
                                while (true)
                                {
                                    if (num3 >= m_hashSize)
                                    {
                                        list.Add(new string(My5BitEncoding.Default.Encode(data.ToArray<byte>())) + "X");
                                        num++;
                                        break;
                                    }
                                    data[m_dataSize + num3] = buffer4[num3];
                                    num3++;
                                }
                                break;
                            }
                            data[index] = (byte) (data[index] ^ data[index + 4]);
                            index++;
                        }
                    }
                }
            }
            return strArray;
        }

        public static string RemoveDashes(string key)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                if (((i + 1) % 6) != 0)
                {
                    builder.Append(key[i]);
                }
            }
            return builder.ToString();
        }

        public static bool ValidateSerial(string serialKey, out int productTypeId, out int distributorId)
        {
            bool flag;
            using (new RNGCryptoServiceProvider())
            {
                using (SHA1 sha = SHA1.Create())
                {
                    if (serialKey.EndsWith("X"))
                    {
                        byte[] source = My5BitEncoding.Default.Decode(serialKey.Take<char>((serialKey.Length - 1)).ToArray<char>());
                        byte[] buffer = source.Take<byte>((source.Length - m_hashSize)).ToArray<byte>();
                        byte[] buffer3 = sha.ComputeHash(buffer);
                        if (source.Skip<byte>(buffer.Length).Take<byte>(m_hashSize).SequenceEqual<byte>(buffer3.Take<byte>(m_hashSize)))
                        {
                            int index = 0;
                            while (true)
                            {
                                if (index >= 4)
                                {
                                    productTypeId = BitConverter.ToInt16(buffer, 0);
                                    distributorId = BitConverter.ToInt16(buffer, 2);
                                    flag = true;
                                    break;
                                }
                                buffer[index] = (byte) (buffer[index] ^ buffer[index + 4]);
                                index++;
                            }
                            return flag;
                        }
                    }
                    productTypeId = 0;
                    distributorId = 0;
                    flag = false;
                }
            }
            return flag;
        }
    }
}

