namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BehaviorTreeDefinition), (Type) null)]
    public class MyBehaviorDefinition : MyDefinitionBase
    {
        public MyObjectBuilder_BehaviorTreeNode FirstNode;
        public string Behavior;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BehaviorTreeDefinition definition = (MyObjectBuilder_BehaviorTreeDefinition) builder;
            this.FirstNode = definition.FirstNode;
            this.Behavior = definition.Behavior;
        }
    }
}

