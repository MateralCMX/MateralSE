namespace VRageRender
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{Width}x{Height}@{RefreshRate}Hz")]
    public struct MyDisplayMode
    {
        public int Width;
        public int Height;
        public int RefreshRate;
        public int? RefreshRateDenominator;
        public float RefreshRateF =>
            ((this.RefreshRateDenominator != null) ? (((float) this.RefreshRate) / ((float) this.RefreshRateDenominator.Value)) : ((float) this.RefreshRate));
        public MyDisplayMode(int width, int height, int refreshRate, int? refreshRateDenominator = new int?())
        {
            this.Width = width;
            this.Height = height;
            this.RefreshRate = refreshRate;
            this.RefreshRateDenominator = refreshRateDenominator;
        }

        public override string ToString() => 
            ((this.RefreshRateDenominator == null) ? $"{this.Width}x{this.Height}@{this.RefreshRate}Hz" : $"{this.Width}x{this.Height}@{(((float) this.RefreshRate) / ((float) this.RefreshRateDenominator.Value))}Hz");
    }
}

