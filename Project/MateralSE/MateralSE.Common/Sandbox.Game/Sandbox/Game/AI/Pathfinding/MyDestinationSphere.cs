namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDestinationSphere : IMyDestinationShape
    {
        private float m_radius;
        private Vector3D m_center;
        private Vector3D m_relativeCenter;

        public MyDestinationSphere(ref Vector3D worldCenter, float radius)
        {
            this.Init(ref worldCenter, radius);
        }

        public void DebugDraw()
        {
            MyRenderProxy.DebugDrawSphere(this.m_center, Math.Max(this.m_radius, 0.05f), Color.Pink, 1f, false, false, true, false);
            MyRenderProxy.DebugDrawSphere(this.m_center, this.m_radius, Color.Pink, 1f, false, false, true, false);
            MyRenderProxy.DebugDrawText3D(this.m_center, "Destination", Color.Pink, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
        }

        public Vector3D GetBestPoint(Vector3D queryPoint) => 
            this.m_center;

        public Vector3D GetClosestPoint(Vector3D queryPoint)
        {
            Vector3D vectord = queryPoint - this.m_center;
            double num = vectord.Length();
            return ((num >= this.m_radius) ? (this.m_center + ((vectord / num) * this.m_radius)) : queryPoint);
        }

        public Vector3D GetDestination() => 
            this.m_center;

        public void Init(ref Vector3D worldCenter, float radius)
        {
            this.m_radius = radius;
            this.m_center = worldCenter;
        }

        public float PointAdmissibility(Vector3D position, float tolerance)
        {
            float num = (float) Vector3D.Distance(position, this.m_center);
            return ((num > (this.m_radius + tolerance)) ? float.PositiveInfinity : num);
        }

        public void SetRelativeTransform(MatrixD invWorldTransform)
        {
            Vector3D.Transform(ref this.m_center, ref invWorldTransform, out this.m_relativeCenter);
        }

        public void UpdateWorldTransform(MatrixD worldTransform)
        {
            Vector3D.Transform(ref this.m_relativeCenter, ref worldTransform, out this.m_center);
        }
    }
}

