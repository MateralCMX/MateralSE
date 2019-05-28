namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_HydrogenEngineDefinition), (Type) null)]
    public class MyHydrogenEngineDefinition : MyGasFueledPowerProducerDefinition
    {
        public float AnimationSpeed;
        public float PistonAnimationMin;
        public float PistonAnimationMax;
        public float AnimationSpinUpSpeed;
        public float AnimationSpinDownSpeed;
        public float[] PistonAnimationOffsets;
        public float AnimationVisibilityDistanceSq;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_HydrogenEngineDefinition definition = (MyObjectBuilder_HydrogenEngineDefinition) builder;
            this.AnimationSpeed = definition.AnimationSpeed;
            this.PistonAnimationMin = definition.PistonAnimationMin;
            this.PistonAnimationMax = definition.PistonAnimationMax;
            this.AnimationSpinUpSpeed = definition.AnimationSpinUpSpeed;
            this.AnimationSpinDownSpeed = definition.AnimationSpinDownSpeed;
            this.PistonAnimationOffsets = definition.PistonAnimationOffsets;
            this.AnimationVisibilityDistanceSq = definition.AnimationVisibilityDistance * definition.AnimationVisibilityDistance;
        }
    }
}

