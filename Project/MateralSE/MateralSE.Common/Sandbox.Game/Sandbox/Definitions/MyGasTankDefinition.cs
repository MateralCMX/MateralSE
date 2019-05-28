namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_GasTankDefinition), (Type) null)]
    public class MyGasTankDefinition : MyProductionBlockDefinition
    {
        public float Capacity;
        public MyDefinitionId StoredGasId;
        public MyStringHash ResourceSourceGroup;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyDefinitionId storedGasId;
            base.Init(builder);
            MyObjectBuilder_GasTankDefinition definition = builder as MyObjectBuilder_GasTankDefinition;
            this.Capacity = definition.Capacity;
            if (definition.StoredGasId.IsNull())
            {
                storedGasId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
            }
            else
            {
                storedGasId = definition.StoredGasId;
            }
            this.StoredGasId = storedGasId;
            this.ResourceSourceGroup = MyStringHash.GetOrCompute(definition.ResourceSourceGroup);
        }
    }
}

