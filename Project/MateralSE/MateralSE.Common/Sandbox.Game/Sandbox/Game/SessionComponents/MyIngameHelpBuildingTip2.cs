namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_BuildingTip2", 60)]
    internal class MyIngameHelpBuildingTip2 : MyIngameHelpObjective
    {
        private bool m_blockSelected;
        private bool m_gPressed;
        private bool m_toolbarDrop;

        public MyIngameHelpBuildingTip2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Building_Title;
            base.RequiredIds = new string[] { "IngameHelp_Building3" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_BuildingTip2_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_BuildingTip2_Detail2;
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

