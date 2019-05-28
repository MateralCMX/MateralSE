namespace Sandbox.Game.AI.Logic
{
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Runtime.CompilerServices;

    public abstract class MyAgentLogic : MyBotLogic
    {
        protected IMyEntityBot m_entityBot;

        public MyAgentLogic(IMyBot bot) : base(bot)
        {
            this.m_entityBot = base.m_bot as IMyEntityBot;
            this.AiTarget = MyAIComponent.BotFactory.CreateTargetForBot(this.AgentBot);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            this.AiTarget.Cleanup();
        }

        public override void Init()
        {
            base.Init();
            this.AiTarget = this.AgentBot.AgentActions.AiTargetBase;
        }

        public virtual void OnCharacterControlAcquired(MyCharacter character)
        {
        }

        public override void Update()
        {
            base.Update();
            this.AiTarget.Update();
        }

        public MyAgentBot AgentBot =>
            (base.m_bot as MyAgentBot);

        public MyAiTargetBase AiTarget { get; private set; }

        public override Sandbox.Game.AI.Logic.BotType BotType =>
            Sandbox.Game.AI.Logic.BotType.UNKNOWN;
    }
}

