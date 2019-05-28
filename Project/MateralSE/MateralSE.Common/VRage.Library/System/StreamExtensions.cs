namespace System
{
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using Unsharper;

    [UnsharperDisableReflection]
    public static class StreamExtensions
    {
        [ThreadStatic]
        private static byte[] m_buffer;

        public static bool CheckGZipHeader(this Stream stream)
        {
            long position = stream.Position;
            byte[] buffer = new byte[2];
            stream.Seek(0L, SeekOrigin.Begin);
            stream.Read(buffer, 0, 2);
            if ((buffer[0] != 0x1f) || (buffer[1] != 0x8b))
            {
                stream.Seek(position, SeekOrigin.Begin);
                return false;
            }
            stream.Seek(position, SeekOrigin.Begin);
            return true;
        }

        public static int Read7BitEncodedInt(this Stream stream)
        {
            byte[] buffer = Buffer;
            int num = 0;
            int num2 = 0;
            while (num2 != 0x23)
            {
                if (stream.Read(buffer, 0, 1) == 0)
                {
                    throw new EndOfStreamException();
                }
                byte num3 = buffer[0];
                num |= (num3 & 0x7f) << (num2 & 0x1f);
                num2 += 7;
                if ((num3 & 0x80) == 0)
                {
                    return num;
                }
            }
            throw new FormatException("Bad string length. 7bit Int32 format");
        }

        public static byte ReadByteNoAlloc(this Stream stream)
        {
            byte[] buffer = Buffer;
            if (stream.Read(buffer, 0, 1) == 0)
            {
                throw new EndOfStreamException();
            }
            return buffer[0];
        }

        public static unsafe decimal ReadDecimal(this Stream stream)
        {
            decimal num;
            stream.ReadNoAlloc((byte*) &num, 0, 0x10);
            return num;
        }

        public static unsafe double ReadDouble(this Stream stream)
        {
            double num;
            stream.ReadNoAlloc((byte*) &num, 0, 8);
            return num;
        }

        public static unsafe float ReadFloat(this Stream stream)
        {
            float num;
            stream.ReadNoAlloc((byte*) &num, 0, 4);
            return num;
        }

        public static unsafe short ReadInt16(this Stream stream)
        {
            short num;
            stream.ReadNoAlloc((byte*) &num, 0, 2);
            return num;
        }

        public static unsafe int ReadInt32(this Stream stream)
        {
            int num;
            stream.ReadNoAlloc((byte*) &num, 0, 4);
            return num;
        }

        public static unsafe long ReadInt64(this Stream stream)
        {
            long num;
            stream.ReadNoAlloc((byte*) &num, 0, 8);
            return num;
        }

        public static unsafe void ReadNoAlloc(this Stream stream, byte* bytes, int offset, int count)
        {
            byte[] buffer = Buffer;
            int num = 0;
            int index = offset;
            int num3 = offset + count;
            while (index != num3)
            {
                num = Math.Min(count, buffer.Length);
                stream.Read(buffer, 0, num);
                count -= num;
                for (int i = 0; i < num; i++)
                {
                    index++;
                    bytes[index] = buffer[i];
                }
            }
        }

        public static string ReadString(this Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            int count = stream.Read7BitEncodedInt();
            byte[] buffer = Buffer;
            if (count > buffer.Length)
            {
                buffer = new byte[count];
            }
            stream.Read(buffer, 0, count);
            return encoding.GetString(buffer, 0, count);
        }

        public static unsafe ushort ReadUInt16(this Stream stream)
        {
            ushort num;
            stream.ReadNoAlloc((byte*) &num, 0, 2);
            return num;
        }

        public static unsafe uint ReadUInt32(this Stream stream)
        {
            uint num;
            stream.ReadNoAlloc((byte*) &num, 0, 4);
            return num;
        }

        public static unsafe ulong ReadUInt64(this Stream stream)
        {
            ulong num;
            stream.ReadNoAlloc((byte*) &num, 0, 8);
            return num;
        }

        public static void SkipBytes(this Stream stream, int byteCount)
        {
            byte[] buffer = Buffer;
            while (byteCount > 0)
            {
                int num1;
                int count = (byteCount > buffer.Length) ? buffer.Length : num1;
                stream.Read(buffer, 0, count);
                num1 = byteCount;
                byteCount -= count;
            }
        }

        public static Stream UnwrapGZip(this Stream stream) => 
            (stream.CheckGZipHeader() ? new GZipStream(stream, CompressionMode.Decompress, false) : stream);

        public static Stream WrapGZip(this Stream stream, bool buffered = true)
        {
            GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, false);
            return (buffered ? ((Stream) new BufferedStream(stream2, 0x8000)) : ((Stream) stream2));
        }

        public static void Write7BitEncodedInt(this Stream stream, int value)
        {
            byte[] buffer = Buffer;
            int index = 0;
            uint num2 = (uint) value;
            while (num2 >= 0x80)
            {
                index++;
                buffer[index] = (byte) (num2 | 0x80);
                num2 = num2 >> 7;
                if (index == buffer.Length)
                {
                    stream.Write(buffer, 0, index);
                    index = 0;
                }
            }
            buffer[index] = (byte) num2;
            stream.Write(buffer, 0, index + 1);
        }

        public static void WriteNoAlloc(this Stream stream, byte value)
        {
            byte[] buffer = Buffer;
            buffer[0] = value;
            stream.Write(buffer, 0, 1);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, decimal v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 0x10);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, double v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 8);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, short v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 2);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, int v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 4);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, long v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 8);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, float v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 4);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, ushort v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 2);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, uint v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 4);
        }

        public static unsafe void WriteNoAlloc(this Stream stream, ulong v)
        {
            stream.WriteNoAlloc((byte*) &v, 0, 8);
        }

        public static void WriteNoAlloc(this Stream stream, string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            int byteCount = encoding.GetByteCount(text);
            stream.Write7BitEncodedInt(byteCount);
            byte[] buffer = Buffer;
            if (byteCount > buffer.Length)
            {
                buffer = new byte[byteCount];
            }
            stream.Write(buffer, 0, encoding.GetBytes(text, 0, text.Length, buffer, 0));
        }

        public static unsafe void WriteNoAlloc(this Stream stream, byte* bytes, int offset, int count)
        {
            byte[] buffer = Buffer;
            int index = 0;
            int num2 = offset;
            int num3 = offset + count;
            while (num2 != num3)
            {
                index++;
                num2++;
                buffer[index] = bytes[num2];
                if (index == buffer.Length)
                {
                    stream.Write(buffer, 0, index);
                    index = 0;
                }
            }
            if (index != 0)
            {
                stream.Write(buffer, 0, index);
            }
        }

        private static byte[] Buffer
        {
            get
            {
                if (m_buffer == null)
                {
                    m_buffer = new byte[0x100];
                }
                return m_buffer;
            }
        }
    }
}

