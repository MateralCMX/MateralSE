namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WheelModelsDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x11, IsRequired=false)]
        public string AlternativeModel;
        [ProtoMember(20, IsRequired=false)]
        public float AngularVelocityThreshold;
    }
}

