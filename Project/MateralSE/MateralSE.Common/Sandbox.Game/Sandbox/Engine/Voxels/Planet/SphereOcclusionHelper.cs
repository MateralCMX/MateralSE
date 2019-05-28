namespace Sandbox.Engine.Voxels.Planet
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    public class SphereOcclusionHelper
    {
        private float m_minRadius;
        private float m_maxRadius;
        private float m_baseRadius;
        private Vector3 m_lastUpdatePosition;

        public SphereOcclusionHelper(float minRadius, float maxRadius)
        {
            this.m_minRadius = minRadius;
            this.m_maxRadius = maxRadius;
        }

        public void CalculateOcclusion(Vector3 position)
        {
        }

        public void DebugDraw(MatrixD worldMatrix)
        {
            Vector3D translation = worldMatrix.Translation;
            MyRenderProxy.DebugDrawSphere(translation, this.m_minRadius, Color.Red, 0.2f, true, true, true, false);
            MyRenderProxy.DebugDrawSphere(translation, this.m_maxRadius, Color.Red, 0.2f, true, true, true, false);
            float num = this.m_lastUpdatePosition.Length();
            Vector3 v = this.m_lastUpdatePosition / num;
            MyRenderProxy.DebugDrawLine3D(translation, translation + (this.OcclusionDistance * v), Color.Green, Color.Green, true, false);
            Vector3D baseVec = Vector3D.CalculatePerpendicularVector(v) * this.m_baseRadius;
            MyRenderProxy.DebugDrawCone(translation + (v * (num - this.OcclusionDistance)), v * this.OcclusionDistance, baseVec, Color.Blue, true, false);
        }

        public float OcclusionRange { get; private set; }

        public float OcclusionAngleCosine { get; private set; }

        public float OcclusionDistance { get; private set; }
    }
}

