namespace SpaceEngineers.Game.EntityComponents.DebugRenders
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentLandingGear : MyDebugRenderComponent
    {
        private MyLandingGear m_langingGear;

        public MyDebugRenderComponentLandingGear(MyLandingGear landingGear) : base(landingGear)
        {
            this.m_langingGear = landingGear;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES)
            {
                foreach (Matrix matrix in this.m_langingGear.LockPositions)
                {
                    Quaternion quaternion;
                    Vector3D vectord;
                    Vector3 vector;
                    this.m_langingGear.GetBoxFromMatrix(matrix, out vector, out vectord, out quaternion);
                    Matrix matrix2 = Matrix.CreateFromQuaternion(quaternion);
                    matrix2.Translation = (Vector3) vectord;
                    MyRenderProxy.DebugDrawOBB(Matrix.CreateScale((vector * 2f) * new Vector3(2f, 1f, 2f)) * matrix2, Color.Yellow.ToVector3(), 1f, false, false, true, false);
                }
            }
        }
    }
}

