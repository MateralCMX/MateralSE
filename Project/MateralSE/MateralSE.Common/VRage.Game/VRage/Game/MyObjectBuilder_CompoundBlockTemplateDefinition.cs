namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CompoundBlockTemplateDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("Binding"), ProtoMember(0x2e)]
        public CompoundBlockBinding[] Bindings;

        [ProtoContract]
        public class CompoundBlockBinding
        {
            [XmlAttribute, ProtoMember(0x20)]
            public string BuildType;
            [XmlAttribute, ProtoMember(0x24), DefaultValue(false)]
            public bool Multiple;
            [XmlArrayItem("RotationBind"), ProtoMember(0x29)]
            public MyObjectBuilder_CompoundBlockTemplateDefinition.CompoundBlockRotationBinding[] RotationBinds;
        }

        [ProtoContract]
        public class CompoundBlockRotationBinding
        {
            [XmlAttribute, ProtoMember(20)]
            public string BuildTypeReference;
            [XmlArrayItem("Rotation"), ProtoMember(0x18)]
            public SerializableBlockOrientation[] Rotations;
        }
    }
}

