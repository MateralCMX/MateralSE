namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class SByteSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(sbyte);

        public SByteSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadSByte", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteSByte", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadSByte();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteSByte((sbyte) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

