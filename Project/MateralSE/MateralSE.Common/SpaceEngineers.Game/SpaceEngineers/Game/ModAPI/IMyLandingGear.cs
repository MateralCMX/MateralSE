namespace SpaceEngineers.Game.ModAPI
{
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyLandingGear : Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear
    {
        event Action<SpaceEngineers.Game.ModAPI.IMyLandingGear, LandingGearMode> LockModeChanged;

        [Obsolete("Use LockModeChanged instead.")]
        event Action<bool> StateChanged;

        VRage.ModAPI.IMyEntity GetAttachedEntity();
    }
}

