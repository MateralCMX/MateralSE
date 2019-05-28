namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class TagDecorator : ProtoDecoratorBase, IProtoTypeSerializer, IProtoSerializer
    {
        private readonly bool strict;
        private readonly int fieldNumber;
        private readonly WireType wireType;

        public TagDecorator(int fieldNumber, WireType wireType, bool strict, IProtoSerializer tail) : base(tail)
        {
            this.fieldNumber = fieldNumber;
            this.wireType = wireType;
            this.strict = strict;
        }

        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            IProtoTypeSerializer tail = base.Tail as IProtoTypeSerializer;
            if (tail != null)
            {
                tail.Callback(value, callbackType, context);
            }
        }

        public bool CanCreateInstance()
        {
            IProtoTypeSerializer tail = base.Tail as IProtoTypeSerializer;
            return ((tail != null) && tail.CanCreateInstance());
        }

        public object CreateInstance(ProtoReader source) => 
            ((IProtoTypeSerializer) base.Tail).CreateInstance(source);

        public void EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
            ((IProtoTypeSerializer) base.Tail).EmitCallback(ctx, valueFrom, callbackType);
        }

        public void EmitCreateInstance(CompilerContext ctx)
        {
            ((IProtoTypeSerializer) base.Tail).EmitCreateInstance(ctx);
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            if (this.strict || this.NeedsHint)
            {
                ctx.LoadReaderWriter();
                ctx.LoadValue((int) this.wireType);
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod(this.strict ? "Assert" : "Hint"));
            }
            base.Tail.EmitRead(ctx, valueFrom);
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadValue(this.fieldNumber);
            ctx.LoadValue((int) this.wireType);
            ctx.LoadReaderWriter();
            ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("WriteFieldHeader"));
            base.Tail.EmitWrite(ctx, valueFrom);
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            IProtoTypeSerializer tail = base.Tail as IProtoTypeSerializer;
            return ((tail != null) && tail.HasCallbacks(callbackType));
        }

        public override object Read(object value, ProtoReader source)
        {
            if (this.strict)
            {
                source.Assert(this.wireType);
            }
            else if (this.NeedsHint)
            {
                source.Hint(this.wireType);
            }
            return base.Tail.Read(value, source);
        }

        public override void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteFieldHeader(this.fieldNumber, this.wireType, dest);
            base.Tail.Write(value, dest);
        }

        public override Type ExpectedType =>
            base.Tail.ExpectedType;

        public override bool RequiresOldValue =>
            base.Tail.RequiresOldValue;

        public override bool ReturnsValue =>
            base.Tail.ReturnsValue;

        private bool NeedsHint =>
            ((this.wireType & ~(WireType.Fixed32 | WireType.String)) != WireType.Variant);
    }
}

