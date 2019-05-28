namespace Sandbox.ModAPI
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyReactor : Sandbox.ModAPI.Ingame.IMyReactor, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, IMyEntity, Sandbox.ModAPI.IMyPowerProducer
    {
        float PowerOutputMultiplier { get; set; }
    }
}

