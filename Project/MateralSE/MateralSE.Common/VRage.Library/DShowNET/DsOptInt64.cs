namespace DShowNET
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public class DsOptInt64
    {
        public long Value;
        public DsOptInt64(long Value)
        {
            this.Value = Value;
        }
    }
}

