namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct EndpointId
    {
        public readonly ulong Value;
        public static EndpointId Null;
        public bool IsNull =>
            (this.Value == 0L);
        public bool IsValid =>
            !this.IsNull;
        public EndpointId(ulong value)
        {
            this.Value = value;
        }

        public override string ToString() => 
            this.Value.ToString();

        public static bool operator ==(EndpointId a, EndpointId b) => 
            (a.Value == b.Value);

        public static bool operator !=(EndpointId a, EndpointId b) => 
            (a.Value != b.Value);

        public bool Equals(EndpointId other) => 
            (this.Value == other.Value);

        public override bool Equals(object obj) => 
            ((obj is EndpointId) && this.Equals((EndpointId) obj));

        public override int GetHashCode() => 
            this.Value.GetHashCode();

        static EndpointId()
        {
            Null = new EndpointId(0L);
        }
    }
}

