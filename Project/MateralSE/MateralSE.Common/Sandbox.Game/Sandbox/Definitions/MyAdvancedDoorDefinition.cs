namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_AdvancedDoorDefinition), (Type) null)]
    public class MyAdvancedDoorDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float PowerConsumptionIdle;
        public float PowerConsumptionMoving;
        public MyObjectBuilder_AdvancedDoorDefinition.SubpartDefinition[] Subparts;
        public MyObjectBuilder_AdvancedDoorDefinition.Opening[] OpeningSequence;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AdvancedDoorDefinition definition = builder as MyObjectBuilder_AdvancedDoorDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.PowerConsumptionIdle = definition.PowerConsumptionIdle;
            this.PowerConsumptionMoving = definition.PowerConsumptionMoving;
            this.Subparts = definition.Subparts;
            this.OpeningSequence = definition.OpeningSequence;
        }
    }
}

