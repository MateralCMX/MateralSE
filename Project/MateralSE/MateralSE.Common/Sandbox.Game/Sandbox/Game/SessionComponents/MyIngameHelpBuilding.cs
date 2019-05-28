namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_Building", 60)]
    internal class MyIngameHelpBuilding : MyIngameHelpObjective
    {
        private bool m_blockSelected;
        private bool m_gPressed;
        private bool m_toolbarDrop;

        public MyIngameHelpBuilding()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Building_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = new Func<bool>(this.BlockInToolbarSelected);
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Building_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[4];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Building_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.BUILD_SCREEN) };
            detail.FinishCondition = new Func<bool>(this.GCondition);
            detailArray1[1] = detail;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Building_Detail3;
            detail2.FinishCondition = new Func<bool>(this.ToolbarDropCondition);
            detailArray1[2] = detail2;
            MyIngameHelpDetail detail3 = new MyIngameHelpDetail();
            detail3.TextEnum = MySpaceTexts.IngameHelp_Building_Detail4;
            detail3.FinishCondition = new Func<bool>(this.BlockInToolbarSelected);
            detailArray1[3] = detail3;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_BuildingTip";
            if (MyToolbarComponent.CurrentToolbar != null)
            {
                MyToolbarComponent.CurrentToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
                MyToolbarComponent.CurrentToolbarChanged += new Action(this.MyToolbarComponent_CurrentToolbarChanged);
            }
            base.DelayToAppear = (float) TimeSpan.FromMinutes(3.0).TotalSeconds;
        }

        private bool BlockInToolbarSelected() => 
            this.m_blockSelected;

        private void CurrentToolbar_ItemChanged(MyToolbar arg1, MyToolbar.IndexArgs arg2)
        {
            this.m_toolbarDrop = true;
        }

        private void CurrentToolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs args, bool userActivated)
        {
            if ((toolbar.SelectedItem is MyToolbarItemCubeBlock) & userActivated)
            {
                this.m_blockSelected = true;
            }
        }

        private bool GCondition()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.BUILD_SCREEN))
            {
                this.m_gPressed = true;
            }
            return this.m_gPressed;
        }

        private void MyToolbarComponent_CurrentToolbarChanged()
        {
            MyToolbarComponent.CurrentToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
            MyToolbarComponent.CurrentToolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.CurrentToolbar_ItemChanged);
        }

        public override void OnActivated()
        {
            base.OnActivated();
            MyToolbarComponent.CurrentToolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.CurrentToolbar_ItemChanged);
        }

        private bool ToolbarDropCondition() => 
            this.m_toolbarDrop;
    }
}

