namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class BooleanSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(bool);

        public BooleanSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadBoolean", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteBoolean", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadBoolean();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteBoolean((bool) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

