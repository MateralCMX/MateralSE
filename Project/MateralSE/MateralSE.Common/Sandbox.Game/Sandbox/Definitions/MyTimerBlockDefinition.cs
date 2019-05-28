namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_TimerBlockDefinition), (Type) null)]
    public class MyTimerBlockDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public string TimerSoundStart;
        public string TimerSoundMid;
        public string TimerSoundEnd;
        public int MinDelay;
        public int MaxDelay;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_TimerBlockDefinition definition = (MyObjectBuilder_TimerBlockDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.TimerSoundStart = definition.TimerSoundStart;
            this.TimerSoundMid = definition.TimerSoundMid;
            this.TimerSoundEnd = definition.TimerSoundEnd;
            this.MinDelay = definition.MinDelay;
            this.MaxDelay = definition.MaxDelay;
        }
    }
}

