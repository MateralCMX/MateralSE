namespace Sandbox.Game.AI.Logic
{
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.Navigation;
    using System;
    using VRageMath;

    public class MyAnimalBotLogic : MyAgentLogic
    {
        private MyCharacterAvoidance m_characterAvoidance;

        public MyAnimalBotLogic(MyAnimalBot bot) : base(bot)
        {
            MyBotNavigation navigation = this.AnimalBot.Navigation;
            navigation.AddSteering(new MyTreeAvoidance(navigation, 0.1f));
            this.m_characterAvoidance = new MyCharacterAvoidance(navigation, 1f);
            navigation.AddSteering(this.m_characterAvoidance);
            navigation.MaximumRotationAngle = new float?(MathHelper.ToRadians((float) 23f));
        }

        public void EnableCharacterAvoidance(bool isTrue)
        {
            MyBotNavigation navigation = this.AnimalBot.Navigation;
            bool flag = navigation.HasSteeringOfType(this.m_characterAvoidance.GetType());
            if (isTrue && !flag)
            {
                navigation.AddSteering(this.m_characterAvoidance);
            }
            else if (!isTrue & flag)
            {
                navigation.RemoveSteering(this.m_characterAvoidance);
            }
        }

        public MyAnimalBot AnimalBot =>
            (base.m_bot as MyAnimalBot);

        public override Sandbox.Game.AI.Logic.BotType BotType =>
            Sandbox.Game.AI.Logic.BotType.ANIMAL;
    }
}

