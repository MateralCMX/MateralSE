namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ComponentGroupDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("Component"), ProtoMember(0x19)]
        public Component[] Components;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct Component
        {
            [ProtoMember(15), XmlAttribute]
            public string SubtypeId;
            [ProtoMember(0x13), XmlAttribute]
            public int Amount;
        }
    }
}

