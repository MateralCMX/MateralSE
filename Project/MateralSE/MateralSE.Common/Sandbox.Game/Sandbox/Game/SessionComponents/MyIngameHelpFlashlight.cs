namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_Flashlight", 0x248)]
    internal class MyIngameHelpFlashlight : MyIngameHelpObjective
    {
        private bool m_lPressed;

        public MyIngameHelpFlashlight()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Flashlight_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Flashlight_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Flashlight_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.HEADLIGHTS) };
            detail.FinishCondition = new Func<bool>(this.SwitchedLights);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.DelayToAppear = (float) TimeSpan.FromMinutes(4.0).TotalSeconds;
            base.FollowingId = "IngameHelp_FlashlightTip";
        }

        private bool SwitchedLights()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HEADLIGHTS))
            {
                this.m_lPressed = true;
            }
            return this.m_lPressed;
        }
    }
}

