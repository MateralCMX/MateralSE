namespace Sandbox.Game.AI.Logic
{
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using System;

    public abstract class MyBotLogic
    {
        protected IMyBot m_bot;

        public MyBotLogic(IMyBot bot)
        {
            this.m_bot = bot;
        }

        public virtual void Cleanup()
        {
        }

        public virtual void DebugDraw()
        {
        }

        public virtual void Init()
        {
        }

        public virtual void OnControlledEntityChanged(IMyControllableEntity newEntity)
        {
        }

        public virtual void Update()
        {
        }

        public abstract Sandbox.Game.AI.Logic.BotType BotType { get; }
    }
}

