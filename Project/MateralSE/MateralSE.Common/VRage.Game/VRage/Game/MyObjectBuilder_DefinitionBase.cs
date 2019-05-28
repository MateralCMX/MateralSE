namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public abstract class MyObjectBuilder_DefinitionBase : MyObjectBuilder_Base
    {
        [ProtoMember(14)]
        public SerializableDefinitionId Id;
        [ProtoMember(0x11), DefaultValue("")]
        public string DisplayName;
        [ProtoMember(20), DefaultValue("")]
        public string Description;
        [ProtoMember(0x17), DefaultValue(new string[] { "" }), XmlElement("Icon"), ModdableContentFile(new string[] { "dds", "png" })]
        public string[] Icons;
        [ProtoMember(0x1c), DefaultValue(true)]
        public bool Public = true;
        [ProtoMember(0x1f), DefaultValue(true), XmlAttribute(AttributeName="Enabled")]
        public bool Enabled = true;
        [ProtoMember(0x22), DefaultValue(true)]
        public bool AvailableInSurvival = true;
        [ProtoMember(0x25), DefaultValue("")]
        public string DescriptionArgs;
        [ProtoMember(40), DefaultValue((string) null), XmlElement("DLC")]
        public string[] DLCs;

        protected MyObjectBuilder_DefinitionBase()
        {
        }
    }
}

