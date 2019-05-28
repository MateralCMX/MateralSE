namespace VRage.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableDefinitionId
    {
        [XmlIgnore, NoSerialize]
        public MyObjectBuilderType TypeId;
        [XmlIgnore, NoSerialize]
        public string SubtypeName;
        [ProtoMember(15), XmlAttribute("Type"), NoSerialize]
        public string TypeIdStringAttribute
        {
            get => 
                (!this.TypeId.IsNull ? this.TypeId.ToString() : "(null)");
            set
            {
                if (value != null)
                {
                    this.TypeIdString = value;
                }
            }
        }
        [ProtoMember(0x18), XmlElement("TypeId"), NoSerialize]
        public string TypeIdString
        {
            get => 
                (!this.TypeId.IsNull ? this.TypeId.ToString() : "(null)");
            set => 
                (this.TypeId = MyObjectBuilderType.ParseBackwardsCompatible(value));
        }
        public bool ShouldSerializeTypeIdString() => 
            false;

        [ProtoMember(0x26), XmlAttribute("Subtype"), NoSerialize]
        public string SubtypeIdAttribute
        {
            get => 
                this.SubtypeName;
            set => 
                (this.SubtypeName = value);
        }
        [ProtoMember(0x2f), NoSerialize]
        public string SubtypeId
        {
            get => 
                this.SubtypeName;
            set => 
                (this.SubtypeName = value);
        }
        public bool ShouldSerializeSubtypeId() => 
            false;

        [Serialize]
        private ushort m_binaryTypeId
        {
            get => 
                ((MyRuntimeObjectBuilderId) this.TypeId).Value;
            set => 
                (this.TypeId = (MyObjectBuilderType) new MyRuntimeObjectBuilderId(value));
        }
        [Serialize]
        private MyStringHash m_binarySubtypeId
        {
            get => 
                MyStringHash.TryGet(this.SubtypeId);
            set => 
                (this.SubtypeName = value.String);
        }
        public SerializableDefinitionId(MyObjectBuilderType typeId, string subtypeName)
        {
            this.TypeId = typeId;
            this.SubtypeName = subtypeName;
        }

        public override string ToString() => 
            $"{this.TypeId}/{this.SubtypeName}";

        public bool IsNull() => 
            this.TypeId.IsNull;
    }
}

