namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_Decoy)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyDecoy), typeof(Sandbox.ModAPI.Ingame.IMyDecoy) })]
    public class MyDecoy : MyFunctionalBlock, Sandbox.ModAPI.IMyDecoy, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyDecoy
    {
        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.CubeGrid.RegisterDecoy(this);
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            base.CubeGrid.UnregisterDecoy(this);
        }
    }
}

