namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;

    internal class MyAchievement_Colorblind : MySteamAchievementBase
    {
        private const int NUMBER_OF_COLORS_TO_ACHIEV = 20;
        private bool m_isUpdating = true;

        private void Controller_ControlledEntityChanged(IMyControllableEntity oldEnt, IMyControllableEntity newEnt)
        {
            if (((newEnt != null) && !MyCampaignManager.Static.IsCampaignRunning) && (newEnt.Entity is MyCockpit))
            {
                MyCubeGrid parent = newEnt.Entity.Parent as MyCubeGrid;
                if (((parent != null) && ((newEnt.Entity as MyCockpit).BuiltBy == MySession.Static.LocalHumanPlayer.Identity.IdentityId)) && (parent.NumberOfGridColors >= 20))
                {
                    base.NotifyAchieved();
                    MySession.Static.LocalHumanPlayer.Controller.ControlledEntityChanged -= new Action<IMyControllableEntity, IMyControllableEntity>(this.Controller_ControlledEntityChanged);
                }
            }
        }

        public override void SessionLoad()
        {
            base.SessionLoad();
            if (!base.IsAchieved)
            {
                this.m_isUpdating = true;
            }
        }

        public override void SessionUpdate()
        {
            base.SessionUpdate();
            if (this.m_isUpdating && (MySession.Static.LocalHumanPlayer != null))
            {
                MySession.Static.LocalHumanPlayer.Controller.ControlledEntityChanged += new Action<IMyControllableEntity, IMyControllableEntity>(this.Controller_ControlledEntityChanged);
                this.m_isUpdating = false;
            }
        }

        public override string AchievementTag =>
            "MyAchievment_ColorBlind";

        public override bool NeedsUpdate =>
            this.m_isUpdating;
    }
}

