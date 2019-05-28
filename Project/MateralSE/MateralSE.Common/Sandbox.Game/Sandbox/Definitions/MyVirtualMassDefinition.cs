namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_VirtualMassDefinition), (Type) null)]
    public class MyVirtualMassDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public float VirtualMass;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_VirtualMassDefinition definition = builder as MyObjectBuilder_VirtualMassDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.VirtualMass = definition.VirtualMass;
        }
    }
}

