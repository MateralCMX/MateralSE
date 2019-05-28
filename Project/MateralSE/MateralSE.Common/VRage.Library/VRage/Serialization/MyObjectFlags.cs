namespace VRage.Serialization
{
    using System;

    [Flags]
    public enum MyObjectFlags
    {
        None = 0,
        DefaultZero = 1,
        Nullable = 1,
        Dynamic = 2,
        DefaultValueOrEmpty = 4,
        DynamicDefault = 8
    }
}

