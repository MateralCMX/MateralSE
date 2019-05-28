namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Serializers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public sealed class RuntimeTypeModel : TypeModel
    {
        private byte options;
        private const byte OPTIONS_InferTagFromNameDefault = 1;
        private const byte OPTIONS_IsDefaultModel = 2;
        private const byte OPTIONS_Frozen = 4;
        private const byte OPTIONS_AutoAddMissingTypes = 8;
        private const byte OPTIONS_AutoCompile = 0x10;
        private const byte OPTIONS_UseImplicitZeroDefaults = 0x20;
        private const byte OPTIONS_AllowParseableTypes = 0x40;
        private const byte OPTIONS_AutoAddProtoContractTypesOnly = 0x80;
        private readonly BasicList types = new BasicList();
        private int metadataTimeoutMilliseconds = 0x1388;
        private int contentionCounter = 1;
        [CompilerGenerated]
        private LockContentedEventHandler LockContended;
        private MethodInfo defaultFactory;

        public event LockContentedEventHandler LockContended
        {
            [CompilerGenerated] add
            {
                LockContentedEventHandler lockContended = this.LockContended;
                while (true)
                {
                    LockContentedEventHandler a = lockContended;
                    LockContentedEventHandler handler3 = (LockContentedEventHandler) Delegate.Combine(a, value);
                    lockContended = Interlocked.CompareExchange<LockContentedEventHandler>(ref this.LockContended, handler3, a);
                    if (ReferenceEquals(lockContended, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                LockContentedEventHandler lockContended = this.LockContended;
                while (true)
                {
                    LockContentedEventHandler source = lockContended;
                    LockContentedEventHandler handler3 = (LockContentedEventHandler) Delegate.Remove(source, value);
                    lockContended = Interlocked.CompareExchange<LockContentedEventHandler>(ref this.LockContended, handler3, source);
                    if (ReferenceEquals(lockContended, source))
                    {
                        return;
                    }
                }
            }
        }

        internal RuntimeTypeModel(bool isDefault)
        {
            this.AutoAddMissingTypes = true;
            this.UseImplicitZeroDefaults = true;
            this.SetOption(2, isDefault);
            this.AutoCompile = true;
        }

        public MetaType Add(Type type, bool applyDefaultBehaviour)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            MetaType type2 = this.FindWithoutAdd(type);
            if (type2 == null)
            {
                int opaqueToken = 0;
                if ((type.IsInterface && base.MapType(MetaType.ienumerable).IsAssignableFrom(type)) && (GetListItemType(this, type) == null))
                {
                    throw new ArgumentException("IEnumerable[<T>] data cannot be used as a meta-type unless an Add method can be resolved");
                }
                try
                {
                    type2 = this.RecogniseCommonTypes(type);
                    if (type2 != null)
                    {
                        if (!applyDefaultBehaviour)
                        {
                            throw new ArgumentException("Default behaviour must be observed for certain types with special handling; " + type.FullName, "applyDefaultBehaviour");
                        }
                        applyDefaultBehaviour = false;
                    }
                    if (type2 == null)
                    {
                        type2 = this.Create(type);
                    }
                    type2.Pending = true;
                    this.TakeLock(ref opaqueToken);
                    if (this.FindWithoutAdd(type) != null)
                    {
                        throw new ArgumentException("Duplicate type", "type");
                    }
                    this.ThrowIfFrozen();
                    this.types.Add(type2);
                    if (applyDefaultBehaviour)
                    {
                        type2.ApplyDefaultBehaviour();
                    }
                    type2.Pending = false;
                }
                finally
                {
                    this.ReleaseLock(opaqueToken);
                }
            }
            return type2;
        }

        private void AddContention()
        {
            Interlocked.Increment(ref this.contentionCounter);
        }

        private void BuildAllSerializers()
        {
            for (int i = 0; i < this.types.Count; i++)
            {
                MetaType type = (MetaType) this.types[i];
                if (type.Serializer == null)
                {
                    throw new InvalidOperationException("No serializer available for " + type.Type.Name);
                }
            }
        }

        private void CascadeDependents(BasicList list, MetaType metaType)
        {
            MetaType surrogateOrBaseOrSelf;
            if (metaType.IsList)
            {
                WireType type3;
                Type listItemType = GetListItemType(this, metaType.Type);
                if (ValueMember.TryGetCoreSerializer(this, DataFormat.Default, listItemType, out type3, false, false, false, false) == null)
                {
                    int num = this.FindOrAddAuto(listItemType, false, false, false);
                    if (num >= 0)
                    {
                        surrogateOrBaseOrSelf = ((MetaType) this.types[num]).GetSurrogateOrBaseOrSelf(false);
                        if (!list.Contains(surrogateOrBaseOrSelf))
                        {
                            list.Add(surrogateOrBaseOrSelf);
                            this.CascadeDependents(list, surrogateOrBaseOrSelf);
                        }
                    }
                }
            }
            else
            {
                if (metaType.IsAutoTuple)
                {
                    MemberInfo[] infoArray;
                    if (MetaType.ResolveTupleConstructor(metaType.Type, out infoArray) != null)
                    {
                        for (int i = 0; i < infoArray.Length; i++)
                        {
                            WireType type5;
                            Type propertyType = null;
                            if (infoArray[i] is PropertyInfo)
                            {
                                propertyType = ((PropertyInfo) infoArray[i]).PropertyType;
                            }
                            else if (infoArray[i] is FieldInfo)
                            {
                                propertyType = ((FieldInfo) infoArray[i]).FieldType;
                            }
                            if (ValueMember.TryGetCoreSerializer(this, DataFormat.Default, propertyType, out type5, false, false, false, false) == null)
                            {
                                int num3 = this.FindOrAddAuto(propertyType, false, false, false);
                                if (num3 >= 0)
                                {
                                    surrogateOrBaseOrSelf = ((MetaType) this.types[num3]).GetSurrogateOrBaseOrSelf(false);
                                    if (!list.Contains(surrogateOrBaseOrSelf))
                                    {
                                        list.Add(surrogateOrBaseOrSelf);
                                        this.CascadeDependents(list, surrogateOrBaseOrSelf);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (ValueMember member in metaType.Fields)
                    {
                        WireType type7;
                        Type itemType = member.ItemType;
                        if (itemType == null)
                        {
                            itemType = member.MemberType;
                        }
                        if (ValueMember.TryGetCoreSerializer(this, DataFormat.Default, itemType, out type7, false, false, false, false) == null)
                        {
                            int num4 = this.FindOrAddAuto(itemType, false, false, false);
                            if (num4 >= 0)
                            {
                                surrogateOrBaseOrSelf = ((MetaType) this.types[num4]).GetSurrogateOrBaseOrSelf(false);
                                if (!list.Contains(surrogateOrBaseOrSelf))
                                {
                                    list.Add(surrogateOrBaseOrSelf);
                                    this.CascadeDependents(list, surrogateOrBaseOrSelf);
                                }
                            }
                        }
                    }
                }
                if (metaType.HasSubtypes)
                {
                    foreach (SubType type8 in metaType.GetSubtypes())
                    {
                        surrogateOrBaseOrSelf = type8.DerivedType.GetSurrogateOrSelf();
                        if (!list.Contains(surrogateOrBaseOrSelf))
                        {
                            list.Add(surrogateOrBaseOrSelf);
                            this.CascadeDependents(list, surrogateOrBaseOrSelf);
                        }
                    }
                }
                surrogateOrBaseOrSelf = metaType.BaseType;
                if (surrogateOrBaseOrSelf != null)
                {
                    surrogateOrBaseOrSelf = surrogateOrBaseOrSelf.GetSurrogateOrSelf();
                }
                if ((surrogateOrBaseOrSelf != null) && !list.Contains(surrogateOrBaseOrSelf))
                {
                    list.Add(surrogateOrBaseOrSelf);
                    this.CascadeDependents(list, surrogateOrBaseOrSelf);
                }
            }
        }

        public TypeModel Compile() => 
            this.Compile(null, null);

        public TypeModel Compile(CompilerOptions options)
        {
            string str3;
            string str4;
            CompilerContext context;
            int num2;
            Type type3;
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            string typeName = options.TypeName;
            string outputPath = options.OutputPath;
            this.BuildAllSerializers();
            this.Freeze();
            bool flag = !Helpers.IsNullOrEmpty(outputPath);
            if (Helpers.IsNullOrEmpty(typeName))
            {
                if (flag)
                {
                    throw new ArgumentNullException("typeName");
                }
                typeName = Guid.NewGuid().ToString();
            }
            if (outputPath == null)
            {
                str3 = typeName;
                str4 = str3 + ".dll";
            }
            else
            {
                str3 = new FileInfo(Path.GetFileNameWithoutExtension(outputPath)).Name;
                str4 = str3 + Path.GetExtension(outputPath);
            }
            AssemblyName name = new AssemblyName {
                Name = str3
            };
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, flag ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run);
            ModuleBuilder builder2 = flag ? builder.DefineDynamicModule(str4, outputPath) : builder.DefineDynamicModule(str4);
            if (!Helpers.IsNullOrEmpty(options.TargetFrameworkName))
            {
                Type type5 = null;
                try
                {
                    type5 = this.GetType("System.Runtime.Versioning.TargetFrameworkAttribute", base.MapType(typeof(string)).Assembly);
                }
                catch
                {
                }
                if (type5 != null)
                {
                    PropertyInfo[] infoArray;
                    object[] objArray;
                    if (Helpers.IsNullOrEmpty(options.TargetFrameworkDisplayName))
                    {
                        infoArray = new PropertyInfo[0];
                        objArray = new object[0];
                    }
                    else
                    {
                        infoArray = new PropertyInfo[] { type5.GetProperty("FrameworkDisplayName") };
                        objArray = new object[] { options.TargetFrameworkDisplayName };
                    }
                    Type[] types = new Type[] { base.MapType(typeof(string)) };
                    object[] constructorArgs = new object[] { options.TargetFrameworkName };
                    builder.SetCustomAttribute(new CustomAttributeBuilder(type5.GetConstructor(types), constructorArgs, infoArray, objArray));
                }
            }
            Type type = null;
            try
            {
                type = base.MapType(typeof(InternalsVisibleToAttribute));
            }
            catch
            {
            }
            if (type != null)
            {
                BasicList list = new BasicList();
                BasicList list2 = new BasicList();
                foreach (MetaType type6 in this.types)
                {
                    Assembly instance = type6.Type.Assembly;
                    if (list2.IndexOfReference(instance) < 0)
                    {
                        list2.Add(instance);
                        AttributeMap[] mapArray = AttributeMap.Create(this, instance);
                        for (int n = 0; n < mapArray.Length; n++)
                        {
                            if (mapArray[n].AttributeType == type)
                            {
                                object obj2;
                                mapArray[n].TryGet("AssemblyName", out obj2);
                                string str5 = obj2 as string;
                                if (((str5 != str3) && !Helpers.IsNullOrEmpty(str5)) && (list.IndexOf(new StringFinder(str5)) < 0))
                                {
                                    list.Add(str5);
                                    Type[] types = new Type[] { base.MapType(typeof(string)) };
                                    object[] constructorArgs = new object[] { str5 };
                                    builder.SetCustomAttribute(new CustomAttributeBuilder(type.GetConstructor(types), constructorArgs));
                                }
                            }
                        }
                    }
                }
            }
            Type parent = base.MapType(typeof(TypeModel));
            TypeAttributes attr = (parent.Attributes & ~TypeAttributes.Abstract) | TypeAttributes.Sealed;
            if (options.Accessibility == Accessibility.Internal)
            {
                attr &= ~TypeAttributes.Public;
            }
            TypeBuilder builder3 = builder2.DefineType(typeName, attr, parent);
            int index = 0;
            bool flag2 = false;
            SerializerPair[] array = new SerializerPair[this.types.Count];
            foreach (MetaType type7 in this.types)
            {
                Type[] parameterTypes = new Type[] { type7.Type, base.MapType(typeof(ProtoWriter)) };
                MethodBuilder serialize = builder3.DefineMethod("Write", MethodAttributes.Static | MethodAttributes.Private, CallingConventions.Standard, base.MapType(typeof(void)), parameterTypes);
                Type[] typeArray4 = new Type[] { type7.Type, base.MapType(typeof(ProtoReader)) };
                MethodBuilder deserialize = builder3.DefineMethod("Read", MethodAttributes.Static | MethodAttributes.Private, CallingConventions.Standard, type7.Type, typeArray4);
                SerializerPair pair = new SerializerPair(this.GetKey(type7.Type, true, false), this.GetKey(type7.Type, true, true), type7, serialize, deserialize, serialize.GetILGenerator(), deserialize.GetILGenerator());
                index++;
                array[index] = pair;
                if (pair.MetaKey != pair.BaseKey)
                {
                    flag2 = true;
                }
            }
            if (flag2)
            {
                Array.Sort<SerializerPair>(array);
            }
            CompilerContext.ILVersion metadataVersion = CompilerContext.ILVersion.Net2;
            if (options.MetaDataVersion == 0x10000)
            {
                metadataVersion = CompilerContext.ILVersion.Net1;
            }
            for (index = 0; index < array.Length; index++)
            {
                SerializerPair pair2 = array[index];
                context = new CompilerContext(pair2.SerializeBody, true, true, array, this, metadataVersion, str3);
                context.CheckAccessibility(pair2.Deserialize.ReturnType);
                pair2.Type.Serializer.EmitWrite(context, Local.InputValue);
                context.Return();
                context = new CompilerContext(pair2.DeserializeBody, true, false, array, this, metadataVersion, str3);
                pair2.Type.Serializer.EmitRead(context, Local.InputValue);
                if (!pair2.Type.Serializer.ReturnsValue)
                {
                    context.LoadValue(Local.InputValue);
                }
                context.Return();
            }
            ILGenerator il = Override(builder3, "GetKeyImpl");
            if (this.types.Count <= 20)
            {
                num2 = 1;
                type3 = this.MapType(typeof(Type[]), true);
            }
            else
            {
                type3 = this.MapType(typeof(Dictionary<Type, int>), false);
                if (type3 != null)
                {
                    num2 = 2;
                }
                else
                {
                    type3 = this.MapType(typeof(Hashtable), true);
                    num2 = 3;
                }
            }
            FieldBuilder field = builder3.DefineField("knownTypes", type3, FieldAttributes.InitOnly | FieldAttributes.Static | FieldAttributes.Private);
            switch (num2)
            {
                case 1:
                {
                    il.Emit(OpCodes.Ldsfld, field);
                    il.Emit(OpCodes.Ldarg_1);
                    Type[] types = new Type[] { base.MapType(typeof(object)) };
                    il.EmitCall(OpCodes.Callvirt, base.MapType(typeof(IList)).GetMethod("IndexOf", types), null);
                    if (!flag2)
                    {
                        il.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        il.DeclareLocal(base.MapType(typeof(int)));
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Stloc_0);
                        BasicList list3 = new BasicList();
                        int baseKey = -1;
                        int num5 = 0;
                        while (true)
                        {
                            if ((num5 >= array.Length) || (array[num5].MetaKey == array[num5].BaseKey))
                            {
                                Label[] labelArray2 = new Label[list3.Count];
                                list3.CopyTo(labelArray2, 0);
                                il.Emit(OpCodes.Switch, labelArray2);
                                il.Emit(OpCodes.Ldloc_0);
                                il.Emit(OpCodes.Ret);
                                baseKey = -1;
                                for (int n = labelArray2.Length - 1; n >= 0; n--)
                                {
                                    if (baseKey != array[n].BaseKey)
                                    {
                                        baseKey = array[n].BaseKey;
                                        int num7 = -1;
                                        int length = labelArray2.Length;
                                        while (true)
                                        {
                                            if (length < array.Length)
                                            {
                                                if ((array[length].BaseKey != baseKey) || (array[length].MetaKey != baseKey))
                                                {
                                                    length++;
                                                    continue;
                                                }
                                                num7 = length;
                                            }
                                            il.MarkLabel(labelArray2[n]);
                                            CompilerContext.LoadValue(il, num7);
                                            il.Emit(OpCodes.Ret);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            if (baseKey == array[num5].BaseKey)
                            {
                                list3.Add(list3[list3.Count - 1]);
                            }
                            else
                            {
                                list3.Add(il.DefineLabel());
                                baseKey = array[num5].BaseKey;
                            }
                            num5++;
                        }
                    }
                    break;
                }
                case 2:
                {
                    LocalBuilder local = il.DeclareLocal(base.MapType(typeof(int)));
                    Label label = il.DefineLabel();
                    il.Emit(OpCodes.Ldsfld, field);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldloca_S, local);
                    il.EmitCall(OpCodes.Callvirt, type3.GetMethod("TryGetValue", BindingFlags.Public | BindingFlags.Instance), null);
                    il.Emit(OpCodes.Brfalse_S, label);
                    il.Emit(OpCodes.Ldloc_S, local);
                    il.Emit(OpCodes.Ret);
                    il.MarkLabel(label);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.Emit(OpCodes.Ret);
                    break;
                }
                case 3:
                {
                    Label label2 = il.DefineLabel();
                    il.Emit(OpCodes.Ldsfld, field);
                    il.Emit(OpCodes.Ldarg_1);
                    il.EmitCall(OpCodes.Callvirt, type3.GetProperty("Item").GetGetMethod(), null);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Brfalse_S, label2);
                    if (metadataVersion != CompilerContext.ILVersion.Net1)
                    {
                        il.Emit(OpCodes.Unbox_Any, base.MapType(typeof(int)));
                    }
                    else
                    {
                        il.Emit(OpCodes.Unbox, base.MapType(typeof(int)));
                        il.Emit(OpCodes.Ldobj, base.MapType(typeof(int)));
                    }
                    il.Emit(OpCodes.Ret);
                    il.MarkLabel(label2);
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.Emit(OpCodes.Ret);
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }
            il = Override(builder3, "Serialize");
            context = new CompilerContext(il, false, true, array, this, metadataVersion, str3);
            Label[] labels = new Label[this.types.Count];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = il.DefineLabel();
            }
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Switch, labels);
            context.Return();
            for (int j = 0; j < labels.Length; j++)
            {
                SerializerPair pair3 = array[j];
                il.MarkLabel(labels[j]);
                il.Emit(OpCodes.Ldarg_2);
                context.CastFromObject(pair3.Type.Type);
                il.Emit(OpCodes.Ldarg_3);
                il.EmitCall(OpCodes.Call, pair3.Serialize, null);
                context.Return();
            }
            il = Override(builder3, "Deserialize");
            context = new CompilerContext(il, false, false, array, this, metadataVersion, str3);
            for (int k = 0; k < labels.Length; k++)
            {
                labels[k] = il.DefineLabel();
            }
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Switch, labels);
            context.LoadNullRef();
            context.Return();
            for (int m = 0; m < labels.Length; m++)
            {
                SerializerPair pair4 = array[m];
                il.MarkLabel(labels[m]);
                Type valueType = pair4.Type.Type;
                if (valueType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldarg_3);
                    il.EmitCall(OpCodes.Call, EmitBoxedSerializer(builder3, m, valueType, array, this, metadataVersion, str3), null);
                    context.Return();
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_2);
                    context.CastFromObject(valueType);
                    il.Emit(OpCodes.Ldarg_3);
                    il.EmitCall(OpCodes.Call, pair4.Deserialize, null);
                    context.Return();
                }
            }
            builder3.DefineDefaultConstructor(MethodAttributes.Public);
            il = builder3.DefineTypeInitializer().GetILGenerator();
            switch (num2)
            {
                case 1:
                {
                    CompilerContext.LoadValue(il, this.types.Count);
                    il.Emit(OpCodes.Newarr, context.MapType(typeof(Type)));
                    index = 0;
                    SerializerPair[] pairArray2 = array;
                    int num13 = 0;
                    while (true)
                    {
                        if (num13 >= pairArray2.Length)
                        {
                            il.Emit(OpCodes.Stsfld, field);
                            il.Emit(OpCodes.Ret);
                            break;
                        }
                        SerializerPair pair5 = pairArray2[num13];
                        il.Emit(OpCodes.Dup);
                        CompilerContext.LoadValue(il, index);
                        il.Emit(OpCodes.Ldtoken, pair5.Type.Type);
                        il.EmitCall(OpCodes.Call, context.MapType(typeof(Type)).GetMethod("GetTypeFromHandle"), null);
                        il.Emit(OpCodes.Stelem_Ref);
                        index++;
                        num13++;
                    }
                    break;
                }
                case 2:
                {
                    CompilerContext.LoadValue(il, this.types.Count);
                    LocalBuilder builder10 = il.DeclareLocal(type3);
                    Type[] types = new Type[] { base.MapType(typeof(int)) };
                    il.Emit(OpCodes.Newobj, type3.GetConstructor(types));
                    il.Emit(OpCodes.Stsfld, field);
                    int num14 = 0;
                    SerializerPair[] pairArray3 = array;
                    int num15 = 0;
                    while (true)
                    {
                        if (num15 >= pairArray3.Length)
                        {
                            il.Emit(OpCodes.Ret);
                            break;
                        }
                        SerializerPair pair6 = pairArray3[num15];
                        il.Emit(OpCodes.Ldsfld, field);
                        il.Emit(OpCodes.Ldtoken, pair6.Type.Type);
                        il.EmitCall(OpCodes.Call, context.MapType(typeof(Type)).GetMethod("GetTypeFromHandle"), null);
                        num14++;
                        int num16 = num14;
                        int baseKey = pair6.BaseKey;
                        if (baseKey != pair6.MetaKey)
                        {
                            num16 = -1;
                            for (int n = 0; n < array.Length; n++)
                            {
                                if ((array[n].BaseKey == baseKey) && (array[n].MetaKey == baseKey))
                                {
                                    num16 = n;
                                    break;
                                }
                            }
                        }
                        CompilerContext.LoadValue(il, num16);
                        Type[] typeArray7 = new Type[] { base.MapType(typeof(Type)), base.MapType(typeof(int)) };
                        il.EmitCall(OpCodes.Callvirt, type3.GetMethod("Add", typeArray7), null);
                        num15++;
                    }
                    break;
                }
                case 3:
                {
                    CompilerContext.LoadValue(il, this.types.Count);
                    Type[] types = new Type[] { base.MapType(typeof(int)) };
                    il.Emit(OpCodes.Newobj, type3.GetConstructor(types));
                    il.Emit(OpCodes.Stsfld, field);
                    int num19 = 0;
                    SerializerPair[] pairArray4 = array;
                    int num20 = 0;
                    while (true)
                    {
                        if (num20 >= pairArray4.Length)
                        {
                            il.Emit(OpCodes.Ret);
                            break;
                        }
                        SerializerPair pair7 = pairArray4[num20];
                        il.Emit(OpCodes.Ldsfld, field);
                        il.Emit(OpCodes.Ldtoken, pair7.Type.Type);
                        il.EmitCall(OpCodes.Call, context.MapType(typeof(Type)).GetMethod("GetTypeFromHandle"), null);
                        num19++;
                        int num21 = num19;
                        int baseKey = pair7.BaseKey;
                        if (baseKey != pair7.MetaKey)
                        {
                            num21 = -1;
                            for (int n = 0; n < array.Length; n++)
                            {
                                if ((array[n].BaseKey == baseKey) && (array[n].MetaKey == baseKey))
                                {
                                    num21 = n;
                                    break;
                                }
                            }
                        }
                        CompilerContext.LoadValue(il, num21);
                        il.Emit(OpCodes.Box, base.MapType(typeof(int)));
                        Type[] typeArray9 = new Type[] { base.MapType(typeof(object)), base.MapType(typeof(object)) };
                        il.EmitCall(OpCodes.Callvirt, type3.GetMethod("Add", typeArray9), null);
                        num20++;
                    }
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }
            Type type4 = builder3.CreateType();
            if (!Helpers.IsNullOrEmpty(outputPath))
            {
                builder.Save(outputPath);
            }
            return (TypeModel) Activator.CreateInstance(type4);
        }

        public TypeModel Compile(string name, string path)
        {
            CompilerOptions options = new CompilerOptions {
                TypeName = name,
                OutputPath = path
            };
            return this.Compile(options);
        }

        public void CompileInPlace()
        {
            foreach (MetaType type in this.types)
            {
                type.CompileInPlace();
            }
        }

        private MetaType Create(Type type)
        {
            this.ThrowIfFrozen();
            return new MetaType(this, type, this.defaultFactory);
        }

        protected internal override object Deserialize(int key, object value, ProtoReader source)
        {
            IProtoSerializer serializer = ((MetaType) this.types[key]).Serializer;
            if (((value == null) && Helpers.IsValueType(serializer.ExpectedType)) && serializer.RequiresOldValue)
            {
                value = Activator.CreateInstance(serializer.ExpectedType);
            }
            return serializer.Read(value, source);
        }

        private static MethodBuilder EmitBoxedSerializer(TypeBuilder type, int i, Type valueType, SerializerPair[] methodPairs, TypeModel model, CompilerContext.ILVersion ilVersion, string assemblyName)
        {
            MethodInfo deserialize = methodPairs[i].Deserialize;
            Type[] parameterTypes = new Type[] { model.MapType(typeof(object)), model.MapType(typeof(ProtoReader)) };
            MethodBuilder builder = type.DefineMethod("_" + i, MethodAttributes.Static, CallingConventions.Standard, model.MapType(typeof(object)), parameterTypes);
            CompilerContext ctx = new CompilerContext(builder.GetILGenerator(), true, false, methodPairs, model, ilVersion, assemblyName);
            ctx.LoadValue(Local.InputValue);
            CodeLabel label = ctx.DefineLabel();
            ctx.BranchIfFalse(label, true);
            Type type2 = valueType;
            ctx.LoadValue(Local.InputValue);
            ctx.CastFromObject(type2);
            ctx.LoadReaderWriter();
            ctx.EmitCall(deserialize);
            ctx.CastToObject(type2);
            ctx.Return();
            ctx.MarkLabel(label);
            using (Local local = new Local(ctx, type2))
            {
                ctx.LoadAddress(local, type2);
                ctx.EmitCtor(type2);
                ctx.LoadValue(local);
                ctx.LoadReaderWriter();
                ctx.EmitCall(deserialize);
                ctx.CastToObject(type2);
                ctx.Return();
            }
            return builder;
        }

        internal int FindOrAddAuto(Type type, bool demand, bool addWithContractOnly, bool addEvenIfAutoDisabled)
        {
            MetaType type2;
            TypeFinder predicate = new TypeFinder(type);
            int index = this.types.IndexOf(predicate);
            if ((index >= 0) && (type2 = (MetaType) this.types[index]).Pending)
            {
                this.WaitOnLock(type2);
            }
            if (index < 0)
            {
                Type type3 = ResolveProxies(type);
                if (type3 != null)
                {
                    predicate = new TypeFinder(type3);
                    index = this.types.IndexOf(predicate);
                    type = type3;
                }
            }
            if (index < 0)
            {
                int opaqueToken = 0;
                try
                {
                    this.TakeLock(ref opaqueToken);
                    type2 = this.RecogniseCommonTypes(type);
                    if (type2 == null)
                    {
                        MetaType.AttributeFamily family = MetaType.GetContractFamily(this, type, null);
                        if (family == MetaType.AttributeFamily.AutoTuple)
                        {
                            addEvenIfAutoDisabled = true;
                        }
                        if (!(this.AutoAddMissingTypes | addEvenIfAutoDisabled))
                        {
                            goto TR_0005;
                        }
                        else
                        {
                            if ((!Helpers.IsEnum(type) & addWithContractOnly) && (family == MetaType.AttributeFamily.None))
                            {
                                goto TR_0005;
                            }
                            type2 = this.Create(type);
                        }
                    }
                    type2.Pending = true;
                    bool flag = false;
                    int num3 = this.types.IndexOf(predicate);
                    if (num3 >= 0)
                    {
                        index = num3;
                    }
                    else
                    {
                        this.ThrowIfFrozen();
                        index = this.types.Add(type2);
                        flag = true;
                    }
                    if (flag)
                    {
                        type2.ApplyDefaultBehaviour();
                        type2.Pending = false;
                    }
                    return index;
                TR_0005:
                    if (demand)
                    {
                        ThrowUnexpectedType(type);
                    }
                    return index;
                }
                finally
                {
                    this.ReleaseLock(opaqueToken);
                }
            }
            return index;
        }

        internal MetaType FindWithoutAdd(Type type)
        {
            using (IEnumerator enumerator = this.types.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MetaType current = (MetaType) enumerator.Current;
                    if (current.Type == type)
                    {
                        if (current.Pending)
                        {
                            this.WaitOnLock(current);
                        }
                        return current;
                    }
                }
            }
            Type type2 = ResolveProxies(type);
            return ((type2 == null) ? null : this.FindWithoutAdd(type2));
        }

        public void Freeze()
        {
            if (this.GetOption(2))
            {
                throw new InvalidOperationException("The default model cannot be frozen");
            }
            this.SetOption(4, true);
        }

        private int GetContention() => 
            Interlocked.CompareExchange(ref this.contentionCounter, 0, 0);

        internal EnumSerializer.EnumPair[] GetEnumMap(Type type)
        {
            int num = this.FindOrAddAuto(type, false, false, false);
            return ((num < 0) ? null : ((MetaType) this.types[num]).GetEnumMap());
        }

        internal int GetKey(Type type, bool demand, bool getBaseKey)
        {
            int num2;
            try
            {
                int num = this.FindOrAddAuto(type, demand, true, false);
                if (num >= 0)
                {
                    MetaType source = (MetaType) this.types[num];
                    if (getBaseKey)
                    {
                        source = MetaType.GetRootType(source);
                        num = this.FindOrAddAuto(source.Type, true, true, false);
                    }
                }
                num2 = num;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (exception.Message.IndexOf(type.FullName) >= 0)
                {
                    throw;
                }
                throw new ProtoException(exception.Message + " (" + type.FullName + ")", exception);
            }
            return num2;
        }

        protected override int GetKeyImpl(Type type) => 
            this.GetKey(type, false, true);

        private bool GetOption(byte option) => 
            ((this.options & option) == option);

        public override string GetSchema(Type type)
        {
            BasicList list = new BasicList();
            MetaType surrogateOrBaseOrSelf = null;
            bool flag = false;
            if (type == null)
            {
                foreach (MetaType type3 in this.types)
                {
                    MetaType surrogateOrBaseOrSelf = type3.GetSurrogateOrBaseOrSelf(false);
                    if (!list.Contains(surrogateOrBaseOrSelf))
                    {
                        list.Add(surrogateOrBaseOrSelf);
                        this.CascadeDependents(list, surrogateOrBaseOrSelf);
                    }
                }
            }
            else
            {
                WireType type6;
                Type underlyingType = Helpers.GetUnderlyingType(type);
                if (underlyingType != null)
                {
                    type = underlyingType;
                }
                flag = ValueMember.TryGetCoreSerializer(this, DataFormat.Default, type, out type6, false, false, false, false) != null;
                if (!flag)
                {
                    int num = this.FindOrAddAuto(type, false, false, false);
                    if (num < 0)
                    {
                        throw new ArgumentException("The type specified is not a contract-type", "type");
                    }
                    surrogateOrBaseOrSelf = ((MetaType) this.types[num]).GetSurrogateOrBaseOrSelf(false);
                    list.Add(surrogateOrBaseOrSelf);
                    this.CascadeDependents(list, surrogateOrBaseOrSelf);
                }
            }
            StringBuilder builder = new StringBuilder();
            string str = null;
            if (!flag)
            {
                foreach (MetaType type7 in (surrogateOrBaseOrSelf == null) ? this.types : list)
                {
                    if (type7.IsList)
                    {
                        continue;
                    }
                    string str2 = type7.Type.Namespace;
                    if (!Helpers.IsNullOrEmpty(str2) && !str2.StartsWith("System."))
                    {
                        if (str == null)
                        {
                            str = str2;
                            continue;
                        }
                        if (str != str2)
                        {
                            str = null;
                            break;
                        }
                    }
                }
            }
            if (!Helpers.IsNullOrEmpty(str))
            {
                builder.Append("package ").Append(str).Append(';');
                Helpers.AppendLine(builder);
            }
            bool requiresBclImport = false;
            StringBuilder builder2 = new StringBuilder();
            MetaType[] array = new MetaType[list.Count];
            list.CopyTo(array, 0);
            Array.Sort<MetaType>(array, MetaType.Comparer.Default);
            if (flag)
            {
                Helpers.AppendLine(builder2).Append("message ").Append(type.Name).Append(" {");
                MetaType.NewLine(builder2, 1).Append("optional ").Append(this.GetSchemaTypeName(type, DataFormat.Default, false, false, ref requiresBclImport)).Append(" value = 1;");
                Helpers.AppendLine(builder2).Append('}');
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    MetaType objA = array[i];
                    if (!objA.IsList || ReferenceEquals(objA, surrogateOrBaseOrSelf))
                    {
                        objA.WriteSchema(builder2, 0, ref requiresBclImport);
                    }
                }
            }
            if (requiresBclImport)
            {
                builder.Append("import \"bcl.proto\"; // schema for protobuf-net's handling of core .NET types");
                Helpers.AppendLine(builder);
            }
            return Helpers.AppendLine(builder.Append(builder2)).ToString();
        }

        internal string GetSchemaTypeName(Type effectiveType, DataFormat dataFormat, bool asReference, bool dynamicType, ref bool requiresBclImport)
        {
            WireType type2;
            Type underlyingType = Helpers.GetUnderlyingType(effectiveType);
            if (underlyingType != null)
            {
                effectiveType = underlyingType;
            }
            if (effectiveType == base.MapType(typeof(byte[])))
            {
                return "bytes";
            }
            IProtoSerializer serializer = ValueMember.TryGetCoreSerializer(this, dataFormat, effectiveType, out type2, false, false, false, false);
            if (serializer == null)
            {
                if (!(asReference | dynamicType))
                {
                    return this[effectiveType].GetSurrogateOrBaseOrSelf(true).GetSchemaTypeName();
                }
                requiresBclImport = true;
                return "bcl.NetObjectProxy";
            }
            if (serializer is ParseableSerializer)
            {
                if (asReference)
                {
                    requiresBclImport = true;
                }
                return (asReference ? "bcl.NetObjectProxy" : "string");
            }
            ProtoTypeCode typeCode = Helpers.GetTypeCode(effectiveType);
            switch (typeCode)
            {
                case ProtoTypeCode.Boolean:
                    return "bool";

                case ProtoTypeCode.Char:
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.UInt16:
                case ProtoTypeCode.UInt32:
                    return ((dataFormat != DataFormat.FixedSize) ? "uint32" : "fixed32");

                case ProtoTypeCode.SByte:
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.Int32:
                    return ((dataFormat == DataFormat.ZigZag) ? "sint32" : ((dataFormat == DataFormat.FixedSize) ? "sfixed32" : "int32"));

                case ProtoTypeCode.Int64:
                    return ((dataFormat == DataFormat.ZigZag) ? "sint64" : ((dataFormat == DataFormat.FixedSize) ? "sfixed64" : "int64"));

                case ProtoTypeCode.UInt64:
                    return ((dataFormat != DataFormat.FixedSize) ? "uint64" : "fixed64");

                case ProtoTypeCode.Single:
                    return "float";

                case ProtoTypeCode.Double:
                    return "double";

                case ProtoTypeCode.Decimal:
                    requiresBclImport = true;
                    return "bcl.Decimal";

                case ProtoTypeCode.DateTime:
                    requiresBclImport = true;
                    return "bcl.DateTime";

                case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                    break;

                case ProtoTypeCode.String:
                    if (asReference)
                    {
                        requiresBclImport = true;
                    }
                    return (asReference ? "bcl.NetObjectProxy" : "string");

                default:
                    if (typeCode == ProtoTypeCode.TimeSpan)
                    {
                        requiresBclImport = true;
                        return "bcl.TimeSpan";
                    }
                    if (typeCode != ProtoTypeCode.Guid)
                    {
                        break;
                    }
                    requiresBclImport = true;
                    return "bcl.Guid";
            }
            throw new NotSupportedException("No .proto map found for: " + effectiveType.FullName);
        }

        internal ProtoSerializer GetSerializer(IProtoSerializer serializer, bool compiled)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            return (!compiled ? new ProtoSerializer(serializer.Write) : CompilerContext.BuildSerializer(serializer, this));
        }

        public IEnumerable GetTypes() => 
            this.types;

        internal bool IsDefined(Type type, int fieldNumber) => 
            this.FindWithoutAdd(type).IsDefined(fieldNumber);

        internal bool IsPrepared(Type type)
        {
            MetaType type2 = this.FindWithoutAdd(type);
            return ((type2 != null) && type2.IsPrepared());
        }

        private static ILGenerator Override(TypeBuilder type, string name)
        {
            MethodInfo method = type.BaseType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            ParameterInfo[] parameters = method.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }
            MethodBuilder methodInfoBody = type.DefineMethod(method.Name, (method.Attributes & ~MethodAttributes.Abstract) | MethodAttributes.Final, method.CallingConvention, method.ReturnType, parameterTypes);
            ILGenerator iLGenerator = methodInfoBody.GetILGenerator();
            type.DefineMethodOverride(methodInfoBody, method);
            return iLGenerator;
        }

        private MetaType RecogniseCommonTypes(Type type) => 
            null;

        internal void ReleaseLock(int opaqueToken)
        {
            if (opaqueToken != 0)
            {
                Monitor.Exit(this.types);
                if ((opaqueToken != this.GetContention()) && (this.LockContended != null))
                {
                    LockContentedEventHandler handler;
                    string stackTrace;
                    try
                    {
                        throw new Exception();
                    }
                    catch (Exception exception1)
                    {
                        stackTrace = exception1.StackTrace;
                    }
                    handler(this, new LockContentedEventArgs(stackTrace));
                }
            }
        }

        internal void ResolveListTypes(Type type, ref Type itemType, ref Type defaultType)
        {
            if (((type != null) && (Helpers.GetTypeCode(type) == ProtoTypeCode.Unknown)) && !this[type].IgnoreListHandling)
            {
                if (type.IsArray)
                {
                    if (type.GetArrayRank() != 1)
                    {
                        throw new NotSupportedException("Multi-dimension arrays are supported");
                    }
                    itemType = type.GetElementType();
                    if (itemType != base.MapType(typeof(byte)))
                    {
                        defaultType = type;
                    }
                    else
                    {
                        Type type2;
                        itemType = (Type) (type2 = null);
                        defaultType = type2;
                    }
                }
                if (itemType == null)
                {
                    itemType = GetListItemType(this, type);
                }
                if (itemType != null)
                {
                    Type type3 = null;
                    Type type4 = null;
                    this.ResolveListTypes(itemType, ref type3, ref type4);
                    if (type3 != null)
                    {
                        throw CreateNestedListsNotSupported();
                    }
                }
                if ((itemType != null) && (defaultType == null))
                {
                    if ((type.IsClass && !type.IsAbstract) && (Helpers.GetConstructor(type, Helpers.EmptyTypes, true) != null))
                    {
                        defaultType = type;
                    }
                    if ((defaultType == null) && type.IsInterface)
                    {
                        Type[] typeArray;
                        if ((type.IsGenericType && (type.GetGenericTypeDefinition() == base.MapType(typeof(IDictionary<,>)))) && (itemType == base.MapType(typeof(KeyValuePair<,>)).MakeGenericType(typeArray = type.GetGenericArguments())))
                        {
                            defaultType = base.MapType(typeof(Dictionary<,>)).MakeGenericType(typeArray);
                        }
                        else
                        {
                            Type[] typeArguments = new Type[] { itemType };
                            defaultType = base.MapType(typeof(List<>)).MakeGenericType(typeArguments);
                        }
                    }
                    if ((defaultType != null) && !Helpers.IsAssignableFrom(type, defaultType))
                    {
                        defaultType = null;
                    }
                }
            }
        }

        protected internal override void Serialize(int key, object value, ProtoWriter dest)
        {
            ((MetaType) this.types[key]).Serializer.Write(value, dest);
        }

        public void SetDefaultFactory(MethodInfo methodInfo)
        {
            this.VerifyFactory(methodInfo, null);
            this.defaultFactory = methodInfo;
        }

        private void SetOption(byte option, bool value)
        {
            if (value)
            {
                this.options = (byte) (this.options | option);
            }
            else
            {
                this.options = (byte) (this.options & ~option);
            }
        }

        internal void TakeLock(ref int opaqueToken)
        {
            opaqueToken = 0;
            if (Monitor.TryEnter(this.types, this.metadataTimeoutMilliseconds))
            {
                opaqueToken = this.GetContention();
            }
            else
            {
                this.AddContention();
                throw new TimeoutException("Timeout while inspecting metadata; this may indicate a deadlock. This can often be avoided by preparing necessary serializers during application initialization, rather than allowing multiple threads to perform the initial metadata inspection; please also see the LockContended event");
            }
        }

        private void ThrowIfFrozen()
        {
            if (this.GetOption(4))
            {
                throw new InvalidOperationException("The model cannot be changed once frozen");
            }
        }

        internal void VerifyFactory(MethodInfo factory, Type type)
        {
            if (factory != null)
            {
                if ((type != null) && Helpers.IsValueType(type))
                {
                    throw new InvalidOperationException();
                }
                if (!factory.IsStatic)
                {
                    throw new ArgumentException("A factory-method must be static", "factory");
                }
                if (((type != null) && (factory.ReturnType != type)) && (factory.ReturnType != base.MapType(typeof(object))))
                {
                    throw new ArgumentException("The factory-method must return object" + ((type == null) ? "" : (" or " + type.FullName)), "factory");
                }
                if (!CallbackSet.CheckCallbackParameters(this, factory))
                {
                    throw new ArgumentException("Invalid factory signature in " + factory.DeclaringType.FullName + "." + factory.Name, "factory");
                }
            }
        }

        private void WaitOnLock(MetaType type)
        {
            int opaqueToken = 0;
            try
            {
                this.TakeLock(ref opaqueToken);
            }
            finally
            {
                this.ReleaseLock(opaqueToken);
            }
        }

        public bool InferTagFromNameDefault
        {
            get => 
                this.GetOption(1);
            set => 
                this.SetOption(1, value);
        }

        public bool AutoAddProtoContractTypesOnly
        {
            get => 
                this.GetOption(0x80);
            set => 
                this.SetOption(0x80, value);
        }

        public bool UseImplicitZeroDefaults
        {
            get => 
                this.GetOption(0x20);
            set
            {
                if (!value && this.GetOption(2))
                {
                    throw new InvalidOperationException("UseImplicitZeroDefaults cannot be disabled on the default model");
                }
                this.SetOption(0x20, value);
            }
        }

        public bool AllowParseableTypes
        {
            get => 
                this.GetOption(0x40);
            set => 
                this.SetOption(0x40, value);
        }

        public static RuntimeTypeModel Default =>
            Singleton.Value;

        public MetaType this[Type type] =>
            ((MetaType) this.types[this.FindOrAddAuto(type, true, false, false)]);

        public bool AutoCompile
        {
            get => 
                this.GetOption(0x10);
            set => 
                this.SetOption(0x10, value);
        }

        public bool AutoAddMissingTypes
        {
            get => 
                this.GetOption(8);
            set
            {
                if (!value && this.GetOption(2))
                {
                    throw new InvalidOperationException("The default model must allow missing types");
                }
                this.ThrowIfFrozen();
                this.SetOption(8, value);
            }
        }

        public int MetadataTimeoutMilliseconds
        {
            get => 
                this.metadataTimeoutMilliseconds;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("MetadataTimeoutMilliseconds");
                }
                this.metadataTimeoutMilliseconds = value;
            }
        }

        public enum Accessibility
        {
            Public,
            Internal
        }

        public sealed class CompilerOptions
        {
            private string targetFrameworkName;
            private string targetFrameworkDisplayName;
            private string typeName;
            private string outputPath;
            private string imageRuntimeVersion;
            private int metaDataVersion;
            private ProtoBuf.Meta.RuntimeTypeModel.Accessibility accessibility;

            public void SetFrameworkOptions(MetaType from)
            {
                AttributeMap[] mapArray2 = AttributeMap.Create(from.Model, from.Type.Assembly);
                int index = 0;
                while (true)
                {
                    if (index < mapArray2.Length)
                    {
                        object obj2;
                        AttributeMap map = mapArray2[index];
                        if (map.AttributeType.FullName != "System.Runtime.Versioning.TargetFrameworkAttribute")
                        {
                            index++;
                            continue;
                        }
                        if (map.TryGet("FrameworkName", out obj2))
                        {
                            this.TargetFrameworkName = (string) obj2;
                        }
                        if (map.TryGet("FrameworkDisplayName", out obj2))
                        {
                            this.TargetFrameworkDisplayName = (string) obj2;
                            return;
                        }
                    }
                    return;
                }
            }

            public string TargetFrameworkName
            {
                get => 
                    this.targetFrameworkName;
                set => 
                    (this.targetFrameworkName = value);
            }

            public string TargetFrameworkDisplayName
            {
                get => 
                    this.targetFrameworkDisplayName;
                set => 
                    (this.targetFrameworkDisplayName = value);
            }

            public string TypeName
            {
                get => 
                    this.typeName;
                set => 
                    (this.typeName = value);
            }

            public string OutputPath
            {
                get => 
                    this.outputPath;
                set => 
                    (this.outputPath = value);
            }

            public string ImageRuntimeVersion
            {
                get => 
                    this.imageRuntimeVersion;
                set => 
                    (this.imageRuntimeVersion = value);
            }

            public int MetaDataVersion
            {
                get => 
                    this.metaDataVersion;
                set => 
                    (this.metaDataVersion = value);
            }

            public ProtoBuf.Meta.RuntimeTypeModel.Accessibility Accessibility
            {
                get => 
                    this.accessibility;
                set => 
                    (this.accessibility = value);
            }
        }

        internal class SerializerPair : IComparable
        {
            public readonly int MetaKey;
            public readonly int BaseKey;
            public readonly MetaType Type;
            public readonly MethodBuilder Serialize;
            public readonly MethodBuilder Deserialize;
            public readonly ILGenerator SerializeBody;
            public readonly ILGenerator DeserializeBody;

            public SerializerPair(int metaKey, int baseKey, MetaType type, MethodBuilder serialize, MethodBuilder deserialize, ILGenerator serializeBody, ILGenerator deserializeBody)
            {
                this.MetaKey = metaKey;
                this.BaseKey = baseKey;
                this.Serialize = serialize;
                this.Deserialize = deserialize;
                this.SerializeBody = serializeBody;
                this.DeserializeBody = deserializeBody;
                this.Type = type;
            }

            int IComparable.CompareTo(object obj)
            {
                if (obj == null)
                {
                    throw new ArgumentException("obj");
                }
                RuntimeTypeModel.SerializerPair pair = (RuntimeTypeModel.SerializerPair) obj;
                if (this.BaseKey == this.MetaKey)
                {
                    if (pair.BaseKey == pair.MetaKey)
                    {
                        return this.MetaKey.CompareTo(pair.MetaKey);
                    }
                    return 1;
                }
                if (pair.BaseKey == pair.MetaKey)
                {
                    return -1;
                }
                int num2 = this.BaseKey.CompareTo(pair.BaseKey);
                if (num2 == 0)
                {
                    num2 = this.MetaKey.CompareTo(pair.MetaKey);
                }
                return num2;
            }
        }

        private class Singleton
        {
            internal static readonly RuntimeTypeModel Value = new RuntimeTypeModel(true);

            private Singleton()
            {
            }
        }

        private sealed class StringFinder : BasicList.IPredicate
        {
            private readonly string value;

            public StringFinder(string value)
            {
                this.value = value;
            }

            bool BasicList.IPredicate.IsMatch(object obj) => 
                (this.value == ((string) obj));
        }

        private sealed class TypeFinder : BasicList.IPredicate
        {
            private readonly Type type;

            public TypeFinder(Type type)
            {
                this.type = type;
            }

            public bool IsMatch(object obj) => 
                (((MetaType) obj).Type == this.type);
        }
    }
}

