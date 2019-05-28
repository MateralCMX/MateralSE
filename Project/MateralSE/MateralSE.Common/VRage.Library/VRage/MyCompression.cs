namespace VRage
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public static class MyCompression
    {
        private static byte[] m_buffer = new byte[0x4000];

        public static byte[] Compress(byte[] buffer)
        {
            byte[] buffer3;
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    stream2.Write(buffer, 0, buffer.Length);
                    stream2.Close();
                    stream.Position = 0L;
                    byte[] buffer2 = new byte[stream.Length + 4L];
                    stream.Read(buffer2, 4, (int) stream.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, buffer2, 0, 4);
                    buffer3 = buffer2;
                }
            }
            return buffer3;
        }

        public static void CompressFile(string fileName)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Buffer.BlockCopy(BitConverter.GetBytes(new FileInfo(fileName).Length), 0, m_buffer, 0, 4);
                stream.Write(m_buffer, 0, 4);
                using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    using (FileStream stream3 = File.OpenRead(fileName))
                    {
                        for (int i = stream3.Read(m_buffer, 0, m_buffer.Length); i > 0; i = stream3.Read(m_buffer, 0, m_buffer.Length))
                        {
                            stream2.Write(m_buffer, 0, i);
                        }
                    }
                    stream2.Close();
                    stream.Position = 0L;
                    using (FileStream stream4 = File.Create(fileName))
                    {
                        for (int i = stream.Read(m_buffer, 0, m_buffer.Length); i > 0; i = stream.Read(m_buffer, 0, m_buffer.Length))
                        {
                            stream4.Write(m_buffer, 0, i);
                            stream4.Flush();
                        }
                    }
                }
            }
        }

        public static byte[] Decompress(byte[] gzBuffer)
        {
            byte[] buffer2;
            using (MemoryStream stream = new MemoryStream())
            {
                int num = BitConverter.ToInt32(gzBuffer, 0);
                stream.Write(gzBuffer, 4, gzBuffer.Length - 4);
                stream.Position = 0L;
                byte[] buffer = new byte[num];
                using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress))
                {
                    stream2.Read(buffer, 0, buffer.Length);
                    buffer2 = buffer;
                }
            }
            return buffer2;
        }

        public static void DecompressFile(string fileName)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (FileStream stream2 = File.OpenRead(fileName))
                {
                    stream2.Read(m_buffer, 0, 4);
                    using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Decompress))
                    {
                        for (int i = stream3.Read(m_buffer, 0, m_buffer.Length); i > 0; i = stream3.Read(m_buffer, 0, m_buffer.Length))
                        {
                            stream.Write(m_buffer, 0, i);
                        }
                    }
                }
                stream.Position = 0L;
                using (FileStream stream4 = File.Create(fileName))
                {
                    for (int i = stream.Read(m_buffer, 0, m_buffer.Length); i > 0; i = stream.Read(m_buffer, 0, m_buffer.Length))
                    {
                        stream4.Write(m_buffer, 0, i);
                        stream4.Flush();
                    }
                }
            }
        }
    }
}

