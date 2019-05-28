namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Pod2", 0x18)]
    internal class MyIngameHelpPod2 : MyIngameHelpObjective
    {
        public MyIngameHelpPod2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Pod_Title;
            base.RequiredIds = new string[] { "IngameHelp_Pod1" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Pod2_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_Pod3";
        }
    }
}

