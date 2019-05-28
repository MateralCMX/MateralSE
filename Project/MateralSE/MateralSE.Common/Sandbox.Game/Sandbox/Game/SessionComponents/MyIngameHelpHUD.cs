namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_HUD", 50)]
    internal class MyIngameHelpHUD : MyIngameHelpObjective
    {
        private bool m_tabPressed;
        private bool m_hPressed;

        public MyIngameHelpHUD()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_HUD_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_HUD_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[3];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HUD_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_HUD) };
            detail.FinishCondition = new Func<bool>(this.TabCondition);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HUD_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_SIGNALS) };
            detail.FinishCondition = new Func<bool>(this.SignalCondition);
            detailArray1[2] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_HUDTip";
            base.DelayToAppear = (float) TimeSpan.FromMinutes(3.0).TotalSeconds;
        }

        private bool SignalCondition()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOGGLE_SIGNALS))
            {
                this.m_hPressed = true;
            }
            return this.m_hPressed;
        }

        private bool TabCondition()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOGGLE_HUD))
            {
                this.m_tabPressed = true;
            }
            return this.m_tabPressed;
        }
    }
}

