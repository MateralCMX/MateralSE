namespace ProtoBuf.ServiceModel
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class ProtoOperationBehavior : DataContractSerializerOperationBehavior
    {
        private TypeModel model;

        public ProtoOperationBehavior(OperationDescription operation) : base(operation)
        {
            this.model = RuntimeTypeModel.Default;
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            if (this.model == null)
            {
                throw new InvalidOperationException("No Model instance has been assigned to the ProtoOperationBehavior");
            }
            XmlProtoSerializer serializer1 = XmlProtoSerializer.TryCreate(this.model, type);
            return (serializer1 ?? base.CreateSerializer(type, name, ns, knownTypes));
        }

        public TypeModel Model
        {
            get => 
                this.model;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Model");
                }
                this.model = value;
            }
        }
    }
}

