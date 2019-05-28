namespace VRage
{
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using VRage.Generics;

    public abstract class MyXmlSerializerBase<TAbstractBase> : IMyXmlSerializable, IXmlSerializable
    {
        [ThreadStatic]
        private static MyObjectsPool<CustomRootReader> m_readerPool;
        [ThreadStatic]
        private static MyObjectsPool<CustomRootWriter> m_writerPool;
        protected TAbstractBase m_data;

        protected MyXmlSerializerBase()
        {
        }

        protected object Deserialize(XmlReader reader, XmlSerializer serializer, string customRootName)
        {
            CustomRootReader reader2;
            MyXmlSerializerBase<TAbstractBase>.ReaderPool.AllocateOrCreate(out reader2);
            reader2.Init(customRootName, reader);
            reader2.Release();
            MyXmlSerializerBase<TAbstractBase>.ReaderPool.Deallocate(reader2);
            return serializer.Deserialize(reader2);
        }

        public XmlSchema GetSchema() => 
            null;

        public static implicit operator TAbstractBase(MyXmlSerializerBase<TAbstractBase> o) => 
            o.Data;

        public abstract void ReadXml(XmlReader reader);
        public void WriteXml(XmlWriter writer)
        {
            CustomRootWriter writer2;
            Type type = this.m_data.GetType();
            XmlSerializer orCreateSerializer = MyXmlSerializerManager.GetOrCreateSerializer(type);
            MyXmlSerializerBase<TAbstractBase>.WriterPool.AllocateOrCreate(out writer2);
            writer2.Init(MyXmlSerializerManager.GetSerializedName(type), writer);
            orCreateSerializer.Serialize((XmlWriter) writer2, this.m_data);
            writer2.Release();
            MyXmlSerializerBase<TAbstractBase>.WriterPool.Deallocate(writer2);
        }

        protected static MyObjectsPool<CustomRootReader> ReaderPool
        {
            get
            {
                if (MyXmlSerializerBase<TAbstractBase>.m_readerPool == null)
                {
                    MyXmlSerializerBase<TAbstractBase>.m_readerPool = new MyObjectsPool<CustomRootReader>(2, null);
                }
                return MyXmlSerializerBase<TAbstractBase>.m_readerPool;
            }
        }

        protected static MyObjectsPool<CustomRootWriter> WriterPool
        {
            get
            {
                if (MyXmlSerializerBase<TAbstractBase>.m_writerPool == null)
                {
                    MyXmlSerializerBase<TAbstractBase>.m_writerPool = new MyObjectsPool<CustomRootWriter>(2, null);
                }
                return MyXmlSerializerBase<TAbstractBase>.m_writerPool;
            }
        }

        public TAbstractBase Data =>
            this.m_data;

        object IMyXmlSerializable.Data =>
            this.m_data;
    }
}

