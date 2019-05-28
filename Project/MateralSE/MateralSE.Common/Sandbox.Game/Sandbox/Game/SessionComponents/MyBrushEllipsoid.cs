namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyBrushEllipsoid : IMyVoxelBrush
    {
        public static MyBrushEllipsoid Static = new MyBrushEllipsoid();
        private MyShapeEllipsoid m_shape = new MyShapeEllipsoid();
        private MatrixD m_transform = MatrixD.Identity;
        private MyBrushGUIPropertyNumberSlider m_radiusX;
        private MyBrushGUIPropertyNumberSlider m_radiusY;
        private MyBrushGUIPropertyNumberSlider m_radiusZ;
        private List<MyGuiControlBase> m_list;

        private MyBrushEllipsoid()
        {
            float valueStep = 0.25f;
            this.m_radiusX = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, valueStep, MyVoxelBrushGUIPropertyOrder.First, MyStringId.GetOrCompute("Radius X"));
            this.m_radiusX.ValueChanged = (Action) Delegate.Combine(this.m_radiusX.ValueChanged, new Action(this.RadiusChanged));
            this.m_radiusY = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, valueStep, MyVoxelBrushGUIPropertyOrder.Second, MyStringId.GetOrCompute("Radius Y"));
            this.m_radiusY.ValueChanged = (Action) Delegate.Combine(this.m_radiusY.ValueChanged, new Action(this.RadiusChanged));
            this.m_radiusZ = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, valueStep, MyVoxelBrushGUIPropertyOrder.Third, MyStringId.GetOrCompute("Radius Z"));
            this.m_radiusZ.ValueChanged = (Action) Delegate.Combine(this.m_radiusZ.ValueChanged, new Action(this.RadiusChanged));
            this.m_list = new List<MyGuiControlBase>();
            this.m_radiusX.AddControlsToList(this.m_list);
            this.m_radiusY.AddControlsToList(this.m_list);
            this.m_radiusZ.AddControlsToList(this.m_list);
            this.RecomputeShape();
        }

        public void CutOut(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void Draw(ref Color color)
        {
            BoundingBoxD boundaries = this.m_shape.Boundaries;
            MyStringId? faceMaterial = null;
            faceMaterial = null;
            MySimpleObjectDraw.DrawTransparentBox(ref this.m_transform, ref boundaries, ref color, MySimpleObjectRasterizer.Solid, 1, 0.04f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
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
            this.RecomputeShape();
        }

        private void RecomputeShape()
        {
            this.m_shape.Radius = new Vector3(this.m_radiusX.Value, this.m_radiusY.Value, this.m_radiusZ.Value);
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
            0.25f;

        public float MaxScale =>
            (MySessionComponentVoxelHand.GRID_SIZE * 40f);

        public bool AutoRotate =>
            false;

        public string SubtypeName =>
            "Ellipsoid";
    }
}

