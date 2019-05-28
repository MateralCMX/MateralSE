namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct BoolBlit
    {
        private byte m_value;
        internal BoolBlit(byte value)
        {
            this.m_value = value;
        }

        public static implicit operator bool(BoolBlit b) => 
            (b.m_value != 0);

        public static implicit operator BoolBlit(bool b) => 
            new BoolBlit(b ? ((byte) 0xff) : ((byte) 0));

        public override string ToString() => 
            this.ToString();
    }
}

