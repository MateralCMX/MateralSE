namespace VRageRender.Fractures
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class VoronoiFractureSettings : MyFractureSettings
    {
        public VoronoiFractureSettings()
        {
            this.Seed = 0x1e240;
            this.NumSitesToGenerate = 8;
            this.NumIterations = 1;
            this.SplitPlane = "";
        }

        [DisplayName("Seed"), Description("Seed"), Category("Voronoi")]
        public int Seed { get; set; }

        [DisplayName("Sites to generate"), Description("Sites to generate"), Category("Voronoi")]
        public int NumSitesToGenerate { get; set; }

        [DisplayName("Iterations"), Description("Iterations"), Category("Voronoi")]
        public int NumIterations { get; set; }

        [DisplayName("Split plane"), Description("Split plane"), Category("Voronoi"), Editor("Telerik.WinControls.UI.PropertyGridBrowseEditor, Telerik.WinControls.UI", "Telerik.WinControls.UI.BaseInputEditor, Telerik.WinControls.UI")]
        public string SplitPlane { get; set; }
    }
}

