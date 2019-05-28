namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using System;
    using System.Reflection;

    internal sealed class MemberSpecifiedDecorator : ProtoDecoratorBase
    {
        private readonly MethodInfo getSpecified;
        private readonly MethodInfo setSpecified;

        public MemberSpecifiedDecorator(MethodInfo getSpecified, MethodInfo setSpecified, IProtoSerializer tail) : base(tail)
        {
            if ((getSpecified == null) && (setSpecified == null))
            {
                throw new InvalidOperationException();
            }
            this.getSpecified = getSpecified;
            this.setSpecified = setSpecified;
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            if (this.setSpecified == null)
            {
                base.Tail.EmitRead(ctx, valueFrom);
            }
            else
            {
                using (Local local = ctx.GetLocalWithValue(this.ExpectedType, valueFrom))
                {
                    base.Tail.EmitRead(ctx, local);
                    ctx.LoadAddress(local, this.ExpectedType);
                    ctx.LoadValue(1);
                    ctx.EmitCall(this.setSpecified);
                }
            }
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            if (this.getSpecified == null)
            {
                base.Tail.EmitWrite(ctx, valueFrom);
            }
            else
            {
                using (Local local = ctx.GetLocalWithValue(this.ExpectedType, valueFrom))
                {
                    ctx.LoadAddress(local, this.ExpectedType);
                    ctx.EmitCall(this.getSpecified);
                    CodeLabel label = ctx.DefineLabel();
                    ctx.BranchIfFalse(label, false);
                    base.Tail.EmitWrite(ctx, local);
                    ctx.MarkLabel(label);
                }
            }
        }

        public override object Read(object value, ProtoReader source)
        {
            object obj2 = base.Tail.Read(value, source);
            if (this.setSpecified != null)
            {
                object[] parameters = new object[] { true };
                this.setSpecified.Invoke(value, parameters);
            }
            return obj2;
        }

        public override void Write(object value, ProtoWriter dest)
        {
            if ((this.getSpecified == null) || ((bool) this.getSpecified.Invoke(value, null)))
            {
                base.Tail.Write(value, dest);
            }
        }

        public override Type ExpectedType =>
            base.Tail.ExpectedType;

        public override bool RequiresOldValue =>
            base.Tail.RequiresOldValue;

        public override bool ReturnsValue =>
            base.Tail.ReturnsValue;
    }
}

