namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_GasProperties), (Type) null)]
    public class MyGasProperties : MyDefinitionBase
    {
        public float EnergyDensity;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_GasProperties objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_GasProperties;
            objectBuilder.EnergyDensity = this.EnergyDensity;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GasProperties properties = builder as MyObjectBuilder_GasProperties;
            this.EnergyDensity = properties.EnergyDensity;
        }
    }
}

