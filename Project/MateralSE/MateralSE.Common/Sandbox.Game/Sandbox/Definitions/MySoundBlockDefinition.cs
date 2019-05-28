namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_SoundBlockDefinition), (Type) null)]
    public class MySoundBlockDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float MinRange;
        public float MaxRange;
        public float MaxLoopPeriod;
        public int EmitterNumber;
        public int LoopUpdateThreshold;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SoundBlockDefinition definition = (MyObjectBuilder_SoundBlockDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.MinRange = definition.MinRange;
            this.MaxRange = definition.MaxRange;
            this.MaxLoopPeriod = definition.MaxLoopPeriod;
            this.EmitterNumber = definition.EmitterNumber;
            this.LoopUpdateThreshold = definition.LoopUpdateThreshold;
        }
    }
}

