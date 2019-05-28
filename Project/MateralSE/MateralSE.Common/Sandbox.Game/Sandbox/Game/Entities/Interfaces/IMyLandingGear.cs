namespace Sandbox.Game.Entities.Interfaces
{
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.ModAPI;

    public interface IMyLandingGear
    {
        event LockModeChangedHandler LockModeChanged;

        IMyEntity GetAttachedEntity();
        void RequestLock(bool enable);
        void ResetAutolock();

        bool AutoLock { get; }

        LandingGearMode LockMode { get; }
    }
}

