namespace Sandbox.Engine.Voxels
{
    using System;
    using VRage.Noise;
    using VRageMath;

    internal abstract class MyCsgShapeBase
    {
        protected bool m_enableModulation = true;
        protected float m_detailSize = 6f;

        protected MyCsgShapeBase()
        {
        }

        internal abstract Vector3 Center();
        internal abstract ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize);
        internal virtual void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
        }

        internal abstract MyCsgShapeBase DeepCopy();
        internal virtual void ReleaseMaps()
        {
        }

        internal abstract void ShrinkTo(float percentage);
        internal abstract float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator);
        internal abstract float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator);
    }
}

