namespace Sandbox.ModAPI.Ingame
{
    using System;

    [Flags]
    public enum MyTransmitTarget
    {
        None = 0,
        Owned = 1,
        Ally = 2,
        Neutral = 4,
        Enemy = 8,
        Everyone = 15,
        Default = 3
    }
}

