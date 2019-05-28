namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class StringSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(string);

        public StringSerializer(TypeModel model)
        {
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadString", this.ExpectedType);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteString", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            source.ReadString();

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteString((string) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

