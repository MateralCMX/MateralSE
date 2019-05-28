namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class BlueprintItem
    {
        [XmlIgnore, ProtoMember(12)]
        public SerializableDefinitionId Id;
        [XmlAttribute, ProtoMember(0x22)]
        public string Amount;

        [XmlAttribute]
        public string TypeId
        {
            get => 
                (!this.Id.TypeId.IsNull ? this.Id.TypeId.ToString() : "(null)");
            set => 
                (this.Id.TypeId = MyObjectBuilderType.ParseBackwardsCompatible(value));
        }

        [XmlAttribute]
        public string SubtypeId
        {
            get => 
                this.Id.SubtypeId;
            set => 
                (this.Id.SubtypeId = value);
        }
    }
}

