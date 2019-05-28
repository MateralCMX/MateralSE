namespace VRage.Game
{
    using System;

    [Flags]
    public enum MyMemoryParameterType : byte
    {
        IN = 1,
        OUT = 2,
        IN_OUT = 3,
        PARAMETER = 4
    }
}

