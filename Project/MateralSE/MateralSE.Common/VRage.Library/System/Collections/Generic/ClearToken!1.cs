namespace System.Collections.Generic
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ClearToken<T> : IDisposable
    {
        public List<T> List;
        public void Dispose()
        {
            this.List.Clear();
        }
    }
}

