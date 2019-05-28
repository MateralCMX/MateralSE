namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Turbine2", 520)]
    internal class MyIngameHelpTurbine2 : MyIngameHelpObjective
    {
        public MyIngameHelpTurbine2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Turbine_Title;
            base.RequiredIds = new string[] { "IngameHelp_Turbine" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Turbine2_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }
    }
}

