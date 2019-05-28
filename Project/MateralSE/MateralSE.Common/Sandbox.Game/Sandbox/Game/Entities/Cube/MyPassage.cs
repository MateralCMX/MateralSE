namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_Passage))]
    public class MyPassage : MyCubeBlock, Sandbox.ModAPI.IMyPassage, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyPassage
    {
        public override bool GetIntersectionWithSphere(ref BoundingSphereD sphere) => 
            false;
    }
}

