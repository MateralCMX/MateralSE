namespace Sandbox.Game.Weapons
{
    using System;

    [Flags]
    public enum MyTurretTargetFlags : ushort
    {
        Players = 1,
        SmallShips = 2,
        LargeShips = 4,
        Stations = 8,
        Asteroids = 0x10,
        Missiles = 0x20,
        Moving = 0x40,
        NotNeutrals = 0x80
    }
}

