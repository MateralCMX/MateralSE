namespace Sandbox.Game.Entities.Blocks
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    internal static class StringCompressor
    {
        public static byte[] CompressString(string str)
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (MemoryStream stream2 = new MemoryStream())
                {
                    using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress))
                    {
                        CopyTo(stream, stream3);
                    }
                    buffer = stream2.ToArray();
                }
            }
            return buffer;
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            int num;
            byte[] buffer = new byte[0x1000];
            while ((num = src.Read(buffer, 0, buffer.Length)) != 0)
            {
                dest.Write(buffer, 0, num);
            }
        }

        public static string DecompressString(byte[] bytes)
        {
            string str;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (MemoryStream stream2 = new MemoryStream())
                {
                    using (GZipStream stream3 = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        CopyTo(stream3, stream2);
                    }
                    str = Encoding.UTF8.GetString(stream2.ToArray());
                }
            }
            return str;
        }
    }
}

