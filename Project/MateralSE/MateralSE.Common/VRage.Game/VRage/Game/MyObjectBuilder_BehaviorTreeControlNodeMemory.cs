namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BehaviorTreeControlNodeMemory : MyObjectBuilder_BehaviorTreeNodeMemory
    {
        [XmlAttribute, ProtoMember(14), DefaultValue(0)]
        public int InitialIndex;
    }
}

