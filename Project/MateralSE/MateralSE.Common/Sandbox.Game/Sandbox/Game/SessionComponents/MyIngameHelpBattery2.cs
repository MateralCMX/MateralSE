namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Battery2", 670)]
    internal class MyIngameHelpBattery2 : MyIngameHelpObjective
    {
        public MyIngameHelpBattery2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Battery_Title;
            base.RequiredIds = new string[] { "IngameHelp_Battery" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Battery2_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }
    }
}

