namespace VRage.Game.ModAPI
{
    using System;

    [Flags]
    public enum SpawningOptions
    {
        None = 0,
        RotateFirstCockpitTowardsDirection = 2,
        SpawnRandomCargo = 4,
        DisableDampeners = 8,
        SetNeutralOwner = 0x10,
        TurnOffReactors = 0x20,
        DisableSave = 0x40,
        UseGridOrigin = 0x80,
        SetAuthorship = 0x100
    }
}

