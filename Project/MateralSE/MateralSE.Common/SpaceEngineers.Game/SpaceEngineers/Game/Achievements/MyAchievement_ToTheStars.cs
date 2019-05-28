namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game;
    using Sandbox.Game.SessionComponents;
    using System;

    internal class MyAchievement_ToTheStars : MySteamAchievementBase
    {
        public override void SessionBeforeStart()
        {
            if (!base.IsAchieved)
            {
                MyCampaignManager.Static.OnCampaignFinished += new Action(this.Static_OnCampaignFinished);
            }
        }

        private void Static_OnCampaignFinished()
        {
            if (MyCampaignManager.Static.ActiveCampaign.IsVanilla)
            {
                base.NotifyAchieved();
                MyCampaignManager.Static.OnCampaignFinished -= new Action(this.Static_OnCampaignFinished);
            }
        }

        public override string AchievementTag =>
            "MyAchievement_ToTheStars";

        public override bool NeedsUpdate =>
            false;
    }
}

