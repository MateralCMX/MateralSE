namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentMotorBase : MyDebugRenderComponent
    {
        private MyMotorBase m_motor;

        public MyDebugRenderComponentMotorBase(MyMotorBase motor) : base(motor)
        {
            this.m_motor = motor;
        }

        public override unsafe void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_ROTORS)
            {
                Vector3 vector;
                Vector3D vectord;
                Quaternion quaternion;
                this.m_motor.ComputeTopQueryBox(out vectord, out vector, out quaternion);
                MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(vectord, vector, quaternion), Color.Green.ToVector3(), 1f, false, false, false);
                if (this.m_motor.Rotor != null)
                {
                    MyRenderProxy.DebugDrawSphere(Vector3D.Transform(this.m_motor.DummyPosition, this.m_motor.CubeGrid.WorldMatrix) + (Vector3D.Transform((this.m_motor.Rotor as MyMotorRotor).WheelDummy, this.m_motor.RotorGrid.WorldMatrix) - this.m_motor.RotorGrid.WorldMatrix.Translation), 0.1f, Color.Green, 1f, false, false, true, false);
                    BoundingSphere boundingSphere = this.m_motor.Rotor.Model.BoundingSphere;
                    BoundingSphere* spherePtr1 = (BoundingSphere*) ref boundingSphere;
                    spherePtr1->Center = (Vector3) Vector3D.Transform(boundingSphere.Center, this.m_motor.Rotor.WorldMatrix);
                    MyRenderProxy.DebugDrawSphere(boundingSphere.Center, boundingSphere.Radius, Color.Red, 1f, false, false, true, false);
                }
            }
        }
    }
}

