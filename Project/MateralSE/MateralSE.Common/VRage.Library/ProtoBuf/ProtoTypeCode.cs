namespace ProtoBuf
{
    using System;

    internal enum ProtoTypeCode
    {
        Empty = 0,
        Unknown = 1,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 0x10,
        String = 0x12,
        TimeSpan = 100,
        ByteArray = 0x65,
        Guid = 0x66,
        Uri = 0x67,
        Type = 0x68
    }
}

