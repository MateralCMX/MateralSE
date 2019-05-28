namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_FlyingAShipLG", 0xa5)]
    internal class MyIngameHelpFlyingAShipLG : MyIngameHelpObjective
    {
        private bool m_toggleLandingGear;

        public MyIngameHelpFlyingAShipLG()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_FlyingAShip_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.InsidePoweredGridWithLG));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_FlyingAShipLG_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_FlyingAShipLG_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.LANDING_GEAR) };
            detail.FinishCondition = new Func<bool>(this.ToggleLandingGear);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_FlyingAShipLGTip";
        }

        private bool InsidePoweredGridWithLG()
        {
            int controlThrusters;
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            if (((controlledEntity == null) || !controlledEntity.CubeGrid.IsPowered) || !controlledEntity.BlockDefinition.EnableShipControl)
            {
                controlThrusters = 0;
            }
            else
            {
                controlThrusters = (int) controlledEntity.ControlThrusters;
            }
            return ((controlThrusters != 0) && (controlledEntity.CubeGrid.GridSystems.LandingSystem.TotalGearCount > 0));
        }

        private bool ToggleLandingGear()
        {
            if ((this.InsidePoweredGridWithLG() && !this.m_toggleLandingGear) && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.LANDING_GEAR))
            {
                this.m_toggleLandingGear = true;
            }
            return this.m_toggleLandingGear;
        }
    }
}

