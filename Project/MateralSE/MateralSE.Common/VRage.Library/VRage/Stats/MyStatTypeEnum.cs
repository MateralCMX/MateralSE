namespace VRage.Stats
{
    using System;

    public enum MyStatTypeEnum : byte
    {
        Unset = 0,
        CurrentValue = 1,
        Min = 2,
        Max = 3,
        Avg = 4,
        MinMax = 5,
        MinMaxAvg = 6,
        Sum = 7,
        Counter = 8,
        CounterSum = 9,
        DontDisappearFlag = 0x10,
        KeepInactiveLongerFlag = 0x20,
        LongFlag = 0x40,
        FormatFlag = 0x80,
        AllFlags = 240
    }
}

