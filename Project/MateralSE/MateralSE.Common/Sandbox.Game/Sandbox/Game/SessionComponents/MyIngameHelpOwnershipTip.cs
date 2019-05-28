namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_OwnershipTip", 90)]
    internal class MyIngameHelpOwnershipTip : MyIngameHelpObjective
    {
        public MyIngameHelpOwnershipTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Ownership_Title;
            base.RequiredIds = new string[] { "IngameHelp_Ownership" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_OwnershipTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_OwnershipTip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.HELP_SCREEN) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

