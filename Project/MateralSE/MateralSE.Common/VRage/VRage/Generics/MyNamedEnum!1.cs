namespace VRage.Generics
{
    using ProtoBuf;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyNamedEnum<T> : IXmlSerializable where T: struct, IConvertible
    {
        [ProtoMember(0x13), XmlIgnore]
        private int m_enumInt;
        private T EnumType;
        public MyNamedEnum(T s)
        {
            this.EnumType = s;
            this.m_enumInt = s.ToInt32(CultureInfo.InvariantCulture);
        }

        public MyNamedEnum(int i)
        {
            this.EnumType = default(T);
            this.m_enumInt = 0;
            this.CreateFromInt(i);
        }

        private void CreateFromString(string s)
        {
            if (!Enum.TryParse<T>(s, out this.EnumType))
            {
                this.EnumType = (T) Enum.ToObject(typeof(T), MyUtils.GetHash(s, -2128831035));
            }
            this.m_enumInt = this.EnumType.ToInt32(CultureInfo.InvariantCulture);
        }

        private void CreateFromInt(int i)
        {
            this.m_enumInt = i;
            this.EnumType = (T) Enum.ToObject(typeof(T), i);
        }

        public XmlSchema GetSchema() => 
            null;

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            string s = reader.ReadElementContentAsString();
            this.CreateFromString(s);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this.EnumType.ToString());
        }

        [ProtoAfterDeserialization]
        private void OnProtoDeserialize()
        {
            this.CreateFromInt(this.m_enumInt);
        }

        public static implicit operator T(MyNamedEnum<T> f) => 
            f.EnumType;

        public static implicit operator MyNamedEnum<T>(T f) => 
            new MyNamedEnum<T>(f);

        public static implicit operator int(MyNamedEnum<T> f) => 
            f.m_enumInt;

        public static implicit operator MyNamedEnum<T>(int f) => 
            new MyNamedEnum<T>(f);
    }
}

