namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Utils;

    [ProtoContract]
    public class MyGroupedIds
    {
        [ProtoMember(0x1b), XmlAttribute]
        public string Tag;
        [ProtoMember(30), DefaultValue((string) null), XmlArrayItem("GroupEntry")]
        public GroupedId[] Entries;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct GroupedId
        {
            [ProtoMember(14), XmlAttribute]
            public string TypeId;
            [ProtoMember(0x11), XmlAttribute]
            public string SubtypeName;
            [XmlIgnore]
            public MyStringHash SubtypeId =>
                MyStringHash.GetOrCompute(this.SubtypeName);
        }
    }
}

