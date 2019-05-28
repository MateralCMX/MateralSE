namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationLayer : MyObjectBuilder_Base
    {
        [ProtoMember(20)]
        public string Name;
        [ProtoMember(0x18)]
        public MyLayerMode Mode;
        [ProtoMember(0x1c)]
        public string StateMachine;
        [ProtoMember(0x20)]
        public string InitialSMNode;
        [ProtoMember(0x25)]
        public string BoneMask;

        [ProtoContract]
        public enum MyLayerMode
        {
            Replace,
            Add
        }
    }
}

