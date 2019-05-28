namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class DoubleSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(double);

        public DoubleSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadDouble", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteDouble", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadDouble();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteDouble((double) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

