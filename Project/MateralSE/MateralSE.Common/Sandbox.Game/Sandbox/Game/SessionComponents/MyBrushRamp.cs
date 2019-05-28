namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    public class MyBrushRamp : IMyVoxelBrush
    {
        public static MyBrushRamp Static = new MyBrushRamp();
        private MyShapeRamp m_shape = new MyShapeRamp();
        private MatrixD m_transform = MatrixD.Identity;
        private MyBrushGUIPropertyNumberSlider m_width;
        private MyBrushGUIPropertyNumberSlider m_height;
        private MyBrushGUIPropertyNumberSlider m_depth;
        private List<MyGuiControlBase> m_list;

        private MyBrushRamp()
        {
            this.m_width = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.First, MyCommonTexts.VoxelHandProperty_Box_Width);
            this.m_width.ValueChanged = (Action) Delegate.Combine(this.m_width.ValueChanged, new Action(this.RecomputeShape));
            this.m_height = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.Second, MyCommonTexts.VoxelHandProperty_Box_Height);
            this.m_height.ValueChanged = (Action) Delegate.Combine(this.m_height.ValueChanged, new Action(this.RecomputeShape));
            this.m_depth = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.Third, MyCommonTexts.VoxelHandProperty_Box_Depth);
            this.m_depth.ValueChanged = (Action) Delegate.Combine(this.m_depth.ValueChanged, new Action(this.RecomputeShape));
            this.m_list = new List<MyGuiControlBase>();
            this.m_width.AddControlsToList(this.m_list);
            this.m_height.AddControlsToList(this.m_list);
            this.m_depth.AddControlsToList(this.m_list);
            this.RecomputeShape();
        }

        public void CutOut(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void Draw(ref Color color)
        {
            MyStringId? faceMaterial = null;
            MySimpleObjectDraw.DrawTransparentRamp(ref this.m_transform, ref this.m_shape.Boundaries, ref color, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.LDR);
        }

        public void Fill(MyVoxelBase map, byte matId)
        {
            MyVoxelGenerator.RequestFillInShape(map, this.m_shape, matId);
        }

        public BoundingBoxD GetBoundaries() => 
            this.m_shape.Boundaries;

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

        private unsafe void RecomputeShape()
        {
            Vector3D vectord = new Vector3D((double) this.m_width.Value, (double) this.m_height.Value, (double) this.m_depth.Value) * 0.5;
            this.m_shape.Boundaries.Min = -vectord;
            this.m_shape.Boundaries.Max = vectord;
            Vector3D min = this.m_shape.Boundaries.Min;
            double* numPtr1 = (double*) ref min.X;
            numPtr1[0] -= this.m_shape.Boundaries.Size.Z;
            Vector3D vectord3 = Vector3D.Normalize((this.m_shape.Boundaries.Min - min).Cross(this.m_shape.Boundaries.Max - min));
            double num = vectord3.Dot(min);
            this.m_shape.RampNormal = vectord3;
            this.m_shape.RampNormalW = -num;
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
            4.5f;

        public float MaxScale =>
            (MySessionComponentVoxelHand.GRID_SIZE * 40f);

        public bool AutoRotate =>
            true;

        public string SubtypeName =>
            "Ramp";
    }
}

