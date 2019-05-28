namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct NetworkId : IComparable<NetworkId>, IEquatable<NetworkId>
    {
        public static readonly NetworkId Invalid;
        internal uint Value;
        public bool IsInvalid =>
            (this.Value == 0);
        public bool IsValid =>
            (this.Value != 0);
        internal NetworkId(uint value)
        {
            this.Value = value;
        }

        public int CompareTo(NetworkId other) => 
            this.Value.CompareTo(other.Value);

        public bool Equals(NetworkId other) => 
            (this.Value == other.Value);

        public override string ToString() => 
            this.Value.ToString();

        static NetworkId()
        {
            Invalid = new NetworkId(0);
        }
    }
}

