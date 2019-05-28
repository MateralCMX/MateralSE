namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class DateTimeSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(DateTime);

        public DateTimeSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadDateTime", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteDateTime", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            BclHelpers.ReadDateTime(source);

        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteDateTime((DateTime) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

