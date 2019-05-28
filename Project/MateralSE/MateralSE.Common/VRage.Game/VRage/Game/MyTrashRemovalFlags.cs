namespace VRage.Game
{
    using System;

    [Flags]
    public enum MyTrashRemovalFlags
    {
        None = 0,
        Default = 0x1e1a,
        Fixed = 1,
        Stationary = 2,
        Linear = 8,
        Accelerating = 0x10,
        Powered = 0x20,
        Controlled = 0x40,
        WithProduction = 0x80,
        WithMedBay = 0x100,
        WithBlockCount = 0x200,
        DistanceFromPlayer = 0x400,
        RevertMaterials = 0x800,
        RevertAsteroids = 0x1000,
        RevertWithFloatingsPresent = 0x2000
    }
}

