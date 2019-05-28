namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ContainerDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x22), XmlArrayItem("Component")]
        public DefaultComponentBuilder[] DefaultComponents;
        [ProtoMember(0x26), DefaultValue((string) null)]
        public EntityFlags? Flags;

        [ProtoContract]
        public class DefaultComponentBuilder
        {
            [ProtoMember(0x11), XmlAttribute("BuilderType"), DefaultValue((string) null)]
            public string BuilderType;
            [ProtoMember(0x15), XmlAttribute("InstanceType"), DefaultValue((string) null)]
            public string InstanceType;
            [ProtoMember(0x19), XmlAttribute("ForceCreate"), DefaultValue(false)]
            public bool ForceCreate;
            [ProtoMember(0x1d), XmlAttribute("SubtypeId"), DefaultValue((string) null)]
            public string SubtypeId;
        }
    }
}

