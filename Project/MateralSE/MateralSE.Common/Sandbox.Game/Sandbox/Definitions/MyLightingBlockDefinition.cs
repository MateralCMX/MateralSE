namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_LightingBlockDefinition), (Type) null)]
    public class MyLightingBlockDefinition : MyCubeBlockDefinition
    {
        public MyBounds LightRadius;
        public MyBounds LightReflectorRadius;
        public MyBounds LightFalloff;
        public MyBounds LightIntensity;
        public MyBounds LightOffset;
        public MyBounds BlinkIntervalSeconds;
        public MyBounds BlinkLenght;
        public MyBounds BlinkOffset;
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public string Flare;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_LightingBlockDefinition definition = (MyObjectBuilder_LightingBlockDefinition) builder;
            this.BlinkIntervalSeconds = (MyBounds) definition.LightBlinkIntervalSeconds;
            this.BlinkLenght = (MyBounds) definition.LightBlinkLenght;
            this.BlinkOffset = (MyBounds) definition.LightBlinkOffset;
            this.LightRadius = (MyBounds) definition.LightRadius;
            this.LightReflectorRadius = (MyBounds) definition.LightReflectorRadius;
            this.LightFalloff = (MyBounds) definition.LightFalloff;
            this.LightIntensity = (MyBounds) definition.LightIntensity;
            this.LightOffset = (MyBounds) definition.LightOffset;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.Flare = definition.Flare;
        }
    }
}

