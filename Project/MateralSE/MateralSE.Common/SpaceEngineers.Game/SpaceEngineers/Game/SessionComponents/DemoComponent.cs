namespace SpaceEngineers.Game.SessionComponents
{
    using SpaceEngineers.Game.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Components.Session;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class DemoComponent : MySessionComponentBase
    {
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
        }

        public override void InitFromDefinition(MySessionComponentDefinition definition)
        {
            base.InitFromDefinition(definition);
            MyDemoComponentDefinition definition1 = (MyDemoComponentDefinition) definition;
        }

        public override bool IsRequiredByGame =>
            false;
    }
}

