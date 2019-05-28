namespace SpaceEngineers.Game.ModAPI
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMySolarPanel : Sandbox.ModAPI.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMySolarPanel
    {
        MyResourceSourceComponent SourceComp { get; set; }
    }
}

