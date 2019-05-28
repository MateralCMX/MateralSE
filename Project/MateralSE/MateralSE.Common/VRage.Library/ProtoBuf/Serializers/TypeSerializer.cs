namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal sealed class TypeSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly Type forType;
        private readonly Type constructType;
        private readonly IProtoSerializer[] serializers;
        private readonly int[] fieldNumbers;
        private readonly bool isRootType;
        private readonly bool useConstructor;
        private readonly bool isExtensible;
        private readonly bool hasConstructor;
        private readonly CallbackSet callbacks;
        private readonly MethodInfo[] baseCtorCallbacks;
        private readonly MethodInfo factory;
        private static readonly Type iextensible = typeof(IExtensible);

        public TypeSerializer(TypeModel model, Type forType, int[] fieldNumbers, IProtoSerializer[] serializers, MethodInfo[] baseCtorCallbacks, bool isRootType, bool useConstructor, CallbackSet callbacks, Type constructType, MethodInfo factory)
        {
            Helpers.Sort(fieldNumbers, serializers);
            bool flag = false;
            for (int i = 1; i < fieldNumbers.Length; i++)
            {
                if (fieldNumbers[i] == fieldNumbers[i - 1])
                {
                    throw new InvalidOperationException("Duplicate field-number detected; " + fieldNumbers[i].ToString() + " on: " + forType.FullName);
                }
                if (!flag && (serializers[i].ExpectedType != forType))
                {
                    flag = true;
                }
            }
            this.forType = forType;
            this.factory = factory;
            if (constructType == null)
            {
                constructType = forType;
            }
            else if (!forType.IsAssignableFrom(constructType))
            {
                throw new InvalidOperationException(forType.FullName + " cannot be assigned from " + constructType.FullName);
            }
            this.constructType = constructType;
            this.serializers = serializers;
            this.fieldNumbers = fieldNumbers;
            this.callbacks = callbacks;
            this.isRootType = isRootType;
            this.useConstructor = useConstructor;
            if ((baseCtorCallbacks != null) && (baseCtorCallbacks.Length == 0))
            {
                baseCtorCallbacks = null;
            }
            this.baseCtorCallbacks = baseCtorCallbacks;
            if (Helpers.GetUnderlyingType(forType) != null)
            {
                throw new ArgumentException("Cannot create a TypeSerializer for nullable types", "forType");
            }
            if (model.MapType(iextensible).IsAssignableFrom(forType))
            {
                if ((forType.IsValueType || !isRootType) | flag)
                {
                    throw new NotSupportedException("IExtensible is not supported in structs or classes with inheritance");
                }
                this.isExtensible = true;
            }
            this.hasConstructor = !constructType.IsAbstract && (Helpers.GetConstructor(constructType, Helpers.EmptyTypes, true) != null);
            if (((constructType != forType) & useConstructor) && !this.hasConstructor)
            {
                throw new ArgumentException("The supplied default implementation cannot be created: " + constructType.FullName, "constructType");
            }
        }

        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            if (this.callbacks != null)
            {
                this.InvokeCallback(this.callbacks[callbackType], value, context);
            }
            IProtoTypeSerializer moreSpecificSerializer = (IProtoTypeSerializer) this.GetMoreSpecificSerializer(value);
            if (moreSpecificSerializer != null)
            {
                moreSpecificSerializer.Callback(value, callbackType, context);
            }
        }

        private object CreateInstance(ProtoReader source, bool includeLocalCallback)
        {
            object uninitializedObject;
            if (this.factory != null)
            {
                uninitializedObject = this.InvokeCallback(this.factory, null, source.Context);
            }
            else if (!this.useConstructor)
            {
                uninitializedObject = BclHelpers.GetUninitializedObject(this.constructType);
            }
            else
            {
                if (!this.hasConstructor)
                {
                    TypeModel.ThrowCannotCreateInstance(this.constructType);
                }
                uninitializedObject = Activator.CreateInstance(this.constructType, true);
            }
            ProtoReader.NoteObject(uninitializedObject, source);
            if (this.baseCtorCallbacks != null)
            {
                for (int i = 0; i < this.baseCtorCallbacks.Length; i++)
                {
                    this.InvokeCallback(this.baseCtorCallbacks[i], uninitializedObject, source.Context);
                }
            }
            if (includeLocalCallback && (this.callbacks != null))
            {
                this.InvokeCallback(this.callbacks.BeforeDeserialize, uninitializedObject, source.Context);
            }
            return uninitializedObject;
        }

        private void EmitCallbackIfNeeded(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
            if (this.isRootType && this.HasCallbacks(callbackType))
            {
                ((IProtoTypeSerializer) this).EmitCallback(ctx, valueFrom, callbackType);
            }
        }

        private void EmitCreateIfNull(CompilerContext ctx, Local storage)
        {
            if (!this.ExpectedType.IsValueType)
            {
                CodeLabel label = ctx.DefineLabel();
                ctx.LoadValue(storage);
                ctx.BranchIfTrue(label, false);
                ((IProtoTypeSerializer) this).EmitCreateInstance(ctx);
                if (this.callbacks != null)
                {
                    EmitInvokeCallback(ctx, this.callbacks.BeforeDeserialize, true, null, this.forType);
                }
                ctx.StoreValue(storage);
                ctx.MarkLabel(label);
            }
        }

        private static void EmitInvokeCallback(CompilerContext ctx, MethodInfo method, bool copyValue, Type constructType, Type type)
        {
            if (method != null)
            {
                if (copyValue)
                {
                    ctx.CopyValue();
                }
                ParameterInfo[] parameters = method.GetParameters();
                bool flag = true;
                int num = 0;
                while (true)
                {
                    if (num >= parameters.Length)
                    {
                        if (!flag)
                        {
                            throw CallbackSet.CreateInvalidCallbackSignature(method);
                        }
                        ctx.EmitCall(method);
                        if ((constructType == null) || (method.ReturnType != ctx.MapType(typeof(object))))
                        {
                            break;
                        }
                        ctx.CastFromObject(type);
                        return;
                    }
                    Type parameterType = parameters[0].ParameterType;
                    if (parameterType == ctx.MapType(typeof(SerializationContext)))
                    {
                        ctx.LoadSerializationContext();
                    }
                    else if (parameterType == ctx.MapType(typeof(Type)))
                    {
                        ctx.LoadValue(constructType ?? type);
                    }
                    else if (!(parameterType == ctx.MapType(typeof(StreamingContext))))
                    {
                        flag = false;
                    }
                    else
                    {
                        ctx.LoadSerializationContext();
                        Type[] types = new Type[] { ctx.MapType(typeof(SerializationContext)) };
                        MethodInfo info = ctx.MapType(typeof(SerializationContext)).GetMethod("op_Implicit", types);
                        if (info != null)
                        {
                            ctx.EmitCall(info);
                            flag = true;
                        }
                    }
                    num++;
                }
            }
        }

        private IProtoSerializer GetMoreSpecificSerializer(object value)
        {
            if (this.CanHaveInheritance)
            {
                Type type = value.GetType();
                if (type == this.forType)
                {
                    return null;
                }
                for (int i = 0; i < this.serializers.Length; i++)
                {
                    IProtoSerializer serializer = this.serializers[i];
                    if ((serializer.ExpectedType != this.forType) && Helpers.IsAssignableFrom(serializer.ExpectedType, type))
                    {
                        return serializer;
                    }
                }
                if (type != this.constructType)
                {
                    TypeModel.ThrowUnexpectedSubtype(this.forType, type);
                }
            }
            return null;
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            if ((this.callbacks != null) && (this.callbacks[callbackType] != null))
            {
                return true;
            }
            for (int i = 0; i < this.serializers.Length; i++)
            {
                if ((this.serializers[i].ExpectedType != this.forType) && ((IProtoTypeSerializer) this.serializers[i]).HasCallbacks(callbackType))
                {
                    return true;
                }
            }
            return false;
        }

        private object InvokeCallback(MethodInfo method, object obj, SerializationContext context)
        {
            object obj2 = null;
            if (method != null)
            {
                object[] objArray;
                bool flag;
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    objArray = null;
                    flag = true;
                }
                else
                {
                    objArray = new object[parameters.Length];
                    flag = true;
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        object constructType;
                        Type parameterType = parameters[i].ParameterType;
                        if (parameterType == typeof(SerializationContext))
                        {
                            constructType = context;
                        }
                        else if (parameterType == typeof(Type))
                        {
                            constructType = this.constructType;
                        }
                        else if (parameterType == typeof(StreamingContext))
                        {
                            constructType = (StreamingContext) context;
                        }
                        else
                        {
                            constructType = null;
                            flag = false;
                        }
                        objArray[i] = constructType;
                    }
                }
                if (!flag)
                {
                    throw CallbackSet.CreateInvalidCallbackSignature(method);
                }
                obj2 = method.Invoke(obj, objArray);
            }
            return obj2;
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            Type expectedType = this.ExpectedType;
            using (Local local = ctx.GetLocalWithValue(expectedType, valueFrom))
            {
                using (Local local2 = new Local(ctx, ctx.MapType(typeof(int))))
                {
                    if (this.HasCallbacks(TypeModel.CallbackType.BeforeDeserialize))
                    {
                        if (this.ExpectedType.IsValueType)
                        {
                            this.EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.BeforeDeserialize);
                        }
                        else
                        {
                            CodeLabel label3 = ctx.DefineLabel();
                            ctx.LoadValue(local);
                            ctx.BranchIfFalse(label3, false);
                            this.EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.BeforeDeserialize);
                            ctx.MarkLabel(label3);
                        }
                    }
                    CodeLabel label = ctx.DefineLabel();
                    CodeLabel label2 = ctx.DefineLabel();
                    ctx.Branch(label, false);
                    ctx.MarkLabel(label2);
                    foreach (BasicList.Group group in BasicList.GetContiguousGroups(this.fieldNumbers, this.serializers))
                    {
                        CodeLabel label4 = ctx.DefineLabel();
                        int count = group.Items.Count;
                        if (count == 1)
                        {
                            ctx.LoadValue(local2);
                            ctx.LoadValue(group.First);
                            CodeLabel label5 = ctx.DefineLabel();
                            ctx.BranchIfEqual(label5, true);
                            ctx.Branch(label4, false);
                            this.WriteFieldHandler(ctx, expectedType, local, label5, label, (IProtoSerializer) group.Items[0]);
                        }
                        else
                        {
                            ctx.LoadValue(local2);
                            ctx.LoadValue(group.First);
                            ctx.Subtract();
                            CodeLabel[] jumpTable = new CodeLabel[count];
                            int index = 0;
                            while (true)
                            {
                                if (index >= count)
                                {
                                    ctx.Switch(jumpTable);
                                    ctx.Branch(label4, false);
                                    for (int i = 0; i < count; i++)
                                    {
                                        this.WriteFieldHandler(ctx, expectedType, local, jumpTable[i], label, (IProtoSerializer) group.Items[i]);
                                    }
                                    break;
                                }
                                jumpTable[index] = ctx.DefineLabel();
                                index++;
                            }
                        }
                        ctx.MarkLabel(label4);
                    }
                    this.EmitCreateIfNull(ctx, local);
                    ctx.LoadReaderWriter();
                    if (!this.isExtensible)
                    {
                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
                    }
                    else
                    {
                        ctx.LoadValue(local);
                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("AppendExtensionData"));
                    }
                    ctx.MarkLabel(label);
                    ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
                    ctx.CopyValue();
                    ctx.StoreValue(local2);
                    ctx.LoadValue(0);
                    ctx.BranchIfGreater(label2, false);
                    this.EmitCreateIfNull(ctx, local);
                    this.EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.AfterDeserialize);
                }
            }
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            Type expectedType = this.ExpectedType;
            using (Local local = ctx.GetLocalWithValue(expectedType, valueFrom))
            {
                this.EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.BeforeSerialize);
                CodeLabel label = ctx.DefineLabel();
                if (this.CanHaveInheritance)
                {
                    int num = 0;
                    while (true)
                    {
                        if (num >= this.serializers.Length)
                        {
                            if ((this.constructType != null) && (this.constructType != this.forType))
                            {
                                using (Local local2 = new Local(ctx, ctx.MapType(typeof(Type))))
                                {
                                    ctx.LoadValue(local);
                                    ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("GetType"));
                                    ctx.CopyValue();
                                    ctx.StoreValue(local2);
                                    ctx.LoadValue(this.forType);
                                    ctx.BranchIfEqual(label, true);
                                    ctx.LoadValue(local2);
                                    ctx.LoadValue(this.constructType);
                                    ctx.BranchIfEqual(label, true);
                                    break;
                                }
                            }
                            ctx.LoadValue(local);
                            ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("GetType"));
                            ctx.LoadValue(this.forType);
                            ctx.BranchIfEqual(label, true);
                            break;
                        }
                        IProtoSerializer serializer = this.serializers[num];
                        if (serializer.ExpectedType != this.forType)
                        {
                            CodeLabel label2 = ctx.DefineLabel();
                            CodeLabel label3 = ctx.DefineLabel();
                            ctx.LoadValue(local);
                            ctx.TryCast(serializer.ExpectedType);
                            ctx.CopyValue();
                            ctx.BranchIfTrue(label2, true);
                            ctx.DiscardValue();
                            ctx.Branch(label3, true);
                            ctx.MarkLabel(label2);
                            serializer.EmitWrite(ctx, null);
                            ctx.Branch(label, false);
                            ctx.MarkLabel(label3);
                        }
                        num++;
                    }
                    ctx.LoadValue(this.forType);
                    ctx.LoadValue(local);
                    ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("GetType"));
                    ctx.EmitCall(ctx.MapType(typeof(TypeModel)).GetMethod("ThrowUnexpectedSubtype", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                }
                ctx.MarkLabel(label);
                int index = 0;
                while (true)
                {
                    if (index >= this.serializers.Length)
                    {
                        if (this.isExtensible)
                        {
                            ctx.LoadValue(local);
                            ctx.LoadReaderWriter();
                            ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("AppendExtensionData"));
                        }
                        this.EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.AfterSerialize);
                        break;
                    }
                    IProtoSerializer serializer2 = this.serializers[index];
                    if (serializer2.ExpectedType == this.forType)
                    {
                        serializer2.EmitWrite(ctx, local);
                    }
                    index++;
                }
            }
        }

        bool IProtoTypeSerializer.CanCreateInstance() => 
            true;

        object IProtoTypeSerializer.CreateInstance(ProtoReader source) => 
            this.CreateInstance(source, false);

        void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
            bool copyValue = false;
            if (this.CanHaveInheritance)
            {
                for (int i = 0; i < this.serializers.Length; i++)
                {
                    IProtoSerializer serializer = this.serializers[i];
                    if ((serializer.ExpectedType != this.forType) && ((IProtoTypeSerializer) serializer).HasCallbacks(callbackType))
                    {
                        copyValue = true;
                    }
                }
            }
            MethodInfo method = this.callbacks?[callbackType];
            if ((method != null) || copyValue)
            {
                ctx.LoadAddress(valueFrom, this.ExpectedType);
                EmitInvokeCallback(ctx, method, copyValue, null, this.forType);
                if (copyValue)
                {
                    CodeLabel label = ctx.DefineLabel();
                    int index = 0;
                    while (true)
                    {
                        IProtoTypeSerializer serializer3;
                        if (index >= this.serializers.Length)
                        {
                            ctx.MarkLabel(label);
                            ctx.DiscardValue();
                            break;
                        }
                        IProtoSerializer serializer2 = this.serializers[index];
                        if ((serializer2.ExpectedType != this.forType) && (serializer3 = (IProtoTypeSerializer) serializer2).HasCallbacks(callbackType))
                        {
                            CodeLabel label2 = ctx.DefineLabel();
                            CodeLabel label3 = ctx.DefineLabel();
                            ctx.CopyValue();
                            ctx.TryCast(serializer2.ExpectedType);
                            ctx.CopyValue();
                            ctx.BranchIfTrue(label2, true);
                            ctx.DiscardValue();
                            ctx.Branch(label3, false);
                            ctx.MarkLabel(label2);
                            serializer3.EmitCallback(ctx, null, callbackType);
                            ctx.Branch(label, false);
                            ctx.MarkLabel(label3);
                        }
                        index++;
                    }
                }
            }
        }

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            bool flag = true;
            if (this.factory != null)
            {
                EmitInvokeCallback(ctx, this.factory, false, this.constructType, this.forType);
            }
            else if (!this.useConstructor)
            {
                ctx.LoadValue(this.constructType);
                ctx.EmitCall(ctx.MapType(typeof(BclHelpers)).GetMethod("GetUninitializedObject"));
                ctx.Cast(this.forType);
            }
            else if (this.constructType.IsClass && this.hasConstructor)
            {
                ctx.EmitCtor(this.constructType);
            }
            else
            {
                ctx.LoadValue(this.ExpectedType);
                ctx.EmitCall(ctx.MapType(typeof(TypeModel)).GetMethod("ThrowCannotCreateInstance", BindingFlags.Public | BindingFlags.Static));
                ctx.LoadNullRef();
                flag = false;
            }
            if (flag)
            {
                ctx.CopyValue();
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("NoteObject", BindingFlags.Public | BindingFlags.Static));
            }
            if (this.baseCtorCallbacks != null)
            {
                for (int i = 0; i < this.baseCtorCallbacks.Length; i++)
                {
                    EmitInvokeCallback(ctx, this.baseCtorCallbacks[i], true, null, this.forType);
                }
            }
        }

        public object Read(object value, ProtoReader source)
        {
            int num;
            if (this.isRootType && (value != null))
            {
                this.Callback(value, TypeModel.CallbackType.BeforeDeserialize, source.Context);
            }
            int num2 = 0;
            int num3 = 0;
            while ((num = source.ReadFieldHeader()) > 0)
            {
                bool flag = false;
                if (num < num2)
                {
                    num2 = num3 = 0;
                }
                int index = num3;
                while (true)
                {
                    if (index < this.fieldNumbers.Length)
                    {
                        if (this.fieldNumbers[index] != num)
                        {
                            index++;
                            continue;
                        }
                        IProtoSerializer serializer = this.serializers[index];
                        if (value == null)
                        {
                            if (serializer.ExpectedType == this.forType)
                            {
                                value = this.CreateInstance(source, true);
                            }
                        }
                        else if (((serializer.ExpectedType != this.forType) && ((IProtoTypeSerializer) serializer).CanCreateInstance()) && serializer.ExpectedType.IsSubclassOf(value.GetType()))
                        {
                            object obj1 = ProtoReader.Merge(source, value, ((IProtoTypeSerializer) serializer).CreateInstance(source));
                            value = obj1;
                        }
                        if (!serializer.ReturnsValue)
                        {
                            serializer.Read(value, source);
                        }
                        else
                        {
                            object obj2 = serializer.Read(value, source);
                            value = obj2;
                        }
                        num3 = index;
                        num2 = num;
                        flag = true;
                    }
                    if (!flag)
                    {
                        if (value == null)
                        {
                            value = this.CreateInstance(source, true);
                        }
                        if (this.isExtensible)
                        {
                            source.AppendExtensionData((IExtensible) value);
                        }
                        else
                        {
                            source.SkipField();
                        }
                    }
                    break;
                }
            }
            if (value == null)
            {
                value = this.CreateInstance(source, true);
            }
            if (this.isRootType)
            {
                this.Callback(value, TypeModel.CallbackType.AfterDeserialize, source.Context);
            }
            return value;
        }

        public void Write(object value, ProtoWriter dest)
        {
            if (this.isRootType)
            {
                this.Callback(value, TypeModel.CallbackType.BeforeSerialize, dest.Context);
            }
            IProtoSerializer moreSpecificSerializer = this.GetMoreSpecificSerializer(value);
            if (moreSpecificSerializer != null)
            {
                moreSpecificSerializer.Write(value, dest);
            }
            for (int i = 0; i < this.serializers.Length; i++)
            {
                IProtoSerializer serializer2 = this.serializers[i];
                if (serializer2.ExpectedType == this.forType)
                {
                    serializer2.Write(value, dest);
                }
            }
            if (this.isExtensible)
            {
                ProtoWriter.AppendExtensionData((IExtensible) value, dest);
            }
            if (this.isRootType)
            {
                this.Callback(value, TypeModel.CallbackType.AfterSerialize, dest.Context);
            }
        }

        private void WriteFieldHandler(CompilerContext ctx, Type expected, Local loc, CodeLabel handler, CodeLabel @continue, IProtoSerializer serializer)
        {
            ctx.MarkLabel(handler);
            if (serializer.ExpectedType == this.forType)
            {
                this.EmitCreateIfNull(ctx, loc);
                serializer.EmitRead(ctx, loc);
            }
            else
            {
                RuntimeTypeModel model = (RuntimeTypeModel) ctx.Model;
                if (((IProtoTypeSerializer) serializer).CanCreateInstance())
                {
                    CodeLabel label = ctx.DefineLabel();
                    ctx.LoadValue(loc);
                    ctx.BranchIfFalse(label, false);
                    ctx.LoadValue(loc);
                    ctx.TryCast(serializer.ExpectedType);
                    ctx.BranchIfTrue(label, false);
                    ctx.LoadReaderWriter();
                    ctx.LoadValue(loc);
                    ((IProtoTypeSerializer) serializer).EmitCreateInstance(ctx);
                    ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("Merge"));
                    ctx.Cast(expected);
                    ctx.StoreValue(loc);
                    ctx.MarkLabel(label);
                }
                ctx.LoadValue(loc);
                ctx.Cast(serializer.ExpectedType);
                serializer.EmitRead(ctx, null);
            }
            if (serializer.ReturnsValue)
            {
                ctx.StoreValue(loc);
            }
            ctx.Branch(@continue, false);
        }

        public Type ExpectedType =>
            this.forType;

        private bool CanHaveInheritance =>
            ((this.forType.IsClass || this.forType.IsInterface) && !this.forType.IsSealed);

        bool IProtoSerializer.RequiresOldValue =>
            true;

        bool IProtoSerializer.ReturnsValue =>
            false;
    }
}

