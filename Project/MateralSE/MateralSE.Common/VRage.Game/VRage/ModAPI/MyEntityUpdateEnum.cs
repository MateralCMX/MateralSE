namespace VRage.ModAPI
{
    using System;

    [Flags]
    public enum MyEntityUpdateEnum
    {
        NONE = 0,
        EACH_FRAME = 1,
        EACH_10TH_FRAME = 2,
        EACH_100TH_FRAME = 4,
        BEFORE_NEXT_FRAME = 8,
        SIMULATE = 0x10
    }
}

