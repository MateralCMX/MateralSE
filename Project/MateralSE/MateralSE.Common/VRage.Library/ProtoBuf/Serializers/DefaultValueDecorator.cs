namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;

    internal sealed class DefaultValueDecorator : ProtoDecoratorBase
    {
        private readonly object defaultValue;

        public DefaultValueDecorator(TypeModel model, object defaultValue, IProtoSerializer tail) : base(tail)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException("defaultValue");
            }
            if (model.MapType(defaultValue.GetType()) != tail.ExpectedType)
            {
                throw new ArgumentException("Default value is of incorrect type", "defaultValue");
            }
            this.defaultValue = defaultValue;
        }

        private void EmitBeq(CompilerContext ctx, CodeLabel label, Type type)
        {
            if ((Helpers.GetTypeCode(type) - 3) <= ProtoTypeCode.Int64)
            {
                ctx.BranchIfEqual(label, false);
            }
            else
            {
                Type[] types = new Type[] { type, type };
                MethodInfo method = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static, null, types, null);
                if ((method == null) || (method.ReturnType != ctx.MapType(typeof(bool))))
                {
                    throw new InvalidOperationException("No suitable equality operator found for default-values of type: " + type.FullName);
                }
                ctx.EmitCall(method);
                ctx.BranchIfTrue(label, false);
            }
        }

        private void EmitBranchIfDefaultValue(CompilerContext ctx, CodeLabel label)
        {
            ProtoTypeCode typeCode = Helpers.GetTypeCode(this.ExpectedType);
            switch (typeCode)
            {
                case ProtoTypeCode.Boolean:
                    if ((bool) this.defaultValue)
                    {
                        ctx.BranchIfTrue(label, false);
                        return;
                    }
                    ctx.BranchIfFalse(label, false);
                    return;

                case ProtoTypeCode.Char:
                    if (((char) this.defaultValue) == '\0')
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) ((char) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.SByte:
                    if (((sbyte) this.defaultValue) == 0)
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) ((sbyte) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Byte:
                    if (((byte) this.defaultValue) == 0)
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) ((byte) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Int16:
                    if (((short) this.defaultValue) == 0)
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) ((short) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.UInt16:
                    if (((ushort) this.defaultValue) == 0)
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) ((ushort) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Int32:
                    if (((int) this.defaultValue) == 0)
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) this.defaultValue);
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.UInt32:
                    if (((uint) this.defaultValue) == 0)
                    {
                        ctx.BranchIfFalse(label, false);
                        return;
                    }
                    ctx.LoadValue((int) ((uint) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Int64:
                    ctx.LoadValue((long) this.defaultValue);
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.UInt64:
                    ctx.LoadValue((long) ((ulong) this.defaultValue));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Single:
                    ctx.LoadValue((float) this.defaultValue);
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Double:
                    ctx.LoadValue((double) this.defaultValue);
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case ProtoTypeCode.Decimal:
                {
                    decimal defaultValue = (decimal) this.defaultValue;
                    ctx.LoadValue(defaultValue);
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;
                }
                case ProtoTypeCode.DateTime:
                    ctx.LoadValue(((DateTime) this.defaultValue).ToBinary());
                    ctx.EmitCall(ctx.MapType(typeof(DateTime)).GetMethod("FromBinary"));
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                    break;

                case ProtoTypeCode.String:
                    ctx.LoadValue((string) this.defaultValue);
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;

                default:
                {
                    if (typeCode != ProtoTypeCode.TimeSpan)
                    {
                        if (typeCode != ProtoTypeCode.Guid)
                        {
                            break;
                        }
                        ctx.LoadValue((Guid) this.defaultValue);
                        this.EmitBeq(ctx, label, this.ExpectedType);
                        return;
                    }
                    TimeSpan defaultValue = (TimeSpan) this.defaultValue;
                    if (defaultValue == TimeSpan.Zero)
                    {
                        ctx.LoadValue(typeof(TimeSpan).GetField("Zero"));
                    }
                    else
                    {
                        ctx.LoadValue(defaultValue.Ticks);
                        ctx.EmitCall(ctx.MapType(typeof(TimeSpan)).GetMethod("FromTicks"));
                    }
                    this.EmitBeq(ctx, label, this.ExpectedType);
                    return;
                }
            }
            throw new NotSupportedException("Type cannot be represented as a default value: " + this.ExpectedType.FullName);
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            base.Tail.EmitRead(ctx, valueFrom);
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            CodeLabel label = ctx.DefineLabel();
            if (valueFrom != null)
            {
                ctx.LoadValue(valueFrom);
                this.EmitBranchIfDefaultValue(ctx, label);
                base.Tail.EmitWrite(ctx, valueFrom);
            }
            else
            {
                ctx.CopyValue();
                CodeLabel label2 = ctx.DefineLabel();
                this.EmitBranchIfDefaultValue(ctx, label2);
                base.Tail.EmitWrite(ctx, null);
                ctx.Branch(label, true);
                ctx.MarkLabel(label2);
                ctx.DiscardValue();
            }
            ctx.MarkLabel(label);
        }

        public override object Read(object value, ProtoReader source) => 
            base.Tail.Read(value, source);

        public override void Write(object value, ProtoWriter dest)
        {
            if (!Equals(value, this.defaultValue))
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

