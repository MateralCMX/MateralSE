namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class Int16Serializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(short);

        public Int16Serializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadInt16", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteInt16", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadInt16();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteInt16((short) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

