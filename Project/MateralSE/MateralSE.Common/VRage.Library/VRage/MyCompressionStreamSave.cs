namespace VRage
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class MyCompressionStreamSave : IMyCompressionSave, IDisposable
    {
        private MemoryStream m_output = new MemoryStream();
        private GZipStream m_gz;
        private BufferedStream m_buffer;

        public MyCompressionStreamSave()
        {
            this.m_output.Write(BitConverter.GetBytes(0), 0, 4);
            this.m_gz = new GZipStream(this.m_output, CompressionMode.Compress);
            this.m_buffer = new BufferedStream(this.m_gz, 0x4000);
        }

        public void Add(byte[] value)
        {
            this.m_buffer.Write(value, 0, value.Length);
        }

        public void Add(byte value)
        {
            this.m_buffer.WriteByte(value);
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
        }

        public byte[] Compress()
        {
            byte[] buffer = null;
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
                try
                {
                    this.m_output.Close();
                }
                finally
                {
                    buffer = this.m_output.ToArray();
                    this.m_output = null;
                }
            }
            return buffer;
        }

        public void Dispose()
        {
            this.Compress();
        }
    }
}

