namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Temperature", 0x3f)]
    internal class MyIngameHelpTemperature : MyIngameHelpObjective
    {
        public MyIngameHelpTemperature()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Temperature_Title;
            base.RequiredIds = new string[] { "IngameHelp_Camera" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Temperature_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.DelayToAppear = (float) TimeSpan.FromMinutes(10.0).TotalSeconds;
        }
    }
}

