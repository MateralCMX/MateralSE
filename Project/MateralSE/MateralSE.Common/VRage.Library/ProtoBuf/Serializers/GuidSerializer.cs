namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class GuidSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(Guid);

        public GuidSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadGuid", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteGuid", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            BclHelpers.ReadGuid(source);

        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteGuid((Guid) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

