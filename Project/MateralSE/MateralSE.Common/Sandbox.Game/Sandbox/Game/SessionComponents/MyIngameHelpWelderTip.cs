namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_WelderTip", 190)]
    internal class MyIngameHelpWelderTip : MyIngameHelpObjective
    {
        public MyIngameHelpWelderTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Welder_Title;
            base.RequiredIds = new string[] { "IngameHelp_Welder" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_WelderTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_WelderTip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.SECONDARY_TOOL_ACTION) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

