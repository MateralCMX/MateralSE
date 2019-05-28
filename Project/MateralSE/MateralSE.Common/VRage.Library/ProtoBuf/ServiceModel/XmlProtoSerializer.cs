namespace ProtoBuf.ServiceModel
{
    using ProtoBuf;
    using ProtoBuf.Meta;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    public sealed class XmlProtoSerializer : XmlObjectSerializer
    {
        private readonly TypeModel model;
        private readonly int key;
        private readonly bool isList;
        private readonly Type type;
        private const string PROTO_ELEMENT = "proto";

        public XmlProtoSerializer(TypeModel model, Type type)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.key = GetKey(model, ref type, out this.isList);
            this.model = model;
            this.type = type;
            if (this.key < 0)
            {
                throw new ArgumentOutOfRangeException("type", "Type not recognised by the model: " + type.FullName);
            }
        }

        internal XmlProtoSerializer(TypeModel model, int key, Type type, bool isList)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (key < 0)
            {
                throw new ArgumentOutOfRangeException("key");
            }
            if (type == null)
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.model = model;
            this.key = key;
            this.isList = isList;
            this.type = type;
        }

        private static int GetKey(TypeModel model, ref Type type, out bool isList)
        {
            if ((model != null) && (type != null))
            {
                int key = model.GetKey(ref type);
                if (key >= 0)
                {
                    isList = false;
                    return key;
                }
                Type listItemType = TypeModel.GetListItemType(model, type);
                if (listItemType != null)
                {
                    key = model.GetKey(ref listItemType);
                    if (key >= 0)
                    {
                        isList = true;
                        return key;
                    }
                }
            }
            isList = false;
            return -1;
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            reader.MoveToContent();
            return ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "proto"));
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            object obj2;
            reader.MoveToContent();
            bool isEmptyElement = reader.IsEmptyElement;
            bool flag2 = reader.GetAttribute("nil") == "true";
            reader.ReadStartElement("proto");
            if (flag2)
            {
                if (!isEmptyElement)
                {
                    reader.ReadEndElement();
                }
                return null;
            }
            if (isEmptyElement)
            {
                if (this.isList)
                {
                    return this.model.Deserialize(Stream.Null, null, this.type, (SerializationContext) null);
                }
                using (ProtoReader reader2 = new ProtoReader(Stream.Null, this.model, null))
                {
                    return this.model.Deserialize(this.key, null, reader2);
                }
            }
            using (MemoryStream stream = new MemoryStream(reader.ReadContentAsBase64()))
            {
                if (this.isList)
                {
                    obj2 = this.model.Deserialize(stream, null, this.type, (SerializationContext) null);
                }
                else
                {
                    using (ProtoReader reader3 = new ProtoReader(stream, this.model, null))
                    {
                        obj2 = this.model.Deserialize(this.key, null, reader3);
                    }
                }
            }
            reader.ReadEndElement();
            return obj2;
        }

        public static XmlProtoSerializer TryCreate(TypeModel model, Type type)
        {
            bool flag;
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            int key = GetKey(model, ref type, out flag);
            return ((key < 0) ? null : new XmlProtoSerializer(model, key, type, flag));
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            writer.WriteEndElement();
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            if (graph == null)
            {
                writer.WriteAttributeString("nil", "true");
            }
            else
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    if (this.isList)
                    {
                        this.model.Serialize(stream, graph, null);
                    }
                    else
                    {
                        using (ProtoWriter writer2 = new ProtoWriter(stream, this.model, null))
                        {
                            this.model.Serialize(this.key, graph, writer2);
                        }
                    }
                    byte[] buffer = stream.GetBuffer();
                    writer.WriteBase64(buffer, 0, (int) stream.Length);
                }
            }
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            writer.WriteStartElement("proto");
        }
    }
}

