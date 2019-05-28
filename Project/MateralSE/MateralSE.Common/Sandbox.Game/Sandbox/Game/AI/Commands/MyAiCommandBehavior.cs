namespace Sandbox.Game.AI.Commands
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRageMath;

    public class MyAiCommandBehavior : IMyAiCommand
    {
        private static List<MyPhysics.HitInfo> m_tmpHitInfos = new List<MyPhysics.HitInfo>();

        public void ActivateCommand()
        {
            if (this.Definition.CommandEffect == MyAiCommandEffect.TARGET)
            {
                this.ChangeTarget();
            }
            else if (this.Definition.CommandEffect == MyAiCommandEffect.OWNED_BOTS)
            {
                this.ChangeAllBehaviors();
            }
        }

        private void ChangeAllBehaviors()
        {
            foreach (KeyValuePair<int, IMyBot> pair in MyAIComponent.Static.Bots.GetAllBots())
            {
                MyAgentBot bot = pair.Value as MyAgentBot;
                if ((bot != null) && bot.BotDefinition.Commandable)
                {
                    this.ChangeBotBehavior(bot);
                }
            }
        }

        private void ChangeBotBehavior(MyAgentBot bot)
        {
            MyAIComponent.Static.BehaviorTrees.ChangeBehaviorTree(this.Definition.BehaviorTreeName, bot);
        }

        private void ChangeTarget()
        {
            Vector3D position;
            Vector3D forward;
            if ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity))
            {
                position = MySector.MainCamera.Position;
                forward = MySector.MainCamera.WorldMatrix.Forward;
            }
            else
            {
                MatrixD xd = MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
                position = xd.Translation;
                forward = xd.Forward;
            }
            m_tmpHitInfos.Clear();
            MyPhysics.CastRay(position, position + (forward * 20.0), m_tmpHitInfos, 0x18);
            if (m_tmpHitInfos.Count != 0)
            {
                using (List<MyPhysics.HitInfo>.Enumerator enumerator = m_tmpHitInfos.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyAgentBot bot;
                        MyCharacter hitEntity = enumerator.Current.HkHitInfo.GetHitEntity() as MyCharacter;
                        if ((hitEntity != null) && (this.TryGetBotForCharacter(hitEntity, out bot) && bot.BotDefinition.Commandable))
                        {
                            this.ChangeBotBehavior(bot);
                        }
                    }
                }
            }
        }

        public void InitCommand(MyAiCommandDefinition definition)
        {
            this.Definition = definition as MyAiCommandBehaviorDefinition;
        }

        private bool TryGetBotForCharacter(MyCharacter character, out MyAgentBot bot)
        {
            bot = null;
            using (Dictionary<int, IMyBot>.Enumerator enumerator = MyAIComponent.Static.Bots.GetAllBots().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<int, IMyBot> current = enumerator.Current;
                    MyAgentBot bot2 = current.Value as MyAgentBot;
                    if ((bot2 != null) && ReferenceEquals(bot2.AgentEntity, character))
                    {
                        bot = bot2;
                        return true;
                    }
                }
            }
            return false;
        }

        public MyAiCommandBehaviorDefinition Definition { get; private set; }
    }
}

