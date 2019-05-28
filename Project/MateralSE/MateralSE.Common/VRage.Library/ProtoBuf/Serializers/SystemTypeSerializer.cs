namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal class SystemTypeSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(Type);

        public SystemTypeSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadType", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteType", valueFrom);
        }

        object IProtoSerializer.Read(object value, ProtoReader source) => 
            source.ReadType();

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteType((Type) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

