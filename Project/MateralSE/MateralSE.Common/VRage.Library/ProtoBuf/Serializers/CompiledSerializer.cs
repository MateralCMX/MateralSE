namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class CompiledSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly IProtoTypeSerializer head;
        private readonly ProtoSerializer serializer;
        private readonly ProtoDeserializer deserializer;

        private CompiledSerializer(IProtoTypeSerializer head, TypeModel model)
        {
            this.head = head;
            this.serializer = CompilerContext.BuildSerializer(head, model);
            this.deserializer = CompilerContext.BuildDeserializer(head, model);
        }

        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            this.head.Callback(value, callbackType, context);
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            this.head.EmitRead(ctx, valueFrom);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            this.head.EmitWrite(ctx, valueFrom);
        }

        object IProtoSerializer.Read(object value, ProtoReader source) => 
            this.deserializer(value, source);

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            this.serializer(value, dest);
        }

        bool IProtoTypeSerializer.CanCreateInstance() => 
            this.head.CanCreateInstance();

        object IProtoTypeSerializer.CreateInstance(ProtoReader source) => 
            this.head.CreateInstance(source);

        void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
            this.head.EmitCallback(ctx, valueFrom, callbackType);
        }

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            this.head.EmitCreateInstance(ctx);
        }

        bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType) => 
            this.head.HasCallbacks(callbackType);

        public static CompiledSerializer Wrap(IProtoTypeSerializer head, TypeModel model)
        {
            CompiledSerializer serializer = head as CompiledSerializer;
            if (serializer == null)
            {
                serializer = new CompiledSerializer(head, model);
            }
            return serializer;
        }

        bool IProtoSerializer.RequiresOldValue =>
            this.head.RequiresOldValue;

        bool IProtoSerializer.ReturnsValue =>
            this.head.ReturnsValue;

        Type IProtoSerializer.ExpectedType =>
            this.head.ExpectedType;
    }
}

