namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_JetpackTip", 50)]
    internal class MyIngameHelpJetpackTip : MyIngameHelpObjective
    {
        public MyIngameHelpJetpackTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Jetpack_Title;
            base.RequiredIds = new string[] { "IngameHelp_Jetpack2" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_JetpackTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_JetpackTip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.DAMPING) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

