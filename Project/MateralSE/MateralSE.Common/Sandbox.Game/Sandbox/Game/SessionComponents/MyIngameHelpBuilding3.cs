namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using System;

    [IngameObjective("IngameHelp_Building3", 80)]
    internal class MyIngameHelpBuilding3 : MyIngameHelpObjective
    {
        private bool m_blockAdded;

        public MyIngameHelpBuilding3()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Building_Title;
            base.RequiredIds = new string[] { "IngameHelp_Building2" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Building3_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Building3_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.PRIMARY_TOOL_ACTION) };
            detail.FinishCondition = new Func<bool>(this.BlockAddedCondition);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_BuildingTip2";
        }

        private bool BlockAddedCondition() => 
            this.m_blockAdded;

        public override void OnActivated()
        {
            base.OnActivated();
            MyCubeBuilder.Static.OnBlockAdded += new Action<MyCubeBlockDefinition>(this.Static_OnBlockAdded);
        }

        private void Static_OnBlockAdded(MyCubeBlockDefinition definition)
        {
            this.m_blockAdded = true;
        }
    }
}

