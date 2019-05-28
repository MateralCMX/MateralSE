namespace Sandbox.Engine.Voxels
{
    using System;
    using VRage.Noise;
    using VRageMath;
    using VRageRender;

    internal class MyCsgCapsule : MyCsgShapeBase
    {
        private Vector3 m_pointA;
        private Vector3 m_pointB;
        private float m_radius;
        private float m_halfDeviation;
        private float m_deviationFrequency;
        private float m_detailFrequency;
        private float m_potentialHalfDeviation;

        public MyCsgCapsule(Vector3 pointA, Vector3 pointB, float radius, float halfDeviation, float deviationFrequency, float detailFrequency)
        {
            this.m_pointA = pointA;
            this.m_pointB = pointB;
            this.m_radius = radius;
            this.m_halfDeviation = halfDeviation;
            this.m_deviationFrequency = deviationFrequency;
            this.m_detailFrequency = detailFrequency;
            if (deviationFrequency == 0f)
            {
                base.m_enableModulation = false;
            }
            this.m_potentialHalfDeviation = this.m_halfDeviation + base.m_detailSize;
        }

        internal override Vector3 Center() => 
            ((this.m_pointA + this.m_pointB) / 2f);

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            Vector3 v = this.m_pointB - this.m_pointA;
            float num2 = MathHelper.Clamp((querySphere.Center - this.m_pointA).Dot(ref v), 0f, v.Normalize());
            Vector3 vector2 = this.m_pointA + (num2 * v);
            float num3 = (querySphere.Center - vector2).Length() - this.m_radius;
            float num4 = (this.m_potentialHalfDeviation + lodVoxelSize) + querySphere.Radius;
            return ((num3 <= num4) ? ((num3 >= -num4) ? ContainmentType.Intersects : ContainmentType.Contains) : ContainmentType.Disjoint);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            MyRenderProxy.DebugDrawCapsule(Vector3D.Transform(this.m_pointA, worldTranslation), Vector3D.Transform(this.m_pointB, worldTranslation), this.m_radius, color, true, true, false);
        }

        internal override MyCsgShapeBase DeepCopy() => 
            new MyCsgCapsule(this.m_pointA, this.m_pointB, this.m_radius, this.m_halfDeviation, this.m_deviationFrequency, this.m_detailFrequency);

        internal override void ShrinkTo(float percentage)
        {
            this.m_radius *= percentage;
            this.m_halfDeviation *= percentage;
        }

        internal override float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 vector = position - this.m_pointA;
            Vector3 v = this.m_pointB - this.m_pointA;
            float single1 = vector.Dot(ref v);
            float signedDistance = (vector - (v * MathHelper.Clamp((float) (single1 / v.LengthSquared()), (float) 0f, (float) 1f))).Length() - this.m_radius;
            float num3 = this.m_potentialHalfDeviation + lodVoxelSize;
            return ((signedDistance <= num3) ? ((signedDistance >= -num3) ? this.SignedDistanceInternal(ref position, lodVoxelSize, macroModulator, detailModulator, ref signedDistance) : -1f) : 1f);
        }

        private float SignedDistanceInternal(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator, ref float signedDistance)
        {
            if (base.m_enableModulation)
            {
                float num = (float) macroModulator.GetValue((double) (position.X * this.m_deviationFrequency), (double) (position.Y * this.m_deviationFrequency), (double) (position.Z * this.m_deviationFrequency));
                signedDistance -= num * this.m_halfDeviation;
            }
            if ((base.m_enableModulation && (-base.m_detailSize < signedDistance)) && (signedDistance < base.m_detailSize))
            {
                signedDistance += base.m_detailSize * ((float) detailModulator.GetValue((double) (position.X * this.m_detailFrequency), (double) (position.Y * this.m_detailFrequency), (double) (position.Z * this.m_detailFrequency)));
            }
            return (signedDistance / lodVoxelSize);
        }

        internal override float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 vector = position - this.m_pointA;
            Vector3 v = this.m_pointB - this.m_pointA;
            float single1 = vector.Dot(ref v);
            float signedDistance = (vector - (v * MathHelper.Clamp((float) (single1 / v.LengthSquared()), (float) 0f, (float) 1f))).Length() - this.m_radius;
            return this.SignedDistanceInternal(ref position, lodVoxelSize, macroModulator, detailModulator, ref signedDistance);
        }
    }
}

