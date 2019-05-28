namespace Sandbox.Game.Weapons
{
    using System;
    using VRageMath;

    public class MyDrillCutOut
    {
        private float m_centerOffset;
        private float m_radius;
        protected BoundingSphereD m_sphere;

        public MyDrillCutOut(float centerOffset, float radius)
        {
            this.m_centerOffset = centerOffset;
            this.m_radius = radius;
            this.m_sphere = new BoundingSphereD(Vector3D.Zero, (double) this.m_radius);
        }

        public void UpdatePosition(ref MatrixD worldMatrix)
        {
            this.m_sphere.Center = worldMatrix.Translation + (worldMatrix.Forward * this.m_centerOffset);
        }

        public BoundingSphereD Sphere =>
            this.m_sphere;
    }
}

