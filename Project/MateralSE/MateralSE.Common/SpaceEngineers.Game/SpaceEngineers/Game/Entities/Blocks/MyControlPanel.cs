namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_ControlPanel)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyControlPanel), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyControlPanel) })]
    public class MyControlPanel : MyTerminalBlock, SpaceEngineers.Game.ModAPI.IMyControlPanel, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, SpaceEngineers.Game.ModAPI.Ingame.IMyControlPanel
    {
    }
}

