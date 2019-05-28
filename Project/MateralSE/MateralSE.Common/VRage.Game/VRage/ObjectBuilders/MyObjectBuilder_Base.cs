namespace VRage.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRage.Utils;

    [ProtoContract]
    public abstract class MyObjectBuilder_Base
    {
        private MyStringHash m_subtypeId;
        private string m_subtypeName;

        protected MyObjectBuilder_Base()
        {
        }

        public virtual MyObjectBuilder_Base Clone() => 
            MyObjectBuilderSerializer.Clone(this);

        public void Save(string filepath)
        {
            MyObjectBuilderSerializer.SerializeXML(filepath, false, this, null);
        }

        public bool ShouldSerializeSubtypeId() => 
            false;

        [DefaultValue(0)]
        public MyStringHash SubtypeId =>
            this.m_subtypeId;

        [Serialize]
        private MyStringHash m_serializableSubtypeId
        {
            get => 
                this.m_subtypeId;
            set
            {
                this.m_subtypeId = value;
                this.m_subtypeName = value.String;
            }
        }

        [ProtoMember(0x2f), DefaultValue((string) null), NoSerialize]
        public string SubtypeName
        {
            get => 
                this.m_subtypeName;
            set
            {
                this.m_subtypeName = value;
                this.m_subtypeId = MyStringHash.GetOrCompute(value);
            }
        }

        [XmlIgnore]
        public MyObjectBuilderType TypeId =>
            base.GetType();
    }
}

