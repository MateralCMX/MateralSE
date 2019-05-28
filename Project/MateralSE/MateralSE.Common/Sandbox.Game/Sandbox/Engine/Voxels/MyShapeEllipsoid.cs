namespace Sandbox.Engine.Voxels
{
    using Sandbox.Game.Entities;
    using System;
    using VRageMath;

    public class MyShapeEllipsoid : MyShape
    {
        private BoundingBoxD m_boundaries;
        private Matrix m_scaleMatrix = Matrix.Identity;
        private Matrix m_scaleMatrixInverse = Matrix.Identity;
        private Vector3 m_radius;

        public override MyShape Clone()
        {
            MyShapeEllipsoid ellipsoid1 = new MyShapeEllipsoid();
            ellipsoid1.Transformation = base.Transformation;
            ellipsoid1.Radius = this.Radius;
            return ellipsoid1;
        }

        public override BoundingBoxD GetLocalBounds() => 
            this.m_boundaries;

        public override float GetVolume(ref Vector3D voxelPosition)
        {
            if (base.m_inverseIsDirty)
            {
                base.m_inverse = MatrixD.Invert(base.m_transformation);
                base.m_inverseIsDirty = false;
            }
            voxelPosition = Vector3D.Transform(voxelPosition, base.m_inverse);
            Vector3 position = (Vector3) Vector3D.Transform(voxelPosition, this.m_scaleMatrixInverse);
            position.Normalize();
            Vector3 vector2 = Vector3.Transform(position, this.m_scaleMatrix);
            float signedDistance = ((float) voxelPosition.Length()) - vector2.Length();
            return base.SignedDistanceToDensity(signedDistance);
        }

        public override BoundingBoxD GetWorldBoundaries() => 
            this.m_boundaries.TransformFast(base.Transformation);

        public override BoundingBoxD PeekWorldBoundaries(ref Vector3D targetPosition)
        {
            MatrixD transformation = base.Transformation;
            transformation.Translation = targetPosition;
            return this.m_boundaries.TransformFast(transformation);
        }

        public override void SendCutOutRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationElipsoid(this.Radius, base.Transformation, 0, MyVoxelBase.OperationType.Cut);
        }

        public override void SendFillRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationElipsoid(this.Radius, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Fill);
        }

        public override void SendPaintRequest(MyVoxelBase voxel, byte newMaterialIndex)
        {
            voxel.RequestVoxelOperationElipsoid(this.Radius, base.Transformation, newMaterialIndex, MyVoxelBase.OperationType.Paint);
        }

        public override void SendRevertRequest(MyVoxelBase voxel)
        {
            voxel.RequestVoxelOperationElipsoid(this.Radius, base.Transformation, 0, MyVoxelBase.OperationType.Revert);
        }

        public Vector3 Radius
        {
            get => 
                this.m_radius;
            set
            {
                this.m_radius = value;
                this.m_scaleMatrix = Matrix.CreateScale(this.m_radius);
                this.m_scaleMatrixInverse = Matrix.Invert(this.m_scaleMatrix);
                this.m_boundaries = new BoundingBoxD(-this.Radius, this.Radius);
            }
        }

        public BoundingBoxD Boundaries =>
            this.m_boundaries;
    }
}

