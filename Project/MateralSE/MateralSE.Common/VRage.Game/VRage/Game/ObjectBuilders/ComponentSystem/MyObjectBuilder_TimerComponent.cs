namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TimerComponent : MyObjectBuilder_ComponentBase
    {
        [ProtoMember(11)]
        public bool Repeat;
        [ProtoMember(14)]
        public float TimeToEvent;
        [ProtoMember(0x13)]
        public float SetTimeMinutes;
        [ProtoMember(0x16)]
        public bool TimerEnabled;
        [ProtoMember(0x19)]
        public bool RemoveEntityOnTimer;
    }
}

