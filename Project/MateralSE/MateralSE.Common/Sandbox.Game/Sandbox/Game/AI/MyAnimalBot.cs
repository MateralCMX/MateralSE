namespace Sandbox.Game.AI
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.ObjectBuilders.AI.Bot;

    [MyBotType(typeof(MyObjectBuilder_AnimalBot))]
    public class MyAnimalBot : MyAgentBot
    {
        public MyAnimalBot(MyPlayer player, MyBotDefinition botDefinition) : base(player, botDefinition)
        {
        }

        public MyCharacter AnimalEntity =>
            base.AgentEntity;

        public MyAnimalBotDefinition AnimalDefinition =>
            (base.m_botDefinition as MyAnimalBotDefinition);
    }
}

