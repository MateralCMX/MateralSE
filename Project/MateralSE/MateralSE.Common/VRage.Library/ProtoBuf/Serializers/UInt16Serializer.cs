namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal class UInt16Serializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(ushort);

        public UInt16Serializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadUInt16", ctx.MapType(typeof(ushort)));
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteUInt16", valueFrom);
        }

        public virtual object Read(object value, ProtoReader source) => 
            source.ReadUInt16();

        public virtual void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt16((ushort) value, dest);
        }

        public virtual Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

