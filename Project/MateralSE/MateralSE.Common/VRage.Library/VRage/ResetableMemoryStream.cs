namespace VRage
{
    using System;
    using System.IO;

    public class ResetableMemoryStream : Stream
    {
        private byte[] m_baseArray;
        private int m_position;
        private int m_length;

        public ResetableMemoryStream()
        {
        }

        public ResetableMemoryStream(byte[] baseArray, int length)
        {
            this.Reset(baseArray, length);
        }

        public override void Flush()
        {
        }

        public byte[] GetInternalBuffer() => 
            this.m_baseArray;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = this.m_length - this.m_position;
            if (num > count)
            {
                num = count;
            }
            if (num <= 0)
            {
                return 0;
            }
            if (num > 8)
            {
                Buffer.BlockCopy(this.m_baseArray, this.m_position, buffer, offset, num);
            }
            else
            {
                int num2 = num;
                while (--num2 >= 0)
                {
                    buffer[offset + num2] = this.m_baseArray[this.m_position + num2];
                }
            }
            this.m_position += num;
            return num;
        }

        public void Reset(byte[] newBaseArray, int length)
        {
            if (newBaseArray.Length < length)
            {
                throw new ArgumentException("Length must be >= newBaseArray.Length");
            }
            this.m_baseArray = newBaseArray;
            this.m_length = length;
            this.m_position = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_position = (int) offset;
                    break;

                case SeekOrigin.Current:
                    this.m_position += (int) offset;
                    break;

                case SeekOrigin.End:
                    this.m_position = this.m_length + ((int) offset);
                    break;

                default:
                    throw new ArgumentException("Invalid seek origin");
            }
            return (long) this.m_position;
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.m_length < (this.m_position + count))
            {
                throw new EndOfStreamException();
            }
            int num = this.m_position + count;
            if ((count > 8) || (buffer == this.m_baseArray))
            {
                Buffer.BlockCopy(buffer, offset, this.m_baseArray, this.m_position, count);
            }
            else
            {
                int num2 = count;
                while (--num2 >= 0)
                {
                    this.m_baseArray[this.m_position + num2] = buffer[offset + num2];
                }
            }
            this.m_position = num;
        }

        public override bool CanRead =>
            true;

        public override bool CanSeek =>
            true;

        public override bool CanWrite =>
            true;

        public override long Length =>
            ((long) this.m_length);

        public override long Position
        {
            get => 
                ((long) this.m_position);
            set => 
                (this.m_position = (int) value);
        }
    }
}

