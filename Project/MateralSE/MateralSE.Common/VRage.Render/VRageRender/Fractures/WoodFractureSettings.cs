namespace VRageRender.Fractures
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public class WoodFractureSettings : MyFractureSettings
    {
        private bool m_boardCustomAxis;
        private bool m_splinterCustomAxis;

        public WoodFractureSettings()
        {
            this.SplinterFractureLineShearingRange = this.BoardFractureLineShearingRange = 0f;
            this.SplinterFractureNormalShearingRange = this.BoardFractureNormalShearingRange = 0f;
            this.SplinterNumSubparts = 0;
            this.BoardNumSubparts = 3;
            this.SplinterRotateSplitGeom = this.BoardRotateSplitGeom = Rotation.AutoRotate;
            this.SplinterScale = this.BoardScale = Vector3.One;
            this.SplinterScaleRange = this.BoardScaleRange = Vector3.One;
            this.SplinterSplitGeomShiftRangeY = this.BoardSplitGeomShiftRangeY = 0f;
            this.SplinterSplitGeomShiftRangeZ = this.BoardSplitGeomShiftRangeZ = 0f;
            this.SplinterSplittingAxis = this.BoardSplittingAxis = Vector3.Zero;
            this.SplinterSurfaceNormalShearingRange = this.BoardSurfaceNormalShearingRange = 0f;
            this.SplinterWidthRange = this.BoardWidthRange = 0f;
        }

        [DisplayName("Splitting Axis"), Description("Splitting Axis"), Category("Splinter"), Browsable(false)]
        public Vector3 SplinterSplittingAxis { get; set; }

        [DisplayName("Rotate Split Geometry"), Description("Rotate Split Geometry"), Category("Splinter")]
        public Rotation SplinterRotateSplitGeom { get; set; }

        [DisplayName("Number of subparts"), Description("Number of subparts"), Category("Splinter")]
        public int SplinterNumSubparts { get; set; }

        [DisplayName("Width range "), Description("Width range"), Category("Splinter")]
        public float SplinterWidthRange { get; set; }

        [DisplayName("Scale"), Description("Scale"), Category("Splinter"), Browsable(false)]
        public Vector3 SplinterScale { get; set; }

        [DisplayName("Scale range"), Description("Scale range"), Category("Splinter"), Browsable(false)]
        public Vector3 SplinterScaleRange { get; set; }

        [DisplayName("Splitting geometry Y shift range"), Description("Splitting geometry Y shift range"), Category("Splinter")]
        public float SplinterSplitGeomShiftRangeY { get; set; }

        [DisplayName("Splitting geometry Z shift range"), Description("Splitting geometry Z shift range"), Category("Splinter")]
        public float SplinterSplitGeomShiftRangeZ { get; set; }

        [DisplayName("Surface normal shearing range"), Description("Surface normal shearing range"), Category("Splinter")]
        public float SplinterSurfaceNormalShearingRange { get; set; }

        [DisplayName("Fracture line shearing range"), Description("Fracture line shearing range"), Category("Splinter")]
        public float SplinterFractureLineShearingRange { get; set; }

        [DisplayName("Fracture normal shearing range"), Description("Fracture normal shearing range"), Category("Splinter")]
        public float SplinterFractureNormalShearingRange { get; set; }

        [DisplayName("Splinter split plane"), Description("Splinter split plane"), Category("Splinter"), Editor("Telerik.WinControls.UI.PropertyGridBrowseEditor, Telerik.WinControls.UI", "Telerik.WinControls.UI.BaseInputEditor, Telerik.WinControls.UI")]
        public string SplinterSplittingPlane { get; set; }

        [DisplayName("Use custom split axis"), Description("If false, algorithm will use automatic plane orientation to cut the model. Otherwise you can specify splitting plane orientation"), Category("Splinter")]
        public bool SplinterCustomSplittingPlaneAxis
        {
            get => 
                this.m_splinterCustomAxis;
            set
            {
                this.m_splinterCustomAxis = value;
                if (this.m_splinterCustomAxis)
                {
                    this.SplinterSplittingAxis = new Vector3(0f, 1f, 0f);
                }
                else
                {
                    this.SplinterSplittingAxis = Vector3.Zero;
                }
            }
        }

        [DisplayName("Splitting Axis"), Description("Splitting Axis"), Category("Board"), Browsable(false)]
        public Vector3 BoardSplittingAxis { get; set; }

        [DisplayName("Rotate Split Geometry"), Description("Rotate Split Geometry"), Category("Board")]
        public Rotation BoardRotateSplitGeom { get; set; }

        [DisplayName("Number of subparts"), Description("Number of subparts"), Category("Board")]
        public int BoardNumSubparts { get; set; }

        [DisplayName("Width range "), Description("Width range"), Category("Board")]
        public float BoardWidthRange { get; set; }

        [DisplayName("Scale"), Description("Scale"), Category("Board"), Browsable(false)]
        public Vector3 BoardScale { get; set; }

        [DisplayName("Scale range"), Description("Scale range"), Category("Board"), Browsable(false)]
        public Vector3 BoardScaleRange { get; set; }

        [DisplayName("Splitting geometry Y shift range"), Description("Splitting geometry Y shift range"), Category("Board")]
        public float BoardSplitGeomShiftRangeY { get; set; }

        [DisplayName("Splitting geometry Z shift range"), Description("Splitting geometry Z shift range"), Category("Board")]
        public float BoardSplitGeomShiftRangeZ { get; set; }

        [DisplayName("Surface normal shearing range"), Description("Surface normal shearing range"), Category("Board")]
        public float BoardSurfaceNormalShearingRange { get; set; }

        [DisplayName("Fracture line shearing range"), Description("Fracture line shearing range"), Category("Board")]
        public float BoardFractureLineShearingRange { get; set; }

        [DisplayName("Fracture normal shearing range"), Description("Fracture normal shearing range"), Category("Board")]
        public float BoardFractureNormalShearingRange { get; set; }

        [DisplayName("Board split plane"), Description("Board split plane"), Category("Board"), Editor("Telerik.WinControls.UI.PropertyGridBrowseEditor, Telerik.WinControls.UI", "Telerik.WinControls.UI.BaseInputEditor, Telerik.WinControls.UI")]
        public string BoardSplittingPlane { get; set; }

        [DisplayName("Use custom split axis"), Description("If false, algorithm will use automatic plane orientation to cut the model. Otherwise you can specify splitting plane orientation"), Category("Board")]
        public bool BoardCustomSplittingPlaneAxis
        {
            get => 
                this.m_boardCustomAxis;
            set
            {
                this.m_boardCustomAxis = value;
                if (this.m_boardCustomAxis)
                {
                    this.BoardSplittingAxis = new Vector3(0f, 1f, 0f);
                }
                else
                {
                    this.BoardSplittingAxis = Vector3.Zero;
                }
            }
        }

        public enum Rotation
        {
            AutoRotate,
            NoRotation,
            Rotate90
        }
    }
}

