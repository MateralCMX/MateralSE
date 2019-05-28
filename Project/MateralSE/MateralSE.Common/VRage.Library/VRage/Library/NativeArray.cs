namespace VRage.Library
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class NativeArray : IDisposable
    {
        public readonly int Size;
        public readonly IntPtr Ptr;

        public NativeArray(int size)
        {
            this.Size = size;
            this.Ptr = Marshal.AllocHGlobal(size);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(this.Ptr);
        }

        [Conditional("DEBUG")]
        public void UpdateAllocationTrace()
        {
        }
    }
}

