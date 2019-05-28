namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.ModAPI;

    internal class MyAchievement_DeclareWar : MySteamAchievementBase
    {
        private void Factions_FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if (MySession.Static.LocalHumanPlayer != null)
            {
                long identityId = MySession.Static.LocalHumanPlayer.Identity.IdentityId;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(identityId);
                if ((faction != null) && (((faction.IsFounder(identityId) || faction.IsLeader(identityId)) && (faction.FactionId == fromFactionId)) && (action == MyFactionStateChange.DeclareWar)))
                {
                    base.NotifyAchieved();
                    MySession.Static.Factions.FactionStateChanged -= new Action<MyFactionStateChange, long, long, long, long>(this.Factions_FactionStateChanged);
                }
            }
        }

        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved)
            {
                MySession.Static.Factions.FactionStateChanged += new Action<MyFactionStateChange, long, long, long, long>(this.Factions_FactionStateChanged);
            }
        }

        public override string AchievementTag =>
            "MyAchievment_DeclareWar";

        public override bool NeedsUpdate =>
            false;
    }
}

