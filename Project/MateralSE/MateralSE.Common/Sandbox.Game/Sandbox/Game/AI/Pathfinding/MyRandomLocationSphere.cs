namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRageMath;
    using VRageRender;

    public class MyRandomLocationSphere : IMyDestinationShape
    {
        private Vector3D m_center;
        private Vector3D m_relativeCenter;
        private Vector3D m_desiredDirection;
        private float m_radius;

        public MyRandomLocationSphere(Vector3D worldCenter, float radius, Vector3D direction)
        {
            this.Init(ref worldCenter, radius, direction);
        }

        public void DebugDraw()
        {
            MyRenderProxy.DebugDrawSphere(this.m_center, this.m_radius, Color.Gainsboro, 1f, true, false, true, false);
            MyRenderProxy.DebugDrawSphere(this.m_center + (this.m_desiredDirection * this.m_radius), 4f, Color.Aqua, 1f, true, false, true, false);
        }

        public Vector3D GetBestPoint(Vector3D queryPoint) => 
            (((queryPoint - this.m_center).Length() <= this.m_radius) ? (this.m_center + (this.m_desiredDirection * this.m_radius)) : queryPoint);

        public Vector3D GetClosestPoint(Vector3D queryPoint)
        {
            Vector3D v = queryPoint - this.m_center;
            return ((v.Normalize() <= this.m_radius) ? ((this.m_desiredDirection.Dot(ref v) <= 0.9) ? (this.m_center + (this.m_desiredDirection * this.m_radius)) : (this.m_center + (v * this.m_radius))) : queryPoint);
        }

        public Vector3D GetDestination() => 
            (this.m_center + (this.m_desiredDirection * this.m_radius));

        public void Init(ref Vector3D worldCenter, float radius, Vector3D direction)
        {
            this.m_center = worldCenter;
            this.m_radius = radius;
            this.m_desiredDirection = direction;
        }

        public float PointAdmissibility(Vector3D position, float tolerance)
        {
            Vector3D vectord = position - this.m_center;
            float num = (float) vectord.Normalize();
            if ((num < (this.m_radius + tolerance)) || (vectord.Dot(ref this.m_desiredDirection) < 0.9))
            {
                return float.PositiveInfinity;
            }
            return num;
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

