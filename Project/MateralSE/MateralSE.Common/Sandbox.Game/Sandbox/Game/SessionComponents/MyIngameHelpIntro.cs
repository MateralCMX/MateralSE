namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_Intro", 10)]
    internal class MyIngameHelpIntro : MyIngameHelpObjective
    {
        private bool m_F1pressed;

        public MyIngameHelpIntro()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Intro_Title;
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Intro_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Intro_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.HELP_SCREEN) };
            detail.FinishCondition = new Func<bool>(this.F1Condition);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.FollowingId = "IngameHelp_IntroTip";
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }

        private bool F1Condition()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HELP_SCREEN))
            {
                this.m_F1pressed = true;
            }
            return this.m_F1pressed;
        }
    }
}

