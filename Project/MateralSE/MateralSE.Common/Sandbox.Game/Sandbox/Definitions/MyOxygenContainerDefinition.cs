namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_OxygenContainerDefinition), (Type) null)]
    public class MyOxygenContainerDefinition : MyPhysicalItemDefinition
    {
        public float Capacity;
        public MyDefinitionId StoredGasId;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyDefinitionId storedGasId;
            base.Init(builder);
            MyObjectBuilder_OxygenContainerDefinition definition = builder as MyObjectBuilder_OxygenContainerDefinition;
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
        }
    }
}

