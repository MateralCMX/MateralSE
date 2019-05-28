namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Utils;

    [ProtoContract]
    public class MyMappedId
    {
        [ProtoMember(10), XmlAttribute]
        public string Group;
        [ProtoMember(13), XmlAttribute]
        public string TypeId;
        [ProtoMember(0x10), XmlAttribute]
        public string SubtypeName;

        [XmlIgnore]
        public MyStringHash GroupId =>
            MyStringHash.GetOrCompute(this.Group);

        [XmlIgnore]
        public MyStringHash SubtypeId =>
            MyStringHash.GetOrCompute(this.SubtypeName);
    }
}

