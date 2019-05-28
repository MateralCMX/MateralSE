namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities.Character;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BotDefinition), (Type) null)]
    public class MyBotDefinition : MyDefinitionBase
    {
        public MyDefinitionId BotBehaviorTree;
        public string BehaviorType;
        public string BehaviorSubtype;
        public MyDefinitionId TypeDefinitionId;
        public bool Commandable;

        public virtual void AddItems(MyCharacter character)
        {
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BotDefinition definition = builder as MyObjectBuilder_BotDefinition;
            this.BotBehaviorTree = new MyDefinitionId(definition.BotBehaviorTree.Type, definition.BotBehaviorTree.Subtype);
            this.BehaviorType = definition.BehaviorType;
            this.TypeDefinitionId = new MyDefinitionId(definition.TypeId, definition.SubtypeName);
            this.BehaviorSubtype = !string.IsNullOrWhiteSpace(definition.BehaviorSubtype) ? definition.BehaviorSubtype : definition.BehaviorType;
            this.Commandable = definition.Commandable;
        }
    }
}

