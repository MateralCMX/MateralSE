namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;

    public class MyAchievement_MasterEngineer : MySteamAchievementBase
    {
        public const string StatNameTag = "MasterEngineer_MinutesPlayed";
        private int m_totalMinutesPlayed;
        private int m_lastLoggedMinute;

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_totalMinutesPlayed = MyGameService.GetStatInt("MasterEngineer_MinutesPlayed");
            }
        }

        public override void SessionLoad()
        {
            this.m_lastLoggedMinute = 0;
        }

        public override void SessionUpdate()
        {
            int totalMinutes = (int) MySession.Static.ElapsedPlayTime.TotalMinutes;
            if (this.m_lastLoggedMinute < totalMinutes)
            {
                this.m_totalMinutesPlayed++;
                this.m_lastLoggedMinute = totalMinutes;
                MyGameService.SetStat("MasterEngineer_MinutesPlayed", this.m_totalMinutesPlayed);
                if (this.m_totalMinutesPlayed > 0x2328)
                {
                    base.NotifyAchieved();
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_MasterEngineer";

        public override bool NeedsUpdate =>
            true;
    }
}

