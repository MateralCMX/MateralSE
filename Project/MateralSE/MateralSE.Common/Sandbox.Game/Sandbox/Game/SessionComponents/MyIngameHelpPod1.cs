namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Pod1", 0x17)]
    internal class MyIngameHelpPod1 : MyIngameHelpObjective
    {
        public static bool StartingInPod;

        public MyIngameHelpPod1()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Pod_Title;
            base.RequiredIds = new string[] { "IngameHelp_Camera" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Pod1_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.RequiredCondition = new Func<bool>(this.PlayerInPod);
            base.FollowingId = "IngameHelp_Pod2";
        }

        private bool PlayerInPod() => 
            StartingInPod;
    }
}

