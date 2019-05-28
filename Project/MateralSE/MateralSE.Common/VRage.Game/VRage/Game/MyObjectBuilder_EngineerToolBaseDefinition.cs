namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_EngineerToolBaseDefinition : MyObjectBuilder_HandItemDefinition
    {
        [ProtoMember(12), DefaultValue(1)]
        public float SpeedMultiplier = 1f;
        [ProtoMember(15), DefaultValue(1)]
        public float DistanceMultiplier = 1f;
        public string Flare = "Welder";
    }
}

