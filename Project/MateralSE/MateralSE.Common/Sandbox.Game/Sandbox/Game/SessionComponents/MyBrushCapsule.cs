namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    public class MyBrushCapsule : IMyVoxelBrush
    {
        public static MyBrushCapsule Static = new MyBrushCapsule();
        private MyShapeCapsule m_shape = new MyShapeCapsule();
        private MatrixD m_transform = MatrixD.Identity;
        private MyBrushGUIPropertyNumberSlider m_radius;
        private MyBrushGUIPropertyNumberSlider m_length;
        private List<MyGuiControlBase> m_list;

        private MyBrushCapsule()
        {
            this.m_radius = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.First, MyCommonTexts.VoxelHandProperty_Capsule_Radius);
            this.m_radius.ValueChanged = (Action) Delegate.Combine(this.m_radius.ValueChanged, new Action(this.RecomputeShape));
            this.m_length = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.Second, MyCommonTexts.VoxelHandProperty_Capsule_Length);
            this.m_length.ValueChanged = (Action) Delegate.Combine(this.m_length.ValueChanged, new Action(this.RecomputeShape));
            this.m_list = new List<MyGuiControlBase>();
            this.m_radius.AddControlsToList(this.m_list);
            this.m_length.AddControlsToList(this.m_list);
            this.RecomputeShape();
        }

        public void CutOut(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void Draw(ref Color color)
        {
            MyStringId? faceMaterial = null;
            MySimpleObjectDraw.DrawTransparentCapsule(ref this.m_transform, this.m_shape.Radius, this.m_length.Value, ref color, 20, faceMaterial, -1, MyBillboard.BlendTypeEnum.LDR);
        }

        public void Fill(MyVoxelBase map, byte matId)
        {
            MyVoxelGenerator.RequestFillInShape(map, this.m_shape, matId);
        }

        public BoundingBoxD GetBoundaries() => 
            this.m_shape.GetWorldBoundaries();

        public List<MyGuiControlBase> GetGuiControls() => 
            this.m_list;

        public BoundingBoxD GetWorldBoundaries() => 
            this.m_shape.GetWorldBoundaries();

        public void Paint(MyVoxelBase map, byte matId)
        {
            MyVoxelGenerator.RequestPaintInShape(map, this.m_shape, matId);
        }

        public BoundingBoxD PeekWorldBoundingBox(ref Vector3D targetPosition) => 
            this.m_shape.PeekWorldBoundaries(ref targetPosition);

        private void RecomputeShape()
        {
            this.m_shape.Radius = this.m_radius.Value;
            double num = this.m_length.Value * 0.5;
            this.m_shape.A.X = this.m_shape.A.Z = 0.0;
            this.m_shape.B.X = this.m_shape.B.Z = 0.0;
            this.m_shape.A.Y = -num;
            this.m_shape.B.Y = num;
        }

        public void Revert(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void SetPosition(ref Vector3D targetPosition)
        {
            this.m_transform.Translation = targetPosition;
            this.m_shape.Transformation = this.m_transform;
        }

        public void SetRotation(ref MatrixD rotationMat)
        {
            if (rotationMat.IsRotation())
            {
                this.m_transform.M11 = rotationMat.M11;
                this.m_transform.M12 = rotationMat.M12;
                this.m_transform.M13 = rotationMat.M13;
                this.m_transform.M21 = rotationMat.M21;
                this.m_transform.M22 = rotationMat.M22;
                this.m_transform.M23 = rotationMat.M23;
                this.m_transform.M31 = rotationMat.M31;
                this.m_transform.M32 = rotationMat.M32;
                this.m_transform.M33 = rotationMat.M33;
                this.m_shape.Transformation = this.m_transform;
            }
        }

        public float MinScale =>
            1.5f;

        public float MaxScale =>
            (MySessionComponentVoxelHand.GRID_SIZE * 40f);

        public bool AutoRotate =>
            true;

        public string SubtypeName =>
            "Capsule";
    }
}

