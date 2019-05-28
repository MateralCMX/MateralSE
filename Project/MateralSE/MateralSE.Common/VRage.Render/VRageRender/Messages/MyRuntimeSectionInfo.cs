namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRuntimeSectionInfo
    {
        public int IndexStart;
        public int TriCount;
        public string MaterialName;
    }
}

