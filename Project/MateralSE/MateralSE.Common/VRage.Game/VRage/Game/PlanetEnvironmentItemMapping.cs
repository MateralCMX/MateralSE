namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct PlanetEnvironmentItemMapping
    {
        [ProtoMember(0x1e5), XmlArrayItem("Material")]
        public string[] Materials;
        [ProtoMember(0x1e9), XmlArrayItem("Biome")]
        public int[] Biomes;
        [ProtoMember(0x1ed), XmlArrayItem("Item")]
        public MyPlanetEnvironmentItemDef[] Items;
        [ProtoMember(0x1f1)]
        public MyPlanetSurfaceRule Rule;
    }
}

