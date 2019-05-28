namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ComponentDefinition : MyObjectBuilder_PhysicalItemDefinition
    {
        [ProtoMember(11)]
        public int MaxIntegrity;
        [ProtoMember(13)]
        public float DropProbability;
        [ProtoMember(15)]
        public float DeconstructionEfficiency = 1f;
    }
}

