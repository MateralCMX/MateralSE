namespace VRage.Utils
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    public class MyBinaryReader : BinaryReader
    {
        private System.Text.Decoder m_decoder;
        private int m_maxCharsSize;
        private byte[] m_charBytes;
        private char[] m_charBuffer;

        public MyBinaryReader(Stream stream) : this(stream, new UTF8Encoding())
        {
        }

        public MyBinaryReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
            this.m_decoder = encoding.GetDecoder();
            this.m_maxCharsSize = encoding.GetMaxCharCount(0x80);
            encoding.GetMaxByteCount(1);
        }

        public int Read7BitEncodedInt()
        {
            int num2 = 0;
            int num3 = 0;
            while (num3 != 0x23)
            {
                byte num = this.ReadByte();
                num2 |= (num & 0x7f) << (num3 & 0x1f);
                num3 += 7;
                if ((num & 0x80) == 0)
                {
                    return num2;
                }
            }
            return -1;
        }

        [SecuritySafeCritical]
        public string ReadStringIncomplete(out bool isComplete)
        {
            if (this.BaseStream == null)
            {
                isComplete = false;
                return string.Empty;
            }
            int num = 0;
            int capacity = this.Read7BitEncodedInt();
            if (capacity < 0)
            {
                isComplete = false;
                return string.Empty;
            }
            if (capacity == 0)
            {
                isComplete = true;
                return string.Empty;
            }
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }
            if (this.m_charBuffer == null)
            {
                this.m_charBuffer = new char[this.m_maxCharsSize];
            }
            StringBuilder builder = null;
            while (true)
            {
                int count = ((capacity - num) > 0x80) ? 0x80 : (capacity - num);
                int byteCount = this.BaseStream.Read(this.m_charBytes, 0, count);
                if (byteCount == 0)
                {
                    isComplete = false;
                    return ((builder != null) ? builder.ToString() : string.Empty);
                }
                int length = this.m_decoder.GetChars(this.m_charBytes, 0, byteCount, this.m_charBuffer, 0);
                if ((num == 0) && (byteCount == capacity))
                {
                    isComplete = true;
                    return new string(this.m_charBuffer, 0, length);
                }
                if (builder == null)
                {
                    builder = new StringBuilder(capacity);
                }
                builder.Append(this.m_charBuffer, 0, length);
                num += byteCount;
                if (num >= capacity)
                {
                    isComplete = true;
                    return builder.ToString();
                }
            }
        }
    }
}

