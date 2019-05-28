namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    public class MyBrushSphere : IMyVoxelBrush
    {
        public static MyBrushSphere Static = new MyBrushSphere();
        private MyShapeSphere m_shape = new MyShapeSphere();
        private MatrixD m_transform;
        private MyBrushGUIPropertyNumberSlider m_radius;
        private List<MyGuiControlBase> m_list;

        private MyBrushSphere()
        {
            this.m_shape.Radius = this.MinScale;
            this.m_transform = MatrixD.Identity;
            this.m_radius = new MyBrushGUIPropertyNumberSlider(this.m_shape.Radius, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.First, MyCommonTexts.VoxelHandProperty_Sphere_Radius);
            this.m_radius.ValueChanged = (Action) Delegate.Combine(this.m_radius.ValueChanged, new Action(this.RadiusChanged));
            this.m_list = new List<MyGuiControlBase>();
            this.m_radius.AddControlsToList(this.m_list);
        }

        public void CutOut(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void Draw(ref Color color)
        {
            MyStringId? faceMaterial = null;
            faceMaterial = null;
            MySimpleObjectDraw.DrawTransparentSphere(ref this.m_transform, this.m_shape.Radius, ref color, MySimpleObjectRasterizer.Solid, 20, faceMaterial, faceMaterial, -1f, -1, null, MyBillboard.BlendTypeEnum.LDR, 1f);
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

        private void RadiusChanged()
        {
            this.m_shape.Radius = this.m_radius.Value;
        }

        public void Revert(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestRevertShape(map, this.m_shape);
        }

        public void SetPosition(ref Vector3D targetPosition)
        {
            this.m_shape.Center = targetPosition;
            this.m_transform.Translation = targetPosition;
        }

        public void SetRotation(ref MatrixD rotationMat)
        {
        }

        public float MinScale =>
            1.5f;

        public float MaxScale =>
            (MySessionComponentVoxelHand.GRID_SIZE * 40f);

        public bool AutoRotate =>
            false;

        public string SubtypeName =>
            "Sphere";
    }
}

