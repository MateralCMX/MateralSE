namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using System;
    using System.Reflection;

    internal sealed class FieldDecorator : ProtoDecoratorBase
    {
        private readonly FieldInfo field;
        private readonly Type forType;

        public FieldDecorator(Type forType, FieldInfo field, IProtoSerializer tail) : base(tail)
        {
            this.forType = forType;
            this.field = field;
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = ctx.GetLocalWithValue(this.ExpectedType, valueFrom))
            {
                ctx.LoadAddress(local, this.ExpectedType);
                if (base.Tail.RequiresOldValue)
                {
                    ctx.CopyValue();
                    ctx.LoadValue(this.field);
                }
                ctx.ReadNullCheckedTail(this.field.FieldType, base.Tail, null);
                if (!base.Tail.ReturnsValue)
                {
                    ctx.DiscardValue();
                }
                else if (this.field.FieldType.IsValueType)
                {
                    ctx.StoreValue(this.field);
                }
                else
                {
                    CodeLabel label = ctx.DefineLabel();
                    CodeLabel label2 = ctx.DefineLabel();
                    ctx.CopyValue();
                    ctx.BranchIfTrue(label, true);
                    ctx.DiscardValue();
                    ctx.DiscardValue();
                    ctx.Branch(label2, true);
                    ctx.MarkLabel(label);
                    ctx.StoreValue(this.field);
                    ctx.MarkLabel(label2);
                }
            }
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadAddress(valueFrom, this.ExpectedType);
            ctx.LoadValue(this.field);
            ctx.WriteNullCheckedTail(this.field.FieldType, base.Tail, null);
        }

        public override object Read(object value, ProtoReader source)
        {
            object obj2 = base.Tail.Read(base.Tail.RequiresOldValue ? this.field.GetValue(value) : null, source);
            if (obj2 != null)
            {
                this.field.SetValue(value, obj2);
            }
            return null;
        }

        public override void Write(object value, ProtoWriter dest)
        {
            object obj1 = this.field.GetValue(value);
            value = obj1;
            if (value != null)
            {
                base.Tail.Write(value, dest);
            }
        }

        public override Type ExpectedType =>
            this.forType;

        public override bool RequiresOldValue =>
            true;

        public override bool ReturnsValue =>
            false;
    }
}

