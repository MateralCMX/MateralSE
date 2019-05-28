namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    public class MyAbstractXmlSerializer<TAbstractBase> : MyXmlSerializerBase<TAbstractBase>
    {
        public MyAbstractXmlSerializer()
        {
        }

        public MyAbstractXmlSerializer(TAbstractBase data)
        {
            base.m_data = data;
        }

        private XmlSerializer GetSerializer(XmlReader reader, out string customRootName)
        {
            XmlSerializer serializer;
            string typeAttribute = this.GetTypeAttribute(reader);
            if ((typeAttribute == null) || !MyXmlSerializerManager.TryGetSerializer(typeAttribute, out serializer))
            {
                typeAttribute = MyXmlSerializerManager.GetSerializedName(typeof(TAbstractBase));
                serializer = MyXmlSerializerManager.GetSerializer(typeAttribute);
            }
            customRootName = typeAttribute;
            return serializer;
        }

        protected virtual string GetTypeAttribute(XmlReader reader) => 
            reader.GetAttribute("xsi:type");

        public static implicit operator MyAbstractXmlSerializer<TAbstractBase>(TAbstractBase builder) => 
            ((builder == null) ? null : new MyAbstractXmlSerializer<TAbstractBase>(builder));

        public override void ReadXml(XmlReader reader)
        {
            string str;
            XmlSerializer serializer = this.GetSerializer(reader, out str);
            base.m_data = (TAbstractBase) base.Deserialize(reader, serializer, str);
        }
    }
}

