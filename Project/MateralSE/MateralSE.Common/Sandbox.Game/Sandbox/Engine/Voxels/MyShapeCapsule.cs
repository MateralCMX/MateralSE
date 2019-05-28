namespace Sandbox.Engine.Voxels
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyShapeCapsule : MyShape, IMyVoxelShapeCapsule, IMyVoxelShape
    {
        public Vector3D A;
        public Vector3D B;
        public float Radius;

        public override MyShape Clone()
        {
            MyShapeCapsule capsule1 = new MyShapeCapsule();
            capsule1.Transformation = base.Transformation;
            capsule1.A = this.A;
            capsule1.B = this.B;
            capsule1.Radius = this.Radius;
            return capsule1;
        }

        public override BoundingBoxD GetLocalBounds() => 
            new BoundingBoxD(this.A - this.Radius, this.B + this.Radius);

        public override float GetVolume(ref Vector3D voxelPosition)
        {
            if (base.m_inverseIsDirty)
            {
                base.m_inverse = MatrixD.Invert(base.m_transformation);
                base.m_inverseIsDirty = false;
            }
            voxelPosition = Vector3D.Transform(voxelPosition, base.m_inverse);
            Vector3D vectord = voxelPosition - this.A;
            Vector3D v = this.B - this.A;
            double num1 = vectord.Dot(ref v);
            float signedDistance = ((float) (vectord - (v * MathHelper.Clamp((double) (num1 / v.LengthSquared()), (double) 0.0, (double) 1.0))).Length()) - this.Radius;
            return base.SignedDistanceToDensity(signedDistance);
        }

        public override BoundingBoxD GetWorldBoundaries()
        {
            BoundingBoxD xd = new BoundingBoxD(this.A - this.Radius, this.B + this.Radius);
            return xd.TransformFast(base.Transformation);
        }

        public override BoundingBoxD PeekWorldBoundaries(ref Vector3D targetPosition)
        {
            MatrixD transformation = base.Transformation;
            transformation.Translation = targetPosition;
            BoundingBoxD xd2 = new BoundingBoxD(this.A - this.Radius, this.B + this.Radius);
            return xd2.TransformFast(transformation);
        }

        public override void SendCutOutRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationCapsule(this.A, this.B, this.Radius, base.Transformation, 0, MyVoxelBase.OperationType.Cut);
        }

        public override void SendFillRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationCapsule(this.A, this.B, this.Radius, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Fill);
        }

        public override void SendPaintRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationCapsule(this.A, this.B, this.Radius, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Paint);
        }

        public override void SendRevertRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationCapsule(this.A, this.B, this.Radius, base.Transformation, 0, MyVoxelBase.OperationType.Revert);
        }

        Vector3D IMyVoxelShapeCapsule.A
        {
            get => 
                this.A;
            set => 
                (this.A = value);
        }

        Vector3D IMyVoxelShapeCapsule.B
        {
            get => 
                this.B;
            set => 
                (this.B = value);
        }

        float IMyVoxelShapeCapsule.Radius
        {
            get => 
                this.Radius;
            set => 
                (this.Radius = value);
        }
    }
}

