namespace VRage.Library.Collections
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    public struct BitReader
    {
        private unsafe ulong* m_buffer;
        private int m_bitLength;
        public int BitPosition;
        private const long Int64Msb = -9223372036854775808L;
        private const int Int32Msb = -2147483648;
        public unsafe BitReader(IntPtr data, int bitLength)
        {
            this.m_buffer = (ulong*) data;
            this.m_bitLength = bitLength;
            this.BitPosition = 0;
        }

        public unsafe void Reset(IntPtr data, int bitLength)
        {
            this.m_buffer = (ulong*) data;
            this.m_bitLength = bitLength;
            this.BitPosition = 0;
        }

        private unsafe ulong ReadInternal(int bitSize)
        {
            if (this.m_bitLength < (this.BitPosition + bitSize))
            {
                throw new BitStreamException(new EndOfStreamException("Cannot read from bit stream, end of steam"));
            }
            int index = this.BitPosition >> 6;
            int num2 = ((this.BitPosition + bitSize) - 1) >> 6;
            ulong num3 = (ulong) (-1L >> ((0x40 - bitSize) & 0x3f));
            int num4 = this.BitPosition & -65;
            ulong num5 = this.m_buffer[index] >> (num4 & 0x3f);
            if (num2 != index)
            {
                num5 |= this.m_buffer[num2] << ((0x40 - num4) & 0x3f);
            }
            this.BitPosition += bitSize;
            return (num5 & num3);
        }

        public unsafe double ReadDouble() => 
            *(((double*) &this.ReadInternal(0x40)));

        public unsafe float ReadFloat() => 
            *(((float*) &this.ReadInternal(0x20)));

        public unsafe decimal ReadDecimal()
        {
            decimal num;
            *((long*) &num) = this.ReadInternal(0x40);
            &num[8] = this.ReadInternal(0x40);
            return num;
        }

        public bool ReadBool() => 
            (this.ReadInternal(1) != 0L);

        public sbyte ReadSByte(int bitCount = 8) => 
            ((sbyte) this.ReadInternal(bitCount));

        public short ReadInt16(int bitCount = 0x10) => 
            ((short) this.ReadInternal(bitCount));

        public int ReadInt32(int bitCount = 0x20) => 
            ((int) this.ReadInternal(bitCount));

        public long ReadInt64(int bitCount = 0x40) => 
            ((long) this.ReadInternal(bitCount));

        public byte ReadByte(int bitCount = 8) => 
            ((byte) this.ReadInternal(bitCount));

        public ushort ReadUInt16(int bitCount = 0x10) => 
            ((ushort) this.ReadInternal(bitCount));

        public uint ReadUInt32(int bitCount = 0x20) => 
            ((uint) this.ReadInternal(bitCount));

        public ulong ReadUInt64(int bitCount = 0x40) => 
            this.ReadInternal(bitCount);

        private static int Zag(uint ziggedValue)
        {
            int num = (int) ziggedValue;
            return (-(num & 1) ^ ((num >> 1) & 0x7fffffff));
        }

        private static long Zag(ulong ziggedValue)
        {
            long num = (long) ziggedValue;
            return (-(num & 1L) ^ ((num >> 1) & 0x7fffffffffffffffL));
        }

        public int ReadInt32Variant() => 
            Zag(this.ReadUInt32Variant());

        public long ReadInt64Variant() => 
            Zag(this.ReadUInt64Variant());

        public uint ReadUInt32Variant()
        {
            uint num = this.ReadByte(8);
            if ((num & 0x80) != 0)
            {
                uint num2 = this.ReadByte(8);
                num = (uint) ((num & 0x7f) | ((num2 & 0x7f) << 7));
                if ((num2 & 0x80) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (uint) ((num2 & 0x7f) << 14);
                if ((num2 & 0x80) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (uint) ((num2 & 0x7f) << 0x15);
                if ((num2 & 0x80) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= num2 << 0x1c;
                if ((num2 & 240) != 0)
                {
                    throw new BitStreamException(new OverflowException("Error when deserializing variant uint32"));
                }
            }
            return num;
        }

        public ulong ReadUInt64Variant()
        {
            ulong num = this.ReadByte(8);
            if ((num & ((ulong) 0x80L)) != 0)
            {
                ulong num2 = this.ReadByte(8);
                num = (num & 0x7f) | ((num2 & 0x7f) << 7);
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 14;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 0x15;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 0x1c;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 0x23;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 0x2a;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 0x31;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= (num2 & 0x7f) << 0x38;
                if ((num2 & ((ulong) 0x80L)) == 0)
                {
                    return num;
                }
                num2 = this.ReadByte(8);
                num |= num2 << 0x3f;
                if ((num2 & -2) != 0)
                {
                    throw new BitStreamException(new OverflowException("Error when deserializing variant uint64"));
                }
            }
            return num;
        }

        public char ReadChar(int bitCount = 0x10) => 
            ((char) this.ReadInternal(bitCount));

        public unsafe void ReadMemory(IntPtr ptr, int bitSize)
        {
            this.ReadMemory((void*) ptr, bitSize);
        }

        public unsafe void ReadMemory(void* ptr, int bitSize)
        {
            int num = (bitSize / 8) / 8;
            ulong* numPtr = (ulong*) ptr;
            for (int i = 0; i < num; i++)
            {
                numPtr[i] = this.ReadUInt64(0x40);
            }
            int num2 = bitSize - ((num * 8) * 8);
            for (byte* numPtr2 = (byte*) (numPtr + num); num2 > 0; numPtr2++)
            {
                int bitCount = Math.Min(num2, 8);
                numPtr2[0] = this.ReadByte(bitCount);
                num2 -= bitCount;
            }
        }

        public unsafe string ReadPrefixLengthString(Encoding encoding)
        {
            byte* numPtr2;
            byte[] pinned buffer2;
            int count = (int) this.ReadUInt32Variant();
            if (count <= 0x400)
            {
                byte* numPtr = stackalloc byte[(IntPtr) count];
                this.ReadMemory((void*) numPtr, count * 8);
                int charCount = encoding.GetCharCount(numPtr, count);
                char* chars = (char*) stackalloc byte[(((IntPtr) charCount) * 2)];
                encoding.GetChars(numPtr, count, chars, charCount);
                return new string(chars, 0, charCount);
            }
            byte[] bytes = new byte[count];
            if (((buffer2 = bytes) == null) || (buffer2.Length == 0))
            {
                numPtr2 = null;
            }
            else
            {
                numPtr2 = buffer2;
            }
            this.ReadMemory((void*) numPtr2, count * 8);
            buffer2 = null;
            return new string(encoding.GetChars(bytes));
        }

        public unsafe void ReadBytes(byte[] bytes, int start, int count)
        {
            byte* numPtr;
            byte[] pinned buffer;
            if (((buffer = bytes) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            this.ReadMemory((void*) (numPtr + start), count * 8);
            buffer = null;
        }
    }
}

