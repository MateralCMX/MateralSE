namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AgentDefinition), (Type) null)]
    public class MyAgentDefinition : MyBotDefinition
    {
        public string BotModel;
        public string TargetType;
        public bool InventoryContentGenerated;
        public MyDefinitionId InventoryContainerTypeId;
        public bool RemoveAfterDeath;
        public int RespawnTimeMs;
        public int RemoveTimeMs;
        public string FactionTag;

        public override void AddItems(MyCharacter character)
        {
            character.GetInventory(0).Clear(true);
            if (this.InventoryContentGenerated)
            {
                MyContainerTypeDefinition containerTypeDefinition = MyDefinitionManager.Static.GetContainerTypeDefinition(this.InventoryContainerTypeId.SubtypeName);
                if (containerTypeDefinition != null)
                {
                    character.GetInventory(0).GenerateContent(containerTypeDefinition);
                }
            }
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AgentDefinition definition = builder as MyObjectBuilder_AgentDefinition;
            this.BotModel = definition.BotModel;
            this.TargetType = definition.TargetType;
            this.InventoryContentGenerated = definition.InventoryContentGenerated;
            if (definition.InventoryContainerTypeId != null)
            {
                this.InventoryContainerTypeId = definition.InventoryContainerTypeId.Value;
            }
            this.RemoveAfterDeath = definition.RemoveAfterDeath;
            this.RespawnTimeMs = definition.RespawnTimeMs;
            this.RemoveTimeMs = definition.RemoveTimeMs;
            this.FactionTag = definition.FactionTag;
        }
    }
}

