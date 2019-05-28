namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_JetpackDefinition
    {
        [ProtoMember(0x24), XmlArrayItem("Thrust")]
        public List<MyJetpackThrustDefinition> Thrusts;
        [ProtoMember(40)]
        public MyObjectBuilder_ThrustDefinition ThrustProperties;
    }
}

