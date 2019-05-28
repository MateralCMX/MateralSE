namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_FlyingAShip", 170)]
    internal class MyIngameHelpFlyingAShip : MyIngameHelpObjective
    {
        private bool m_cPressed;
        private bool m_spacePressed;
        private bool m_qPressed;
        private bool m_ePressed;
        private bool m_powerSwitched;

        public MyIngameHelpFlyingAShip()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_FlyingAShip_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.InsidePoweredGrid));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_FlyingAShip_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[4];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_FlyingAShip_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_REACTORS) };
            detail.FinishCondition = new Func<bool>(this.PowerSwitched);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_FlyingAShip_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.JUMP), GetHighlightedControl(MyControlsSpace.CROUCH) };
            detail.FinishCondition = new Func<bool>(this.FlyCondition);
            detailArray1[2] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_FlyingAShip_Detail4
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.ROLL_LEFT), GetHighlightedControl(MyControlsSpace.ROLL_RIGHT) };
            detail.FinishCondition = new Func<bool>(this.RollCondition);
            detailArray1[3] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_FlyingAShipTip";
        }

        private bool FlyCondition()
        {
            if (this.InsidePoweredGrid())
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

        private bool InsidePoweredGrid()
        {
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            return ((controlledEntity != null) && (controlledEntity.CubeGrid.IsPowered && (controlledEntity.BlockDefinition.EnableShipControl && (controlledEntity.ControlThrusters && ((controlledEntity.EntityThrustComponent != null) && (controlledEntity.EntityThrustComponent.ThrustCount > 0))))));
        }

        private bool PowerSwitched()
        {
            if (!this.m_powerSwitched)
            {
                MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
                this.m_powerSwitched = (controlledEntity != null) && !controlledEntity.CubeGrid.IsPowered;
            }
            return this.m_powerSwitched;
        }

        private bool RollCondition()
        {
            if (this.InsidePoweredGrid())
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
    }
}

