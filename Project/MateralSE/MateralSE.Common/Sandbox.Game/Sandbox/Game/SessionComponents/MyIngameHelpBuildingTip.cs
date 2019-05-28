namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_BuildingTip", 60)]
    internal class MyIngameHelpBuildingTip : MyIngameHelpObjective
    {
        private bool m_blockSelected;
        private bool m_gPressed;
        private bool m_toolbarDrop;

        public MyIngameHelpBuildingTip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Building_Title;
            base.RequiredIds = new string[] { "IngameHelp_Building" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_BuildingTip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_BuildingTip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.SECONDARY_TOOL_ACTION) };
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY * 4f;
        }
    }
}

