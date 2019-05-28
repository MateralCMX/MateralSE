namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VoxelMapStorageDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(13), ModdableContentFile("vx2")]
        public string StorageFile;
        [ProtoMember(0x10)]
        public bool UseForProceduralRemovals;
        [ProtoMember(0x13)]
        public bool UseForProceduralAdditions;
        [ProtoMember(0x16)]
        public bool UseAsPrimaryProceduralAdditionShape;
        [ProtoMember(0x19)]
        public float SpawnProbability = 1f;
    }
}

