namespace Sandbox.Engine.Voxels
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI;
    using VRageMath;

    public abstract class MyShape : IMyVoxelShape
    {
        protected MatrixD m_transformation = MatrixD.Identity;
        protected MatrixD m_inverse = MatrixD.Identity;
        protected bool m_inverseIsDirty;

        protected MyShape()
        {
        }

        public abstract MyShape Clone();
        public abstract BoundingBoxD GetLocalBounds();
        public abstract float GetVolume(ref Vector3D voxelPosition);
        public abstract BoundingBoxD GetWorldBoundaries();
        public abstract BoundingBoxD PeekWorldBoundaries(ref Vector3D targetPosition);
        public abstract void SendCutOutRequest(MyVoxelBase voxelbool);
        public virtual void SendDrillCutOutRequest(MyVoxelBase voxel, bool damage = false)
        {
        }

        public abstract void SendFillRequest(MyVoxelBase voxel, byte newMaterialIndex);
        public abstract void SendPaintRequest(MyVoxelBase voxel, byte newMaterialIndex);
        public abstract void SendRevertRequest(MyVoxelBase voxel);
        protected float SignedDistanceToDensity(float signedDistance) => 
            ((MathHelper.Clamp(-signedDistance, -1f, 1f) * 0.5f) + 0.5f);

        float IMyVoxelShape.GetIntersectionVolume(ref Vector3D voxelPosition) => 
            this.GetVolume(ref voxelPosition);

        BoundingBoxD IMyVoxelShape.GetWorldBoundary() => 
            this.GetWorldBoundaries();

        BoundingBoxD IMyVoxelShape.PeekWorldBoundary(ref Vector3D targetPosition) => 
            this.PeekWorldBoundaries(ref targetPosition);

        MatrixD IMyVoxelShape.Transform
        {
            get => 
                this.Transformation;
            set => 
                (this.Transformation = value);
        }

        public MatrixD Transformation
        {
            get => 
                this.m_transformation;
            set
            {
                this.m_transformation = value;
                this.m_inverseIsDirty = true;
            }
        }

        public MatrixD InverseTransformation
        {
            get
            {
                if (this.m_inverseIsDirty)
                {
                    MatrixD.Invert(ref this.m_transformation, out this.m_inverse);
                    this.m_inverseIsDirty = false;
                }
                return this.m_inverse;
            }
        }
    }
}

