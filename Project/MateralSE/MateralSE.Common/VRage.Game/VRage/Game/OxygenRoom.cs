namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct OxygenRoom
    {
        [ProtoMember(0xb3)]
        public Vector3I StartingPosition;
        [ProtoMember(0xb6), XmlAttribute]
        public float OxygenAmount;
    }
}

