namespace VRage.Game
{
    using ProtoBuf;
    using System;

    [ProtoContract]
    public class MyObjectSeedParams
    {
        [ProtoMember(0x15)]
        public int Index;
        [ProtoMember(0x17)]
        public int Seed;
        [ProtoMember(0x19)]
        public MyObjectSeedType Type;
        [ProtoMember(0x1b)]
        public bool Generated;
        [ProtoMember(0x1d)]
        public int m_proxyId = -1;
        [ProtoMember(0x1f, IsRequired=false)]
        public int GeneratorSeed;

        public override bool Equals(object obj)
        {
            MyObjectSeedParams @params = obj as MyObjectSeedParams;
            return ((this.Seed == @params.Seed) && ((this.Index == @params.Index) && (this.GeneratorSeed == @params.GeneratorSeed)));
        }

        public override int GetHashCode() => 
            this.Seed;
    }
}

