namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((System.Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationSMNode : MyObjectBuilder_Base
    {
        [ProtoMember(0x18)]
        public string Name;
        [ProtoMember(0x1c)]
        public string StateMachineName;
        [ProtoMember(0x20)]
        public MyObjectBuilder_AnimationTree AnimationTree;
        [ProtoMember(0x24)]
        public Vector2I? EdPos;
        [ProtoMember(40)]
        public MySMNodeType Type;
        [ProtoMember(0x2c), XmlArrayItem("Variable")]
        public List<MyObjectBuilder_AnimationSMVariable> Variables = new List<MyObjectBuilder_AnimationSMVariable>();

        public enum MySMNodeType
        {
            Normal,
            PassThrough,
            Any,
            AnyExceptTarget
        }
    }
}

