namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_AirVentDefinition), (Type) null)]
    public class MyAirVentDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public MyStringHash ResourceSourceGroup;
        public float StandbyPowerConsumption;
        public float OperationalPowerConsumption;
        public float VentilationCapacityPerSecond;
        public MySoundPair PressurizeSound;
        public MySoundPair DepressurizeSound;
        public MySoundPair IdleSound;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AirVentDefinition definition = builder as MyObjectBuilder_AirVentDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.ResourceSourceGroup = MyStringHash.GetOrCompute(definition.ResourceSourceGroup);
            this.StandbyPowerConsumption = definition.StandbyPowerConsumption;
            this.OperationalPowerConsumption = definition.OperationalPowerConsumption;
            this.VentilationCapacityPerSecond = definition.VentilationCapacityPerSecond;
            this.PressurizeSound = new MySoundPair(definition.PressurizeSound, true);
            this.DepressurizeSound = new MySoundPair(definition.DepressurizeSound, true);
            this.IdleSound = new MySoundPair(definition.IdleSound, true);
        }
    }
}

