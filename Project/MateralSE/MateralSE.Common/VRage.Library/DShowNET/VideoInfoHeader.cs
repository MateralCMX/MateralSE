namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public class VideoInfoHeader
    {
        public DsRECT SrcRect;
        public DsRECT TagRect;
        public int BitRate;
        public int BitErrorRate;
        public long AvgTimePerFrame;
        public DsBITMAPINFOHEADER BmiHeader;
    }
}

