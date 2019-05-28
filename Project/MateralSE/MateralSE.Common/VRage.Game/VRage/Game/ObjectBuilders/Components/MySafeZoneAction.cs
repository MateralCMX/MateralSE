namespace VRage.Game.ObjectBuilders.Components
{
    using System;

    [Flags]
    public enum MySafeZoneAction
    {
        Damage = 1,
        Shooting = 2,
        Drilling = 4,
        Welding = 8,
        Grinding = 0x10,
        VoxelHand = 0x20,
        Building = 0x40,
        All = 0x7f
    }
}

