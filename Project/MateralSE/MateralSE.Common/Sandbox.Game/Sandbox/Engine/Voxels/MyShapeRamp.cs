namespace Sandbox.Engine.Voxels
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyShapeRamp : MyShape, IMyVoxelShapeRamp, IMyVoxelShape
    {
        public BoundingBoxD Boundaries;
        public Vector3D RampNormal;
        public double RampNormalW;

        public override MyShape Clone()
        {
            MyShapeRamp ramp1 = new MyShapeRamp();
            ramp1.Transformation = base.Transformation;
            ramp1.Boundaries = this.Boundaries;
            ramp1.RampNormal = this.RampNormal;
            ramp1.RampNormalW = this.RampNormalW;
            return ramp1;
        }

        public override BoundingBoxD GetLocalBounds() => 
            this.Boundaries;

        public override float GetVolume(ref Vector3D voxelPosition)
        {
            if (base.m_inverseIsDirty)
            {
                base.m_inverse = MatrixD.Invert(base.m_transformation);
                base.m_inverseIsDirty = false;
            }
            voxelPosition = Vector3D.Transform(voxelPosition, base.m_inverse);
            Vector3D vectord = Vector3D.Abs(voxelPosition) - this.Boundaries.HalfExtents;
            double num = Vector3D.Dot(voxelPosition, this.RampNormal) + this.RampNormalW;
            return base.SignedDistanceToDensity((float) Math.Max(vectord.Max(), -num));
        }

        public override BoundingBoxD GetWorldBoundaries() => 
            this.Boundaries.TransformFast(base.Transformation);

        public override BoundingBoxD PeekWorldBoundaries(ref Vector3D targetPosition)
        {
            MatrixD transformation = base.Transformation;
            transformation.Translation = targetPosition;
            return this.Boundaries.TransformFast(transformation);
        }

        public override void SendCutOutRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationRamp(this.Boundaries, this.RampNormal, this.RampNormalW, base.Transformation, 0, MyVoxelBase.OperationType.Cut);
        }

        public override void SendFillRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationRamp(this.Boundaries, this.RampNormal, this.RampNormalW, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Fill);
        }

        public override void SendPaintRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationRamp(this.Boundaries, this.RampNormal, this.RampNormalW, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Paint);
        }

        public override void SendRevertRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationRamp(this.Boundaries, this.RampNormal, this.RampNormalW, base.Transformation, 0, MyVoxelBase.OperationType.Revert);
        }

        BoundingBoxD IMyVoxelShapeRamp.Boundaries
        {
            get => 
                this.Boundaries;
            set => 
                (this.Boundaries = value);
        }

        Vector3D IMyVoxelShapeRamp.RampNormal
        {
            get => 
                this.RampNormal;
            set => 
                (this.RampNormal = value);
        }

        double IMyVoxelShapeRamp.RampNormalW
        {
            get => 
                this.RampNormalW;
            set => 
                (this.RampNormalW = value);
        }
    }
}

