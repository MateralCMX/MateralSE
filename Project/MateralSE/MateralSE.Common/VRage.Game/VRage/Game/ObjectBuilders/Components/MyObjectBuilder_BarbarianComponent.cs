namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BarbarianComponent : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(11)]
        public bool PeaceTime = true;
        [ProtoMember(14)]
        public int LastWarDay;
        [ProtoMember(0x11)]
        public int WaveDayNumber;
    }
}

