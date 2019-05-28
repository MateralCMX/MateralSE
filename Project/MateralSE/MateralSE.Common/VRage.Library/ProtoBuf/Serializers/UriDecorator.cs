namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class UriDecorator : ProtoDecoratorBase
    {
        private static readonly Type expectedType = typeof(Uri);

        public UriDecorator(TypeModel model, IProtoSerializer tail) : base(tail)
        {
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            base.Tail.EmitRead(ctx, valueFrom);
            ctx.CopyValue();
            CodeLabel label = ctx.DefineLabel();
            CodeLabel label2 = ctx.DefineLabel();
            ctx.LoadValue(typeof(string).GetProperty("Length"));
            ctx.BranchIfTrue(label, true);
            ctx.DiscardValue();
            ctx.LoadNullRef();
            ctx.Branch(label2, true);
            ctx.MarkLabel(label);
            Type[] parameterTypes = new Type[] { ctx.MapType(typeof(string)) };
            ctx.EmitCtor(ctx.MapType(typeof(Uri)), parameterTypes);
            ctx.MarkLabel(label2);
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.LoadValue(typeof(Uri).GetProperty("AbsoluteUri"));
            base.Tail.EmitWrite(ctx, null);
        }

        public override object Read(object value, ProtoReader source)
        {
            string uriString = (string) base.Tail.Read(null, source);
            return ((uriString.Length == 0) ? null : new Uri(uriString));
        }

        public override void Write(object value, ProtoWriter dest)
        {
            base.Tail.Write(((Uri) value).AbsoluteUri, dest);
        }

        public override Type ExpectedType =>
            expectedType;

        public override bool RequiresOldValue =>
            false;

        public override bool ReturnsValue =>
            true;
    }
}

