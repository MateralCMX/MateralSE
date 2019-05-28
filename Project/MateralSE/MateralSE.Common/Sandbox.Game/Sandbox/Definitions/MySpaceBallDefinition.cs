namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_SpaceBallDefinition), (Type) null)]
    public class MySpaceBallDefinition : MyCubeBlockDefinition
    {
        public float MaxVirtualMass;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SpaceBallDefinition definition = builder as MyObjectBuilder_SpaceBallDefinition;
            this.MaxVirtualMass = definition.MaxVirtualMass;
        }
    }
}

