namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_LCDTextureDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(12), ModdableContentFile("dds")]
        public string TexturePath;
        [ProtoMember(0x10, IsRequired=false)]
        public string LocalizationId;
        [ProtoMember(0x13)]
        public bool Selectable = true;
        [ProtoMember(0x16), ModdableContentFile("dds")]
        public string SpritePath;
    }
}

