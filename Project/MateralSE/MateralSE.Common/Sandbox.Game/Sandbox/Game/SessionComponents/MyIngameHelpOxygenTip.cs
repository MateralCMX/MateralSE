namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_OxygenTip", 130)]
    internal class MyIngameHelpOxygenTip : MyIngameHelpObjective
    {
        public MyIngameHelpOxygenTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Oxygen_Title;
            base.RequiredIds = new string[] { "IngameHelp_Oxygen" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_OxygenTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_OxygenTip_Detail2;
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

