namespace Sandbox.Game.Replication.StateGroups
{
    using System;
    using System.IO;
    using System.IO.Compression;

    internal static class MemoryCompressor
    {
        public static byte[] Compress(byte[] bytes)
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream(bytes))
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

        private static void CopyTo(Stream src, Stream dest)
        {
            int num;
            byte[] buffer = new byte[0x1000];
            while ((num = src.Read(buffer, 0, buffer.Length)) != 0)
            {
                dest.Write(buffer, 0, num);
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (MemoryStream stream2 = new MemoryStream())
                {
                    using (GZipStream stream3 = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        CopyTo(stream3, stream2);
                    }
                    buffer = stream2.ToArray();
                }
            }
            return buffer;
        }
    }
}

