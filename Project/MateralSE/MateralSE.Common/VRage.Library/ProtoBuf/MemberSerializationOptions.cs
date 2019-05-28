﻿namespace ProtoBuf
{
    using System;

    [Flags]
    public enum MemberSerializationOptions
    {
        None = 0,
        Packed = 1,
        Required = 2,
        AsReference = 4,
        DynamicType = 8,
        OverwriteList = 0x10,
        AsReferenceHasValue = 0x20
    }
}

