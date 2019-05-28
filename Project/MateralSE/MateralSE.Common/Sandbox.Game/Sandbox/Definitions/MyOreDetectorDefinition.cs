namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_OreDetectorDefinition), (Type) null)]
    public class MyOreDetectorDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float MaximumRange;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_OreDetectorDefinition definition = builder as MyObjectBuilder_OreDetectorDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.MaximumRange = definition.MaximumRange;
        }
    }
}

