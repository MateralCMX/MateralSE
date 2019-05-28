namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Input;

    [IngameObjective("IngameHelp_Stuck", 470)]
    internal class MyIngameHelpStuck : MyIngameHelpObjective
    {
        private Queue<float> m_averageSpeed = new Queue<float>();
        private int CountForAverage = 60;

        public MyIngameHelpStuck()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Stuck_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.StuckedInsideGrid));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Stuck_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[3];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Stuck_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TOGGLE_REACTORS) };
            detail.FinishCondition = new Func<bool>(this.MovingInsideGrid);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Stuck_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.LANDING_GEAR) };
            detail.FinishCondition = new Func<bool>(this.MovingInsideGrid);
            detailArray1[2] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_StuckTip";
        }

        public override bool IsCritical() => 
            this.StuckedInsideGrid();

        private bool MovingInsideGrid()
        {
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            if (controlledEntity == null)
            {
                goto TR_0006;
            }
            else if (!controlledEntity.BlockDefinition.EnableShipControl)
            {
                goto TR_0006;
            }
            else
            {
                if ((controlledEntity.ControlThrusters && (controlledEntity.EntityThrustComponent != null)) && (controlledEntity.EntityThrustComponent.ThrustCount > 0))
                {
                    if (MyInput.Static.IsGameControlPressed(MyControlsSpace.FORWARD))
                    {
                        goto TR_0004;
                    }
                    else if (MyInput.Static.IsGameControlPressed(MyControlsSpace.BACKWARD))
                    {
                        goto TR_0004;
                    }
                    else if (MyInput.Static.IsGameControlPressed(MyControlsSpace.STRAFE_LEFT))
                    {
                        goto TR_0004;
                    }
                    else if (!MyInput.Static.IsGameControlPressed(MyControlsSpace.STRAFE_RIGHT))
                    {
                        this.m_averageSpeed.Clear();
                    }
                    else
                    {
                        goto TR_0004;
                    }
                }
                goto TR_0006;
            }
        TR_0004:
            this.m_averageSpeed.Enqueue(controlledEntity.CubeGrid.Physics.LinearVelocity.LengthSquared());
            if (this.m_averageSpeed.Count < this.CountForAverage)
            {
                return false;
            }
            if (this.m_averageSpeed.Count > this.CountForAverage)
            {
                this.m_averageSpeed.Dequeue();
            }
            return (((IEnumerable<float>) this.m_averageSpeed).Average() > 1f);
        TR_0006:
            return false;
        }

        private bool StuckedInsideGrid()
        {
            MyCockpit controlledEntity = MySession.Static.ControlledEntity as MyCockpit;
            if (controlledEntity == null)
            {
                goto TR_0006;
            }
            else if (!controlledEntity.BlockDefinition.EnableShipControl)
            {
                goto TR_0006;
            }
            else
            {
                if ((controlledEntity.ControlThrusters && (controlledEntity.EntityThrustComponent != null)) && (controlledEntity.EntityThrustComponent.ThrustCount > 0))
                {
                    if (MyInput.Static.IsGameControlPressed(MyControlsSpace.FORWARD))
                    {
                        goto TR_0004;
                    }
                    else if (MyInput.Static.IsGameControlPressed(MyControlsSpace.BACKWARD))
                    {
                        goto TR_0004;
                    }
                    else if (MyInput.Static.IsGameControlPressed(MyControlsSpace.STRAFE_LEFT))
                    {
                        goto TR_0004;
                    }
                    else if (!MyInput.Static.IsGameControlPressed(MyControlsSpace.STRAFE_RIGHT))
                    {
                        this.m_averageSpeed.Clear();
                    }
                    else
                    {
                        goto TR_0004;
                    }
                }
                goto TR_0006;
            }
        TR_0004:
            this.m_averageSpeed.Enqueue(controlledEntity.CubeGrid.Physics.LinearVelocity.LengthSquared());
            if (this.m_averageSpeed.Count < this.CountForAverage)
            {
                return false;
            }
            if (this.m_averageSpeed.Count > this.CountForAverage)
            {
                this.m_averageSpeed.Dequeue();
            }
            return (((IEnumerable<float>) this.m_averageSpeed).Average() < 0.001f);
        TR_0006:
            return false;
        }
    }
}

