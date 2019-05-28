namespace Sandbox.Game.EntityComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyDefinitionType(typeof(MyObjectBuilder_TimerComponentDefinition), (Type) null)]
    public class MyTimerComponentDefinition : MyComponentDefinitionBase
    {
        public float TimeToRemoveMin;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_TimerComponentDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_TimerComponentDefinition;
            objectBuilder.TimeToRemoveMin = this.TimeToRemoveMin;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_TimerComponentDefinition definition = builder as MyObjectBuilder_TimerComponentDefinition;
            this.TimeToRemoveMin = definition.TimeToRemoveMin;
        }
    }
}

