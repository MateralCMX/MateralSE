namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GpsCollectionDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("Position"), ProtoMember(0x13)]
        public string[] Positions;
    }
}

