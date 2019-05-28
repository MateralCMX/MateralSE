namespace Sandbox.ModAPI.Ingame
{
    using System;

    [Flags]
    public enum UpdateType
    {
        None = 0,
        Terminal = 1,
        Trigger = 2,
        Antenna = 4,
        Mod = 8,
        Script = 0x10,
        Update1 = 0x20,
        Update10 = 0x40,
        Update100 = 0x80,
        Once = 0x100,
        IGC = 0x200
    }
}

