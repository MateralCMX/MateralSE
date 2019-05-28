namespace VRage.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBufferStatistics
    {
        public string Name;
        public int ActiveBuffers;
        public int ActiveBytes;
        public int TotalBuffersAllocated;
        public int TotalBytesAllocated;
    }
}

