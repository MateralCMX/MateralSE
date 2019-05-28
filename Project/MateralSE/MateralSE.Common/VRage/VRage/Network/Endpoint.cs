namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Endpoint
    {
        public readonly EndpointId Id;
        public readonly byte Index;
        public Endpoint(EndpointId id, byte index)
        {
            this.Id = id;
            this.Index = index;
        }

        public Endpoint(ulong id, byte index)
        {
            this.Id = new EndpointId(id);
            this.Index = index;
        }

        public static bool operator ==(Endpoint a, Endpoint b) => 
            ((a.Id == b.Id) && (a.Index == b.Index));

        public static bool operator !=(Endpoint a, Endpoint b) => 
            !(a == b);

        private bool Equals(Endpoint other) => 
            (this == other);

        public override bool Equals(object obj) => 
            ((obj is Endpoint) && this.Equals((Endpoint) obj));

        public override int GetHashCode() => 
            (this.Id.GetHashCode() ^ this.Index.GetHashCode());
    }
}

