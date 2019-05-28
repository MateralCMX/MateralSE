namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;

    internal class MyAchievement_LockAndLoad : MySteamAchievementBase
    {
        private void MyCharacter_OnCharacterDied(MyCharacter character)
        {
            MyEntity entity;
            MyEntities.TryGetEntityById(character.StatComp.LastDamage.AttackerId, out entity, false);
            if (((character.GetPlayerIdentityId() != MySession.Static.LocalHumanPlayer.Identity.IdentityId) && ((character.StatComp.LastDamage.Type == MyDamageType.Bullet) && (entity is MyAutomaticRifleGun))) && ((entity as MyAutomaticRifleGun).Owner.GetPlayerIdentityId() == MySession.Static.LocalHumanPlayer.Identity.IdentityId))
            {
                base.NotifyAchieved();
                MyCharacter.OnCharacterDied -= new Action<MyCharacter>(this.MyCharacter_OnCharacterDied);
            }
        }

        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved)
            {
                MyCharacter.OnCharacterDied += new Action<MyCharacter>(this.MyCharacter_OnCharacterDied);
            }
        }

        public override string AchievementTag =>
            "MyAchievement_LockAndLoad";

        public override bool NeedsUpdate =>
            false;
    }
}

