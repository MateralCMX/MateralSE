namespace Sandbox.Engine.Voxels
{
    using System;
    using VRage.Noise;
    using VRageMath;
    using VRageRender;

    internal class MyCsgBox : MyCsgShapeBase
    {
        private Vector3 m_translation;
        private float m_halfExtents;

        public MyCsgBox(Vector3 translation, float halfExtents)
        {
            this.m_translation = translation;
            this.m_halfExtents = halfExtents;
        }

        internal override Vector3 Center() => 
            this.m_translation;

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            ContainmentType type;
            ContainmentType type2;
            BoundingBox.CreateFromHalfExtent(this.m_translation, (float) (this.m_halfExtents + lodVoxelSize)).Contains(ref queryAabb, out type);
            if (type == ContainmentType.Disjoint)
            {
                return ContainmentType.Disjoint;
            }
            BoundingBox.CreateFromHalfExtent(this.m_translation, (float) (this.m_halfExtents - lodVoxelSize)).Contains(ref queryAabb, out type2);
            return ((type2 != ContainmentType.Contains) ? ContainmentType.Intersects : ContainmentType.Contains);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            BoundingBoxD xd = new BoundingBoxD(this.m_translation - this.m_halfExtents, this.m_translation + this.m_halfExtents);
            MyRenderProxy.DebugDrawAABB(xd.TransformFast((MatrixD) worldTranslation), color, 0.5f, 1f, false, false, false);
        }

        internal override MyCsgShapeBase DeepCopy() => 
            new MyCsgBox(this.m_translation, this.m_halfExtents);

        internal override void ShrinkTo(float percentage)
        {
            this.m_halfExtents *= percentage;
        }

        internal override float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator) => 
            MathHelper.Clamp(this.SignedDistanceUnchecked(ref position, lodVoxelSize, macroModulator, detailModulator), -1f, 1f);

        internal override float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 vector = Vector3.Abs(position - this.m_translation) - this.m_halfExtents;
            return ((Math.Min(Math.Max(vector.X, Math.Max(vector.Y, vector.Z)), 0f) + Vector3.Max(vector, Vector3.Zero).Length()) / lodVoxelSize);
        }

        internal float HalfExtents =>
            this.m_halfExtents;
    }
}

