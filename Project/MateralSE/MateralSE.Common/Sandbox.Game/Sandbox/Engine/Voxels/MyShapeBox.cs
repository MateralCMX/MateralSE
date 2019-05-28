namespace Sandbox.Engine.Voxels
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyShapeBox : MyShape, IMyVoxelShapeBox, IMyVoxelShape
    {
        public BoundingBoxD Boundaries;

        public override MyShape Clone()
        {
            MyShapeBox box1 = new MyShapeBox();
            box1.Transformation = base.Transformation;
            box1.Boundaries = this.Boundaries;
            return box1;
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
            Vector3D center = this.Boundaries.Center;
            Vector3D vectord2 = Vector3D.Abs(voxelPosition - center) - (center - this.Boundaries.Min);
            return base.SignedDistanceToDensity((float) vectord2.Max());
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
            voxel.RequestVoxelOperationBox(this.Boundaries, base.Transformation, 0, MyVoxelBase.OperationType.Cut);
        }

        public override void SendFillRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationBox(this.Boundaries, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Fill);
        }

        public override void SendPaintRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationBox(this.Boundaries, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Paint);
        }

        public override void SendRevertRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationBox(this.Boundaries, base.Transformation, 0, MyVoxelBase.OperationType.Revert);
        }

        BoundingBoxD IMyVoxelShapeBox.Boundaries
        {
            get => 
                this.Boundaries;
            set => 
                (this.Boundaries = value);
        }
    }
}

