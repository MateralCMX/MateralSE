namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipToolDefinition), (Type) null)]
    public class MyShipToolDefinition : MyCubeBlockDefinition
    {
        public string Flare;
        public float SensorRadius;
        public float SensorOffset;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipToolDefinition definition = builder as MyObjectBuilder_ShipToolDefinition;
            this.SensorRadius = definition.SensorRadius;
            this.SensorOffset = definition.SensorOffset;
            this.Flare = definition.Flare;
        }
    }
}

