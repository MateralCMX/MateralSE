namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class UInt32Serializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(uint);

        public UInt32Serializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadUInt32", ctx.MapType(typeof(uint)));
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteUInt32", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadUInt32();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt32((uint) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

