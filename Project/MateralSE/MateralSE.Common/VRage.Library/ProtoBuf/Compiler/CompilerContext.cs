namespace ProtoBuf.Compiler
{
    using ProtoBuf;
    using ProtoBuf.Meta;
    using ProtoBuf.Serializers;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class CompilerContext
    {
        private readonly DynamicMethod method;
        private static int next;
        private readonly bool isStatic;
        private readonly RuntimeTypeModel.SerializerPair[] methodPairs;
        private readonly bool nonPublic;
        private readonly bool isWriter;
        private readonly string assemblyName;
        private readonly ILGenerator il;
        private MutableList locals;
        private int nextLabel;
        private BasicList knownTrustedAssemblies;
        private BasicList knownUntrustedAssemblies;
        private readonly TypeModel model;
        private readonly ILVersion metadataVersion;

        private CompilerContext(Type associatedType, bool isWriter, bool isStatic, TypeModel model)
        {
            Type[] typeArray;
            Type type;
            this.locals = new MutableList();
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            this.metadataVersion = ILVersion.Net2;
            this.isStatic = isStatic;
            this.isWriter = isWriter;
            this.model = model;
            this.nonPublic = true;
            if (isWriter)
            {
                type = typeof(void);
                typeArray = new Type[] { typeof(object), typeof(ProtoWriter) };
            }
            else
            {
                type = typeof(object);
                typeArray = new Type[] { typeof(object), typeof(ProtoReader) };
            }
            int num = Interlocked.Increment(ref next);
            this.method = new DynamicMethod("proto_" + num.ToString(), type, typeArray, associatedType.IsInterface ? typeof(object) : associatedType, true);
            this.il = this.method.GetILGenerator();
        }

        internal CompilerContext(ILGenerator il, bool isStatic, bool isWriter, RuntimeTypeModel.SerializerPair[] methodPairs, TypeModel model, ILVersion metadataVersion, string assemblyName)
        {
            this.locals = new MutableList();
            if (il == null)
            {
                throw new ArgumentNullException("il");
            }
            if (methodPairs == null)
            {
                throw new ArgumentNullException("methodPairs");
            }
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (Helpers.IsNullOrEmpty(assemblyName))
            {
                throw new ArgumentNullException("assemblyName");
            }
            this.assemblyName = assemblyName;
            this.isStatic = isStatic;
            this.methodPairs = methodPairs;
            this.il = il;
            this.nonPublic = false;
            this.isWriter = isWriter;
            this.model = model;
            this.metadataVersion = metadataVersion;
        }

        internal void Add()
        {
            this.Emit(OpCodes.Add);
        }

        internal bool AllowInternal(PropertyInfo property) => 
            (this.nonPublic || this.InternalsVisible(property.DeclaringType.Assembly));

        internal void BeginFinally()
        {
            this.il.BeginFinallyBlock();
        }

        internal CodeLabel BeginTry()
        {
            int nextLabel = this.nextLabel;
            this.nextLabel = nextLabel + 1;
            return new CodeLabel(this.il.BeginExceptionBlock(), nextLabel);
        }

        internal void Branch(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Br_S : OpCodes.Br;
            this.il.Emit(opcode, label.Value);
        }

        internal void BranchIfEqual(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Beq_S : OpCodes.Beq;
            this.il.Emit(opcode, label.Value);
        }

        internal void BranchIfFalse(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Brfalse_S : OpCodes.Brfalse;
            this.il.Emit(opcode, label.Value);
        }

        internal void BranchIfGreater(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Bgt_S : OpCodes.Bgt;
            this.il.Emit(opcode, label.Value);
        }

        internal void BranchIfLess(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Blt_S : OpCodes.Blt;
            this.il.Emit(opcode, label.Value);
        }

        internal void BranchIfTrue(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Brtrue_S : OpCodes.Brtrue;
            this.il.Emit(opcode, label.Value);
        }

        public static ProtoDeserializer BuildDeserializer(IProtoSerializer head, TypeModel model)
        {
            Type expectedType = head.ExpectedType;
            CompilerContext ctx = new CompilerContext(expectedType, false, true, model);
            using (Local local = new Local(ctx, expectedType))
            {
                if (!expectedType.IsValueType)
                {
                    ctx.LoadValue(Local.InputValue);
                    ctx.CastFromObject(expectedType);
                    ctx.StoreValue(local);
                }
                else
                {
                    ctx.LoadValue(Local.InputValue);
                    CodeLabel label = ctx.DefineLabel();
                    CodeLabel label2 = ctx.DefineLabel();
                    ctx.BranchIfTrue(label, true);
                    ctx.LoadAddress(local, expectedType);
                    ctx.EmitCtor(expectedType);
                    ctx.Branch(label2, true);
                    ctx.MarkLabel(label);
                    ctx.LoadValue(Local.InputValue);
                    ctx.CastFromObject(expectedType);
                    ctx.StoreValue(local);
                    ctx.MarkLabel(label2);
                }
                head.EmitRead(ctx, local);
                if (head.ReturnsValue)
                {
                    ctx.StoreValue(local);
                }
                ctx.LoadValue(local);
                ctx.CastToObject(expectedType);
            }
            ctx.Emit(OpCodes.Ret);
            return (ProtoDeserializer) ctx.method.CreateDelegate(typeof(ProtoDeserializer));
        }

        public static ProtoSerializer BuildSerializer(IProtoSerializer head, TypeModel model)
        {
            Type expectedType = head.ExpectedType;
            CompilerContext context = new CompilerContext(expectedType, true, true, model);
            context.LoadValue(Local.InputValue);
            context.CastFromObject(expectedType);
            context.WriteNullCheckedTail(expectedType, head, null);
            context.Emit(OpCodes.Ret);
            return (ProtoSerializer) context.method.CreateDelegate(typeof(ProtoSerializer));
        }

        internal void Cast(Type type)
        {
            this.il.Emit(OpCodes.Castclass, type);
        }

        internal void CastFromObject(Type type)
        {
            if (!IsObject(type))
            {
                if (type.IsValueType)
                {
                    if (this.MetadataVersion != ILVersion.Net1)
                    {
                        this.il.Emit(OpCodes.Unbox_Any, type);
                    }
                    else
                    {
                        this.il.Emit(OpCodes.Unbox, type);
                        this.il.Emit(OpCodes.Ldobj, type);
                    }
                }
                else
                {
                    this.il.Emit(OpCodes.Castclass, type);
                }
            }
        }

        internal void CastToObject(Type type)
        {
            if (!IsObject(type))
            {
                if (type.IsValueType)
                {
                    this.il.Emit(OpCodes.Box, type);
                }
                else
                {
                    this.il.Emit(OpCodes.Castclass, this.MapType(typeof(object)));
                }
            }
        }

        internal void CheckAccessibility(MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            if (!this.NonPublic)
            {
                bool flag;
                MemberTypes memberType = member.MemberType;
                if (memberType > MemberTypes.Method)
                {
                    if (memberType == MemberTypes.Property)
                    {
                        flag = true;
                    }
                    else if (memberType == MemberTypes.TypeInfo)
                    {
                        flag = ((Type) member).IsPublic || this.InternalsVisible(((Type) member).Assembly);
                    }
                    else if (memberType == MemberTypes.NestedType)
                    {
                        Type type = (Type) member;
                        do
                        {
                            int num7;
                            int num8;
                            if (type.IsNestedPublic || type.IsPublic)
                            {
                                num7 = 1;
                            }
                            else if (((type.DeclaringType == null) || type.IsNestedAssembly) || type.IsNestedFamORAssem)
                            {
                                num7 = (int) this.InternalsVisible(type.Assembly);
                            }
                            else
                            {
                                num7 = 0;
                            }
                            flag = (bool) num8;
                        }
                        while (flag && ((type = type.DeclaringType) != null));
                    }
                    else
                    {
                        goto TR_0001;
                    }
                }
                else if (memberType == MemberTypes.Constructor)
                {
                    int num5;
                    int num6;
                    ConstructorInfo info2 = (ConstructorInfo) member;
                    if (info2.IsPublic)
                    {
                        num5 = 1;
                    }
                    else if (info2.IsAssembly || info2.IsFamilyOrAssembly)
                    {
                        num5 = (int) this.InternalsVisible(info2.DeclaringType.Assembly);
                    }
                    else
                    {
                        num5 = 0;
                    }
                    flag = (bool) num6;
                }
                else if (memberType == MemberTypes.Field)
                {
                    int num3;
                    int num4;
                    FieldInfo info = (FieldInfo) member;
                    if (info.IsPublic)
                    {
                        num3 = 1;
                    }
                    else if (info.IsAssembly || info.IsFamilyOrAssembly)
                    {
                        num3 = (int) this.InternalsVisible(info.DeclaringType.Assembly);
                    }
                    else
                    {
                        num3 = 0;
                    }
                    flag = (bool) num4;
                }
                else if (memberType == MemberTypes.Method)
                {
                    int num1;
                    int num2;
                    MethodInfo info3 = (MethodInfo) member;
                    if (info3.IsPublic)
                    {
                        num1 = 1;
                    }
                    else if (info3.IsAssembly || info3.IsFamilyOrAssembly)
                    {
                        num1 = (int) this.InternalsVisible(info3.DeclaringType.Assembly);
                    }
                    else
                    {
                        num1 = 0;
                    }
                    flag = (bool) num2;
                    if (!flag && ((member is MethodBuilder) || (member.DeclaringType == this.MapType(typeof(TypeModel)))))
                    {
                        flag = true;
                    }
                }
                else
                {
                    goto TR_0001;
                }
                if (!flag)
                {
                    memberType = member.MemberType;
                    if ((memberType == MemberTypes.TypeInfo) || (memberType == MemberTypes.NestedType))
                    {
                        throw new InvalidOperationException("Non-public type cannot be used with full dll compilation: " + ((Type) member).FullName);
                    }
                    throw new InvalidOperationException("Non-public member cannot be used with full dll compilation: " + member.DeclaringType.FullName + "." + member.Name);
                }
            }
            return;
        TR_0001:
            throw new NotSupportedException(member.MemberType.ToString());
        }

        internal void Constrain(Type type)
        {
            this.il.Emit(OpCodes.Constrained, type);
        }

        internal void ConvertFromInt32(ProtoTypeCode typeCode, bool uint32Overflow)
        {
            switch (typeCode)
            {
                case ProtoTypeCode.SByte:
                    this.Emit(OpCodes.Conv_Ovf_I1);
                    return;

                case ProtoTypeCode.Byte:
                    this.Emit(OpCodes.Conv_Ovf_U1);
                    return;

                case ProtoTypeCode.Int16:
                    this.Emit(OpCodes.Conv_Ovf_I2);
                    return;

                case ProtoTypeCode.UInt16:
                    this.Emit(OpCodes.Conv_Ovf_U2);
                    return;

                case ProtoTypeCode.Int32:
                    return;

                case ProtoTypeCode.UInt32:
                    this.Emit(uint32Overflow ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_U4);
                    return;

                case ProtoTypeCode.Int64:
                    this.Emit(OpCodes.Conv_I8);
                    return;

                case ProtoTypeCode.UInt64:
                    this.Emit(OpCodes.Conv_U8);
                    return;
            }
            throw new InvalidOperationException();
        }

        internal void ConvertToInt32(ProtoTypeCode typeCode, bool uint32Overflow)
        {
            switch (typeCode)
            {
                case ProtoTypeCode.SByte:
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.UInt16:
                    this.Emit(OpCodes.Conv_I4);
                    return;

                case ProtoTypeCode.Int32:
                    return;

                case ProtoTypeCode.UInt32:
                    this.Emit(uint32Overflow ? OpCodes.Conv_Ovf_I4_Un : OpCodes.Conv_Ovf_I4);
                    return;

                case ProtoTypeCode.Int64:
                    this.Emit(OpCodes.Conv_Ovf_I4);
                    return;

                case ProtoTypeCode.UInt64:
                    this.Emit(OpCodes.Conv_Ovf_I4_Un);
                    return;
            }
            throw new InvalidOperationException("ConvertToInt32 not implemented for: " + typeCode);
        }

        internal void CopyValue()
        {
            this.Emit(OpCodes.Dup);
        }

        internal void CreateArray(Type elementType, Local length)
        {
            this.LoadValue(length);
            this.il.Emit(OpCodes.Newarr, elementType);
        }

        internal CodeLabel DefineLabel()
        {
            int nextLabel = this.nextLabel;
            this.nextLabel = nextLabel + 1;
            return new CodeLabel(this.il.DefineLabel(), nextLabel);
        }

        internal void DiscardValue()
        {
            this.Emit(OpCodes.Pop);
        }

        private void Emit(OpCode opcode)
        {
            this.il.Emit(opcode);
        }

        internal void EmitBasicRead(string methodName, Type expectedType)
        {
            MethodInfo method = this.MapType(typeof(ProtoReader)).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (((method == null) || (method.ReturnType != expectedType)) || (method.GetParameters().Length != 0))
            {
                throw new ArgumentException("methodName");
            }
            this.LoadReaderWriter();
            this.EmitCall(method);
        }

        internal void EmitBasicRead(Type helperType, string methodName, Type expectedType)
        {
            MethodInfo method = helperType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (((method == null) || (method.ReturnType != expectedType)) || (method.GetParameters().Length != 1))
            {
                throw new ArgumentException("methodName");
            }
            this.LoadReaderWriter();
            this.EmitCall(method);
        }

        internal void EmitBasicWrite(string methodName, Local fromValue)
        {
            if (Helpers.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName");
            }
            this.LoadValue(fromValue);
            this.LoadReaderWriter();
            this.EmitCall(this.GetWriterMethod(methodName));
        }

        public void EmitCall(MethodInfo method)
        {
            OpCode call;
            this.CheckAccessibility(method);
            if (method.IsStatic || method.DeclaringType.IsValueType)
            {
                call = OpCodes.Call;
            }
            else
            {
                call = OpCodes.Callvirt;
            }
            OpCode opcode = call;
            this.il.EmitCall(opcode, method, null);
        }

        public void EmitCtor(ConstructorInfo ctor)
        {
            if (ctor == null)
            {
                throw new ArgumentNullException("ctor");
            }
            this.CheckAccessibility(ctor);
            this.il.Emit(OpCodes.Newobj, ctor);
        }

        public void EmitCtor(Type type)
        {
            this.EmitCtor(type, Helpers.EmptyTypes);
        }

        public void EmitCtor(Type type, params Type[] parameterTypes)
        {
            if (type.IsValueType && (parameterTypes.Length == 0))
            {
                this.il.Emit(OpCodes.Initobj, type);
            }
            else
            {
                ConstructorInfo ctor = Helpers.GetConstructor(type, parameterTypes, true);
                if (ctor == null)
                {
                    throw new InvalidOperationException("No suitable constructor found for " + type.FullName);
                }
                this.EmitCtor(ctor);
            }
        }

        internal void EmitInstance()
        {
            if (this.isStatic)
            {
                throw new InvalidOperationException();
            }
            this.Emit(OpCodes.Ldarg_0);
        }

        internal void EmitWrite(Type helperType, string methodName, Local valueFrom)
        {
            if (Helpers.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName");
            }
            MethodInfo method = helperType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if ((method == null) || (method.ReturnType != this.MapType(typeof(void))))
            {
                throw new ArgumentException("methodName");
            }
            this.LoadValue(valueFrom);
            this.LoadReaderWriter();
            this.EmitCall(method);
        }

        internal void EndFinally()
        {
            this.il.EndExceptionBlock();
        }

        internal void EndTry(CodeLabel label, bool @short)
        {
            OpCode opcode = @short ? OpCodes.Leave_S : OpCodes.Leave;
            this.il.Emit(opcode, label.Value);
        }

        internal MethodBuilder GetDedicatedMethod(int metaKey, bool read)
        {
            if (this.methodPairs == null)
            {
                return null;
            }
            for (int i = 0; i < this.methodPairs.Length; i++)
            {
                if (this.methodPairs[i].MetaKey == metaKey)
                {
                    return (read ? this.methodPairs[i].Deserialize : this.methodPairs[i].Serialize);
                }
            }
            throw new ArgumentException("Meta-key not found", "metaKey");
        }

        internal LocalBuilder GetFromPool(Type type)
        {
            int count = this.locals.Count;
            for (int i = 0; i < count; i++)
            {
                LocalBuilder builder2 = (LocalBuilder) this.locals[i];
                if ((builder2 != null) && (builder2.LocalType == type))
                {
                    this.locals[i] = null;
                    return builder2;
                }
            }
            return this.il.DeclareLocal(type);
        }

        public Local GetLocalWithValue(Type type, Local fromValue)
        {
            if (fromValue != null)
            {
                return fromValue.AsCopy();
            }
            Local local = new Local(this, type);
            this.StoreValue(local);
            return local;
        }

        private MethodInfo GetWriterMethod(string methodName)
        {
            Type type = this.MapType(typeof(ProtoWriter));
            foreach (MethodInfo info in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                if (info.Name == methodName)
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if ((parameters.Length == 2) && (parameters[1].ParameterType == type))
                    {
                        return info;
                    }
                }
            }
            throw new ArgumentException("No suitable method found for: " + methodName, "methodName");
        }

        private bool InternalsVisible(Assembly assembly)
        {
            if (Helpers.IsNullOrEmpty(this.assemblyName))
            {
                return false;
            }
            if ((this.knownTrustedAssemblies != null) && (this.knownTrustedAssemblies.IndexOfReference(assembly) >= 0))
            {
                return true;
            }
            if ((this.knownUntrustedAssemblies != null) && (this.knownUntrustedAssemblies.IndexOfReference(assembly) >= 0))
            {
                return false;
            }
            bool flag = false;
            Type attributeType = this.MapType(typeof(InternalsVisibleToAttribute));
            if (attributeType == null)
            {
                return false;
            }
            object[] customAttributes = assembly.GetCustomAttributes(attributeType, false);
            int index = 0;
            while (true)
            {
                if (index < customAttributes.Length)
                {
                    InternalsVisibleToAttribute attribute = (InternalsVisibleToAttribute) customAttributes[index];
                    if (attribute.AssemblyName != this.assemblyName)
                    {
                        index++;
                        continue;
                    }
                    flag = true;
                }
                if (flag)
                {
                    if (this.knownTrustedAssemblies == null)
                    {
                        this.knownTrustedAssemblies = new BasicList();
                    }
                    this.knownTrustedAssemblies.Add(assembly);
                }
                else
                {
                    if (this.knownUntrustedAssemblies == null)
                    {
                        this.knownUntrustedAssemblies = new BasicList();
                    }
                    this.knownUntrustedAssemblies.Add(assembly);
                }
                return flag;
            }
        }

        private static bool IsObject(Type type) => 
            (type == typeof(object));

        internal void LoadAddress(Local local, Type type)
        {
            if (!type.IsValueType)
            {
                this.LoadValue(local);
            }
            else
            {
                if (local == null)
                {
                    throw new InvalidOperationException("Cannot load the address of a struct at the head of the stack");
                }
                if (ReferenceEquals(local, Local.InputValue))
                {
                    this.il.Emit(OpCodes.Ldarga_S, this.isStatic ? ((byte) 0) : ((byte) 1));
                }
                else
                {
                    OpCode opcode = this.UseShortForm(local) ? OpCodes.Ldloca_S : OpCodes.Ldloca;
                    this.il.Emit(opcode, local.Value);
                }
            }
        }

        internal void LoadArrayValue(Local arr, Local i)
        {
            Type elementType = arr.Type.GetElementType();
            this.LoadValue(arr);
            this.LoadValue(i);
            switch (Helpers.GetTypeCode(elementType))
            {
                case ProtoTypeCode.SByte:
                    this.Emit(OpCodes.Ldelem_I1);
                    return;

                case ProtoTypeCode.Byte:
                    this.Emit(OpCodes.Ldelem_U1);
                    return;

                case ProtoTypeCode.Int16:
                    this.Emit(OpCodes.Ldelem_I2);
                    return;

                case ProtoTypeCode.UInt16:
                    this.Emit(OpCodes.Ldelem_U2);
                    return;

                case ProtoTypeCode.Int32:
                    this.Emit(OpCodes.Ldelem_I4);
                    return;

                case ProtoTypeCode.UInt32:
                    this.Emit(OpCodes.Ldelem_U4);
                    return;

                case ProtoTypeCode.Int64:
                    this.Emit(OpCodes.Ldelem_I8);
                    return;

                case ProtoTypeCode.UInt64:
                    this.Emit(OpCodes.Ldelem_I8);
                    return;

                case ProtoTypeCode.Single:
                    this.Emit(OpCodes.Ldelem_R4);
                    return;

                case ProtoTypeCode.Double:
                    this.Emit(OpCodes.Ldelem_R8);
                    return;
            }
            if (!elementType.IsValueType)
            {
                this.Emit(OpCodes.Ldelem_Ref);
            }
            else
            {
                this.il.Emit(OpCodes.Ldelema, elementType);
                this.il.Emit(OpCodes.Ldobj, elementType);
            }
        }

        internal void LoadLength(Local arr, bool zeroIfNull)
        {
            if (!zeroIfNull)
            {
                this.LoadValue(arr);
                this.Emit(OpCodes.Ldlen);
                this.Emit(OpCodes.Conv_I4);
            }
            else
            {
                CodeLabel label = this.DefineLabel();
                CodeLabel label2 = this.DefineLabel();
                this.LoadValue(arr);
                this.CopyValue();
                this.BranchIfTrue(label, true);
                this.DiscardValue();
                this.LoadValue(0);
                this.Branch(label2, true);
                this.MarkLabel(label);
                this.Emit(OpCodes.Ldlen);
                this.Emit(OpCodes.Conv_I4);
                this.MarkLabel(label2);
            }
        }

        public void LoadNullRef()
        {
            this.Emit(OpCodes.Ldnull);
        }

        public void LoadReaderWriter()
        {
            this.Emit(this.isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2);
        }

        internal void LoadSerializationContext()
        {
            this.LoadReaderWriter();
            this.LoadValue((this.isWriter ? typeof(ProtoWriter) : typeof(ProtoReader)).GetProperty("Context"));
        }

        public void LoadValue(Local local)
        {
            if (local != null)
            {
                if (ReferenceEquals(local, Local.InputValue))
                {
                    this.Emit(this.isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                }
                else
                {
                    switch (local.Value.LocalIndex)
                    {
                        case 0:
                            this.Emit(OpCodes.Ldloc_0);
                            return;

                        case 1:
                            this.Emit(OpCodes.Ldloc_1);
                            return;

                        case 2:
                            this.Emit(OpCodes.Ldloc_2);
                            return;

                        case 3:
                            this.Emit(OpCodes.Ldloc_3);
                            return;

                        default:
                        {
                            OpCode opcode = this.UseShortForm(local) ? OpCodes.Ldloc_S : OpCodes.Ldloc;
                            this.il.Emit(opcode, local.Value);
                            break;
                        }
                    }
                }
            }
        }

        internal void LoadValue(bool value)
        {
            this.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }

        internal void LoadValue(decimal value)
        {
            if (value == 0M)
            {
                this.LoadValue(typeof(decimal).GetField("Zero"));
            }
            else
            {
                int[] bits = decimal.GetBits(value);
                this.LoadValue(bits[0]);
                this.LoadValue(bits[1]);
                this.LoadValue(bits[2]);
                this.LoadValue((int) (bits[3] >> 0x1f));
                this.LoadValue((int) ((bits[3] >> 0x10) & 0xff));
                Type[] parameterTypes = new Type[] { this.MapType(typeof(int)), this.MapType(typeof(int)), this.MapType(typeof(int)), this.MapType(typeof(bool)), this.MapType(typeof(byte)) };
                this.EmitCtor(this.MapType(typeof(decimal)), parameterTypes);
            }
        }

        public void LoadValue(double value)
        {
            this.il.Emit(OpCodes.Ldc_R8, value);
        }

        internal void LoadValue(Guid value)
        {
            if (value == Guid.Empty)
            {
                this.LoadValue(typeof(Guid).GetField("Empty"));
            }
            else
            {
                byte[] buffer = value.ToByteArray();
                int num = ((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18);
                this.LoadValue(num);
                short num2 = (short) (buffer[4] | (buffer[5] << 8));
                this.LoadValue((int) num2);
                num2 = (short) (buffer[6] | (buffer[7] << 8));
                this.LoadValue((int) num2);
                for (num = 8; num <= 15; num++)
                {
                    this.LoadValue((int) buffer[num]);
                }
                Type[] parameterTypes = new Type[11];
                parameterTypes[0] = this.MapType(typeof(int));
                parameterTypes[1] = this.MapType(typeof(short));
                parameterTypes[2] = this.MapType(typeof(short));
                parameterTypes[3] = this.MapType(typeof(byte));
                parameterTypes[4] = this.MapType(typeof(byte));
                parameterTypes[5] = this.MapType(typeof(byte));
                parameterTypes[6] = this.MapType(typeof(byte));
                parameterTypes[7] = this.MapType(typeof(byte));
                parameterTypes[8] = this.MapType(typeof(byte));
                parameterTypes[9] = this.MapType(typeof(byte));
                parameterTypes[10] = this.MapType(typeof(byte));
                this.EmitCtor(this.MapType(typeof(Guid)), parameterTypes);
            }
        }

        public void LoadValue(int value)
        {
            switch (value)
            {
                case -1:
                    this.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    this.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    this.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    this.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    this.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    this.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    this.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    this.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    this.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    this.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            if ((value < -128) || (value > 0x7f))
            {
                this.il.Emit(OpCodes.Ldc_I4, value);
            }
            else
            {
                this.il.Emit(OpCodes.Ldc_I4_S, (sbyte) value);
            }
        }

        public void LoadValue(long value)
        {
            this.il.Emit(OpCodes.Ldc_I8, value);
        }

        public void LoadValue(FieldInfo field)
        {
            this.CheckAccessibility(field);
            OpCode opcode = field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld;
            this.il.Emit(opcode, field);
        }

        public void LoadValue(PropertyInfo property)
        {
            this.CheckAccessibility(property);
            this.EmitCall(Helpers.GetGetMethod(property, true, true));
        }

        public void LoadValue(float value)
        {
            this.il.Emit(OpCodes.Ldc_R4, value);
        }

        public void LoadValue(string value)
        {
            if (value == null)
            {
                this.LoadNullRef();
            }
            else
            {
                this.il.Emit(OpCodes.Ldstr, value);
            }
        }

        internal void LoadValue(Type type)
        {
            this.il.Emit(OpCodes.Ldtoken, type);
            this.EmitCall(this.MapType(typeof(Type)).GetMethod("GetTypeFromHandle"));
        }

        internal static void LoadValue(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            il.Emit(OpCodes.Ldc_I4, value);
        }

        internal int MapMetaKeyToCompiledKey(int metaKey)
        {
            if ((metaKey < 0) || (this.methodPairs == null))
            {
                return metaKey;
            }
            for (int i = 0; i < this.methodPairs.Length; i++)
            {
                if (this.methodPairs[i].MetaKey == metaKey)
                {
                    return i;
                }
            }
            throw new ArgumentException("Key could not be mapped: " + metaKey, "metaKey");
        }

        internal Type MapType(Type type) => 
            this.model.MapType(type);

        internal void MarkLabel(CodeLabel label)
        {
            this.il.MarkLabel(label.Value);
        }

        internal void ReadNullCheckedTail(Type type, IProtoSerializer tail, Local valueFrom)
        {
            Type type2;
            if (!type.IsValueType || ((type2 = Helpers.GetUnderlyingType(type)) == null))
            {
                tail.EmitRead(this, valueFrom);
            }
            else
            {
                if (tail.RequiresOldValue)
                {
                    using (Local local = this.GetLocalWithValue(type, valueFrom))
                    {
                        this.LoadAddress(local, type);
                        this.EmitCall(type.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                    }
                }
                tail.EmitRead(this, null);
                if (tail.ReturnsValue)
                {
                    Type[] parameterTypes = new Type[] { type2 };
                    this.EmitCtor(type, parameterTypes);
                }
            }
        }

        internal void ReleaseToPool(LocalBuilder value)
        {
            int count = this.locals.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.locals[i] == null)
                {
                    this.locals[i] = value;
                    return;
                }
            }
            this.locals.Add(value);
        }

        internal void Return()
        {
            this.Emit(OpCodes.Ret);
        }

        public void StoreValue(Local local)
        {
            if (ReferenceEquals(local, Local.InputValue))
            {
                byte arg = this.isStatic ? ((byte) 0) : ((byte) 1);
                this.il.Emit(OpCodes.Starg_S, arg);
            }
            else
            {
                switch (local.Value.LocalIndex)
                {
                    case 0:
                        this.Emit(OpCodes.Stloc_0);
                        return;

                    case 1:
                        this.Emit(OpCodes.Stloc_1);
                        return;

                    case 2:
                        this.Emit(OpCodes.Stloc_2);
                        return;

                    case 3:
                        this.Emit(OpCodes.Stloc_3);
                        return;
                }
                OpCode opcode = this.UseShortForm(local) ? OpCodes.Stloc_S : OpCodes.Stloc;
                this.il.Emit(opcode, local.Value);
            }
        }

        public void StoreValue(FieldInfo field)
        {
            this.CheckAccessibility(field);
            OpCode opcode = field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld;
            this.il.Emit(opcode, field);
        }

        public void StoreValue(PropertyInfo property)
        {
            this.CheckAccessibility(property);
            this.EmitCall(Helpers.GetSetMethod(property, true, true));
        }

        public void Subtract()
        {
            this.Emit(OpCodes.Sub);
        }

        public void Switch(CodeLabel[] jumpTable)
        {
            Label[] labels = new Label[jumpTable.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = jumpTable[i].Value;
            }
            this.il.Emit(OpCodes.Switch, labels);
        }

        internal void TestEqual()
        {
            this.Emit(OpCodes.Ceq);
        }

        internal void TryCast(Type type)
        {
            this.il.Emit(OpCodes.Isinst, type);
        }

        private bool UseShortForm(Local local) => 
            (local.Value.LocalIndex < 0x100);

        public IDisposable Using(Local local) => 
            new UsingBlock(this, local);

        internal void WriteNullCheckedTail(Type type, IProtoSerializer tail, Local valueFrom)
        {
            if (type.IsValueType)
            {
                if (Helpers.GetUnderlyingType(type) == null)
                {
                    tail.EmitWrite(this, valueFrom);
                    return;
                }
                using (Local local = this.GetLocalWithValue(type, valueFrom))
                {
                    this.LoadAddress(local, type);
                    this.LoadValue(type.GetProperty("HasValue"));
                    CodeLabel label = this.DefineLabel();
                    this.BranchIfFalse(label, false);
                    this.LoadAddress(local, type);
                    this.EmitCall(type.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                    tail.EmitWrite(this, null);
                    this.MarkLabel(label);
                    return;
                }
            }
            this.LoadValue(valueFrom);
            this.CopyValue();
            CodeLabel label2 = this.DefineLabel();
            CodeLabel label3 = this.DefineLabel();
            this.BranchIfTrue(label2, true);
            this.DiscardValue();
            this.Branch(label3, false);
            this.MarkLabel(label2);
            tail.EmitWrite(this, null);
            this.MarkLabel(label3);
        }

        public TypeModel Model =>
            this.model;

        internal bool NonPublic =>
            this.nonPublic;

        public ILVersion MetadataVersion =>
            this.metadataVersion;

        public enum ILVersion
        {
            Net1,
            Net2
        }

        private class UsingBlock : IDisposable
        {
            private Local local;
            private CompilerContext ctx;
            private CodeLabel label;

            public UsingBlock(CompilerContext ctx, Local local)
            {
                if (ctx == null)
                {
                    throw new ArgumentNullException("ctx");
                }
                if (local == null)
                {
                    throw new ArgumentNullException("local");
                }
                Type c = local.Type;
                if ((!c.IsValueType && !c.IsSealed) || ctx.MapType(typeof(IDisposable)).IsAssignableFrom(c))
                {
                    this.local = local;
                    this.ctx = ctx;
                    this.label = ctx.BeginTry();
                }
            }

            public void Dispose()
            {
                if ((this.local != null) && (this.ctx != null))
                {
                    this.ctx.EndTry(this.label, false);
                    this.ctx.BeginFinally();
                    Type type = this.ctx.MapType(typeof(IDisposable));
                    MethodInfo method = type.GetMethod("Dispose");
                    Type type2 = this.local.Type;
                    if (type2.IsValueType)
                    {
                        this.ctx.LoadAddress(this.local, type2);
                        if (this.ctx.MetadataVersion != CompilerContext.ILVersion.Net1)
                        {
                            this.ctx.Constrain(type2);
                        }
                        else
                        {
                            this.ctx.LoadValue(this.local);
                            this.ctx.CastToObject(type2);
                        }
                        this.ctx.EmitCall(method);
                    }
                    else
                    {
                        CodeLabel label = this.ctx.DefineLabel();
                        if (type.IsAssignableFrom(type2))
                        {
                            this.ctx.LoadValue(this.local);
                            this.ctx.BranchIfFalse(label, true);
                            this.ctx.LoadAddress(this.local, type2);
                        }
                        else
                        {
                            using (Local local = new Local(this.ctx, type))
                            {
                                this.ctx.LoadValue(this.local);
                                this.ctx.TryCast(type);
                                this.ctx.CopyValue();
                                this.ctx.StoreValue(local);
                                this.ctx.BranchIfFalse(label, true);
                                this.ctx.LoadAddress(local, type);
                            }
                        }
                        this.ctx.EmitCall(method);
                        this.ctx.MarkLabel(label);
                    }
                    this.ctx.EndFinally();
                    this.local = null;
                    this.ctx = null;
                    this.label = new CodeLabel();
                }
            }
        }
    }
}

