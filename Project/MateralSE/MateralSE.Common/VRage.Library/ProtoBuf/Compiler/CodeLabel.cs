namespace ProtoBuf.Compiler
{
    using System;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CodeLabel
    {
        public readonly Label Value;
        public readonly int Index;
        public CodeLabel(Label value, int index)
        {
            this.Value = value;
            this.Index = index;
        }
    }
}

