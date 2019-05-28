namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_WheeledVehicles", 230)]
    internal class MyIngameHelpWheeledVehicles : MyIngameHelpObjective
    {
        private bool m_powerSwitched;
        private bool m_toggleLandingGear;
        private bool m_wPressed;
        private bool m_sPressed;
        private bool m_aPressed;
        private bool m_dPressed;
        private bool m_spacePressed;

        public MyIngameHelpWheeledVehicles()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.InsidePoweredWheeledGrid));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[5];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_REACTORS) };
            detail.FinishCondition = new Func<bool>(this.PowerSwitched);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.LANDING_GEAR) };
            detail.FinishCondition = new Func<bool>(this.ToggleLandingGear);
            detailArray1[2] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Detail4
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.FORWARD), GetHighlightedControl(MyControlsSpace.BACKWARD), GetHighlightedControl(MyControlsSpace.STRAFE_LEFT), GetHighlightedControl(MyControlsSpace.STRAFE_RIGHT) };
            detail.FinishCondition = new Func<bool>(this.WSADCondition);
            detailArray1[3] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Detail5
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.JUMP) };
            detail.FinishCondition = new Func<bool>(this.BrakeCondition);
            detailArray1[4] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }

        private bool BrakeCondition()
        {
            if (this.InsidePoweredWheeledGrid() && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.JUMP))
            {
                this.m_spacePressed = true;
            }
            return this.m_spacePressed;
        }

        private bool InsidePoweredWheeledGrid()
        {
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            return ((controlledEntity != null) && (controlledEntity.CubeGrid.IsPowered && (controlledEntity.BlockDefinition.EnableShipControl && (controlledEntity.ControlWheels && (controlledEntity.CubeGrid.GridSystems.WheelSystem.WheelCount > 0)))));
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

        private bool ToggleLandingGear()
        {
            if ((this.InsidePoweredWheeledGrid() && !this.m_toggleLandingGear) && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.LANDING_GEAR))
            {
                this.m_toggleLandingGear = true;
            }
            return this.m_toggleLandingGear;
        }

        private bool WSADCondition()
        {
            if (this.InsidePoweredWheeledGrid())
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

