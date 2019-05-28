namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRageMath;
    using VRageRender;

    public class MyDestinationRing : IMyDestinationShape
    {
        private float m_innerRadius;
        private float m_outerRadius;
        private Vector3D m_center;
        private Vector3D m_relativeCenter;

        public MyDestinationRing(ref Vector3D worldCenter, float innerRadius, float outerRadius)
        {
            this.Init(ref worldCenter, innerRadius, outerRadius);
        }

        public void DebugDraw()
        {
            MyRenderProxy.DebugDrawSphere(this.m_center, this.m_innerRadius, Color.RoyalBlue, 0.4f, true, false, true, false);
            MyRenderProxy.DebugDrawSphere(this.m_center, this.m_outerRadius, Color.Aqua, 0.4f, true, false, true, false);
        }

        public Vector3D GetBestPoint(Vector3D queryPoint)
        {
            Vector3D vectord = Vector3D.Normalize(queryPoint - this.m_center);
            return (this.m_center + (vectord * ((this.m_innerRadius + this.m_outerRadius) * 0.5f)));
        }

        public Vector3D GetClosestPoint(Vector3D queryPoint)
        {
            Vector3D vectord = queryPoint - this.m_center;
            double num = vectord.Length();
            return ((num >= this.m_innerRadius) ? ((num <= this.m_outerRadius) ? queryPoint : (this.m_center + ((vectord / num) * this.m_outerRadius))) : (this.m_center + ((vectord / num) * this.m_innerRadius)));
        }

        public Vector3D GetDestination() => 
            this.m_center;

        public void Init(ref Vector3D worldCenter, float innerRadius, float outerRadius)
        {
            this.m_center = worldCenter;
            this.m_innerRadius = innerRadius;
            this.m_outerRadius = outerRadius;
        }

        public float PointAdmissibility(Vector3D position, float tolerance)
        {
            float num = (float) Vector3D.Distance(position, this.m_center);
            if ((num < Math.Min((float) (this.m_innerRadius - tolerance), (float) 0f)) || (num > (this.m_outerRadius + tolerance)))
            {
                return float.PositiveInfinity;
            }
            return num;
        }

        public void Reinit(ref Vector3D worldCenter)
        {
            this.m_center = worldCenter;
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

