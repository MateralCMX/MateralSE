namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_PistonTop))]
    public class MyPistonTop : MyAttachableTopBlockBase, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyPistonTop, Sandbox.ModAPI.IMyAttachableTopBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyAttachableTopBlock, Sandbox.ModAPI.Ingame.IMyPistonTop
    {
        private MyPistonBase m_pistonBlock;
        private MyAttachableConveyorEndpoint m_conveyorEndpoint;

        public bool AllowSelfPulling() => 
            false;

        public override void Attach(MyMechanicalConnectionBlockBase pistonBase)
        {
            base.Attach(pistonBase);
            this.m_pistonBlock = pistonBase as MyPistonBase;
        }

        public override void ContactPointCallback(ref MyGridContactInfo value)
        {
            base.ContactPointCallback(ref value);
            if ((this.m_pistonBlock != null) && ReferenceEquals(value.CollidingEntity, this.m_pistonBlock.Subpart3))
            {
                value.EnableDeformation = false;
                value.EnableParticles = false;
            }
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyAttachableConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        bool Sandbox.ModAPI.Ingame.IMyAttachableTopBlock.IsAttached =>
            (this.m_pistonBlock != null);

        Sandbox.ModAPI.IMyMechanicalConnectionBlock Sandbox.ModAPI.IMyAttachableTopBlock.Base =>
            this.m_pistonBlock;

        Sandbox.ModAPI.IMyPistonBase Sandbox.ModAPI.IMyPistonTop.Base =>
            this.m_pistonBlock;

        Sandbox.ModAPI.IMyPistonBase Sandbox.ModAPI.IMyPistonTop.Piston =>
            this.m_pistonBlock;
    }
}

