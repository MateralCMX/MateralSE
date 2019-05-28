namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyEncounterId : IEquatable<MyEncounterId>
    {
        [ProtoMember(13)]
        public BoundingBoxD BoundingBox;
        [ProtoMember(15)]
        public int Seed;
        [ProtoMember(0x11)]
        public int EncounterId;
        public MyEncounterId(BoundingBoxD box, int seed, int encounterId)
        {
            this.Seed = seed;
            this.EncounterId = encounterId;
            this.BoundingBox = box.Round(2);
        }

        public static bool operator ==(MyEncounterId x, MyEncounterId y) => 
            (x.BoundingBox.Equals(y.BoundingBox, 2.0) && ((x.Seed == y.Seed) && (x.EncounterId == y.EncounterId)));

        public static bool operator !=(MyEncounterId x, MyEncounterId y) => 
            !(x == y);

        public override bool Equals(object o) => 
            ((o is MyEncounterId) && this.Equals((MyEncounterId) o));

        public bool Equals(MyEncounterId other) => 
            (this == other);

        public override int GetHashCode() => 
            this.Seed;

        public override string ToString() => 
            $"{this.Seed}:{this.EncounterId}_{this.BoundingBox}";
    }
}

