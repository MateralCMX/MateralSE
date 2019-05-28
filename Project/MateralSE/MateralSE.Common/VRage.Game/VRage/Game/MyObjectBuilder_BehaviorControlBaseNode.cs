namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BehaviorControlBaseNode : MyObjectBuilder_BehaviorTreeNode
    {
        [ProtoMember(13), XmlArrayItem("BTNode", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_BehaviorTreeNode>))]
        public MyObjectBuilder_BehaviorTreeNode[] BTNodes;
        [ProtoMember(0x11)]
        public string Name;
        [ProtoMember(20), DefaultValue(false)]
        public bool IsMemorable;
    }
}

