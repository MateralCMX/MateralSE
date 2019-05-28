namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AiCommandBehaviorDefinition), (Type) null)]
    public class MyAiCommandBehaviorDefinition : MyAiCommandDefinition
    {
        public string BehaviorTreeName;
        public MyAiCommandEffect CommandEffect;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AiCommandBehaviorDefinition definition = builder as MyObjectBuilder_AiCommandBehaviorDefinition;
            this.BehaviorTreeName = definition.BehaviorTreeName;
            this.CommandEffect = definition.CommandEffect;
        }
    }
}

