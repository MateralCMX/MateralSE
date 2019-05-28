namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AreaMarkerDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(12)]
        public SerializableVector3 ColorHSV = new SerializableVector3(0f, 0f, 0f);
        [ProtoMember(15), ModdableContentFile("mwm")]
        public string Model;
        [ProtoMember(0x13), ModdableContentFile("dds")]
        public string ColorMetalTexture;
        [ProtoMember(0x17), ModdableContentFile("dds")]
        public string AddMapsTexture;
        [ProtoMember(0x1b)]
        public SerializableVector3 MarkerPosition = new SerializableVector3(0f, 0f, 0f);
        [ProtoMember(30)]
        public int MaxNumber = 1;
    }
}

