namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Pod3", 0x19)]
    internal class MyIngameHelpPod3 : MyIngameHelpObjective
    {
        public MyIngameHelpPod3()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Pod_Title;
            base.RequiredIds = new string[] { "IngameHelp_Pod1" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Pod3_Detail1;
            base.Details = new MyIngameHelpDetail[] { detail1 };
            base.DelayToHide = 4f * MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }
    }
}

