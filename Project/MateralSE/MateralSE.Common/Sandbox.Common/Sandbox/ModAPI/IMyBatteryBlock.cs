namespace Sandbox.ModAPI
{
    using Sandbox.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyBatteryBlock : Sandbox.ModAPI.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, IMyEntity, Sandbox.ModAPI.Ingame.IMyBatteryBlock
    {
    }
}

