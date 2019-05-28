namespace Sandbox.Engine.Voxels
{
    using System;
    using VRage.Noise;
    using VRageMath;
    using VRageRender;

    internal sealed class MyCsgTorus : MyCsgShapeBase
    {
        private Vector3 m_translation;
        private Quaternion m_invRotation;
        private float m_primaryRadius;
        private float m_secondaryRadius;
        private float m_secondaryHalfDeviation;
        private float m_deviationFrequency;
        private float m_detailFrequency;
        private float m_potentialHalfDeviation;

        internal MyCsgTorus(Vector3 translation, Quaternion invRotation, float primaryRadius, float secondaryRadius, float secondaryHalfDeviation, float deviationFrequency, float detailFrequency)
        {
            this.m_translation = translation;
            this.m_invRotation = invRotation;
            this.m_primaryRadius = primaryRadius;
            this.m_secondaryRadius = secondaryRadius;
            this.m_deviationFrequency = deviationFrequency;
            this.m_detailFrequency = detailFrequency;
            this.m_potentialHalfDeviation = this.m_secondaryHalfDeviation + base.m_detailSize;
            if (this.m_detailFrequency == 0f)
            {
                base.m_enableModulation = false;
            }
        }

        internal override Vector3 Center() => 
            this.m_translation;

        internal override unsafe ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            BoundingSphere sphere = querySphere;
            Vector3* vectorPtr1 = (Vector3*) ref sphere.Center;
            vectorPtr1[0] -= this.m_translation;
            Vector3.Transform(ref sphere.Center, ref this.m_invRotation, out sphere.Center);
            float* singlePtr1 = (float*) ref sphere.Radius;
            singlePtr1[0] += lodVoxelSize;
            float num = new Vector2(new Vector2(sphere.Center.X, sphere.Center.Z).Length() - this.m_primaryRadius, sphere.Center.Y).Length() - this.m_secondaryRadius;
            float num2 = (this.m_potentialHalfDeviation + lodVoxelSize) + sphere.Radius;
            return ((num <= num2) ? ((num >= -num2) ? ContainmentType.Intersects : ContainmentType.Contains) : ContainmentType.Disjoint);
        }

        internal override unsafe void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            MatrixD xd2;
            MatrixD xd = MatrixD.CreateTranslation(this.m_translation) * worldTranslation;
            float num = (this.m_primaryRadius + this.m_secondaryRadius) * 2f;
            float num2 = this.m_secondaryRadius * 2f;
            MatrixD.CreateFromQuaternion(ref this.m_invRotation, out xd2);
            MatrixD* xdPtr1 = (MatrixD*) ref xd2;
            MatrixD.Transpose(ref (MatrixD) ref xdPtr1, out xd2);
            MyRenderProxy.DebugDrawCylinder((MatrixD.CreateScale((double) num, (double) num2, (double) num) * xd2) * xd, color.ToVector3(), 0.5f, true, false, false);
        }

        internal override MyCsgShapeBase DeepCopy() => 
            new MyCsgTorus(this.m_translation, this.m_invRotation, this.m_primaryRadius, this.m_secondaryRadius, this.m_secondaryHalfDeviation, this.m_deviationFrequency, this.m_detailFrequency);

        internal override void ShrinkTo(float percentage)
        {
            this.m_secondaryRadius *= percentage;
            this.m_secondaryHalfDeviation *= percentage;
        }

        internal override unsafe float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 result = position - this.m_translation;
            Vector3* vectorPtr1 = (Vector3*) ref result;
            Vector3.Transform(ref (Vector3) ref vectorPtr1, ref this.m_invRotation, out result);
            float signedDistance = new Vector2(new Vector2(result.X, result.Z).Length() - this.m_primaryRadius, result.Y).Length() - this.m_secondaryRadius;
            float num2 = this.m_potentialHalfDeviation + lodVoxelSize;
            return ((signedDistance <= num2) ? ((signedDistance >= -num2) ? this.SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref result, ref signedDistance) : -1f) : 1f);
        }

        private float SignedDistanceInternal(float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator, ref Vector3 localPosition, ref float signedDistance)
        {
            if (base.m_enableModulation)
            {
                float num = 0.5f * this.m_deviationFrequency;
                Vector3 vector = localPosition * num;
                float num2 = (float) macroModulator.GetValue((double) vector.X, (double) vector.Y, (double) vector.Z);
                signedDistance -= num2 * this.m_secondaryHalfDeviation;
            }
            if ((base.m_enableModulation && (-base.m_detailSize < signedDistance)) && (signedDistance < base.m_detailSize))
            {
                float num3 = 0.5f * this.m_detailFrequency;
                Vector3 vector2 = localPosition * num3;
                signedDistance += base.m_detailSize * ((float) detailModulator.GetValue((double) vector2.X, (double) vector2.Y, (double) vector2.Z));
            }
            return (signedDistance / lodVoxelSize);
        }

        internal override unsafe float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 result = position - this.m_translation;
            Vector3* vectorPtr1 = (Vector3*) ref result;
            Vector3.Transform(ref (Vector3) ref vectorPtr1, ref this.m_invRotation, out result);
            float signedDistance = new Vector2(new Vector2(result.X, result.Z).Length() - this.m_primaryRadius, result.Y).Length() - this.m_secondaryRadius;
            return this.SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref result, ref signedDistance);
        }
    }
}

