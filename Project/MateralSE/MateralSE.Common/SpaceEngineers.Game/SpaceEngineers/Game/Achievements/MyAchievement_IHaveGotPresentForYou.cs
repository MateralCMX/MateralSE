namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;

    internal class MyAchievement_IHaveGotPresentForYou : MySteamAchievementBase
    {
        private bool m_someoneIsDead;
        private bool m_imDead;
        private long m_lastAttackerID;
        private List<long> m_warheadList = new List<long>();

        private void MyCharacter_OnCharacterDied(MyCharacter character)
        {
            if (character.StatComp.LastDamage.Type == MyDamageType.Explosion)
            {
                long attackerId = character.StatComp.LastDamage.AttackerId;
                if (attackerId != this.m_lastAttackerID)
                {
                    this.m_someoneIsDead = false;
                    this.m_imDead = false;
                    this.m_lastAttackerID = attackerId;
                }
                if (character.GetPlayerIdentityId() == MySession.Static.LocalHumanPlayer.Identity.IdentityId)
                {
                    this.m_imDead = true;
                }
                else if (character.IsPlayer)
                {
                    this.m_someoneIsDead = true;
                }
                if ((this.m_imDead && (this.m_someoneIsDead && (this.m_lastAttackerID == attackerId))) && this.m_warheadList.Contains(this.m_lastAttackerID))
                {
                    base.NotifyAchieved();
                    MyCharacter.OnCharacterDied -= new Action<MyCharacter>(this.MyCharacter_OnCharacterDied);
                    MyWarhead.OnCreated = (Action<MyWarhead>) Delegate.Remove(MyWarhead.OnCreated, new Action<MyWarhead>(this.MyWarhead_OnCreated));
                    MyWarhead.OnDeleted = (Action<MyWarhead>) Delegate.Remove(MyWarhead.OnDeleted, new Action<MyWarhead>(this.MyWarhead_OnDeleted));
                }
            }
        }

        private void MyWarhead_OnCreated(MyWarhead obj)
        {
            if ((obj.BuiltBy == MySession.Static.LocalPlayerId) && !this.m_warheadList.Contains(obj.CubeGrid.EntityId))
            {
                this.m_warheadList.Add(obj.CubeGrid.EntityId);
            }
        }

        private void MyWarhead_OnDeleted(MyWarhead obj)
        {
            this.m_warheadList.Remove(obj.CubeGrid.EntityId);
        }

        public override void SessionBeforeStart()
        {
            base.SessionBeforeStart();
            if (!base.IsAchieved)
            {
                MyCharacter.OnCharacterDied += new Action<MyCharacter>(this.MyCharacter_OnCharacterDied);
                MyWarhead.OnCreated = (Action<MyWarhead>) Delegate.Combine(MyWarhead.OnCreated, new Action<MyWarhead>(this.MyWarhead_OnCreated));
                MyWarhead.OnDeleted = (Action<MyWarhead>) Delegate.Combine(MyWarhead.OnDeleted, new Action<MyWarhead>(this.MyWarhead_OnDeleted));
            }
        }

        public override string AchievementTag =>
            "MyAchievement_IHaveGotPresentForYou";

        public override bool NeedsUpdate =>
            false;
    }
}

