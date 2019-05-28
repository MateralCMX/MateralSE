namespace SpaceEngineers.Game.Definitions
{
    using Sandbox.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_DemoComponentDefinition), (Type) null)]
    public class MyDemoComponentDefinition : MySessionComponentDefinition
    {
        public float Float;
        public int Int;
        public string String;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_DemoComponentDefinition definition = (MyObjectBuilder_DemoComponentDefinition) builder;
            this.Float = definition.Float;
            this.Int = definition.Int;
            this.String = definition.String;
        }
    }
}

