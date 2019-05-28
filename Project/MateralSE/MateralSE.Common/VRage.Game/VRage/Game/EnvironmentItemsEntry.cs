namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [ProtoContract]
    public class EnvironmentItemsEntry
    {
        [ProtoMember(10), XmlAttribute]
        public string Type;
        [ProtoMember(14), XmlAttribute]
        public string Subtype;
        [ProtoMember(0x12), XmlAttribute]
        public string ItemSubtype;
        [ProtoMember(0x16), DefaultValue(true)]
        public bool Enabled = true;
        [ProtoMember(0x19), XmlAttribute]
        public float Frequency = 1f;

        public override bool Equals(object other)
        {
            EnvironmentItemsEntry entry = other as EnvironmentItemsEntry;
            return ((entry != null) && (entry.Type.Equals(this.Type) && (entry.Subtype.Equals(this.Subtype) && entry.ItemSubtype.Equals(this.ItemSubtype))));
        }

        public override int GetHashCode() => 
            (((this.Type.GetHashCode() * 0x180005) ^ (this.Subtype.GetHashCode() * 0xc005)) ^ this.ItemSubtype.GetHashCode());
    }
}

