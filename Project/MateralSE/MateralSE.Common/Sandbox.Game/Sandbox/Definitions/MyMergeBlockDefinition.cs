namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_MergeBlockDefinition), (Type) null)]
    public class MyMergeBlockDefinition : MyCubeBlockDefinition
    {
        public float Strength;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MergeBlockDefinition definition = builder as MyObjectBuilder_MergeBlockDefinition;
            this.Strength = definition.Strength;
        }
    }
}

