namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationSM : MyObjectBuilder_Base
    {
        [ProtoMember(14)]
        public string Name;
        [ProtoMember(0x12), XmlArrayItem("Node")]
        public MyObjectBuilder_AnimationSMNode[] Nodes;
        [ProtoMember(0x17), XmlArrayItem("Transition")]
        public MyObjectBuilder_AnimationSMTransition[] Transitions;
    }
}

