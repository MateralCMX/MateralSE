namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_HandDrillDefinition), (Type) null)]
    public class MyHandDrillDefinition : MyEngineerToolBaseDefinition
    {
        public float HarvestRatioMultiplier;
        public Vector3D ParticleOffset;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_HandDrillDefinition objectBuilder = (MyObjectBuilder_HandDrillDefinition) base.GetObjectBuilder();
            objectBuilder.HarvestRatioMultiplier = this.HarvestRatioMultiplier;
            objectBuilder.ParticleOffset = this.ParticleOffset;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_HandDrillDefinition definition = builder as MyObjectBuilder_HandDrillDefinition;
            this.HarvestRatioMultiplier = definition.HarvestRatioMultiplier;
            Vector3D particleOffset = definition.ParticleOffset;
            this.ParticleOffset = definition.ParticleOffset;
        }
    }
}

