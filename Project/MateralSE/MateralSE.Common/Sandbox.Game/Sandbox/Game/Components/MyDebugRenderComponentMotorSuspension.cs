namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using System;

    internal class MyDebugRenderComponentMotorSuspension : MyDebugRenderComponent
    {
        private MyMotorSuspension m_motor;

        public MyDebugRenderComponentMotorSuspension(MyMotorSuspension motor) : base(motor)
        {
            this.m_motor = motor;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS && ((MySector.MainCamera.Position - this.m_motor.PositionComp.GetPosition()).LengthSquared() < 10000.0))
            {
                this.m_motor.DebugDrawConstraint();
            }
        }
    }
}

