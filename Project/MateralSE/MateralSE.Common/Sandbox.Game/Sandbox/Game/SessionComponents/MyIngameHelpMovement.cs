namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Input;

    [IngameObjective("IngameHelp_Movement", 30)]
    internal class MyIngameHelpMovement : MyIngameHelpObjective
    {
        private bool m_wPressed;
        private bool m_sPressed;
        private bool m_aPressed;
        private bool m_dPressed;
        private bool m_spacePressed;
        private bool m_running;
        private bool m_crouching;

        public MyIngameHelpMovement()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Movement_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = new Func<bool>(this.StandingCondition);
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Movement_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[5];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Movement_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.FORWARD), GetHighlightedControl(MyControlsSpace.BACKWARD), GetHighlightedControl(MyControlsSpace.STRAFE_LEFT), GetHighlightedControl(MyControlsSpace.STRAFE_RIGHT) };
            detail.FinishCondition = new Func<bool>(this.WSADCondition);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Movement_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.SPRINT), GetHighlightedControl(MyControlsSpace.FORWARD) };
            detail.FinishCondition = new Func<bool>(this.SprintCondition);
            detailArray1[2] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Movement_Detail4
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.JUMP) };
            detail.FinishCondition = new Func<bool>(this.JumpCondition);
            detailArray1[3] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Movement_Detail5
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.CROUCH) };
            detail.FinishCondition = new Func<bool>(this.CrouchCondition);
            detailArray1[4] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }

        private bool CrouchCondition()
        {
            if ((this.StandingCondition() && (MySession.Static.ControlledEntity is MyCharacter)) && ((MyCharacter) MySession.Static.ControlledEntity).IsCrouching)
            {
                this.m_crouching = true;
            }
            return this.m_crouching;
        }

        private bool JumpCondition()
        {
            if (((MySession.Static.ControlledEntity is MyCharacter) && ((((MyCharacter) MySession.Static.ControlledEntity).CurrentMovementState == MyCharacterMovementEnum.Jump) || ((MyCharacter) MySession.Static.ControlledEntity).IsMagneticBootsActive)) && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.JUMP))
            {
                this.m_spacePressed = true;
            }
            return this.m_spacePressed;
        }

        private bool SprintCondition()
        {
            if ((this.StandingCondition() && (MySession.Static.ControlledEntity is MyCharacter)) && ((MyCharacter) MySession.Static.ControlledEntity).IsSprinting)
            {
                this.m_running = true;
            }
            return this.m_running;
        }

        private bool StandingCondition() => 
            ((MySession.Static.ControlledEntity is MyCharacter) && (((MyCharacter) MySession.Static.ControlledEntity).CharacterGroundState == HkCharacterStateType.HK_CHARACTER_ON_GROUND));

        private bool WSADCondition()
        {
            if (this.StandingCondition())
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

