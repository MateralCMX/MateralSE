namespace VRage.Audio
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyCueId
    {
        [ProtoMember(12)]
        public MyStringHash Hash;
        public static readonly ComparerType Comparer;
        public MyCueId(MyStringHash hash)
        {
            this.Hash = hash;
        }

        public bool IsNull =>
            (this.Hash == MyStringHash.NullOrEmpty);
        public static bool operator ==(MyCueId r, MyCueId l) => 
            (r.Hash == l.Hash);

        public static bool operator !=(MyCueId r, MyCueId l) => 
            (r.Hash != l.Hash);

        public override bool Equals(object obj) => 
            ((obj is MyCueId) && ((MyCueId) obj).Hash.Equals(this.Hash));

        public override int GetHashCode() => 
            this.Hash.GetHashCode();

        public override string ToString() => 
            this.Hash.ToString();

        static MyCueId()
        {
            Comparer = new ComparerType();
        }
        public class ComparerType : IEqualityComparer<MyCueId>
        {
            bool IEqualityComparer<MyCueId>.Equals(MyCueId x, MyCueId y) => 
                (x.Hash == y.Hash);

            int IEqualityComparer<MyCueId>.GetHashCode(MyCueId obj) => 
                obj.Hash.GetHashCode();
        }
    }
}

