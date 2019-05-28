namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_IntroTip2", 10)]
    internal class MyIngameHelpIntroTip2 : MyIngameHelpObjective
    {
        public MyIngameHelpIntroTip2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Intro_Title;
            base.RequiredIds = new string[] { "IngameHelp_IntroTip" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_IntroTip2_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

