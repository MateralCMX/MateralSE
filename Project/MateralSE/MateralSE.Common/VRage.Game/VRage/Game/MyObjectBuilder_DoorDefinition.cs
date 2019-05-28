namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DoorDefinition : MyObjectBuilder_CubeBlockDefinition
    {
        [ProtoMember(11)]
        public string ResourceSinkGroup;
        [ProtoMember(14)]
        public float MaxOpen;
        [ProtoMember(0x11)]
        public string OpenSound;
        [ProtoMember(20)]
        public string CloseSound;
        [ProtoMember(0x17)]
        public float OpeningSpeed = 1f;
    }
}

