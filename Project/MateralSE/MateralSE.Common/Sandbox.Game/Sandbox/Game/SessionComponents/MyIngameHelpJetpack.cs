namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_Jetpack", 40)]
    internal class MyIngameHelpJetpack : MyIngameHelpObjective
    {
        private bool m_jetpackEnabled;
        private bool m_cPressed;
        private bool m_spacePressed;
        private bool m_wPressed;
        private bool m_sPressed;
        private bool m_aPressed;
        private bool m_dPressed;
        private bool m_qPressed;
        private bool m_ePressed;

        public MyIngameHelpJetpack()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Jetpack_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = new Func<bool>(this.JetpackInWorldSettings);
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Jetpack_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[5];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Jetpack_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.THRUSTS) };
            detail.FinishCondition = new Func<bool>(this.JetpackCondition);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Jetpack_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.JUMP), GetHighlightedControl(MyControlsSpace.CROUCH) };
            detail.FinishCondition = new Func<bool>(this.FlyCondition);
            detailArray1[2] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Jetpack_Detail4
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.FORWARD), GetHighlightedControl(MyControlsSpace.BACKWARD), GetHighlightedControl(MyControlsSpace.STRAFE_LEFT), GetHighlightedControl(MyControlsSpace.STRAFE_RIGHT) };
            detail.FinishCondition = new Func<bool>(this.WSADCondition);
            detailArray1[3] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Jetpack_Detail5
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.ROLL_LEFT), GetHighlightedControl(MyControlsSpace.ROLL_RIGHT) };
            detail.FinishCondition = new Func<bool>(this.RollCondition);
            detailArray1[4] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_Jetpack2";
        }

        private bool FlyCondition()
        {
            if ((MySession.Static.ControlledEntity != null) && MySession.Static.ControlledEntity.EnabledThrusts)
            {
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CROUCH))
                {
                    this.m_cPressed = true;
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.JUMP))
                {
                    this.m_spacePressed = true;
                }
            }
            return (this.m_cPressed && this.m_spacePressed);
        }

        private bool JetpackCondition()
        {
            if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.THRUSTS) && (MySession.Static.ControlledEntity != null)) && MySession.Static.ControlledEntity.EnabledThrusts)
            {
                this.m_jetpackEnabled = true;
            }
            return this.m_jetpackEnabled;
        }

        private bool JetpackInWorldSettings() => 
            ((MySession.Static != null) && (MySession.Static.Settings.EnableJetpack && ((MySession.Static.LocalCharacter != null) && !MySession.Static.LocalCharacter.IsSitting)));

        private bool RollCondition()
        {
            if ((MySession.Static.ControlledEntity != null) && MySession.Static.ControlledEntity.EnabledThrusts)
            {
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.ROLL_LEFT))
                {
                    this.m_qPressed = true;
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.ROLL_RIGHT))
                {
                    this.m_ePressed = true;
                }
            }
            return (this.m_qPressed && this.m_ePressed);
        }

        private bool WSADCondition()
        {
            if ((MySession.Static.ControlledEntity != null) && MySession.Static.ControlledEntity.EnabledThrusts)
            {
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.FORWARD))
                {
                    this.m_wPressed = true;
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.BACKWARD))
                {
                    this.m_sPressed = true;
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.STRAFE_LEFT))
                {
                    this.m_aPressed = true;
                }
                if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.STRAFE_RIGHT))
                {
                    this.m_dPressed = true;
                }
            }
            return (this.m_wPressed && (this.m_sPressed && (this.m_aPressed && this.m_dPressed)));
        }
    }
}

