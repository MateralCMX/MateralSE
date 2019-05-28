namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PowerProducerDefinition : MyObjectBuilder_CubeBlockDefinition
    {
        [ProtoMember(11)]
        public string ResourceSourceGroup;
        [ProtoMember(14)]
        public float MaxPowerOutput;
    }
}

