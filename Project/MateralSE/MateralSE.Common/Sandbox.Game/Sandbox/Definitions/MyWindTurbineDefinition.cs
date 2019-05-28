namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_WindTurbineDefinition), (Type) null)]
    public class MyWindTurbineDefinition : MyPowerProducerDefinition
    {
        public int RaycasterSize;
        public int RaycastersCount;
        public float MinRaycasterClearance;
        public float OptimalGroundClearance;
        public float RaycastersToFullEfficiency;
        public float OptimalWindSpeed;
        public float TurbineSpinUpSpeed;
        public float TurbineSpinDownSpeed;
        public float TurbineRotationSpeed;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WindTurbineDefinition definition = (MyObjectBuilder_WindTurbineDefinition) builder;
            this.RaycasterSize = definition.RaycasterSize;
            this.RaycastersCount = definition.RaycastersCount;
            this.MinRaycasterClearance = definition.MinRaycasterClearance;
            this.RaycastersToFullEfficiency = definition.RaycastersToFullEfficiency;
            this.OptimalWindSpeed = definition.OptimalWindSpeed;
            this.TurbineSpinUpSpeed = definition.TurbineSpinUpSpeed;
            this.TurbineSpinDownSpeed = definition.TurbineSpinDownSpeed;
            this.TurbineRotationSpeed = definition.TurbineRotationSpeed;
            this.OptimalGroundClearance = definition.OptimalGroundClearance;
        }
    }
}

