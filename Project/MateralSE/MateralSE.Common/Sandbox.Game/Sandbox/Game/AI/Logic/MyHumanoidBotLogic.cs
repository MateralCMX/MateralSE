namespace Sandbox.Game.AI.Logic
{
    using Sandbox.Game.AI;
    using System;

    public abstract class MyHumanoidBotLogic : MyAgentLogic
    {
        public MyReservationStatus ReservationStatus;
        public MyAiTargetManager.ReservedEntityData ReservationEntityData;
        public MyAiTargetManager.ReservedAreaData ReservationAreaData;

        public MyHumanoidBotLogic(IMyBot bot) : base(bot)
        {
        }

        public MyHumanoidBot HumanoidBot =>
            (base.m_bot as MyHumanoidBot);

        public override Sandbox.Game.AI.Logic.BotType BotType =>
            Sandbox.Game.AI.Logic.BotType.HUMANOID;
    }
}

