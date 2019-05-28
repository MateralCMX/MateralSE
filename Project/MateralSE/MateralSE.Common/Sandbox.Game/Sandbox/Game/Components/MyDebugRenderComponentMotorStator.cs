namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentMotorStator : MyDebugRenderComponent
    {
        private MyMotorStator m_motor;

        public MyDebugRenderComponentMotorStator(MyMotorStator motor) : base(motor)
        {
            this.m_motor = motor;
        }

        public override void DebugDraw()
        {
            if (this.m_motor.CanDebugDraw() && MyDebugDrawSettings.DEBUG_DRAW_ROTORS)
            {
                MatrixD worldMatrix = this.m_motor.PositionComp.WorldMatrix;
                MatrixD xd2 = this.m_motor.Rotor.WorldMatrix;
                Vector3 pointFrom = Vector3.Lerp((Vector3) worldMatrix.Translation, (Vector3) xd2.Translation, 0.5f);
                MyRenderProxy.DebugDrawLine3D(pointFrom, pointFrom + Vector3.Normalize(worldMatrix.Up), Color.Yellow, Color.Yellow, false, false);
                MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, xd2.Translation, Color.Red, Color.Green, false, false);
            }
        }
    }
}

