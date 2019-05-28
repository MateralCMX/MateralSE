namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ResourceDistributionGroup), (Type) null)]
    public class MyResourceDistributionGroupDefinition : MyDefinitionBase
    {
        public int Priority;
        public bool IsSource;
        public bool IsAdaptible;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ResourceDistributionGroup group = builder as MyObjectBuilder_ResourceDistributionGroup;
            this.IsSource = group.IsSource;
            this.Priority = group.Priority;
            this.IsAdaptible = group.IsAdaptible;
        }
    }
}

