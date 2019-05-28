namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;

    [IngameObjective("IngameHelp_Power", 100)]
    internal class MyIngameHelpPower : MyIngameHelpObjective
    {
        private bool m_powerEnabled;

        public MyIngameHelpPower()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Power_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.InsideUnpoweredGrid));
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Power_Detail1
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_REACTORS) };
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Power_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_REACTORS) };
            detail.FinishCondition = new Func<bool>(this.PowerEnabled);
            detailArray1[1] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_PowerTip";
        }

        private bool InsideUnpoweredGrid()
        {
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            return ((controlledEntity != null) && (!controlledEntity.CubeGrid.IsPowered && (controlledEntity.BlockDefinition.EnableShipControl && controlledEntity.ControlThrusters)));
        }

        private bool PowerEnabled()
        {
            if (!this.m_powerEnabled)
            {
                MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
                this.m_powerEnabled = (controlledEntity != null) && controlledEntity.CubeGrid.IsPowered;
            }
            return this.m_powerEnabled;
        }
    }
}

