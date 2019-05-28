namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class NullDecorator : ProtoDecoratorBase
    {
        private readonly Type expectedType;
        public const int Tag = 1;

        public NullDecorator(TypeModel model, IProtoSerializer tail) : base(tail)
        {
            if (!tail.ReturnsValue)
            {
                throw new NotSupportedException("NullDecorator only supports implementations that return values");
            }
            if (!Helpers.IsValueType(tail.ExpectedType))
            {
                this.expectedType = tail.ExpectedType;
            }
            else
            {
                Type[] typeArguments = new Type[] { tail.ExpectedType };
                this.expectedType = model.MapType(typeof(Nullable<>)).MakeGenericType(typeArguments);
            }
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = ctx.GetLocalWithValue(this.expectedType, valueFrom))
            {
                using (Local local2 = new Local(ctx, ctx.MapType(typeof(SubItemToken))))
                {
                    using (Local local3 = new Local(ctx, ctx.MapType(typeof(int))))
                    {
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("StartSubItem"));
                        ctx.StoreValue(local2);
                        CodeLabel label = ctx.DefineLabel();
                        CodeLabel label2 = ctx.DefineLabel();
                        CodeLabel label3 = ctx.DefineLabel();
                        ctx.MarkLabel(label);
                        ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
                        ctx.CopyValue();
                        ctx.StoreValue(local3);
                        ctx.LoadValue(1);
                        ctx.BranchIfEqual(label2, true);
                        ctx.LoadValue(local3);
                        ctx.LoadValue(1);
                        ctx.BranchIfLess(label3, false);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
                        ctx.Branch(label, true);
                        ctx.MarkLabel(label2);
                        if (base.Tail.RequiresOldValue)
                        {
                            if (!this.expectedType.IsValueType)
                            {
                                ctx.LoadValue(local);
                            }
                            else
                            {
                                ctx.LoadAddress(local, this.expectedType);
                                ctx.EmitCall(this.expectedType.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                            }
                        }
                        base.Tail.EmitRead(ctx, null);
                        if (this.expectedType.IsValueType)
                        {
                            Type[] parameterTypes = new Type[] { base.Tail.ExpectedType };
                            ctx.EmitCtor(this.expectedType, parameterTypes);
                        }
                        ctx.StoreValue(local);
                        ctx.Branch(label, false);
                        ctx.MarkLabel(label3);
                        ctx.LoadValue(local2);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("EndSubItem"));
                        ctx.LoadValue(local);
                    }
                }
            }
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = ctx.GetLocalWithValue(this.expectedType, valueFrom))
            {
                using (Local local2 = new Local(ctx, ctx.MapType(typeof(SubItemToken))))
                {
                    ctx.LoadNullRef();
                    ctx.LoadReaderWriter();
                    ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("StartSubItem"));
                    ctx.StoreValue(local2);
                    if (!this.expectedType.IsValueType)
                    {
                        ctx.LoadValue(local);
                    }
                    else
                    {
                        ctx.LoadAddress(local, this.expectedType);
                        ctx.LoadValue(this.expectedType.GetProperty("HasValue"));
                    }
                    CodeLabel label = ctx.DefineLabel();
                    ctx.BranchIfFalse(label, false);
                    if (!this.expectedType.IsValueType)
                    {
                        ctx.LoadValue(local);
                    }
                    else
                    {
                        ctx.LoadAddress(local, this.expectedType);
                        ctx.EmitCall(this.expectedType.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                    }
                    base.Tail.EmitWrite(ctx, null);
                    ctx.MarkLabel(label);
                    ctx.LoadValue(local2);
                    ctx.LoadReaderWriter();
                    ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("EndSubItem"));
                }
            }
        }

        public override object Read(object value, ProtoReader source)
        {
            int num;
            SubItemToken token = ProtoReader.StartSubItem(source);
            while ((num = source.ReadFieldHeader()) > 0)
            {
                if (num != 1)
                {
                    source.SkipField();
                    continue;
                }
                object obj1 = base.Tail.Read(value, source);
                value = obj1;
            }
            ProtoReader.EndSubItem(token, source);
            return value;
        }

        public override void Write(object value, ProtoWriter dest)
        {
            SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            if (value != null)
            {
                base.Tail.Write(value, dest);
            }
            ProtoWriter.EndSubItem(token, dest);
        }

        public override Type ExpectedType =>
            this.expectedType;

        public override bool ReturnsValue =>
            true;

        public override bool RequiresOldValue =>
            true;
    }
}

