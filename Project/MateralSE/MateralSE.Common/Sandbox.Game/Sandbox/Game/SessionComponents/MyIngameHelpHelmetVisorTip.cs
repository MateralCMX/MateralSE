namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_HelmetVisorTip", 330)]
    internal class MyIngameHelpHelmetVisorTip : MyIngameHelpObjective
    {
        public MyIngameHelpHelmetVisorTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_HelmetVisor_Title;
            base.RequiredIds = new string[] { "IngameHelp_HelmetVisor" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_HelmetVisorTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HelmetVisorTip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.HELP_SCREEN) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

