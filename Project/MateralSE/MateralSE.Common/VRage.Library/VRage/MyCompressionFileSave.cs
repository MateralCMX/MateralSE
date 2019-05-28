namespace VRage
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class MyCompressionFileSave : IMyCompressionSave, IDisposable
    {
        private int m_uncompressedSize;
        private FileStream m_output;
        private GZipStream m_gz;
        private BufferedStream m_buffer;

        public MyCompressionFileSave(string targetFile)
        {
            this.m_output = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
            for (int i = 0; i < 4; i++)
            {
                this.m_output.WriteByte(0);
            }
            this.m_gz = new GZipStream(this.m_output, CompressionMode.Compress, true);
            this.m_buffer = new BufferedStream(this.m_gz, 0x4000);
        }

        public void Add(byte[] value)
        {
            this.Add(value, value.Length);
        }

        public void Add(byte value)
        {
            this.m_buffer.WriteByte(value);
            this.m_uncompressedSize++;
        }

        public void Add(int value)
        {
            this.Add(BitConverter.GetBytes(value));
        }

        public void Add(float value)
        {
            this.Add(BitConverter.GetBytes(value));
        }

        public void Add(byte[] value, int count)
        {
            this.m_buffer.Write(value, 0, count);
            this.m_uncompressedSize += count;
        }

        public void Dispose()
        {
            if (this.m_output != null)
            {
                try
                {
                    this.m_buffer.Close();
                }
                finally
                {
                    this.m_buffer = null;
                }
                try
                {
                    this.m_gz.Close();
                }
                finally
                {
                    this.m_gz = null;
                }
                this.m_output.Position = 0L;
                WriteUncompressedSize(this.m_output, this.m_uncompressedSize);
                try
                {
                    this.m_output.Close();
                }
                finally
                {
                    this.m_output = null;
                }
            }
        }

        private static unsafe void WriteUncompressedSize(FileStream output, int uncompressedSize)
        {
            byte* numPtr = (byte*) &uncompressedSize;
            for (int i = 0; i < 4; i++)
            {
                output.WriteByte(numPtr[i]);
            }
        }
    }
}

