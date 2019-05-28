namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_CameraBlockDefinition), (Type) null)]
    public class MyCameraBlockDefinition : MyCubeBlockDefinition
    {
        public string ResourceSinkGroup;
        public float RequiredPowerInput;
        public float RequiredChargingInput;
        public string OverlayTexture;
        public float MinFov;
        public float MaxFov;
        public float RaycastConeLimit;
        public double RaycastDistanceLimit;
        public float RaycastTimeMultiplier;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CameraBlockDefinition definition = builder as MyObjectBuilder_CameraBlockDefinition;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.RequiredChargingInput = definition.RequiredChargingInput;
            this.OverlayTexture = definition.OverlayTexture;
            this.MinFov = definition.MinFov;
            this.MaxFov = definition.MaxFov;
            this.RaycastConeLimit = definition.RaycastConeLimit;
            this.RaycastDistanceLimit = definition.RaycastDistanceLimit;
            this.RaycastTimeMultiplier = definition.RaycastTimeMultiplier;
        }
    }
}

