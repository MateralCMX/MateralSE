namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Components;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_MotorAdvancedRotor))]
    public class MyMotorAdvancedRotor : MyMotorRotor, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyMotorAdvancedRotor, Sandbox.ModAPI.IMyMotorRotor, Sandbox.ModAPI.IMyAttachableTopBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyAttachableTopBlock, Sandbox.ModAPI.Ingame.IMyMotorRotor, Sandbox.ModAPI.Ingame.IMyMotorAdvancedRotor
    {
        private MyAttachableConveyorEndpoint m_conveyorEndpoint;

        public bool AllowSelfPulling() => 
            false;

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
    }
}

