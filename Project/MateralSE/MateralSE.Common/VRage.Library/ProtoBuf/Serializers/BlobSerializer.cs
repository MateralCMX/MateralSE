namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class BlobSerializer : IProtoSerializer
    {
        private static readonly Type expectedType = typeof(byte[]);
        private readonly bool overwriteList;

        public BlobSerializer(TypeModel model, bool overwriteList)
        {
            this.overwriteList = overwriteList;
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            if (this.overwriteList)
            {
                ctx.LoadNullRef();
            }
            else
            {
                ctx.LoadValue(valueFrom);
            }
            ctx.LoadReaderWriter();
            ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("AppendBytes"));
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteBytes", valueFrom);
        }

        public object Read(object value, ProtoReader source) => 
            ProtoReader.AppendBytes(this.overwriteList ? null : ((byte[]) value), source);

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteBytes((byte[]) value, dest);
        }

        public Type ExpectedType =>
            expectedType;

        bool IProtoSerializer.RequiresOldValue =>
            !this.overwriteList;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

