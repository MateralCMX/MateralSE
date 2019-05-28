namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    public class MyBrushAutoLevel : IMyVoxelBrush
    {
        public static MyBrushAutoLevel Static = new MyBrushAutoLevel();
        private MyShapeBox m_shape = new MyShapeBox();
        private MatrixD m_transform = MatrixD.Identity;
        private MyBrushGUIPropertyNumberCombo m_axis = new MyBrushGUIPropertyNumberCombo(MyVoxelBrushGUIPropertyOrder.First, MyCommonTexts.VoxelHandProperty_AutoLevel_Axis);
        private MyBrushGUIPropertyNumberSlider m_area;
        private MyBrushGUIPropertyNumberSlider m_height;
        private List<MyGuiControlBase> m_list;
        private const long X_ASIS = 0L;
        private const long Y_ASIS = 1L;
        private const long Z_ASIS = 2L;
        private bool m_painting;
        private double m_Xpos;
        private double m_Ypos;
        private double m_Zpos;

        private MyBrushAutoLevel()
        {
            this.m_axis.AddItem(0L, MyCommonTexts.VoxelHandProperty_AutoLevel_AxisX);
            this.m_axis.AddItem(1L, MyCommonTexts.VoxelHandProperty_AutoLevel_AxisY);
            this.m_axis.AddItem(2L, MyCommonTexts.VoxelHandProperty_AutoLevel_AxisZ);
            this.m_axis.SelectItem(1L);
            this.m_area = new MyBrushGUIPropertyNumberSlider(this.MinScale * 2f, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.Second, MyCommonTexts.VoxelHandProperty_AutoLevel_Area);
            this.m_area.ValueChanged = (Action) Delegate.Combine(this.m_area.ValueChanged, new Action(this.RecomputeShape));
            this.m_height = new MyBrushGUIPropertyNumberSlider(this.MinScale, this.MinScale, this.MaxScale, 0.5f, MyVoxelBrushGUIPropertyOrder.Third, MyCommonTexts.VoxelHandProperty_Box_Height);
            this.m_height.ValueChanged = (Action) Delegate.Combine(this.m_height.ValueChanged, new Action(this.RecomputeShape));
            this.m_list = new List<MyGuiControlBase>();
            this.m_axis.AddControlsToList(this.m_list);
            this.m_area.AddControlsToList(this.m_list);
            this.m_height.AddControlsToList(this.m_list);
            this.RecomputeShape();
        }

        public void CutOut(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void Draw(ref Color color)
        {
            MyStringId? faceMaterial = null;
            faceMaterial = null;
            MySimpleObjectDraw.DrawTransparentBox(ref this.m_transform, ref this.m_shape.Boundaries, ref color, MySimpleObjectRasterizer.Solid, 1, 0.04f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
        }

        public void Fill(MyVoxelBase map, byte matId)
        {
            MyVoxelGenerator.RequestFillInShape(map, this.m_shape, matId);
        }

        public void FixAxis()
        {
            long num;
            this.m_painting = true;
            Vector3D center = this.m_shape.Boundaries.TransformFast(this.m_transform).Center;
            if (num > 2L)
            {
                long selectedKey = this.m_axis.SelectedKey;
            }
            else
            {
                switch (((uint) this.m_axis.SelectedKey))
                {
                    case 0:
                        this.m_Xpos = center.X;
                        return;

                    case 1:
                        this.m_Ypos = center.Y;
                        return;

                    case 2:
                        this.m_Zpos = center.Z;
                        return;
                }
            }
        }

        public BoundingBoxD GetBoundaries() => 
            this.m_shape.Boundaries;

        public List<MyGuiControlBase> GetGuiControls() => 
            this.m_list;

        public BoundingBoxD GetWorldBoundaries() => 
            this.m_shape.GetWorldBoundaries();

        public void Paint(MyVoxelBase map, byte matId)
        {
        }

        public BoundingBoxD PeekWorldBoundingBox(ref Vector3D targetPosition) => 
            this.m_shape.PeekWorldBoundaries(ref targetPosition);

        private void RecomputeShape()
        {
            double num = this.m_area.Value * 0.5;
            double num2 = this.m_height.Value * 0.5;
            this.m_shape.Boundaries.Min.X = -num;
            this.m_shape.Boundaries.Min.Y = -num2;
            this.m_shape.Boundaries.Min.Z = -num;
            this.m_shape.Boundaries.Max.X = num;
            this.m_shape.Boundaries.Max.Y = num2;
            this.m_shape.Boundaries.Max.Z = num;
        }

        public void Revert(MyVoxelBase map)
        {
            MyVoxelGenerator.RequestCutOutShape(map, this.m_shape);
        }

        public void SetPosition(ref Vector3D targetPosition)
        {
            if (this.m_painting)
            {
                long num;
                if (num > 2L)
                {
                    long selectedKey = this.m_axis.SelectedKey;
                }
                else
                {
                    switch (((uint) this.m_axis.SelectedKey))
                    {
                        case 0:
                            targetPosition.X = this.m_Xpos;
                            break;

                        case 1:
                            targetPosition.Y = this.m_Ypos;
                            break;

                        case 2:
                            targetPosition.Z = this.m_Zpos;
                            break;

                        default:
                            break;
                    }
                }
            }
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

        public void UnFix()
        {
            this.m_painting = false;
        }

        public float MinScale =>
            MySessionComponentVoxelHand.GRID_SIZE;

        public float MaxScale =>
            (MySessionComponentVoxelHand.GRID_SIZE * 40f);

        public bool AutoRotate =>
            true;

        public string SubtypeName =>
            "AutoLevel";
    }
}

