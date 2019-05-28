namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;

    public class MyBasicObstacle : IMyObstacle
    {
        public MatrixD m_worldInv;
        public Vector3D m_halfExtents;
        private MyEntity m_entity;
        private bool m_valid;

        public MyBasicObstacle(MyEntity entity)
        {
            this.m_entity = entity;
            this.m_entity.OnClosing += new Action<MyEntity>(this.OnEntityClosing);
            this.Update();
            this.m_valid = true;
        }

        public bool Contains(ref Vector3D point)
        {
            Vector3D vectord;
            Vector3D.Transform(ref point, ref this.m_worldInv, out vectord);
            return ((Math.Abs(vectord.X) < this.m_halfExtents.X) && ((Math.Abs(vectord.Y) < this.m_halfExtents.Y) && (Math.Abs(vectord.Z) < this.m_halfExtents.Z)));
        }

        public void DebugDraw()
        {
            MatrixD xd = MatrixD.Invert(this.m_worldInv);
            MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(MatrixD.CreateScale(this.m_halfExtents) * xd), Color.Red, 0.3f, false, false, false);
        }

        private void OnEntityClosing(MyEntity entity)
        {
            this.m_valid = false;
            this.m_entity = null;
        }

        public void Update()
        {
            this.m_worldInv = this.m_entity.PositionComp.WorldMatrixNormalizedInv;
            this.m_halfExtents = this.m_entity.PositionComp.LocalAABB.Extents;
        }

        public bool Valid =>
            this.m_valid;
    }
}

