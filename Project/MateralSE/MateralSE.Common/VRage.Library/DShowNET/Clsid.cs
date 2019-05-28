namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public class Clsid
    {
        public static readonly Guid FilterGraph = new Guid(0xe436ebb3, 0x524f, 0x11ce, 0x9f, 0x53, 0, 0x20, 0xaf, 11, 0xa7, 0x70);
        public static readonly Guid WMVideoDecoderDMO = new Guid("{82D353DF-90BD-4382-8BC2-3F6192B76E34}");
        public static readonly Guid WMVideoDecoderDMO_cat = new Guid("{4A69B442-28BE-4991-969C-B500ADF5D8A8}");
        public static readonly Guid SampleGrabber = new Guid(0xc1f400a0, 0x3f08, 0x11d3, 0x9f, 11, 0, 0x60, 8, 3, 0x9e, 0x37);
        public static readonly Guid NullRenderer = new Guid(0xc1f400a4, 0x3f08, 0x11d3, 0x9f, 11, 0, 0x60, 8, 3, 0x9e, 0x37);
    }
}

