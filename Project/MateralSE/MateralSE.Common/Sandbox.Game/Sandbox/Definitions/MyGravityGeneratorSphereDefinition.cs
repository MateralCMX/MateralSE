namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_GravityGeneratorSphereDefinition), (Type) null)]
    public class MyGravityGeneratorSphereDefinition : MyGravityGeneratorBaseDefinition
    {
        public float MinRadius;
        public float MaxRadius;
        public float BasePowerInput;
        public float ConsumptionPower;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GravityGeneratorSphereDefinition definition = builder as MyObjectBuilder_GravityGeneratorSphereDefinition;
            this.MinRadius = definition.MinRadius;
            this.MaxRadius = definition.MaxRadius;
            this.BasePowerInput = definition.BasePowerInput;
            this.ConsumptionPower = definition.ConsumptionPower;
        }
    }
}

