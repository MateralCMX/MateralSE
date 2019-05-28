namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    public class MyAchievement_SmileAndWave : MySteamAchievementBase
    {
        private const string WAVE_ANIMATION_NAME = "RightHand/Wave";
        private MyStringId m_waveAnimationId;
        private MyCharacter m_localCharacter;

        private void AnimationControllerOnActionTriggered(MyStringId animationAction)
        {
            if (animationAction == this.m_waveAnimationId)
            {
                Vector3D position = MySession.Static.LocalCharacter.PositionComp.GetPosition();
                MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(MySession.Static.LocalPlayerId);
                long num = (playerFaction == null) ? 0L : playerFaction.FactionId;
                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                {
                    if (player.Character == null)
                    {
                        continue;
                    }
                    if (!ReferenceEquals(player.Character, MySession.Static.LocalCharacter))
                    {
                        double num2;
                        Vector3D.DistanceSquared(ref player.Character.PositionComp.GetPosition(), ref position, out num2);
                        if (num2 < 25.0)
                        {
                            MyFaction faction2 = MySession.Static.Factions.GetPlayerFaction(player.Identity.IdentityId);
                            if (MySession.Static.Factions.AreFactionsEnemies(num, (faction2 == null) ? 0L : faction2.FactionId) && (this.IsPlayerWaving(player.Character) && this.PlayersLookingFaceToFace(MySession.Static.LocalCharacter, player.Character)))
                            {
                                base.NotifyAchieved();
                                MySession.Static.LocalCharacter.AnimationController.ActionTriggered -= new Action<MyStringId>(this.AnimationControllerOnActionTriggered);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool IsPlayerWaving(MyCharacter character)
        {
            MyAnimationController controller = character.AnimationController.Controller;
            for (int i = 0; i < controller.GetLayerCount(); i++)
            {
                MyAnimationStateMachine layerByIndex = controller.GetLayerByIndex(i);
                if (((layerByIndex.CurrentNode != null) && (layerByIndex.CurrentNode.Name != null)) && (layerByIndex.CurrentNode.Name == "RightHand/Wave"))
                {
                    return true;
                }
            }
            return false;
        }

        private bool PlayersLookingFaceToFace(MyCharacter firstCharacter, MyCharacter secondCharacter)
        {
            double num;
            Vector3D forward = firstCharacter.GetHeadMatrix(false, true, false, false, false).Forward;
            Vector3D vectord2 = secondCharacter.GetHeadMatrix(false, true, false, false, false).Forward;
            Vector3D.Dot(ref forward, ref vectord2, out num);
            return (num < -0.5);
        }

        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved && !MySession.Static.CreativeMode)
            {
                this.m_waveAnimationId = MyStringId.GetOrCompute("wave");
                this.m_localCharacter = null;
            }
        }

        public override void SessionUpdate()
        {
            if (!base.IsAchieved)
            {
                this.m_localCharacter = MySession.Static.LocalCharacter;
                if (this.m_localCharacter != null)
                {
                    MySession.Static.LocalCharacter.AnimationController.ActionTriggered += new Action<MyStringId>(this.AnimationControllerOnActionTriggered);
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_SmileAndWave";

        public override bool NeedsUpdate =>
            ReferenceEquals(this.m_localCharacter, null);
    }
}

