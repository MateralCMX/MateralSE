namespace VRage.Game.ObjectBuilders.AI
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DroneAI : MyObjectBuilder_AutomaticBehaviour
    {
        [ProtoMember(11), Serialize(MyObjectFlags.DefaultZero)]
        public string CurrentPreset = string.Empty;
        [ProtoMember(15)]
        public bool AlternativebehaviorSwitched;
        [ProtoMember(0x12)]
        public SerializableVector3D ReturnPosition;
        [ProtoMember(0x15)]
        public bool CanSkipWaypoint = true;
    }
}

