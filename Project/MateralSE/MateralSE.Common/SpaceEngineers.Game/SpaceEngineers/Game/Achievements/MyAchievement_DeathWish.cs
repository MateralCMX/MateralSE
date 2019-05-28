namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;

    public class MyAchievement_DeathWish : MySteamAchievementBase
    {
        public const string StatNameTag = "DeathWish_MinutesPlayed";
        private bool m_conditionsMet;
        private int m_lastElapsedMinutes;
        private int m_totalMinutesPlayedInArmageddonSettings;

        public override void Init()
        {
            base.Init();
            if (!base.IsAchieved)
            {
                this.m_totalMinutesPlayedInArmageddonSettings = MyGameService.GetStatInt("DeathWish_MinutesPlayed");
            }
        }

        public override void SessionLoad()
        {
            this.m_conditionsMet = (MySession.Static.Settings.EnvironmentHostility == MyEnvironmentHostilityEnum.CATACLYSM_UNREAL) && !MySession.Static.CreativeMode;
            this.m_lastElapsedMinutes = 0;
        }

        public override void SessionUpdate()
        {
            if (this.m_conditionsMet)
            {
                int totalMinutes = (int) MySession.Static.ElapsedPlayTime.TotalMinutes;
                if (this.m_lastElapsedMinutes < totalMinutes)
                {
                    this.m_lastElapsedMinutes = totalMinutes;
                    this.m_totalMinutesPlayedInArmageddonSettings++;
                    MyGameService.SetStat("DeathWish_MinutesPlayed", this.m_totalMinutesPlayedInArmageddonSettings);
                    if (this.m_totalMinutesPlayedInArmageddonSettings > 300)
                    {
                        base.NotifyAchieved();
                    }
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_DeathWish";

        public override bool NeedsUpdate =>
            true;
    }
}

