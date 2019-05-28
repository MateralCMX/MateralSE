namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Data;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PhysicalItemDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(15)]
        public Vector3 Size;
        [ProtoMember(0x12)]
        public float Mass;
        [ProtoMember(0x15), ModdableContentFile("mwm")]
        public string Model = @"Models\Components\Sphere.mwm";
        [ProtoMember(0x19), ModdableContentFile("mwm"), XmlArrayItem("Model")]
        public string[] Models;
        [ProtoMember(30), DefaultValue((string) null)]
        public string IconSymbol;
        [ProtoMember(0x22), DefaultValue((string) null)]
        public float? Volume;
        [ProtoMember(0x25), DefaultValue((string) null)]
        public float? ModelVolume;
        [ProtoMember(40)]
        public string PhysicalMaterial;
        [ProtoMember(0x2b)]
        public string VoxelMaterial;
        [ProtoMember(0x2e), DefaultValue(true)]
        public bool CanSpawnFromScreen = true;
        [ProtoMember(50)]
        public bool RotateOnSpawnX;
        [ProtoMember(0x35)]
        public bool RotateOnSpawnY;
        [ProtoMember(0x38)]
        public bool RotateOnSpawnZ;
        [ProtoMember(0x3b)]
        public int Health = 100;
        [ProtoMember(0x3e), DefaultValue((string) null)]
        public SerializableDefinitionId? DestroyedPieceId;
        [ProtoMember(0x41)]
        public int DestroyedPieces;
        [ProtoMember(0x44), DefaultValue((string) null)]
        public string ExtraInventoryTooltipLine;
        [ProtoMember(0x47)]
        public MyFixedPoint MaxStackAmount = MyFixedPoint.MaxValue;

        public bool ShouldSerializeIconSymbol() => 
            (this.IconSymbol != null);
    }
}

