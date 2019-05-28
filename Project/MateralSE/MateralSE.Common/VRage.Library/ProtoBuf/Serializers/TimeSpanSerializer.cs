namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class TimeSpanSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(TimeSpan);

        public TimeSpanSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadTimeSpan", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteTimeSpan", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            BclHelpers.ReadTimeSpan(source);

        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteTimeSpan((TimeSpan) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

