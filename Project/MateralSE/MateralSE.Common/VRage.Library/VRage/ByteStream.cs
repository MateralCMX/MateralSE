namespace VRage
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.Win32;

    public class ByteStream : Stream
    {
        private byte[] m_baseArray;
        private int m_position;
        private int m_length;
        public readonly bool Expandable;
        public readonly bool Resetable;

        public ByteStream()
        {
            this.Resetable = true;
            this.Expandable = false;
        }

        public ByteStream(int capacity, bool expandable = true)
        {
            this.Expandable = expandable;
            this.Resetable = false;
            this.m_baseArray = new byte[capacity];
            this.m_length = this.m_baseArray.Length;
        }

        public ByteStream(byte[] newBaseArray, int length) : this()
        {
            this.Reset(newBaseArray, length);
        }

        public void CheckCapacity(long minimumSize)
        {
            if (this.m_length < minimumSize)
            {
                throw new EndOfStreamException("Stream does not have enough size");
            }
        }

        public void EnsureCapacity(long minimumSize)
        {
            if (this.m_length < minimumSize)
            {
                if (!this.Expandable)
                {
                    throw new EndOfStreamException("ByteSteam is not large enough and is not expandable");
                }
                if (minimumSize < 0x100L)
                {
                    minimumSize = 0x100L;
                }
                if (minimumSize < (this.m_length * 2))
                {
                    minimumSize = this.m_length * 2;
                }
                this.Resize(minimumSize);
            }
        }

        public override void Flush()
        {
        }

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

        public byte ReadByte()
        {
            this.CheckCapacity((long) (this.m_position + 1));
            byte num = this.m_baseArray[this.m_position];
            this.m_position++;
            return num;
        }

        public unsafe ushort ReadUShort()
        {
            this.CheckCapacity((long) (this.m_position + 2));
            byte* numPtr = &(this.m_baseArray[this.m_position]);
            this.m_position += 2;
            return *(((ushort*) numPtr));
        }

        public void Reset(byte[] newBaseArray, int length)
        {
            if (!this.Resetable)
            {
                throw new InvalidOperationException("Stream is not created as resetable");
            }
            if (newBaseArray.Length < length)
            {
                throw new ArgumentException("Length must be >= newBaseArray.Length");
            }
            this.m_baseArray = newBaseArray;
            this.m_length = length;
            this.m_position = 0;
        }

        private void Resize(long size)
        {
            Array.Resize<byte>(ref this.m_baseArray, (int) size);
            this.m_length = this.m_baseArray.Length;
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
            if (!this.Expandable)
            {
                throw new InvalidOperationException("ByteStream is not expandable");
            }
            this.Resize((long) ((int) value));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.EnsureCapacity((long) (this.m_position + count));
            int num = this.m_position + count;
            if ((count > 0x80) || (buffer == this.m_baseArray))
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

        internal unsafe void Write(IntPtr srcPtr, int offset, int count)
        {
            this.EnsureCapacity((long) (this.m_position + count));
            WinApi.CopyMemory((void*) &(this.m_baseArray[this.m_position]), srcPtr.ToPointer() + offset, (ulong) count);
            fixed (byte* numRef = null)
            {
                this.m_position += count;
                return;
            }
        }

        public void WriteByte(byte value)
        {
            this.EnsureCapacity((long) (this.m_position + 1));
            this.m_baseArray[this.m_position] = value;
            this.m_position++;
        }

        public unsafe void WriteUShort(ushort value)
        {
            this.EnsureCapacity((long) (this.m_position + 2));
            *((IntPtr) &(this.m_baseArray[this.m_position])) = value;
            fixed (byte* numRef = null)
            {
                this.m_position += 2;
                return;
            }
        }

        public byte[] Data =>
            this.m_baseArray;

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

