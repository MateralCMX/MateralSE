namespace VRageRender.Animations
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRageMath;

    [ProtoContract, XmlType("Property")]
    public class GenerationProperty
    {
        [ProtoMember(0x1d), XmlAttribute("Name")]
        public string Name = "";
        [ProtoMember(0x20), XmlAttribute("AnimationType")]
        public PropertyAnimationType AnimationType;
        [ProtoMember(0x23), XmlAttribute("Type")]
        public string Type = "";
        [ProtoMember(0x26)]
        public float ValueFloat;
        [ProtoMember(0x29)]
        public bool ValueBool;
        [ProtoMember(0x2c)]
        public int ValueInt;
        [ProtoMember(0x2f)]
        public string ValueString = "";
        [ProtoMember(50)]
        public Vector3 ValueVector3;
        [ProtoMember(0x35)]
        public Vector4 ValueVector4;
        [ProtoMember(0x38)]
        public List<AnimationKey> Keys;
    }
}

