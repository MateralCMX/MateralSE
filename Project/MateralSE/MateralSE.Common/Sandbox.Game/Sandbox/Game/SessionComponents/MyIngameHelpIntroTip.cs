namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_IntroTip", 10)]
    internal class MyIngameHelpIntroTip : MyIngameHelpObjective
    {
        public MyIngameHelpIntroTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Intro_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_IntroTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_IntroTip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.CHAT_SCREEN) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.FollowingId = "IngameHelp_IntroTip2";
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

