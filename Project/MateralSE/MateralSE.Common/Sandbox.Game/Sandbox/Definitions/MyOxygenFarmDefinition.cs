namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_OxygenFarmDefinition), (Type) null)]
    public class MyOxygenFarmDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public MyStringHash ResourceSourceGroup;
        public Vector3 PanelOrientation;
        public bool IsTwoSided;
        public float PanelOffset;
        public MyDefinitionId ProducedGas;
        public float MaxGasOutput;
        public float OperationalPowerConsumption;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyDefinitionId id;
            base.Init(builder);
            MyObjectBuilder_OxygenFarmDefinition definition = builder as MyObjectBuilder_OxygenFarmDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.ResourceSourceGroup = MyStringHash.GetOrCompute(definition.ResourceSourceGroup);
            this.PanelOrientation = definition.PanelOrientation;
            this.IsTwoSided = definition.TwoSidedPanel;
            this.PanelOffset = definition.PanelOffset;
            if (definition.ProducedGas.Id.IsNull())
            {
                id = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
            }
            else
            {
                id = definition.ProducedGas.Id;
            }
            this.ProducedGas = id;
            this.MaxGasOutput = definition.ProducedGas.MaxOutputPerSecond;
            this.OperationalPowerConsumption = definition.OperationalPowerConsumption;
        }
    }
}

