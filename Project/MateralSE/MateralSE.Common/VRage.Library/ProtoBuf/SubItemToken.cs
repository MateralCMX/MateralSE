namespace ProtoBuf
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SubItemToken
    {
        internal readonly int value;
        internal SubItemToken(int value)
        {
            this.value = value;
        }
    }
}

