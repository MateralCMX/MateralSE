namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;

    internal sealed class TupleSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly MemberInfo[] members;
        private readonly ConstructorInfo ctor;
        private IProtoSerializer[] tails;

        public TupleSerializer(RuntimeTypeModel model, ConstructorInfo ctor, MemberInfo[] members)
        {
            if (ctor == null)
            {
                throw new ArgumentNullException("ctor");
            }
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }
            this.ctor = ctor;
            this.members = members;
            this.tails = new IProtoSerializer[members.Length];
            ParameterInfo[] parameters = ctor.GetParameters();
            for (int i = 0; i < members.Length; i++)
            {
                WireType type;
                Type parameterType = parameters[i].ParameterType;
                Type itemType = null;
                Type defaultType = null;
                MetaType.ResolveListTypes(model, parameterType, ref itemType, ref defaultType);
                Type type5 = (itemType == null) ? parameterType : itemType;
                bool asReference = false;
                if (model.FindOrAddAuto(type5, false, true, false) >= 0)
                {
                    asReference = model[type5].AsReferenceDefault;
                }
                IProtoSerializer tail = ValueMember.TryGetCoreSerializer(model, DataFormat.Default, type5, out type, asReference, false, false, true);
                if (tail == null)
                {
                    throw new InvalidOperationException("No serializer defined for type: " + type5.FullName);
                }
                tail = new TagDecorator(i + 1, type, false, tail);
                this.tails[i] = (itemType != null) ? (!parameterType.IsArray ? ((IProtoSerializer) new ListDecorator(model, parameterType, defaultType, tail, i + 1, false, type, true, false, false)) : ((IProtoSerializer) new ArrayDecorator(model, tail, i + 1, false, type, parameterType, false, false))) : tail;
            }
        }

        public void EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
        }

        public void EmitRead(CompilerContext ctx, Local incoming)
        {
            using (Local local = ctx.GetLocalWithValue(this.ExpectedType, incoming))
            {
                Local[] localArray = new Local[this.members.Length];
                try
                {
                    bool flag;
                    int index = 0;
                    goto TR_0043;
                TR_0031:
                    index++;
                    goto TR_0043;
                TR_0033:
                    if (flag)
                    {
                        ctx.StoreValue(localArray[index]);
                    }
                    goto TR_0031;
                TR_0043:
                    while (true)
                    {
                        if (index < localArray.Length)
                        {
                            Type memberType = this.GetMemberType(index);
                            flag = true;
                            localArray[index] = new Local(ctx, memberType);
                            if (this.ExpectedType.IsValueType)
                            {
                                goto TR_0031;
                            }
                            else if (!memberType.IsValueType)
                            {
                                ctx.LoadNullRef();
                            }
                            else
                            {
                                ProtoTypeCode typeCode = Helpers.GetTypeCode(memberType);
                                switch (typeCode)
                                {
                                    case ProtoTypeCode.Boolean:
                                    case ProtoTypeCode.SByte:
                                    case ProtoTypeCode.Byte:
                                    case ProtoTypeCode.Int16:
                                    case ProtoTypeCode.UInt16:
                                    case ProtoTypeCode.Int32:
                                    case ProtoTypeCode.UInt32:
                                        ctx.LoadValue(0);
                                        goto TR_0033;

                                    case ProtoTypeCode.Char:
                                        break;

                                    case ProtoTypeCode.Int64:
                                    case ProtoTypeCode.UInt64:
                                        ctx.LoadValue((long) 0L);
                                        goto TR_0033;

                                    case ProtoTypeCode.Single:
                                        ctx.LoadValue((float) 0f);
                                        goto TR_0033;

                                    case ProtoTypeCode.Double:
                                        ctx.LoadValue((double) 0.0);
                                        goto TR_0033;

                                    case ProtoTypeCode.Decimal:
                                        ctx.LoadValue((decimal) 0M);
                                        goto TR_0033;

                                    default:
                                        if (typeCode != ProtoTypeCode.Guid)
                                        {
                                            break;
                                        }
                                        ctx.LoadValue(Guid.Empty);
                                        goto TR_0033;
                                }
                                ctx.LoadAddress(localArray[index], memberType);
                                ctx.EmitCtor(memberType);
                                flag = false;
                            }
                        }
                        else
                        {
                            CodeLabel label1;
                            if (!this.ExpectedType.IsValueType)
                            {
                                label1 = ctx.DefineLabel();
                            }
                            else
                            {
                                label1 = new CodeLabel();
                            }
                            CodeLabel label = label1;
                            if (!this.ExpectedType.IsValueType)
                            {
                                ctx.LoadAddress(local, this.ExpectedType);
                                ctx.BranchIfFalse(label, false);
                            }
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= this.members.Length)
                                {
                                    if (!this.ExpectedType.IsValueType)
                                    {
                                        ctx.MarkLabel(label);
                                    }
                                    using (Local local2 = new Local(ctx, ctx.MapType(typeof(int))))
                                    {
                                        CodeLabel label3 = ctx.DefineLabel();
                                        CodeLabel label4 = ctx.DefineLabel();
                                        CodeLabel label5 = ctx.DefineLabel();
                                        ctx.Branch(label3, false);
                                        CodeLabel[] jumpTable = new CodeLabel[this.members.Length];
                                        int num3 = 0;
                                        while (true)
                                        {
                                            if (num3 >= this.members.Length)
                                            {
                                                ctx.MarkLabel(label4);
                                                ctx.LoadValue(local2);
                                                ctx.LoadValue(1);
                                                ctx.Subtract();
                                                ctx.Switch(jumpTable);
                                                ctx.Branch(label5, false);
                                                int num4 = 0;
                                                while (true)
                                                {
                                                    if (num4 >= jumpTable.Length)
                                                    {
                                                        ctx.MarkLabel(label5);
                                                        ctx.LoadReaderWriter();
                                                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
                                                        ctx.MarkLabel(label3);
                                                        ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
                                                        ctx.CopyValue();
                                                        ctx.StoreValue(local2);
                                                        ctx.LoadValue(0);
                                                        ctx.BranchIfGreater(label4, false);
                                                        break;
                                                    }
                                                    ctx.MarkLabel(jumpTable[num4]);
                                                    IProtoSerializer tail = this.tails[num4];
                                                    ctx.ReadNullCheckedTail(localArray[num4].Type, tail, tail.RequiresOldValue ? localArray[num4] : null);
                                                    if (tail.ReturnsValue)
                                                    {
                                                        if (localArray[num4].Type.IsValueType)
                                                        {
                                                            ctx.StoreValue(localArray[num4]);
                                                        }
                                                        else
                                                        {
                                                            CodeLabel label6 = ctx.DefineLabel();
                                                            CodeLabel label7 = ctx.DefineLabel();
                                                            ctx.CopyValue();
                                                            ctx.BranchIfTrue(label6, true);
                                                            ctx.DiscardValue();
                                                            ctx.Branch(label7, true);
                                                            ctx.MarkLabel(label6);
                                                            ctx.StoreValue(localArray[num4]);
                                                            ctx.MarkLabel(label7);
                                                        }
                                                    }
                                                    ctx.Branch(label3, false);
                                                    num4++;
                                                }
                                                break;
                                            }
                                            jumpTable[num3] = ctx.DefineLabel();
                                            num3++;
                                        }
                                    }
                                    int num5 = 0;
                                    while (true)
                                    {
                                        if (num5 >= localArray.Length)
                                        {
                                            ctx.EmitCtor(this.ctor);
                                            ctx.StoreValue(local);
                                            break;
                                        }
                                        ctx.LoadValue(localArray[num5]);
                                        num5++;
                                    }
                                    break;
                                }
                                ctx.LoadAddress(local, this.ExpectedType);
                                MemberTypes memberType = this.members[num2].MemberType;
                                if (memberType == MemberTypes.Field)
                                {
                                    ctx.LoadValue((FieldInfo) this.members[num2]);
                                }
                                else if (memberType == MemberTypes.Property)
                                {
                                    ctx.LoadValue((PropertyInfo) this.members[num2]);
                                }
                                ctx.StoreValue(localArray[num2]);
                                num2++;
                            }
                            return;
                        }
                        break;
                    }
                    goto TR_0033;
                }
                finally
                {
                    for (int i = 0; i < localArray.Length; i++)
                    {
                        if (localArray[i] != null)
                        {
                            localArray[i].Dispose();
                        }
                    }
                }
            }
        }

        public void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = ctx.GetLocalWithValue(this.ctor.DeclaringType, valueFrom))
            {
                for (int i = 0; i < this.tails.Length; i++)
                {
                    Type memberType = this.GetMemberType(i);
                    ctx.LoadAddress(local, this.ExpectedType);
                    MemberTypes types = this.members[i].MemberType;
                    if (types == MemberTypes.Field)
                    {
                        ctx.LoadValue((FieldInfo) this.members[i]);
                    }
                    else if (types == MemberTypes.Property)
                    {
                        ctx.LoadValue((PropertyInfo) this.members[i]);
                    }
                    ctx.WriteNullCheckedTail(memberType, this.tails[i], null);
                }
            }
        }

        private Type GetMemberType(int index)
        {
            Type memberType = Helpers.GetMemberType(this.members[index]);
            if (memberType == null)
            {
                throw new InvalidOperationException();
            }
            return memberType;
        }

        private object GetValue(object obj, int index)
        {
            PropertyInfo info = this.members[index] as PropertyInfo;
            if (info != null)
            {
                return ((obj != null) ? info.GetValue(obj, null) : (Helpers.IsValueType(info.PropertyType) ? Activator.CreateInstance(info.PropertyType) : null));
            }
            FieldInfo info2 = this.members[index] as FieldInfo;
            if (info2 == null)
            {
                throw new InvalidOperationException();
            }
            return ((obj != null) ? info2.GetValue(obj) : (Helpers.IsValueType(info2.FieldType) ? Activator.CreateInstance(info2.FieldType) : null));
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType) => 
            false;

        void IProtoTypeSerializer.Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
        }

        bool IProtoTypeSerializer.CanCreateInstance() => 
            false;

        object IProtoTypeSerializer.CreateInstance(ProtoReader source)
        {
            throw new NotSupportedException();
        }

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            throw new NotSupportedException();
        }

        public object Read(object value, ProtoReader source)
        {
            int num;
            object[] parameters = new object[this.members.Length];
            bool flag = false;
            if (value == null)
            {
                flag = true;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = this.GetValue(value, i);
            }
            while ((num = source.ReadFieldHeader()) > 0)
            {
                flag = true;
                if (num > this.tails.Length)
                {
                    source.SkipField();
                    continue;
                }
                IProtoSerializer serializer = this.tails[num - 1];
                parameters[num - 1] = this.tails[num - 1].Read(serializer.RequiresOldValue ? parameters[num - 1] : null, source);
            }
            return (flag ? this.ctor.Invoke(parameters) : value);
        }

        public void Write(object value, ProtoWriter dest)
        {
            for (int i = 0; i < this.tails.Length; i++)
            {
                object obj2 = this.GetValue(value, i);
                if (obj2 != null)
                {
                    this.tails[i].Write(obj2, dest);
                }
            }
        }

        public Type ExpectedType =>
            this.ctor.DeclaringType;

        public bool RequiresOldValue =>
            true;

        public bool ReturnsValue =>
            false;
    }
}

