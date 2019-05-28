namespace VRageRender.Animations
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRageMath;

    [ProtoContract, XmlType("Key")]
    public class AnimationKey
    {
        [ProtoMember(70)]
        public float Time;
        [ProtoMember(0x49)]
        public float ValueFloat;
        [ProtoMember(0x4c)]
        public bool ValueBool;
        [ProtoMember(0x4f)]
        public int ValueInt;
        [ProtoMember(0x52)]
        public string ValueString = "";
        [ProtoMember(0x55)]
        public Vector3 ValueVector3;
        [ProtoMember(0x58)]
        public Vector4 ValueVector4;
        [ProtoMember(0x5b)]
        public Generation2DProperty Value2D;
    }
}

