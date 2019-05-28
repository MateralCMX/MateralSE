namespace Sandbox.Engine.Voxels
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyShapeSphere : MyShape, IMyVoxelShapeSphere, IMyVoxelShape
    {
        public Vector3D Center;
        public float Radius;

        public MyShapeSphere()
        {
        }

        public MyShapeSphere(Vector3D center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public override MyShape Clone()
        {
            MyShapeSphere sphere1 = new MyShapeSphere();
            sphere1.Transformation = base.Transformation;
            sphere1.Center = this.Center;
            sphere1.Radius = this.Radius;
            return sphere1;
        }

        public override BoundingBoxD GetLocalBounds() => 
            new BoundingBoxD(this.Center - this.Radius, this.Center + this.Radius);

        public override float GetVolume(ref Vector3D voxelPosition)
        {
            if (base.m_inverseIsDirty)
            {
                MatrixD.Invert(ref base.m_transformation, out base.m_inverse);
                base.m_inverseIsDirty = false;
            }
            Vector3D.Transform(ref voxelPosition, ref base.m_inverse, out voxelPosition);
            float signedDistance = ((float) (voxelPosition - this.Center).Length()) - this.Radius;
            return base.SignedDistanceToDensity(signedDistance);
        }

        public override BoundingBoxD GetWorldBoundaries()
        {
            BoundingBoxD xd = new BoundingBoxD(this.Center - this.Radius, this.Center + this.Radius);
            return xd.TransformFast(base.Transformation);
        }

        public override BoundingBoxD PeekWorldBoundaries(ref Vector3D targetPosition) => 
            new BoundingBoxD(targetPosition - this.Radius, targetPosition + this.Radius);

        public override void SendCutOutRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationSphere(this.Center, this.Radius, 0, MyVoxelBase.OperationType.Cut);
        }

        public override void SendDrillCutOutRequest(MyVoxelBase voxel, bool damage = false)
        {
            voxel.RequestVoxelCutoutSphere(this.Center, this.Radius, false, damage);
        }

        public override void SendFillRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationSphere(this.Center, this.Radius, newMaterialIndex, MyVoxelBase.OperationType.Fill);
        }

        public override void SendPaintRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationSphere(this.Center, this.Radius, newMaterialIndex, MyVoxelBase.OperationType.Paint);
        }

        public override void SendRevertRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationSphere(this.Center, this.Radius, 0, MyVoxelBase.OperationType.Revert);
        }

        Vector3D IMyVoxelShapeSphere.Center
        {
            get => 
                this.Center;
            set => 
                (this.Center = value);
        }

        float IMyVoxelShapeSphere.Radius
        {
            get => 
                this.Radius;
            set => 
                (this.Radius = value);
        }
    }
}

