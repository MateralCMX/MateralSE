namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("SetupBasePrefab")]
    public class MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab : MyObjectBuilder_WorldGeneratorOperation
    {
        [ProtoMember(0xf5), XmlAttribute]
        public string PrefabFile;
        [ProtoMember(0xf8)]
        public SerializableVector3 Offset;
        [ProtoMember(0xfc), XmlAttribute]
        public string AsteroidName;
        [ProtoMember(0xff), XmlAttribute]
        public string BeaconName;

        public bool ShouldSerializeBeaconName() => 
            !string.IsNullOrEmpty(this.BeaconName);

        public bool ShouldSerializeOffset() => 
            (this.Offset != new SerializableVector3(0f, 0f, 0f));
    }
}

