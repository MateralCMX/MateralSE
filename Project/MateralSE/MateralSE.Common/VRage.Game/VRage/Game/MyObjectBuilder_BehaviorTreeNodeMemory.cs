namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BehaviorTreeNodeMemory : MyObjectBuilder_Base
    {
        [XmlAttribute, ProtoMember(14), DefaultValue(false)]
        public bool InitCalled;
    }
}

