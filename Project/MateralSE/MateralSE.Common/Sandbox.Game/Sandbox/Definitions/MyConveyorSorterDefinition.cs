namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ConveyorSorterDefinition), (Type) null)]
    public class MyConveyorSorterDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float PowerInput;
        public Vector3 InventorySize;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ConveyorSorterDefinition definition = (MyObjectBuilder_ConveyorSorterDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.PowerInput = definition.PowerInput;
            this.InventorySize = definition.InventorySize;
        }
    }
}

