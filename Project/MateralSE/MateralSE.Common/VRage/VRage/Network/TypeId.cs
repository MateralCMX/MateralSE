namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TypeId
    {
        internal uint Value;
        public TypeId(uint value)
        {
            this.Value = value;
        }

        public static implicit operator uint(TypeId tp) => 
            tp.Value;

        public override string ToString() => 
            this.Value.ToString();
    }
}

