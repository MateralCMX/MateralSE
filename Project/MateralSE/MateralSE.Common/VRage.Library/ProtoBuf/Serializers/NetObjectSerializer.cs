namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class NetObjectSerializer : IProtoSerializer
    {
        private readonly int key;
        private readonly Type type;
        private readonly BclHelpers.NetObjectOptions options;

        public NetObjectSerializer(TypeModel model, Type type, int key, BclHelpers.NetObjectOptions options)
        {
            bool flag = (options & BclHelpers.NetObjectOptions.DynamicType) != BclHelpers.NetObjectOptions.None;
            this.key = flag ? -1 : key;
            this.type = flag ? model.MapType(typeof(object)) : type;
            this.options = options;
        }

        public void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.CastToObject(this.type);
            ctx.LoadReaderWriter();
            ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(this.key));
            if (this.type == ctx.MapType(typeof(object)))
            {
                ctx.LoadNullRef();
            }
            else
            {
                ctx.LoadValue(this.type);
            }
            ctx.LoadValue((int) this.options);
            ctx.EmitCall(ctx.MapType(typeof(BclHelpers)).GetMethod("ReadNetObject"));
            ctx.CastFromObject(this.type);
        }

        public void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.CastToObject(this.type);
            ctx.LoadReaderWriter();
            ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(this.key));
            ctx.LoadValue((int) this.options);
            ctx.EmitCall(ctx.MapType(typeof(BclHelpers)).GetMethod("WriteNetObject"));
        }

        public object Read(object value, ProtoReader source) => 
            BclHelpers.ReadNetObject(value, source, this.key, (this.type == typeof(object)) ? null : this.type, this.options);

        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteNetObject(value, dest, this.key, this.options);
        }

        public Type ExpectedType =>
            this.type;

        public bool ReturnsValue =>
            true;

        public bool RequiresOldValue =>
            true;
    }
}

