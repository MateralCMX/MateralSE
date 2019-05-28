namespace VRage
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class MyCompressionFileLoad : IMyCompressionLoad, IDisposable
    {
        [ThreadStatic]
        private static byte[] m_intBytesBuffer;
        private FileStream m_input;
        private GZipStream m_gz;
        private BufferedStream m_buffer;

        public MyCompressionFileLoad(string fileName)
        {
            if (m_intBytesBuffer == null)
            {
                m_intBytesBuffer = new byte[4];
            }
            this.m_input = File.OpenRead(fileName);
            this.m_input.Read(m_intBytesBuffer, 0, 4);
            this.m_gz = new GZipStream(this.m_input, CompressionMode.Decompress);
            this.m_buffer = new BufferedStream(this.m_gz, 0x4000);
        }

        public void Dispose()
        {
            if (this.m_buffer != null)
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
                    this.m_input.Close();
                }
                finally
                {
                    this.m_input = null;
                }
            }
        }

        public bool EndOfFile() => 
            (this.m_input.Position == this.m_input.Length);

        public byte GetByte() => 
            ((byte) this.m_buffer.ReadByte());

        public int GetBytes(int bytes, byte[] output) => 
            this.m_buffer.Read(output, 0, bytes);

        public byte[] GetCompressedBuffer()
        {
            this.m_input.Position = 0L;
            byte[] buffer = new byte[this.m_input.Length];
            this.m_input.Read(buffer, 0, (int) this.m_input.Length);
            return buffer;
        }

        public int GetInt32()
        {
            this.m_buffer.Read(m_intBytesBuffer, 0, 4);
            return BitConverter.ToInt32(m_intBytesBuffer, 0);
        }
    }
}

