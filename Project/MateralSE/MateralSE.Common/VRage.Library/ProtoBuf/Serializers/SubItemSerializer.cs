namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection.Emit;

    internal sealed class SubItemSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly int key;
        private readonly Type type;
        private readonly ISerializerProxy proxy;
        private readonly bool recursionCheck;

        public SubItemSerializer(Type type, int key, ISerializerProxy proxy, bool recursionCheck)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }
            this.type = type;
            this.proxy = proxy;
            this.key = key;
            this.recursionCheck = recursionCheck;
        }

        private bool EmitDedicatedMethod(CompilerContext ctx, Local valueFrom, bool read)
        {
            MethodBuilder dedicatedMethod = ctx.GetDedicatedMethod(this.key, read);
            if (dedicatedMethod == null)
            {
                return false;
            }
            using (Local local = new Local(ctx, ctx.MapType(typeof(SubItemToken))))
            {
                Type type = ctx.MapType(read ? typeof(ProtoReader) : typeof(ProtoWriter));
                ctx.LoadValue(valueFrom);
                if (!read)
                {
                    if (this.type.IsValueType || !this.recursionCheck)
                    {
                        ctx.LoadNullRef();
                    }
                    else
                    {
                        ctx.CopyValue();
                    }
                }
                ctx.LoadReaderWriter();
                ctx.EmitCall(type.GetMethod("StartSubItem"));
                ctx.StoreValue(local);
                ctx.LoadReaderWriter();
                ctx.EmitCall(dedicatedMethod);
                if (read && (this.type != dedicatedMethod.ReturnType))
                {
                    ctx.Cast(this.type);
                }
                ctx.LoadValue(local);
                ctx.LoadReaderWriter();
                ctx.EmitCall(type.GetMethod("EndSubItem"));
            }
            return true;
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            if (!this.EmitDedicatedMethod(ctx, valueFrom, true))
            {
                ctx.LoadValue(valueFrom);
                if (this.type.IsValueType)
                {
                    ctx.CastToObject(this.type);
                }
                ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(this.key));
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("ReadObject"));
                ctx.CastFromObject(this.type);
            }
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            if (!this.EmitDedicatedMethod(ctx, valueFrom, false))
            {
                ctx.LoadValue(valueFrom);
                if (this.type.IsValueType)
                {
                    ctx.CastToObject(this.type);
                }
                ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(this.key));
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod(this.recursionCheck ? "WriteObject" : "WriteRecursionSafeObject"));
            }
        }

        object IProtoSerializer.Read(object value, ProtoReader source) => 
            ProtoReader.ReadObject(value, this.key, source);

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            if (this.recursionCheck)
            {
                ProtoWriter.WriteObject(value, this.key, dest);
            }
            else
            {
                ProtoWriter.WriteRecursionSafeObject(value, this.key, dest);
            }
        }

        void IProtoTypeSerializer.Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            ((IProtoTypeSerializer) this.proxy.Serializer).Callback(value, callbackType, context);
        }

        bool IProtoTypeSerializer.CanCreateInstance() => 
            ((IProtoTypeSerializer) this.proxy.Serializer).CanCreateInstance();

        object IProtoTypeSerializer.CreateInstance(ProtoReader source) => 
            ((IProtoTypeSerializer) this.proxy.Serializer).CreateInstance(source);

        void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
            ((IProtoTypeSerializer) this.proxy.Serializer).EmitCallback(ctx, valueFrom, callbackType);
        }

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            ((IProtoTypeSerializer) this.proxy.Serializer).EmitCreateInstance(ctx);
        }

        bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType) => 
            ((IProtoTypeSerializer) this.proxy.Serializer).HasCallbacks(callbackType);

        Type IProtoSerializer.ExpectedType =>
            this.type;

        bool IProtoSerializer.RequiresOldValue =>
            true;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

