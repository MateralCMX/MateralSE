namespace Sandbox.Game.Entities.Interfaces
{
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;

    public delegate void LockModeChangedHandler(IMyLandingGear gear, LandingGearMode oldMode);
}

