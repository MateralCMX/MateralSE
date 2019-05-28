namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, XmlType("VoxelMaterial"), MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VoxelMaterialDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(15)]
        public string MaterialTypeName = "Rock";
        [ProtoMember(0x12)]
        public string MinedOre;
        [ProtoMember(0x15)]
        public float MinedOreRatio;
        [ProtoMember(0x18)]
        public bool CanBeHarvested;
        [ProtoMember(0x1b)]
        public bool IsRare;
        [ProtoMember(0x27)]
        public bool UseTwoTextures;
        [ProtoMember(0x30), ModdableContentFile("dds")]
        public string VoxelHandPreview;
        [ProtoMember(0x44)]
        public int MinVersion;
        [ProtoMember(0x47)]
        public int MaxVersion = 0x7fffffff;
        [ProtoMember(0x4a)]
        public bool SpawnsInAsteroids = true;
        [ProtoMember(0x4d)]
        public bool SpawnsFromMeteorites = true;
        public string DamagedMaterial;
        [ProtoMember(0x5d, IsRequired=false)]
        public float Friction = 1f;
        [ProtoMember(0x60, IsRequired=false)]
        public float Restitution = 1f;
        [ProtoMember(0x63, IsRequired=false)]
        public ColorDefinitionRGBA? ColorKey;
        [ProtoMember(0x66, IsRequired=false), DefaultValue("")]
        public string LandingEffect;
        [ProtoMember(0x69, IsRequired=false)]
        public int AsteroidGeneratorSpawnProbabilityMultiplier = 1;
        [ProtoMember(0x6c, IsRequired=false), DefaultValue("")]
        public string BareVariant;
    }
}

