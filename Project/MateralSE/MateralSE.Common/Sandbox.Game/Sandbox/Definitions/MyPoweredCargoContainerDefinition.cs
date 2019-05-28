namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_PoweredCargoContainerDefinition), (Type) null)]
    public class MyPoweredCargoContainerDefinition : MyCargoContainerDefinition
    {
        public string ResourceSinkGroup;
        public float RequiredPowerInput;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PoweredCargoContainerDefinition definition = builder as MyObjectBuilder_PoweredCargoContainerDefinition;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.RequiredPowerInput = definition.RequiredPowerInput;
        }
    }
}

