namespace VRage
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public class MyCompressionStreamLoad : IMyCompressionLoad
    {
        private static byte[] m_intBytesBuffer = new byte[4];
        private MemoryStream m_input;
        private GZipStream m_gz;
        private BufferedStream m_buffer;

        public MyCompressionStreamLoad(byte[] compressedData)
        {
            this.m_input = new MemoryStream(compressedData);
            this.m_input.Read(m_intBytesBuffer, 0, 4);
            this.m_gz = new GZipStream(this.m_input, CompressionMode.Decompress);
            this.m_buffer = new BufferedStream(this.m_gz, 0x4000);
        }

        public bool EndOfFile() => 
            (this.m_input.Position == this.m_input.Length);

        public byte GetByte() => 
            ((byte) this.m_buffer.ReadByte());

        public int GetBytes(int bytes, byte[] output) => 
            this.m_buffer.Read(output, 0, bytes);

        public int GetInt32()
        {
            this.m_buffer.Read(m_intBytesBuffer, 0, 4);
            return BitConverter.ToInt32(m_intBytesBuffer, 0);
        }
    }
}

