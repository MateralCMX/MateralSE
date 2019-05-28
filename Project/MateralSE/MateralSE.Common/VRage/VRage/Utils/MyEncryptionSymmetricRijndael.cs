namespace VRage.Utils
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class MyEncryptionSymmetricRijndael
    {
        public static string DecryptString(string inputText, string password)
        {
            if (inputText.Length <= 0)
            {
                return "";
            }
            byte[] buffer = Convert.FromBase64String(inputText);
            byte[] rgbSalt = Encoding.ASCII.GetBytes(password.Length.ToString());
            PasswordDeriveBytes bytes = new PasswordDeriveBytes(password, rgbSalt);
            MemoryStream stream = new MemoryStream(buffer);
            byte[] buffer3 = new byte[buffer.Length];
            CryptoStream stream1 = new CryptoStream(stream, new RijndaelManaged().CreateDecryptor(bytes.GetBytes(0x20), bytes.GetBytes(0x10)), CryptoStreamMode.Read);
            stream.Close();
            stream1.Close();
            return Encoding.Unicode.GetString(buffer3, 0, stream1.Read(buffer3, 0, buffer3.Length));
        }

        public static string EncryptString(string inputText, string password)
        {
            if (inputText.Length <= 0)
            {
                return "";
            }
            byte[] buffer = Encoding.Unicode.GetBytes(inputText);
            byte[] rgbSalt = Encoding.ASCII.GetBytes(password.Length.ToString());
            PasswordDeriveBytes bytes = new PasswordDeriveBytes(password, rgbSalt);
            MemoryStream stream = new MemoryStream();
            CryptoStream stream1 = new CryptoStream(stream, new RijndaelManaged().CreateEncryptor(bytes.GetBytes(0x20), bytes.GetBytes(0x10)), CryptoStreamMode.Write);
            stream1.Write(buffer, 0, buffer.Length);
            stream1.FlushFinalBlock();
            byte[] inArray = stream.ToArray();
            stream.Close();
            stream1.Close();
            return Convert.ToBase64String(inArray);
        }
    }
}

