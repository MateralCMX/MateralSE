namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class ByteSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(byte);

        public ByteSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadByte", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteByte", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadByte();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteByte((byte) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

