namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_ExtendedPistonBase)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyExtendedPistonBase), typeof(Sandbox.ModAPI.Ingame.IMyExtendedPistonBase) })]
    public class MyExtendedPistonBase : MyPistonBase, Sandbox.ModAPI.IMyExtendedPistonBase, Sandbox.ModAPI.IMyPistonBase, Sandbox.ModAPI.IMyMechanicalConnectionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock, Sandbox.ModAPI.Ingame.IMyPistonBase, Sandbox.ModAPI.Ingame.IMyExtendedPistonBase
    {
    }
}

