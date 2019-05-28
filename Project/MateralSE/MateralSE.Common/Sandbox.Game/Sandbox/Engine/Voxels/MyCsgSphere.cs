namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Noise;
    using VRageMath;
    using VRageRender;

    internal class MyCsgSphere : MyCsgShapeBase
    {
        private Vector3 m_translation;
        private float m_radius;
        private float m_halfDeviation;
        private float m_deviationFrequency;
        private float m_detailFrequency;
        private float m_outerRadius;
        private float m_innerRadius;

        public MyCsgSphere(Vector3 translation, float radius, float halfDeviation = 0f, float deviationFrequency = 0f, float detailFrequency = 0f)
        {
            this.m_translation = translation;
            this.m_radius = radius;
            this.m_halfDeviation = halfDeviation;
            this.m_deviationFrequency = deviationFrequency;
            this.m_detailFrequency = detailFrequency;
            if (((this.m_halfDeviation == 0f) && (this.m_deviationFrequency == 0f)) && (detailFrequency == 0f))
            {
                base.m_enableModulation = false;
                base.m_detailSize = 0f;
            }
            this.ComputeDerivedProperties();
        }

        internal override Vector3 Center() => 
            this.m_translation;

        private void ComputeDerivedProperties()
        {
            this.m_outerRadius = (this.m_radius + this.m_halfDeviation) + base.m_detailSize;
            this.m_innerRadius = (this.m_radius - this.m_halfDeviation) - base.m_detailSize;
        }

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            ContainmentType type;
            ContainmentType type2;
            BoundingSphere sphere = new BoundingSphere(this.m_translation, this.m_outerRadius + lodVoxelSize);
            sphere.Contains(ref queryAabb, out type);
            if (type == ContainmentType.Disjoint)
            {
                return ContainmentType.Disjoint;
            }
            sphere.Radius = this.m_innerRadius - lodVoxelSize;
            sphere.Contains(ref queryAabb, out type2);
            return ((type2 != ContainmentType.Contains) ? ContainmentType.Intersects : ContainmentType.Contains);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            MyRenderProxy.DebugDrawSphere(Vector3D.Transform(this.m_translation, worldTranslation), this.m_radius, color.ToVector3(), 0.5f, true, false, true, false);
        }

        internal override MyCsgShapeBase DeepCopy() => 
            new MyCsgSphere(this.m_translation, this.m_radius, this.m_halfDeviation, this.m_deviationFrequency, this.m_detailFrequency);

        internal override void ShrinkTo(float percentage)
        {
            this.m_radius *= percentage;
            this.m_halfDeviation *= percentage;
            this.ComputeDerivedProperties();
        }

        internal override float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 localPosition = position - this.m_translation;
            float distance = localPosition.Length();
            return (((this.m_innerRadius - lodVoxelSize) <= distance) ? (((this.m_outerRadius + lodVoxelSize) >= distance) ? this.SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref localPosition, distance) : 1f) : -1f);
        }

        private float SignedDistanceInternal(float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator, ref Vector3 localPosition, float distance)
        {
            float num;
            if (!base.m_enableModulation)
            {
                num = 0f;
            }
            else
            {
                float num3 = 0f;
                if (distance != 0f)
                {
                    num3 = 1f / distance;
                }
                float num4 = (this.m_deviationFrequency * this.m_radius) * num3;
                Vector3 vector = localPosition * num4;
                num = (float) macroModulator.GetValue((double) vector.X, (double) vector.Y, (double) vector.Z);
            }
            float num2 = (distance - this.m_radius) - (num * this.m_halfDeviation);
            if ((base.m_enableModulation && (-base.m_detailSize < num2)) && (num2 < base.m_detailSize))
            {
                float num5 = (this.m_detailFrequency * this.m_radius) / ((distance == 0f) ? 1f : distance);
                Vector3 vector2 = localPosition * num5;
                num2 += base.m_detailSize * ((float) detailModulator.GetValue((double) vector2.X, (double) vector2.Y, (double) vector2.Z));
            }
            return (num2 / lodVoxelSize);
        }

        internal override float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 localPosition = position - this.m_translation;
            return this.SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref localPosition, localPosition.Length());
        }
    }
}

