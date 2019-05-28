namespace Sandbox.Game.Entities.Blocks
{
    using System;

    [Flags]
    public enum MySensorFilterFlags : ushort
    {
        Players = 1,
        FloatingObjects = 2,
        SmallShips = 4,
        LargeShips = 8,
        Stations = 0x10,
        Asteroids = 0x20,
        Subgrids = 0x40,
        Owner = 0x100,
        Friendly = 0x200,
        Neutral = 0x400,
        Enemy = 0x800
    }
}

