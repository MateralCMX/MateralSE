namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_Jetpack2", 50)]
    internal class MyIngameHelpJetpack2 : MyIngameHelpObjective
    {
        private bool m_zPressed;

        public MyIngameHelpJetpack2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Jetpack_Title;
            base.RequiredIds = new string[] { "IngameHelp_Jetpack" };
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Jetpack2_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Jetpack2_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.DAMPING) };
            detail.FinishCondition = new Func<bool>(this.DampenersCondition);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.FollowingId = "IngameHelp_JetpackTip";
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }

        private bool DampenersCondition()
        {
            if (((MySession.Static.ControlledEntity != null) && MySession.Static.ControlledEntity.EnabledThrusts) && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.DAMPING))
            {
                this.m_zPressed = true;
            }
            return this.m_zPressed;
        }
    }
}

