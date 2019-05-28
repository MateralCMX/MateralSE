namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableBounds
    {
        [ProtoMember(14), XmlAttribute]
        public float Min;
        [ProtoMember(0x11), XmlAttribute]
        public float Max;
        [ProtoMember(20), XmlAttribute]
        public float Default;
        public SerializableBounds(float min, float max, float def)
        {
            this.Min = min;
            this.Max = max;
            this.Default = def;
        }

        public static implicit operator MyBounds(SerializableBounds v) => 
            new MyBounds(v.Min, v.Max, v.Default);

        public static implicit operator SerializableBounds(MyBounds v) => 
            new SerializableBounds(v.Min, v.Max, v.Default);
    }
}

