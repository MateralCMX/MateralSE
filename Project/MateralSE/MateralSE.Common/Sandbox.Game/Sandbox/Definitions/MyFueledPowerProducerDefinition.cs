namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_FueledPowerProducerDefinition), (Type) null)]
    public class MyFueledPowerProducerDefinition : MyPowerProducerDefinition
    {
        public float FuelProductionToCapacityMultiplier;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyObjectBuilder_FueledPowerProducerDefinition definition = (MyObjectBuilder_FueledPowerProducerDefinition) builder;
            this.FuelProductionToCapacityMultiplier = definition.FuelProductionToCapacityMultiplier;
            base.Init(builder);
        }
    }
}

