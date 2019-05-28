namespace VRage.Serialization
{
    using System;

    public enum MyPrimitiveFlags
    {
        None = 0,
        Signed = 1,
        Normalized = 2,
        Variant = 4,
        VariantSigned = 5,
        Ascii = 8,
        Utf8 = 0x10,
        FixedPoint8 = 0x20,
        FixedPoint16 = 0x40
    }
}

