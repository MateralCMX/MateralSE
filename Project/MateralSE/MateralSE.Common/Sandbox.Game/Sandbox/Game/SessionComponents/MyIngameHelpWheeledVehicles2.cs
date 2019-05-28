namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using VRage.Input;

    [IngameObjective("IngameHelp_WheeledVehicles2", 230)]
    internal class MyIngameHelpWheeledVehicles2 : MyIngameHelpObjective
    {
        private bool m_xPressed;

        public MyIngameHelpWheeledVehicles2()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_WheeledVehicles_Title;
            base.RequiredIds = new string[] { "IngameHelp_WheeledVehicles" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.InsidePoweredWheeledGrid));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles2_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_WheeledVehicles2_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.THRUSTS) };
            detail.FinishCondition = new Func<bool>(this.XPressed);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_WheeledVehiclesTip";
        }

        private bool InsidePoweredWheeledGrid()
        {
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            return ((controlledEntity != null) && (controlledEntity.CubeGrid.IsPowered && (controlledEntity.BlockDefinition.EnableShipControl && controlledEntity.ControlWheels)));
        }

        private bool XPressed()
        {
            if ((this.InsidePoweredWheeledGrid() && !this.m_xPressed) && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.THRUSTS))
            {
                this.m_xPressed = true;
            }
            return this.m_xPressed;
        }
    }
}

