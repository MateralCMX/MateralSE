namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.ModAPI;

    internal class MyAchievement_WinWin : MySteamAchievementBase
    {
        private void Factions_FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if (MySession.Static.LocalHumanPlayer != null)
            {
                long identityId = MySession.Static.LocalHumanPlayer.Identity.IdentityId;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(identityId);
                if (faction != null)
                {
                    if (((faction.IsLeader(identityId) || faction.IsFounder(identityId)) && ((faction.FactionId == fromFactionId) || (faction.FactionId == toFactionId))) && ((action == MyFactionStateChange.AcceptPeace) || (action == MyFactionStateChange.SendPeaceRequest)))
                    {
                        base.NotifyAchieved();
                        MySession.Static.Factions.FactionStateChanged -= new Action<MyFactionStateChange, long, long, long, long>(this.Factions_FactionStateChanged);
                    }
                    if (((faction.IsLeader(identityId) || faction.IsFounder(identityId)) && (faction.FactionId == toFactionId)) && (action == MyFactionStateChange.AcceptPeace))
                    {
                        base.NotifyAchieved();
                        MySession.Static.Factions.FactionStateChanged -= new Action<MyFactionStateChange, long, long, long, long>(this.Factions_FactionStateChanged);
                    }
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
            "MyAchievement_WinWin";

        public override bool NeedsUpdate =>
            false;
    }
}

