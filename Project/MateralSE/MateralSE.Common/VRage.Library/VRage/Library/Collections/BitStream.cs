namespace VRage.Library.Collections
{
    using SharpDX;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Unsharper;
    using VRage.Library.Utils;

    [UnsharperDisableReflection]
    public class BitStream : IDisposable
    {
        private const long Int64Msb = -9223372036854775808L;
        private const int Int32Msb = -2147483648;
        private unsafe ulong* m_ownedBuffer;
        private int m_ownedBufferBitLength;
        private unsafe ulong* m_buffer;
        private GCHandle m_bufferHandle;
        private readonly int m_defaultByteSize;
        public const int TERMINATOR_SIZE = 2;
        private const ushort TERMINATOR = 0xc8b9;

        public BitStream(int defaultByteSize = 0x600)
        {
            this.m_defaultByteSize = Math.Max(0x10, MyLibraryUtils.GetDivisionCeil(defaultByteSize, 8) * 8);
        }

        public bool CheckTerminator() => 
            (this.ReadUInt16(0x10) == 0xc8b9);

        private unsafe void Clear(int fromPosition)
        {
            int num = fromPosition >> 6;
            int num2 = fromPosition & 0x3f;
            ulong* numPtr1 = this.m_buffer + num;
            numPtr1[0] &= (ulong) ~(-1L << (num2 & 0x3f));
            int divisionCeil = MyLibraryUtils.GetDivisionCeil(this.BitPosition, 0x40);
            for (int i = num + 1; i < divisionCeil; i++)
            {
                this.m_buffer[i] = 0L;
            }
        }

        public void Dispose()
        {
            this.ReleaseInternalBuffer();
            GC.SuppressFinalize(this);
        }

        private void EnsureSize(int bitCount)
        {
            if (this.BitLength < bitCount)
            {
                this.Resize(bitCount);
            }
        }

        ~BitStream()
        {
            this.ReleaseInternalBuffer();
        }

        private unsafe void FreeNotOwnedBuffer()
        {
            if (!this.OwnsBuffer && (this.m_buffer != null))
            {
                this.m_bufferHandle.Free();
            }
        }

        public bool ReadBool() => 
            (this.ReadInternal(1) != 0L);

        public byte ReadByte(int bitCount = 8) => 
            ((byte) this.ReadInternal(bitCount));

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

        public char ReadChar(int bitCount = 0x10) => 
            ((char) this.ReadInternal(bitCount));

        private unsafe int ReadChars(byte* tmpBuffer, int byteCount, ref char[] outputArray, Encoding encoding)
        {
            this.ReadMemory((void*) tmpBuffer, byteCount * 8);
            int charCount = encoding.GetCharCount(tmpBuffer, byteCount);
            if (charCount > outputArray.Length)
            {
                outputArray = new char[Math.Max(charCount, outputArray.Length * 2)];
            }
            char* chars = outputArray;
            encoding.GetChars(tmpBuffer, byteCount, chars, charCount);
            fixed (char* chRef = null)
            {
                return charCount;
            }
        }

        public unsafe decimal ReadDecimal()
        {
            decimal num;
            *((long*) &num) = this.ReadInternal(0x40);
            &num[8] = this.ReadInternal(0x40);
            return num;
        }

        public unsafe double ReadDouble() => 
            *(((double*) &this.ReadInternal(0x40)));

        public Type ReadDynamicType(Type baseType, DynamicSerializerDelegate typeResolver)
        {
            Type type = null;
            typeResolver(this, baseType, ref type);
            return type;
        }

        public unsafe float ReadFloat() => 
            *(((float*) &this.ReadInternal(0x20)));

        public float ReadHalf() => 
            ((float) new Half(this.ReadUInt16(0x10)));

        public short ReadInt16(int bitCount = 0x10) => 
            ((short) this.ReadInternal(bitCount));

        public int ReadInt32(int bitCount = 0x20) => 
            ((int) this.ReadInternal(bitCount));

        public int ReadInt32Variant() => 
            Zag(this.ReadUInt32Variant());

        public long ReadInt64(int bitCount = 0x40) => 
            ((long) this.ReadInternal(bitCount));

        public long ReadInt64Variant() => 
            Zag(this.ReadUInt64Variant());

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private unsafe ulong ReadInternal(int bitSize)
        {
            int index = this.BitPosition >> 6;
            int num2 = ((this.BitPosition + bitSize) - 1) >> 6;
            ulong num3 = (ulong) (-1L >> ((0x40 - bitSize) & 0x3f));
            int num4 = this.BitPosition & 0x3f;
            ulong num5 = this.m_buffer[index] >> (num4 & 0x3f);
            if (num2 != index)
            {
                num5 |= this.m_buffer[num2] << ((0x40 - num4) & 0x3f);
            }
            this.BitPosition += bitSize;
            return (num5 & num3);
        }

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

        public float ReadNormalizedSignedFloat(int bits) => 
            MyLibraryUtils.DenormalizeFloatCenter(this.ReadUInt32(bits), -1f, 1f, bits);

        public byte[] ReadPrefixBytes()
        {
            int count = (int) this.ReadUInt32Variant();
            byte[] bytes = new byte[count];
            this.ReadBytes(bytes, 0, count);
            return bytes;
        }

        public unsafe string ReadPrefixLengthString(Encoding encoding)
        {
            byte* numPtr2;
            byte[] pinned buffer2;
            int count = (int) this.ReadUInt32Variant();
            if (count == 0)
            {
                return string.Empty;
            }
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

        public unsafe int ReadPrefixLengthString(ref char[] value, Encoding encoding)
        {
            byte* numPtr2;
            byte[] pinned buffer2;
            int byteCount = (int) this.ReadUInt32Variant();
            if (byteCount == 0)
            {
                return 0;
            }
            if (byteCount <= 0x400)
            {
                byte* tmpBuffer = stackalloc byte[(IntPtr) byteCount];
                return this.ReadChars(tmpBuffer, byteCount, ref value, encoding);
            }
            if (((buffer2 = new byte[byteCount]) == null) || (buffer2.Length == 0))
            {
                numPtr2 = null;
            }
            else
            {
                numPtr2 = buffer2;
            }
            return this.ReadChars(numPtr2, byteCount, ref value, encoding);
        }

        public sbyte ReadSByte(int bitCount = 8) => 
            ((sbyte) this.ReadInternal(bitCount));

        public ushort ReadUInt16(int bitCount = 0x10) => 
            ((ushort) this.ReadInternal(bitCount));

        public uint ReadUInt32(int bitCount = 0x20) => 
            ((uint) this.ReadInternal(bitCount));

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

        public ulong ReadUInt64(int bitCount = 0x40) => 
            this.ReadInternal(bitCount);

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

        private unsafe void ReleaseInternalBuffer()
        {
            this.FreeNotOwnedBuffer();
            if (this.m_ownedBuffer != null)
            {
                if (this.OwnsBuffer)
                {
                    this.m_buffer = null;
                    this.BitLength = 0;
                }
                Utilities.FreeMemory((IntPtr) this.m_ownedBuffer);
                this.m_ownedBuffer = null;
                this.m_ownedBufferBitLength = 0;
            }
        }

        public unsafe void ResetRead()
        {
            this.FreeNotOwnedBuffer();
            this.BitLength = this.BitPosition;
            this.m_buffer = this.m_ownedBuffer;
            this.Writing = false;
            this.BitPosition = 0;
        }

        public void ResetRead(BitStream source, bool copy = true)
        {
            this.ResetRead(source.DataPointer + source.BytePosition, source.BitLength - (source.BytePosition * 8), copy);
        }

        private unsafe void ResetRead(IntPtr buffer, int bitLength, bool copy)
        {
            this.FreeNotOwnedBuffer();
            if (!copy)
            {
                this.m_buffer = (ulong*) buffer;
                this.BitLength = bitLength;
                this.BitPosition = 0;
                this.Writing = false;
            }
            else
            {
                int divisionCeil = MyLibraryUtils.GetDivisionCeil(bitLength, 8);
                int sizeInBytes = Math.Max(divisionCeil, this.m_defaultByteSize);
                if (this.m_ownedBuffer == null)
                {
                    this.m_ownedBuffer = (ulong*) Utilities.AllocateMemory(sizeInBytes, 0x10);
                    this.m_ownedBufferBitLength = sizeInBytes * 8;
                }
                else if (this.m_ownedBufferBitLength < bitLength)
                {
                    Utilities.FreeMemory((IntPtr) this.m_ownedBuffer);
                    this.m_ownedBuffer = (ulong*) Utilities.AllocateMemory(sizeInBytes, 0x10);
                    this.m_ownedBufferBitLength = sizeInBytes * 8;
                }
                Utilities.CopyMemory((IntPtr) this.m_ownedBuffer, buffer, divisionCeil);
                this.m_buffer = this.m_ownedBuffer;
                this.BitLength = bitLength;
                this.BitPosition = 0;
                this.Writing = false;
            }
        }

        public unsafe void ResetRead(byte[] data, int byteOffset, int bitLength, bool copy = true)
        {
            byte* numPtr = &(data[byteOffset]);
            this.ResetRead((IntPtr) numPtr, bitLength, copy);
            if (!copy)
            {
                this.m_bufferHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            }
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        public unsafe void ResetWrite()
        {
            this.FreeNotOwnedBuffer();
            if (this.m_ownedBuffer == null)
            {
                this.m_ownedBuffer = (ulong*) Utilities.AllocateMemory(this.m_defaultByteSize, 0x10);
                this.m_ownedBufferBitLength = this.m_defaultByteSize * 8;
            }
            this.m_buffer = this.m_ownedBuffer;
            this.BitLength = this.m_ownedBufferBitLength;
            this.BitPosition = 0;
            this.m_buffer[0] = 0L;
            this.Writing = true;
        }

        private unsafe void Resize(int bitSize)
        {
            if (!this.OwnsBuffer)
            {
                throw new BitStreamException("BitStream cannot write more data. Buffer is full and it's not owned by BitStream", new EndOfStreamException());
            }
            int num = Math.Max(this.BitLength * 2, bitSize);
            IntPtr dest = Utilities.AllocateClearedMemory(MyLibraryUtils.GetDivisionCeil(num, 0x40) * 8, 0, 0x10);
            Utilities.CopyMemory(dest, (IntPtr) this.m_buffer, this.BytePosition);
            Utilities.FreeMemory((IntPtr) this.m_buffer);
            this.FreeNotOwnedBuffer();
            this.m_buffer = (ulong*) dest;
            this.BitLength = num;
            this.m_ownedBuffer = this.m_buffer;
            this.m_ownedBufferBitLength = this.BitLength;
        }

        public void Serialize(ref bool value)
        {
            if (this.Writing)
            {
                this.WriteBool(value);
            }
            else
            {
                value = this.ReadBool();
            }
        }

        public void Serialize(ref char value)
        {
            if (this.Writing)
            {
                this.WriteChar(value, 0x10);
            }
            else
            {
                value = this.ReadChar(0x10);
            }
        }

        public void Serialize(ref decimal value)
        {
            if (this.Writing)
            {
                this.WriteDecimal(value);
            }
            else
            {
                value = this.ReadDecimal();
            }
        }

        public void Serialize(ref double value)
        {
            if (this.Writing)
            {
                this.WriteDouble(value);
            }
            else
            {
                value = this.ReadDouble();
            }
        }

        public void Serialize(ref float value)
        {
            if (this.Writing)
            {
                this.WriteFloat(value);
            }
            else
            {
                value = this.ReadFloat();
            }
        }

        public void Serialize(ref byte value, int bitCount = 8)
        {
            if (this.Writing)
            {
                this.WriteByte(value, bitCount);
            }
            else
            {
                value = this.ReadByte(bitCount);
            }
        }

        public void Serialize(ref short value, int bitCount = 0x10)
        {
            if (this.Writing)
            {
                this.WriteInt16(value, bitCount);
            }
            else
            {
                value = this.ReadInt16(bitCount);
            }
        }

        public void Serialize(ref int value, int bitCount = 0x20)
        {
            if (this.Writing)
            {
                this.WriteInt32(value, bitCount);
            }
            else
            {
                value = this.ReadInt32(bitCount);
            }
        }

        public void Serialize(ref long value, int bitCount = 0x40)
        {
            if (this.Writing)
            {
                this.WriteInt64(value, bitCount);
            }
            else
            {
                value = this.ReadInt64(bitCount);
            }
        }

        public void Serialize(ref sbyte value, int bitCount = 8)
        {
            if (this.Writing)
            {
                this.WriteSByte(value, bitCount);
            }
            else
            {
                value = this.ReadSByte(bitCount);
            }
        }

        public void Serialize(ref ushort value, int bitCount = 0x10)
        {
            if (this.Writing)
            {
                this.WriteUInt16(value, bitCount);
            }
            else
            {
                value = this.ReadUInt16(bitCount);
            }
        }

        public void Serialize(ref uint value, int bitCount = 0x20)
        {
            if (this.Writing)
            {
                this.WriteUInt32(value, bitCount);
            }
            else
            {
                value = this.ReadUInt32(bitCount);
            }
        }

        public void Serialize(ref ulong value, int bitCount = 0x40)
        {
            if (this.Writing)
            {
                this.WriteUInt64(value, bitCount);
            }
            else
            {
                value = this.ReadUInt64(bitCount);
            }
        }

        public void Serialize(StringBuilder value, ref char[] tmpArray, Encoding encoding)
        {
            if (!this.Writing)
            {
                value.Clear();
                int charCount = this.ReadPrefixLengthString(ref tmpArray, encoding);
                value.Append(tmpArray, 0, charCount);
            }
            else
            {
                if (value.Length > tmpArray.Length)
                {
                    tmpArray = new char[Math.Max(value.Length, tmpArray.Length * 2)];
                }
                value.CopyTo(0, tmpArray, 0, value.Length);
                this.WritePrefixLengthString(tmpArray, 0, value.Length, encoding);
            }
        }

        public void SerializeBytes(ref byte[] bytes, int start, int count)
        {
            if (this.Writing)
            {
                this.WriteBytes(bytes, start, count);
            }
            else
            {
                this.ReadBytes(bytes, start, count);
            }
        }

        public void SerializeMemory(IntPtr ptr, int bitSize)
        {
            if (this.Writing)
            {
                this.WriteMemory(ptr, bitSize);
            }
            else
            {
                this.ReadMemory(ptr, bitSize);
            }
        }

        public unsafe void SerializeMemory(void* ptr, int bitSize)
        {
            if (this.Writing)
            {
                this.WriteMemory(ptr, bitSize);
            }
            else
            {
                this.ReadMemory(ptr, bitSize);
            }
        }

        public void SerializePrefixBytes(ref byte[] bytes)
        {
            if (this.Writing)
            {
                this.WriteVariant((uint) bytes.Length);
                this.WriteBytes(bytes, 0, bytes.Length);
            }
            else
            {
                int count = (int) this.ReadUInt32Variant();
                bytes = new byte[count];
                this.ReadBytes(bytes, 0, count);
            }
        }

        public void SerializePrefixString(ref string str, Encoding encoding)
        {
            if (this.Writing)
            {
                this.WritePrefixLengthString(str, 0, str.Length, encoding);
            }
            else
            {
                str = this.ReadPrefixLengthString(encoding);
            }
        }

        public void SerializePrefixStringAscii(ref string str)
        {
            this.SerializePrefixString(ref str, Encoding.ASCII);
        }

        public void SerializePrefixStringUtf8(ref string str)
        {
            this.SerializePrefixString(ref str, Encoding.UTF8);
        }

        public void SerializeVariant(ref int value)
        {
            if (this.Writing)
            {
                this.WriteVariantSigned(value);
            }
            else
            {
                value = this.ReadInt32Variant();
            }
        }

        public void SerializeVariant(ref long value)
        {
            if (this.Writing)
            {
                this.WriteVariantSigned(value);
            }
            else
            {
                value = this.ReadInt64Variant();
            }
        }

        public void SerializeVariant(ref uint value)
        {
            if (this.Writing)
            {
                this.WriteVariant(value);
            }
            else
            {
                value = this.ReadUInt32Variant();
            }
        }

        public void SerializeVariant(ref ulong value)
        {
            if (this.Writing)
            {
                this.WriteVariant(value);
            }
            else
            {
                value = this.ReadUInt64Variant();
            }
        }

        public void SetBitPositionRead(int newReadBitPosition)
        {
            this.BitPosition = newReadBitPosition;
        }

        public void SetBitPositionWrite(int newBitPosition)
        {
            this.BitPosition = newBitPosition;
        }

        public void Terminate()
        {
            this.WriteUInt16(0xc8b9, 0x10);
        }

        public void WriteBitStream(BitStream readStream)
        {
            int num2;
            for (int i = readStream.BitLength - readStream.BitPosition; i > 0; i -= num2)
            {
                num2 = Math.Min(0x40, i);
                ulong num3 = readStream.ReadUInt64(num2);
                this.WriteUInt64(num3, num2);
            }
        }

        public void WriteBool(bool value)
        {
            this.WriteInternal(value ? ulong.MaxValue : ((ulong) 0L), 1);
        }

        public void WriteByte(byte value, int bitCount = 8)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public unsafe void WriteBytes(byte[] bytes, int start, int count)
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
            this.WriteMemory((void*) (numPtr + start), count * 8);
            buffer = null;
        }

        public void WriteChar(char value, int bitCount = 0x10)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public unsafe void WriteDecimal(decimal value)
        {
            this.WriteInternal(*((ulong*) &value), 0x40);
            this.WriteInternal(*((ulong*) (&value + 8)), 0x40);
        }

        public unsafe void WriteDouble(double value)
        {
            this.WriteInternal(*((ulong*) &value), 0x40);
        }

        public void WriteDynamicType(Type baseType, Type obj, DynamicSerializerDelegate typeResolver)
        {
            typeResolver(this, baseType, ref obj);
        }

        public unsafe void WriteFloat(float value)
        {
            this.WriteInternal((ulong) *(((uint*) &value)), 0x20);
        }

        public void WriteHalf(float value)
        {
            Half half = new Half(value);
            this.WriteUInt16(half.RawValue, 0x10);
        }

        public void WriteInt16(short value, int bitCount = 0x10)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public void WriteInt32(int value, int bitCount = 0x20)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public void WriteInt64(long value, int bitCount = 0x40)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        private unsafe void WriteInternal(ulong value, int bitSize)
        {
            if (bitSize != 0)
            {
                this.EnsureSize(this.BitPosition + bitSize);
                int num = this.BitPosition >> 6;
                int num2 = ((this.BitPosition + bitSize) - 1) >> 6;
                ulong num3 = (ulong) (-1L >> ((0x40 - bitSize) & 0x3f));
                int num4 = this.BitPosition & 0x3f;
                value &= num3;
                ulong* numPtr1 = this.m_buffer + num;
                numPtr1[0] &= ~(num3 << (num4 & 0x3f));
                ulong* numPtr2 = this.m_buffer + num;
                numPtr2[0] |= value << (num4 & 0x3f);
                if (num2 != num)
                {
                    ulong* numPtr3 = this.m_buffer + num2;
                    numPtr3[0] &= ~(num3 >> ((0x40 - num4) & 0x3f));
                    ulong* numPtr4 = this.m_buffer + num2;
                    numPtr4[0] |= value >> ((0x40 - num4) & 0x3f);
                }
                this.BitPosition += bitSize;
            }
        }

        public unsafe void WriteMemory(IntPtr ptr, int bitSize)
        {
            this.WriteMemory((void*) ptr, bitSize);
        }

        public unsafe void WriteMemory(void* ptr, int bitSize)
        {
            int num = (bitSize / 8) / 8;
            ulong* numPtr = (ulong*) ptr;
            for (int i = 0; i < num; i++)
            {
                this.WriteUInt64(numPtr[i], 0x40);
            }
            int num2 = bitSize - ((num * 8) * 8);
            for (byte* numPtr2 = (byte*) (numPtr + num); num2 > 0; numPtr2++)
            {
                int bitCount = Math.Min(num2, 8);
                this.WriteByte(numPtr2[0], bitCount);
                num2 -= bitCount;
            }
        }

        public void WriteNormalizedSignedFloat(float value, int bits)
        {
            this.WriteUInt32(MyLibraryUtils.NormalizeFloatCenter(value, -1f, 1f, bits), bits);
        }

        public void WritePrefixBytes(byte[] bytes, int start, int count)
        {
            this.WriteVariant((uint) count);
            this.WriteBytes(bytes, start, count);
        }

        public unsafe void WritePrefixLengthString(char[] str, int characterStart, int characterCount, Encoding encoding)
        {
            char* chPtr;
            char[] pinned chArray;
            if (((chArray = str) == null) || (chArray.Length == 0))
            {
                chPtr = null;
            }
            else
            {
                chPtr = chArray;
            }
            this.WritePrefixLengthString(characterStart, characterCount, encoding, chPtr);
            chArray = null;
        }

        private unsafe void WritePrefixLengthString(int characterStart, int characterCount, Encoding encoding, char* ptr)
        {
            char* chars = ptr + characterStart;
            int byteCount = encoding.GetByteCount(chars, characterCount);
            this.WriteVariant((uint) byteCount);
            byte* bytes = stackalloc byte[0x100];
            int num2 = 0x100 / encoding.GetMaxByteCount(1);
            while (characterCount > 0)
            {
                int charCount = Math.Min(num2, characterCount);
                int num4 = encoding.GetBytes(chars, charCount, bytes, 0x100);
                this.WriteMemory((void*) bytes, num4 * 8);
                chars += charCount;
                characterCount -= charCount;
            }
        }

        public unsafe void WritePrefixLengthString(string str, int characterStart, int characterCount, Encoding encoding)
        {
            char* ptr = (char*) str;
            if (ptr != null)
            {
                ptr += RuntimeHelpers.OffsetToStringData;
            }
            this.WritePrefixLengthString(characterStart, characterCount, encoding, ptr);
        }

        public void WriteSByte(sbyte value, int bitCount = 8)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public void WriteUInt16(ushort value, int bitCount = 0x10)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public void WriteUInt32(uint value, int bitCount = 0x20)
        {
            this.WriteInternal((ulong) value, bitCount);
        }

        public void WriteUInt64(ulong value, int bitCount = 0x40)
        {
            this.WriteInternal(value, bitCount);
        }

        public unsafe void WriteVariant(uint value)
        {
            ulong num;
            byte* numPtr = (byte*) &num;
            int num2 = 0;
            int index = 0;
            while (true)
            {
                index++;
                numPtr[index] = (byte) (value | 0x80);
                num2++;
                if ((value = value >> 7) == 0)
                {
                    byte* numPtr1 = numPtr + (index - 1);
                    numPtr1[0] = (byte) (numPtr1[0] & 0x7f);
                    this.WriteInternal(num, num2 * 8);
                    return;
                }
            }
        }

        public unsafe void WriteVariant(ulong value)
        {
            byte* numPtr = stackalloc byte[0x10];
            int num = 0;
            int index = 0;
            while (true)
            {
                index++;
                numPtr[index] = (byte) ((value & 0x7f) | ((ulong) 0x80L));
                num++;
                if ((value = value >> 7) == 0)
                {
                    byte* numPtr1 = numPtr + (index - 1);
                    numPtr1[0] = (byte) (numPtr1[0] & 0x7f);
                    if (num <= 8)
                    {
                        this.WriteInternal(*((ulong*) numPtr), num * 8);
                        return;
                    }
                    this.WriteInternal(*((ulong*) numPtr), 0x40);
                    this.WriteInternal(*((ulong*) (numPtr + 8)), (num - 8) * 8);
                    return;
                }
            }
        }

        public void WriteVariantSigned(int value)
        {
            this.WriteVariant(Zig(value));
        }

        public void WriteVariantSigned(long value)
        {
            this.WriteVariant(Zig(value));
        }

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

        private static uint Zig(int value) => 
            ((uint) ((value << 1) ^ (value >> 0x1f)));

        private static ulong Zig(long value) => 
            ((ulong) ((value << 1) ^ (value >> 0x3f)));

        public int BitPosition { get; private set; }

        public int BitLength { get; private set; }

        public int BytePosition =>
            MyLibraryUtils.GetDivisionCeil(this.BitPosition, 8);

        public int ByteLength =>
            MyLibraryUtils.GetDivisionCeil(this.BitLength, 8);

        private bool OwnsBuffer =>
            (this.m_ownedBuffer == this.m_buffer);

        public bool Reading =>
            !this.Writing;

        public bool Writing { get; private set; }

        public IntPtr DataPointer =>
            ((IntPtr) this.m_buffer);
    }
}

