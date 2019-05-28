namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyPlanetMaps
    {
        [ProtoMember(0x178), XmlAttribute]
        public bool Material;
        [ProtoMember(380), XmlAttribute]
        public bool Ores;
        [ProtoMember(0x180), XmlAttribute]
        public bool Biome;
        [ProtoMember(0x184), XmlAttribute]
        public bool Occlusion;
        public MyPlanetMapTypeSet ToSet()
        {
            MyPlanetMapTypeSet set = 0;
            if (this.Material)
            {
                set |= MyPlanetMapTypeSet.Material;
            }
            if (this.Ores)
            {
                set |= MyPlanetMapTypeSet.Ore;
            }
            if (this.Biome)
            {
                set |= MyPlanetMapTypeSet.Biome;
            }
            return set;
        }
    }
}

