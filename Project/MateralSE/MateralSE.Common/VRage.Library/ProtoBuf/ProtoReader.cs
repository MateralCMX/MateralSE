namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class ProtoReader : IDisposable
    {
        private Stream source;
        private byte[] ioBuffer;
        private TypeModel model;
        private int fieldNumber;
        private ProtoBuf.WireType wireType;
        private int dataRemaining;
        private readonly bool isFixedLength;
        private bool internStrings;
        private readonly SerializationContext context;
        private int ioIndex;
        private int position;
        private int available;
        private const long Int64Msb = -9223372036854775808L;
        private const int Int32Msb = -2147483648;
        private Dictionary<string, string> stringInterner;
        private static readonly UTF8Encoding encoding = new UTF8Encoding();
        private int depth;
        private int blockEnd;
        private static readonly byte[] EmptyBlob = new byte[0];
        private readonly NetObjectCache netCache;
        private uint trapCount;

        public ProtoReader(Stream source, TypeModel model, SerializationContext context) : this(source, model, context, -1)
        {
        }

        public ProtoReader(Stream source, TypeModel model, SerializationContext context, int length)
        {
            this.wireType = ProtoBuf.WireType.None;
            this.internStrings = true;
            this.blockEnd = 0x7fffffff;
            this.netCache = new NetObjectCache();
            this.trapCount = 1;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.CanRead)
            {
                throw new ArgumentException("Cannot read from stream", "source");
            }
            this.source = source;
            this.ioBuffer = BufferPool.GetBuffer();
            this.model = model;
            this.isFixedLength = length >= 0;
            this.dataRemaining = this.isFixedLength ? length : 0;
            if (context == null)
            {
                context = SerializationContext.Default;
            }
            else
            {
                context.Freeze();
            }
            this.context = context;
        }

        internal static Exception AddErrorData(Exception exception, ProtoReader source)
        {
            if (((exception != null) && (source != null)) && !exception.Data.Contains("protoSource"))
            {
                exception.Data.Add("protoSource", $"tag={source.fieldNumber}; wire-type={source.wireType}; offset={source.position}; depth={source.depth}");
            }
            return exception;
        }

        public static byte[] AppendBytes(byte[] value, ProtoReader reader)
        {
            int length;
            if (reader.wireType != ProtoBuf.WireType.String)
            {
                throw reader.CreateWireTypeException();
            }
            int count = (int) reader.ReadUInt32Variant(false);
            reader.wireType = ProtoBuf.WireType.None;
            if (count == 0)
            {
                return ((value == null) ? EmptyBlob : value);
            }
            if ((value == null) || (value.Length == 0))
            {
                length = 0;
                value = new byte[count];
            }
            else
            {
                length = value.Length;
                byte[] to = new byte[value.Length + count];
                Helpers.BlockCopy(value, 0, to, 0, value.Length);
                value = to;
            }
            reader.position += count;
            while (count > reader.available)
            {
                if (reader.available > 0)
                {
                    Helpers.BlockCopy(reader.ioBuffer, reader.ioIndex, value, length, reader.available);
                    count -= reader.available;
                    length += reader.available;
                    reader.ioIndex = reader.available = 0;
                }
                int num3 = (count > reader.ioBuffer.Length) ? reader.ioBuffer.Length : count;
                if (num3 > 0)
                {
                    reader.Ensure(num3, true);
                }
            }
            if (count > 0)
            {
                Helpers.BlockCopy(reader.ioBuffer, reader.ioIndex, value, length, count);
                reader.ioIndex += count;
                reader.available -= count;
            }
            return value;
        }

        public void AppendExtensionData(IExtensible instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            bool commit = false;
            Stream dest = instance.GetExtensionObject(true).BeginAppend();
            try
            {
                using (ProtoWriter writer = new ProtoWriter(dest, this.model, null))
                {
                    this.AppendExtensionField(writer);
                    writer.Close();
                }
                commit = true;
            }
            finally
            {
                IExtension extension;
                extension.EndAppend(dest, commit);
            }
        }

        private void AppendExtensionField(ProtoWriter writer)
        {
            ProtoWriter.WriteFieldHeader(this.fieldNumber, this.wireType, writer);
            switch (this.wireType)
            {
                case ProtoBuf.WireType.Variant:
                case ProtoBuf.WireType.Fixed64:
                case ProtoBuf.WireType.SignedVariant:
                    ProtoWriter.WriteInt64(this.ReadInt64(), writer);
                    return;

                case ProtoBuf.WireType.String:
                    ProtoWriter.WriteBytes(AppendBytes(null, this), writer);
                    return;

                case ProtoBuf.WireType.StartGroup:
                {
                    SubItemToken token = StartSubItem(this);
                    SubItemToken token2 = ProtoWriter.StartSubItem(null, writer);
                    while (this.ReadFieldHeader() > 0)
                    {
                        this.AppendExtensionField(writer);
                    }
                    EndSubItem(token, this);
                    ProtoWriter.EndSubItem(token2, writer);
                    return;
                }
                case ProtoBuf.WireType.Fixed32:
                    ProtoWriter.WriteInt32(this.ReadInt32(), writer);
                    return;
            }
            throw this.CreateWireTypeException();
        }

        public void Assert(ProtoBuf.WireType wireType)
        {
            if (this.wireType != wireType)
            {
                if ((wireType & (ProtoBuf.WireType.Fixed32 | ProtoBuf.WireType.String)) != this.wireType)
                {
                    throw this.CreateWireTypeException();
                }
                this.wireType = wireType;
            }
        }

        internal void CheckFullyConsumed()
        {
            if (this.isFixedLength)
            {
                if (this.dataRemaining != 0)
                {
                    throw new ProtoException("Incorrect number of bytes consumed");
                }
            }
            else if (this.available != 0)
            {
                throw new ProtoException("Unconsumed data left in the buffer; this suggests corrupt input");
            }
        }

        private Exception CreateException(string message) => 
            AddErrorData(new ProtoException(message), this);

        private Exception CreateWireTypeException() => 
            this.CreateException("Invalid wire-type; this usually means you have over-written a file without truncating or setting the length; see http://stackoverflow.com/q/2152978/23354");

        internal Type DeserializeType(string value) => 
            TypeModel.DeserializeType(this.model, value);

        public static int DirectReadBigEndianInt32(Stream source) => 
            ((((ReadByteOrThrow(source) << 0x18) | (ReadByteOrThrow(source) << 0x10)) | (ReadByteOrThrow(source) << 8)) | ReadByteOrThrow(source));

        public static byte[] DirectReadBytes(Stream source, int count)
        {
            byte[] buffer = new byte[count];
            DirectReadBytes(source, buffer, 0, count);
            return buffer;
        }

        public static void DirectReadBytes(Stream source, byte[] buffer, int offset, int count)
        {
            int num;
            while ((count > 0) && ((num = source.Read(buffer, offset, count)) > 0))
            {
                count -= num;
                offset += num;
            }
            if (count > 0)
            {
                throw EoF(null);
            }
        }

        public static int DirectReadLittleEndianInt32(Stream source) => 
            (((ReadByteOrThrow(source) | (ReadByteOrThrow(source) << 8)) | (ReadByteOrThrow(source) << 0x10)) | (ReadByteOrThrow(source) << 0x18));

        public static string DirectReadString(Stream source, int length)
        {
            byte[] buffer = new byte[length];
            DirectReadBytes(source, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        public static int DirectReadVarintInt32(Stream source)
        {
            uint num;
            if (TryReadUInt32Variant(source, out num) <= 0)
            {
                throw EoF(null);
            }
            return (int) num;
        }

        public void Dispose()
        {
            this.source = null;
            this.model = null;
            BufferPool.ReleaseBufferToPool(ref this.ioBuffer);
        }

        public static void EndSubItem(SubItemToken token, ProtoReader reader)
        {
            int num = token.value;
            if (reader.wireType == ProtoBuf.WireType.EndGroup)
            {
                if (num >= 0)
                {
                    throw AddErrorData(new ArgumentException("token"), reader);
                }
                if (-num != reader.fieldNumber)
                {
                    throw reader.CreateException("Wrong group was ended");
                }
                reader.wireType = ProtoBuf.WireType.None;
                reader.depth--;
            }
            else
            {
                if (num < reader.position)
                {
                    throw reader.CreateException("Sub-message not read entirely");
                }
                if ((reader.blockEnd != reader.position) && (reader.blockEnd != 0x7fffffff))
                {
                    throw reader.CreateException("Sub-message not read correctly");
                }
                reader.blockEnd = num;
                reader.depth--;
            }
        }

        internal void Ensure(int count, bool strict)
        {
            int num2;
            if (count > this.ioBuffer.Length)
            {
                BufferPool.ResizeAndFlushLeft(ref this.ioBuffer, count, this.ioIndex, this.available);
                this.ioIndex = 0;
            }
            else if ((this.ioIndex + count) >= this.ioBuffer.Length)
            {
                Helpers.BlockCopy(this.ioBuffer, this.ioIndex, this.ioBuffer, 0, this.available);
                this.ioIndex = 0;
            }
            count -= this.available;
            int offset = this.ioIndex + this.available;
            int dataRemaining = this.ioBuffer.Length - offset;
            if (this.isFixedLength && (this.dataRemaining < dataRemaining))
            {
                dataRemaining = this.dataRemaining;
            }
            while (((count > 0) && (dataRemaining > 0)) && ((num2 = this.source.Read(this.ioBuffer, offset, dataRemaining)) > 0))
            {
                this.available += num2;
                count -= num2;
                dataRemaining -= num2;
                offset += num2;
                if (this.isFixedLength)
                {
                    this.dataRemaining -= num2;
                }
            }
            if (strict && (count > 0))
            {
                throw EoF(this);
            }
        }

        private static Exception EoF(ProtoReader source) => 
            AddErrorData(new EndOfStreamException(), source);

        internal int GetTypeKey(ref Type type) => 
            this.model.GetKey(ref type);

        public static bool HasSubValue(ProtoBuf.WireType wireType, ProtoReader source)
        {
            if ((source.blockEnd <= source.position) || (wireType == ProtoBuf.WireType.EndGroup))
            {
                return false;
            }
            source.wireType = wireType;
            return true;
        }

        public void Hint(ProtoBuf.WireType wireType)
        {
            if ((this.wireType != wireType) && ((wireType & (ProtoBuf.WireType.Fixed32 | ProtoBuf.WireType.String)) == this.wireType))
            {
                this.wireType = wireType;
            }
        }

        private string Intern(string value)
        {
            if (value == null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return "";
            }
            if (this.stringInterner == null)
            {
                this.stringInterner = new Dictionary<string, string>();
                this.stringInterner.Add(value, value);
            }
            else
            {
                string str;
                if (this.stringInterner.TryGetValue(value, out str))
                {
                    value = str;
                }
                else
                {
                    this.stringInterner.Add(value, value);
                }
            }
            return value;
        }

        public static object Merge(ProtoReader parent, object from, object to)
        {
            TypeModel model = parent.Model;
            SerializationContext context = parent.Context;
            if (model == null)
            {
                throw new InvalidOperationException("Types cannot be merged unless a type-model has been specified");
            }
            using (MemoryStream stream = new MemoryStream())
            {
                model.Serialize(stream, from, context);
                stream.Position = 0L;
                return model.Deserialize(stream, to, null);
            }
        }

        public static void NoteObject(object value, ProtoReader reader)
        {
            if (reader.trapCount != 0)
            {
                reader.netCache.RegisterTrappedObject(value);
                reader.trapCount--;
            }
        }

        public bool ReadBoolean()
        {
            uint num = this.ReadUInt32();
            if (num == 0)
            {
                return false;
            }
            if (num != 1)
            {
                throw this.CreateException("Unexpected boolean value");
            }
            return true;
        }

        public byte ReadByte() => 
            ((byte) this.ReadUInt32());

        private static int ReadByteOrThrow(Stream source)
        {
            int num = source.ReadByte();
            if (num < 0)
            {
                throw EoF(null);
            }
            return num;
        }

        private static byte[] ReadBytes(Stream stream, int length)
        {
            int num2;
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            byte[] buffer = new byte[length];
            int offset = 0;
            while ((length > 0) && ((num2 = stream.Read(buffer, offset, length)) > 0))
            {
                length -= num2;
            }
            if (length > 0)
            {
                throw EoF(null);
            }
            return buffer;
        }

        public unsafe double ReadDouble()
        {
            ProtoBuf.WireType wireType = this.wireType;
            if (wireType == ProtoBuf.WireType.Fixed64)
            {
                return *(((double*) &this.ReadInt64()));
            }
            if (wireType != ProtoBuf.WireType.Fixed32)
            {
                throw this.CreateWireTypeException();
            }
            return (double) this.ReadSingle();
        }

        public int ReadFieldHeader()
        {
            if ((this.blockEnd > this.position) && (this.wireType != ProtoBuf.WireType.EndGroup))
            {
                uint num;
                if (!this.TryReadUInt32Variant(out num))
                {
                    this.wireType = ProtoBuf.WireType.None;
                    this.fieldNumber = 0;
                }
                else
                {
                    this.wireType = ((ProtoBuf.WireType) num) & (ProtoBuf.WireType.Fixed32 | ProtoBuf.WireType.String);
                    this.fieldNumber = (int) (num >> 3);
                    if (this.fieldNumber < 1)
                    {
                        throw new ProtoException("Invalid field in source data: " + this.fieldNumber);
                    }
                }
                if (this.wireType != ProtoBuf.WireType.EndGroup)
                {
                    return this.fieldNumber;
                }
                if (this.depth <= 0)
                {
                    throw new ProtoException("Unexpected end-group in source data; this usually means the source data is corrupt");
                }
            }
            return 0;
        }

        public short ReadInt16() => 
            ((short) this.ReadInt32());

        public int ReadInt32()
        {
            ProtoBuf.WireType wireType = this.wireType;
            if (wireType <= ProtoBuf.WireType.Fixed64)
            {
                if (wireType == ProtoBuf.WireType.Variant)
                {
                    return (int) this.ReadUInt32Variant(true);
                }
                if (wireType == ProtoBuf.WireType.Fixed64)
                {
                    return (int) this.ReadInt64();
                }
            }
            else
            {
                if (wireType == ProtoBuf.WireType.Fixed32)
                {
                    if (this.available < 4)
                    {
                        this.Ensure(4, true);
                    }
                    this.position += 4;
                    this.available -= 4;
                    int ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    return (((this.ioBuffer[ioIndex] | (this.ioBuffer[ioIndex] << 8)) | (this.ioBuffer[ioIndex] << 0x10)) | (this.ioBuffer[ioIndex] << 0x18));
                }
                if (wireType == ProtoBuf.WireType.SignedVariant)
                {
                    return Zag(this.ReadUInt32Variant(true));
                }
            }
            throw this.CreateWireTypeException();
        }

        public long ReadInt64()
        {
            ProtoBuf.WireType wireType = this.wireType;
            if (wireType > ProtoBuf.WireType.Fixed64)
            {
                if (wireType == ProtoBuf.WireType.Fixed32)
                {
                    return (long) this.ReadInt32();
                }
                if (wireType == ProtoBuf.WireType.SignedVariant)
                {
                    return Zag(this.ReadUInt64Variant());
                }
            }
            else
            {
                if (wireType == ProtoBuf.WireType.Variant)
                {
                    return (long) this.ReadUInt64Variant();
                }
                if (wireType == ProtoBuf.WireType.Fixed64)
                {
                    if (this.available < 8)
                    {
                        this.Ensure(8, true);
                    }
                    this.position += 8;
                    this.available -= 8;
                    int ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    ioIndex = this.ioIndex;
                    this.ioIndex = ioIndex + 1;
                    return (long) (((((((this.ioBuffer[ioIndex] | (this.ioBuffer[ioIndex] << 8)) | (this.ioBuffer[ioIndex] << 0x10)) | (this.ioBuffer[ioIndex] << 0x18)) | (this.ioBuffer[ioIndex] << 0x20)) | (this.ioBuffer[ioIndex] << 40)) | (this.ioBuffer[ioIndex] << 0x30)) | (this.ioBuffer[ioIndex] << 0x38));
                }
            }
            throw this.CreateWireTypeException();
        }

        public static int ReadLengthPrefix(Stream source, bool expectHeader, PrefixStyle style, out int fieldNumber)
        {
            int num;
            return ReadLengthPrefix(source, expectHeader, style, out fieldNumber, out num);
        }

        public static int ReadLengthPrefix(Stream source, bool expectHeader, PrefixStyle style, out int fieldNumber, out int bytesRead)
        {
            fieldNumber = 0;
            switch (style)
            {
                case PrefixStyle.None:
                    bytesRead = 0;
                    return 0x7fffffff;

                case PrefixStyle.Base128:
                    uint num;
                    int num2;
                    bytesRead = 0;
                    if (!expectHeader)
                    {
                        num2 = TryReadUInt32Variant(source, out num);
                        bytesRead += num2;
                        return ((bytesRead < 0) ? -1 : ((int) num));
                    }
                    num2 = TryReadUInt32Variant(source, out num);
                    bytesRead += num2;
                    if (num2 <= 0)
                    {
                        bytesRead = 0;
                        return -1;
                    }
                    if ((num & 7) != 2)
                    {
                        throw new InvalidOperationException();
                    }
                    fieldNumber = (int) (num >> 3);
                    num2 = TryReadUInt32Variant(source, out num);
                    bytesRead += num2;
                    if (bytesRead == 0)
                    {
                        throw EoF(null);
                    }
                    return (int) num;

                case PrefixStyle.Fixed32:
                {
                    int num3 = source.ReadByte();
                    if (num3 < 0)
                    {
                        bytesRead = 0;
                        return -1;
                    }
                    bytesRead = 4;
                    return (((num3 | (ReadByteOrThrow(source) << 8)) | (ReadByteOrThrow(source) << 0x10)) | (ReadByteOrThrow(source) << 0x18));
                }
                case PrefixStyle.Fixed32BigEndian:
                {
                    int num4 = source.ReadByte();
                    if (num4 < 0)
                    {
                        bytesRead = 0;
                        return -1;
                    }
                    bytesRead = 4;
                    return ((((num4 << 0x18) | (ReadByteOrThrow(source) << 0x10)) | (ReadByteOrThrow(source) << 8)) | ReadByteOrThrow(source));
                }
            }
            throw new ArgumentOutOfRangeException("style");
        }

        public static object ReadObject(object value, int key, ProtoReader reader) => 
            ReadTypedObject(value, key, reader, null);

        public sbyte ReadSByte() => 
            ((sbyte) this.ReadInt32());

        public unsafe float ReadSingle()
        {
            ProtoBuf.WireType wireType = this.wireType;
            if (wireType != ProtoBuf.WireType.Fixed64)
            {
                if (wireType != ProtoBuf.WireType.Fixed32)
                {
                    throw this.CreateWireTypeException();
                }
                return *(((float*) &this.ReadInt32()));
            }
            double num2 = this.ReadDouble();
            float num3 = (float) num2;
            if (Helpers.IsInfinity(num3) && !Helpers.IsInfinity(num2))
            {
                throw AddErrorData(new OverflowException(), this);
            }
            return num3;
        }

        public string ReadString()
        {
            if (this.wireType != ProtoBuf.WireType.String)
            {
                throw this.CreateWireTypeException();
            }
            int count = (int) this.ReadUInt32Variant(false);
            if (count == 0)
            {
                return "";
            }
            if (this.available < count)
            {
                this.Ensure(count, true);
            }
            string str = encoding.GetString(this.ioBuffer, this.ioIndex, count);
            if (this.internStrings)
            {
                str = this.Intern(str);
            }
            this.available -= count;
            this.position += count;
            this.ioIndex += count;
            return str;
        }

        public Type ReadType() => 
            TypeModel.DeserializeType(this.model, this.ReadString());

        internal static object ReadTypedObject(object value, int key, ProtoReader reader, Type type)
        {
            if (reader.model == null)
            {
                throw AddErrorData(new InvalidOperationException("Cannot deserialize sub-objects unless a model is provided"), reader);
            }
            SubItemToken token = StartSubItem(reader);
            if (key >= 0)
            {
                object obj1 = reader.model.Deserialize(key, value, reader);
                value = obj1;
            }
            else if ((type == null) || !reader.model.TryDeserializeAuxiliaryType(reader, DataFormat.Default, 1, type, ref value, true, false, true, false))
            {
                TypeModel.ThrowUnexpectedType(type);
            }
            EndSubItem(token, reader);
            return value;
        }

        public ushort ReadUInt16() => 
            ((ushort) this.ReadUInt32());

        public uint ReadUInt32()
        {
            ProtoBuf.WireType wireType = this.wireType;
            if (wireType == ProtoBuf.WireType.Variant)
            {
                return this.ReadUInt32Variant(false);
            }
            if (wireType == ProtoBuf.WireType.Fixed64)
            {
                return (uint) this.ReadUInt64();
            }
            if (wireType != ProtoBuf.WireType.Fixed32)
            {
                throw this.CreateWireTypeException();
            }
            if (this.available < 4)
            {
                this.Ensure(4, true);
            }
            this.position += 4;
            this.available -= 4;
            int ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            return (uint) (((this.ioBuffer[ioIndex] | (this.ioBuffer[ioIndex] << 8)) | (this.ioBuffer[ioIndex] << 0x10)) | (this.ioBuffer[ioIndex] << 0x18));
        }

        private uint ReadUInt32Variant(bool trimNegative)
        {
            uint num;
            int num2 = this.TryReadUInt32VariantWithoutMoving(trimNegative, out num);
            if (num2 <= 0)
            {
                throw EoF(this);
            }
            this.ioIndex += num2;
            this.available -= num2;
            this.position += num2;
            return num;
        }

        public ulong ReadUInt64()
        {
            ProtoBuf.WireType wireType = this.wireType;
            if (wireType == ProtoBuf.WireType.Variant)
            {
                return this.ReadUInt64Variant();
            }
            if (wireType != ProtoBuf.WireType.Fixed64)
            {
                if (wireType != ProtoBuf.WireType.Fixed32)
                {
                    throw this.CreateWireTypeException();
                }
                return (ulong) this.ReadUInt32();
            }
            if (this.available < 8)
            {
                this.Ensure(8, true);
            }
            this.position += 8;
            this.available -= 8;
            int ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            ioIndex = this.ioIndex;
            this.ioIndex = ioIndex + 1;
            return (ulong) (((((((this.ioBuffer[ioIndex] | (this.ioBuffer[ioIndex] << 8)) | (this.ioBuffer[ioIndex] << 0x10)) | (this.ioBuffer[ioIndex] << 0x18)) | (this.ioBuffer[ioIndex] << 0x20)) | (this.ioBuffer[ioIndex] << 40)) | (this.ioBuffer[ioIndex] << 0x30)) | (this.ioBuffer[ioIndex] << 0x38));
        }

        private ulong ReadUInt64Variant()
        {
            ulong num;
            int num2 = this.TryReadUInt64VariantWithoutMoving(out num);
            if (num2 <= 0)
            {
                throw EoF(this);
            }
            this.ioIndex += num2;
            this.available -= num2;
            this.position += num2;
            return num;
        }

        internal static void Seek(Stream source, int count, byte[] buffer)
        {
            if (source.CanSeek)
            {
                source.Seek((long) count, SeekOrigin.Current);
                count = 0;
            }
            else if (buffer != null)
            {
                while (true)
                {
                    int num;
                    if ((count <= buffer.Length) || ((num = source.Read(buffer, 0, buffer.Length)) <= 0))
                    {
                        while ((count > 0) && ((num = source.Read(buffer, 0, count)) > 0))
                        {
                            count -= num;
                        }
                        break;
                    }
                    count -= num;
                }
            }
            else
            {
                buffer = BufferPool.GetBuffer();
                try
                {
                    while (true)
                    {
                        int num2;
                        if ((count <= buffer.Length) || ((num2 = source.Read(buffer, 0, buffer.Length)) <= 0))
                        {
                            while ((count > 0) && ((num2 = source.Read(buffer, 0, count)) > 0))
                            {
                                count -= num2;
                            }
                            break;
                        }
                        count -= num2;
                    }
                }
                finally
                {
                    BufferPool.ReleaseBufferToPool(ref buffer);
                }
            }
            if (count > 0)
            {
                throw EoF(null);
            }
        }

        internal void SetRootObject(object value)
        {
            this.netCache.SetKeyedObject(0, value);
            this.trapCount--;
        }

        public void SkipField()
        {
            switch (this.wireType)
            {
                case ProtoBuf.WireType.Variant:
                case ProtoBuf.WireType.SignedVariant:
                    this.ReadUInt64Variant();
                    return;

                case ProtoBuf.WireType.Fixed64:
                    if (this.available < 8)
                    {
                        this.Ensure(8, true);
                    }
                    this.available -= 8;
                    this.ioIndex += 8;
                    this.position += 8;
                    return;

                case ProtoBuf.WireType.String:
                {
                    int count = (int) this.ReadUInt32Variant(false);
                    if (count <= this.available)
                    {
                        this.available -= count;
                        this.ioIndex += count;
                        this.position += count;
                        return;
                    }
                    this.position += count;
                    count -= this.available;
                    this.ioIndex = this.available = 0;
                    if (this.isFixedLength)
                    {
                        if (count > this.dataRemaining)
                        {
                            throw EoF(this);
                        }
                        this.dataRemaining -= count;
                    }
                    Seek(this.source, count, this.ioBuffer);
                    return;
                }
                case ProtoBuf.WireType.StartGroup:
                {
                    int fieldNumber = this.fieldNumber;
                    this.depth++;
                    while (this.ReadFieldHeader() > 0)
                    {
                        this.SkipField();
                    }
                    this.depth--;
                    if ((this.wireType != ProtoBuf.WireType.EndGroup) || (this.fieldNumber != fieldNumber))
                    {
                        throw this.CreateWireTypeException();
                    }
                    this.wireType = ProtoBuf.WireType.None;
                    return;
                }
                case ProtoBuf.WireType.Fixed32:
                    if (this.available < 4)
                    {
                        this.Ensure(4, true);
                    }
                    this.available -= 4;
                    this.ioIndex += 4;
                    this.position += 4;
                    return;
            }
            throw this.CreateWireTypeException();
        }

        public static SubItemToken StartSubItem(ProtoReader reader)
        {
            ProtoBuf.WireType wireType = reader.wireType;
            if (wireType != ProtoBuf.WireType.String)
            {
                if (wireType != ProtoBuf.WireType.StartGroup)
                {
                    throw reader.CreateWireTypeException();
                }
                reader.wireType = ProtoBuf.WireType.None;
                reader.depth++;
                return new SubItemToken(-reader.fieldNumber);
            }
            int num = (int) reader.ReadUInt32Variant(false);
            if (num < 0)
            {
                throw AddErrorData(new InvalidOperationException(), reader);
            }
            int blockEnd = reader.blockEnd;
            reader.blockEnd = reader.position + num;
            reader.depth++;
            return new SubItemToken(blockEnd);
        }

        public void ThrowEnumException(Type type, int value)
        {
            string str = (type == null) ? "<null>" : type.FullName;
            object[] objArray1 = new object[] { "No ", str, " enum is mapped to the wire-value ", value };
            throw AddErrorData(new ProtoException(string.Concat(objArray1)), this);
        }

        internal void TrapNextObject(int newObjectKey)
        {
            this.trapCount++;
            this.netCache.SetKeyedObject(newObjectKey, null);
        }

        public bool TryReadFieldHeader(int field)
        {
            uint num;
            ProtoBuf.WireType type;
            if ((this.blockEnd <= this.position) || (this.wireType == ProtoBuf.WireType.EndGroup))
            {
                return false;
            }
            int num2 = this.TryReadUInt32VariantWithoutMoving(false, out num);
            if (((num2 <= 0) || ((num >> 3) != field)) || ((type = ((ProtoBuf.WireType) num) & (ProtoBuf.WireType.Fixed32 | ProtoBuf.WireType.String)) == ProtoBuf.WireType.EndGroup))
            {
                return false;
            }
            this.wireType = type;
            this.fieldNumber = field;
            this.position += num2;
            this.ioIndex += num2;
            this.available -= num2;
            return true;
        }

        private bool TryReadUInt32Variant(out uint value)
        {
            int num = this.TryReadUInt32VariantWithoutMoving(false, out value);
            if (num <= 0)
            {
                return false;
            }
            this.ioIndex += num;
            this.available -= num;
            this.position += num;
            return true;
        }

        private static int TryReadUInt32Variant(Stream source, out uint value)
        {
            value = 0;
            int num = source.ReadByte();
            if (num < 0)
            {
                return 0;
            }
            value = (uint) num;
            if ((value & 0x80) == 0)
            {
                return 1;
            }
            value = (uint) (value & 0x7f);
            num = source.ReadByte();
            if (num < 0)
            {
                throw EoF(null);
            }
            value = (uint) (value | ((num & 0x7f) << 7));
            if ((num & 0x80) == 0)
            {
                return 2;
            }
            num = source.ReadByte();
            if (num < 0)
            {
                throw EoF(null);
            }
            value = (uint) (value | ((num & 0x7f) << 14));
            if ((num & 0x80) == 0)
            {
                return 3;
            }
            num = source.ReadByte();
            if (num < 0)
            {
                throw EoF(null);
            }
            value = (uint) (value | ((num & 0x7f) << 0x15));
            if ((num & 0x80) == 0)
            {
                return 4;
            }
            num = source.ReadByte();
            if (num < 0)
            {
                throw EoF(null);
            }
            value = (uint) (value | (num << 0x1c));
            if ((num & 240) != 0)
            {
                throw new OverflowException();
            }
            return 5;
        }

        internal int TryReadUInt32VariantWithoutMoving(bool trimNegative, out uint value)
        {
            if (this.available < 10)
            {
                this.Ensure(10, false);
            }
            if (this.available == 0)
            {
                value = 0;
                return 0;
            }
            int ioIndex = this.ioIndex;
            ioIndex++;
            value = this.ioBuffer[ioIndex];
            if ((value & 0x80) == 0)
            {
                return 1;
            }
            value = (uint) (value & 0x7f);
            if (this.available == 1)
            {
                throw EoF(this);
            }
            ioIndex++;
            uint num2 = this.ioBuffer[ioIndex];
            value = (uint) (value | ((num2 & 0x7f) << 7));
            if ((num2 & 0x80) == 0)
            {
                return 2;
            }
            if (this.available == 2)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value = (uint) (value | ((num2 & 0x7f) << 14));
            if ((num2 & 0x80) == 0)
            {
                return 3;
            }
            if (this.available == 3)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value = (uint) (value | ((num2 & 0x7f) << 0x15));
            if ((num2 & 0x80) == 0)
            {
                return 4;
            }
            if (this.available == 4)
            {
                throw EoF(this);
            }
            num2 = this.ioBuffer[ioIndex];
            value |= num2 << 0x1c;
            if ((num2 & 240) == 0)
            {
                return 5;
            }
            if ((!trimNegative || (((num2 & 240) != 240) || ((this.available < 10) || ((this.ioBuffer[++ioIndex] != 0xff) || ((this.ioBuffer[++ioIndex] != 0xff) || ((this.ioBuffer[++ioIndex] != 0xff) || (this.ioBuffer[++ioIndex] != 0xff))))))) || (this.ioBuffer[++ioIndex] != 1))
            {
                throw AddErrorData(new OverflowException(), this);
            }
            return 10;
        }

        private int TryReadUInt64VariantWithoutMoving(out ulong value)
        {
            if (this.available < 10)
            {
                this.Ensure(10, false);
            }
            if (this.available == 0)
            {
                value = 0L;
                return 0;
            }
            int ioIndex = this.ioIndex;
            ioIndex++;
            value = this.ioBuffer[ioIndex];
            if ((value & 0x80L) == 0)
            {
                return 1;
            }
            value &= 0x7f;
            if (this.available == 1)
            {
                throw EoF(this);
            }
            ioIndex++;
            ulong num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 7;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 2;
            }
            if (this.available == 2)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 14;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 3;
            }
            if (this.available == 3)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 0x15;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 4;
            }
            if (this.available == 4)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 0x1c;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 5;
            }
            if (this.available == 5)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 0x23;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 6;
            }
            if (this.available == 6)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 0x2a;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 7;
            }
            if (this.available == 7)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 0x31;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 8;
            }
            if (this.available == 8)
            {
                throw EoF(this);
            }
            ioIndex++;
            num2 = this.ioBuffer[ioIndex];
            value |= (num2 & 0x7f) << 0x38;
            if ((num2 & ((ulong) 0x80L)) == 0)
            {
                return 9;
            }
            if (this.available == 9)
            {
                throw EoF(this);
            }
            num2 = this.ioBuffer[ioIndex];
            value |= num2 << 0x3f;
            if ((num2 & -2) != 0)
            {
                throw AddErrorData(new OverflowException(), this);
            }
            return 10;
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

        public int FieldNumber =>
            this.fieldNumber;

        public ProtoBuf.WireType WireType =>
            this.wireType;

        public bool InternStrings
        {
            get => 
                this.internStrings;
            set => 
                (this.internStrings = value);
        }

        public SerializationContext Context =>
            this.context;

        public int Position =>
            this.position;

        public TypeModel Model =>
            this.model;

        internal NetObjectCache NetCache =>
            this.netCache;
    }
}

