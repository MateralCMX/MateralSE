namespace Sandbox.Engine.Voxels
{
    using System;
    using VRage.Noise;
    using VRageMath;
    using VRageRender;

    internal class MyCsgSimpleSphere : MyCsgShapeBase
    {
        private Vector3 m_translation;
        private float m_radius;

        public MyCsgSimpleSphere(Vector3 translation, float radius)
        {
            this.m_translation = translation;
            this.m_radius = radius;
        }

        internal override Vector3 Center() => 
            this.m_translation;

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            ContainmentType type;
            ContainmentType type2;
            BoundingSphere sphere = new BoundingSphere(this.m_translation, this.m_radius + lodVoxelSize);
            sphere.Contains(ref queryAabb, out type);
            if (type == ContainmentType.Disjoint)
            {
                return ContainmentType.Disjoint;
            }
            sphere.Radius = this.m_radius - lodVoxelSize;
            sphere.Contains(ref queryAabb, out type2);
            return ((type2 != ContainmentType.Contains) ? ContainmentType.Intersects : ContainmentType.Contains);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            MyRenderProxy.DebugDrawSphere(Vector3D.Transform(this.m_translation, worldTranslation), this.m_radius, color.ToVector3(), 0.5f, true, false, true, false);
        }

        internal override MyCsgShapeBase DeepCopy() => 
            new MyCsgSimpleSphere(this.m_translation, this.m_radius);

        internal override void ShrinkTo(float percentage)
        {
            this.m_radius *= percentage;
        }

        internal override float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            float distance = (position - this.m_translation).Length();
            return (((this.m_radius - lodVoxelSize) <= distance) ? (((this.m_radius + lodVoxelSize) >= distance) ? this.SignedDistanceInternal(lodVoxelSize, distance) : 1f) : -1f);
        }

        private float SignedDistanceInternal(float lodVoxelSize, float distance) => 
            ((distance - this.m_radius) / lodVoxelSize);

        internal override float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            float distance = (position - this.m_translation).Length();
            return this.SignedDistanceInternal(lodVoxelSize, distance);
        }
    }
}

