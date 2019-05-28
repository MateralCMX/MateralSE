namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), ComVisible(false)]
    public class FilterInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
        public string achName;
        [MarshalAs(UnmanagedType.IUnknown)]
        public object pUnk;
    }
}

