namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    public abstract class TypeModel
    {
        private static readonly Type ilist = typeof(IList);
        [CompilerGenerated]
        private TypeFormatEventHandler DynamicTypeFormatting;

        public event TypeFormatEventHandler DynamicTypeFormatting
        {
            [CompilerGenerated] add
            {
                TypeFormatEventHandler dynamicTypeFormatting = this.DynamicTypeFormatting;
                while (true)
                {
                    TypeFormatEventHandler a = dynamicTypeFormatting;
                    TypeFormatEventHandler handler3 = (TypeFormatEventHandler) Delegate.Combine(a, value);
                    dynamicTypeFormatting = Interlocked.CompareExchange<TypeFormatEventHandler>(ref this.DynamicTypeFormatting, handler3, a);
                    if (ReferenceEquals(dynamicTypeFormatting, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                TypeFormatEventHandler dynamicTypeFormatting = this.DynamicTypeFormatting;
                while (true)
                {
                    TypeFormatEventHandler source = dynamicTypeFormatting;
                    TypeFormatEventHandler handler3 = (TypeFormatEventHandler) Delegate.Remove(source, value);
                    dynamicTypeFormatting = Interlocked.CompareExchange<TypeFormatEventHandler>(ref this.DynamicTypeFormatting, handler3, source);
                    if (ReferenceEquals(dynamicTypeFormatting, source))
                    {
                        return;
                    }
                }
            }
        }

        protected TypeModel()
        {
        }

        public bool CanSerialize(Type type) => 
            this.CanSerialize(type, true, true, true);

        private bool CanSerialize(Type type, bool allowBasic, bool allowContract, bool allowLists)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Type underlyingType = Helpers.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            if (Helpers.GetTypeCode(type) > ProtoTypeCode.Unknown)
            {
                return allowBasic;
            }
            if (this.GetKey(ref type) >= 0)
            {
                return allowContract;
            }
            if (!allowLists)
            {
                return false;
            }
            Type listItemType = null;
            if (!type.IsArray)
            {
                listItemType = GetListItemType(this, type);
            }
            else if (type.GetArrayRank() == 1)
            {
                listItemType = type.GetElementType();
            }
            return ((listItemType != null) && this.CanSerialize(listItemType, allowBasic, allowContract, false));
        }

        public bool CanSerializeBasicType(Type type) => 
            this.CanSerialize(type, true, false, true);

        public bool CanSerializeContractType(Type type) => 
            this.CanSerialize(type, false, true, true);

        private static bool CheckDictionaryAccessors(TypeModel model, Type pair, Type value) => 
            (pair.IsGenericType && ((pair.GetGenericTypeDefinition() == model.MapType(typeof(KeyValuePair<,>))) && (pair.GetGenericArguments()[1] == value)));

        public static RuntimeTypeModel Create() => 
            new RuntimeTypeModel(false);

        public IFormatter CreateFormatter(Type type) => 
            new Formatter(this, type);

        private static object CreateListInstance(Type listType, Type itemType)
        {
            Type type = listType;
            if (listType.IsArray)
            {
                return Array.CreateInstance(itemType, 0);
            }
            if ((!listType.IsClass || listType.IsAbstract) || (Helpers.GetConstructor(listType, Helpers.EmptyTypes, true) == null))
            {
                string str;
                bool flag = false;
                if ((listType.IsInterface && ((str = listType.FullName) != null)) && (str.IndexOf("Dictionary") >= 0))
                {
                    if (listType.IsGenericType && (listType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                    {
                        Type[] genericArguments = listType.GetGenericArguments();
                        type = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                        flag = true;
                    }
                    if (!flag && (listType == typeof(IDictionary)))
                    {
                        type = typeof(Hashtable);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    Type[] typeArguments = new Type[] { itemType };
                    type = typeof(List<>).MakeGenericType(typeArguments);
                    flag = true;
                }
                if (!flag)
                {
                    type = typeof(ArrayList);
                    flag = true;
                }
            }
            return Activator.CreateInstance(type);
        }

        internal static Exception CreateNestedListsNotSupported() => 
            new NotSupportedException("Nested or jagged lists and arrays are not supported");

        public object DeepClone(object value)
        {
            int num2;
            object obj2;
            if (value == null)
            {
                return null;
            }
            Type type = value.GetType();
            int key = this.GetKey(ref type);
            if ((key >= 0) && !Helpers.IsEnum(type))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (ProtoWriter writer = new ProtoWriter(stream, this, null))
                    {
                        writer.SetRootObject(value);
                        this.Serialize(key, value, writer);
                        writer.Close();
                    }
                    stream.Position = 0L;
                    using (ProtoReader reader = new ProtoReader(stream, this, null))
                    {
                        return this.Deserialize(key, null, reader);
                    }
                }
            }
            if (type == typeof(byte[]))
            {
                byte[] from = (byte[]) value;
                byte[] to = new byte[from.Length];
                Helpers.BlockCopy(from, 0, to, 0, from.Length);
                return to;
            }
            if ((this.GetWireType(Helpers.GetTypeCode(type), DataFormat.Default, ref type, out num2) != WireType.None) && (num2 < 0))
            {
                return value;
            }
            using (MemoryStream stream2 = new MemoryStream())
            {
                using (ProtoWriter writer2 = new ProtoWriter(stream2, this, null))
                {
                    if (!this.TrySerializeAuxiliaryType(writer2, type, DataFormat.Default, 1, value, false))
                    {
                        ThrowUnexpectedType(type);
                    }
                    writer2.Close();
                }
                stream2.Position = 0L;
                using (ProtoReader reader2 = new ProtoReader(stream2, this, null))
                {
                    value = null;
                    this.TryDeserializeAuxiliaryType(reader2, DataFormat.Default, 1, type, ref value, true, false, true, false);
                    obj2 = value;
                }
            }
            return obj2;
        }

        public object Deserialize(ProtoReader source, object value, Type type)
        {
            bool noAutoCreate = this.PrepareDeserialize(value, ref type);
            if (value != null)
            {
                source.SetRootObject(value);
            }
            object obj2 = this.DeserializeCore(source, type, value, noAutoCreate);
            source.CheckFullyConsumed();
            return obj2;
        }

        protected internal abstract object Deserialize(int key, object value, ProtoReader source);
        public object Deserialize(Stream source, object value, Type type) => 
            this.Deserialize(source, value, type, (SerializationContext) null);

        public object Deserialize(Stream source, object value, Type type, SerializationContext context)
        {
            bool noAutoCreate = this.PrepareDeserialize(value, ref type);
            using (ProtoReader reader = new ProtoReader(source, this, context))
            {
                if (value != null)
                {
                    reader.SetRootObject(value);
                }
                object obj2 = this.DeserializeCore(reader, type, value, noAutoCreate);
                reader.CheckFullyConsumed();
                return obj2;
            }
        }

        public object Deserialize(Stream source, object value, Type type, int length) => 
            this.Deserialize(source, value, type, length, null);

        public object Deserialize(Stream source, object value, Type type, int length, SerializationContext context)
        {
            bool noAutoCreate = this.PrepareDeserialize(value, ref type);
            using (ProtoReader reader = new ProtoReader(source, this, context, length))
            {
                if (value != null)
                {
                    reader.SetRootObject(value);
                }
                object obj2 = this.DeserializeCore(reader, type, value, noAutoCreate);
                reader.CheckFullyConsumed();
                return obj2;
            }
        }

        private object DeserializeCore(ProtoReader reader, Type type, object value, bool noAutoCreate)
        {
            int key = this.GetKey(ref type);
            if ((key >= 0) && !Helpers.IsEnum(type))
            {
                return this.Deserialize(key, value, reader);
            }
            this.TryDeserializeAuxiliaryType(reader, DataFormat.Default, 1, type, ref value, true, false, noAutoCreate, false);
            return value;
        }

        public IEnumerable<T> DeserializeItems<T>(Stream source, PrefixStyle style, int expectedField) => 
            this.DeserializeItems<T>(source, style, expectedField, null);

        public IEnumerable<T> DeserializeItems<T>(Stream source, PrefixStyle style, int expectedField, SerializationContext context) => 
            new DeserializeItemsIterator<T>(this, source, style, expectedField, context);

        public IEnumerable DeserializeItems(Stream source, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver) => 
            this.DeserializeItems(source, type, style, expectedField, resolver, null);

        public IEnumerable DeserializeItems(Stream source, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, SerializationContext context) => 
            new DeserializeItemsIterator(this, source, type, style, expectedField, resolver, context);

        internal static Type DeserializeType(TypeModel model, string value)
        {
            TypeFormatEventHandler handler;
            if ((model != null) && ((handler = model.DynamicTypeFormatting) != null))
            {
                TypeFormatEventArgs args = new TypeFormatEventArgs(value);
                handler(model, args);
                if (args.Type != null)
                {
                    return args.Type;
                }
            }
            return Type.GetType(value);
        }

        public object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int fieldNumber)
        {
            int num;
            return this.DeserializeWithLengthPrefix(source, value, type, style, fieldNumber, null, out num);
        }

        public object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver)
        {
            int num;
            return this.DeserializeWithLengthPrefix(source, value, type, style, expectedField, resolver, out num);
        }

        public object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, out int bytesRead)
        {
            bool flag;
            return this.DeserializeWithLengthPrefix(source, value, type, style, expectedField, resolver, out bytesRead, out flag, null);
        }

        private object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, out int bytesRead, out bool haveObject, SerializationContext context)
        {
            haveObject = false;
            bytesRead = 0;
            if ((type == null) && ((style != PrefixStyle.Base128) || (resolver == null)))
            {
                throw new InvalidOperationException("A type must be provided unless base-128 prefixing is being used in combination with a resolver");
            }
            while (true)
            {
                bool flag;
                int num2;
                int num3;
                bool expectHeader = (expectedField > 0) || (resolver != null);
                int count = ProtoReader.ReadLengthPrefix(source, expectHeader, style, out num3, out num2);
                if (num2 == 0)
                {
                    return value;
                }
                bytesRead += num2;
                if (count < 0)
                {
                    return value;
                }
                if (style != PrefixStyle.Base128)
                {
                    flag = false;
                }
                else if ((!expectHeader || ((expectedField != 0) || (type != null))) || (resolver == null))
                {
                    flag = expectedField != num3;
                }
                else
                {
                    type = resolver(num3);
                    flag = type == null;
                }
                if (flag)
                {
                    if (count == 0x7fffffff)
                    {
                        throw new InvalidOperationException();
                    }
                    ProtoReader.Seek(source, count, null);
                    bytesRead += count;
                }
                if (!flag)
                {
                    using (ProtoReader reader = new ProtoReader(source, this, context, count))
                    {
                        int key = this.GetKey(ref type);
                        if (key >= 0)
                        {
                            object obj1 = this.Deserialize(key, value, reader);
                            value = obj1;
                        }
                        else if (!this.TryDeserializeAuxiliaryType(reader, DataFormat.Default, 1, type, ref value, true, false, true, false) && (count != 0))
                        {
                            ThrowUnexpectedType(type);
                        }
                        bytesRead += reader.Position;
                        haveObject = true;
                        return value;
                    }
                }
            }
        }

        protected internal int GetKey(ref Type type)
        {
            int keyImpl = this.GetKeyImpl(type);
            if (keyImpl < 0)
            {
                Type type2 = ResolveProxies(type);
                if (type2 != null)
                {
                    type = type2;
                    keyImpl = this.GetKeyImpl(type);
                }
            }
            return keyImpl;
        }

        protected abstract int GetKeyImpl(Type type);
        internal static Type GetListItemType(TypeModel model, Type listType)
        {
            if (((listType != model.MapType(typeof(string))) && !listType.IsArray) && model.MapType(typeof(IEnumerable)).IsAssignableFrom(listType))
            {
                BasicList list = new BasicList();
                foreach (MethodInfo info in listType.GetMethods())
                {
                    if (!info.IsStatic && (info.Name == "Add"))
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if ((parameters.Length == 1) && !list.Contains(parameters[0].ParameterType))
                        {
                            list.Add(parameters[0].ParameterType);
                        }
                    }
                }
                string name = listType.Name;
                if ((name == null) || (!name.Contains("Queue") && !name.Contains("Stack")))
                {
                    foreach (Type type in listType.GetInterfaces())
                    {
                        if (type.IsGenericType)
                        {
                            Type genericTypeDefinition = type.GetGenericTypeDefinition();
                            if ((genericTypeDefinition == model.MapType(typeof(ICollection<>))) || (genericTypeDefinition.FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1"))
                            {
                                Type[] genericArguments = type.GetGenericArguments();
                                if (!list.Contains(genericArguments[0]))
                                {
                                    list.Add(genericArguments[0]);
                                }
                            }
                        }
                    }
                }
                foreach (PropertyInfo info2 in listType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    if ((info2.Name == "Item") && !list.Contains(info2.PropertyType))
                    {
                        ParameterInfo[] indexParameters = info2.GetIndexParameters();
                        if ((indexParameters.Length == 1) && (indexParameters[0].ParameterType == model.MapType(typeof(int))))
                        {
                            list.Add(info2.PropertyType);
                        }
                    }
                }
                switch (list.Count)
                {
                    case 0:
                        return null;

                    case 1:
                        return (Type) list[0];

                    case 2:
                        if (CheckDictionaryAccessors(model, (Type) list[0], (Type) list[1]))
                        {
                            return (Type) list[0];
                        }
                        if (!CheckDictionaryAccessors(model, (Type) list[1], (Type) list[0]))
                        {
                            break;
                        }
                        return (Type) list[1];

                    default:
                        break;
                }
            }
            return null;
        }

        public virtual string GetSchema(Type type)
        {
            throw new NotSupportedException();
        }

        internal virtual Type GetType(string fullName, Assembly context) => 
            ResolveKnownType(fullName, this, context);

        private WireType GetWireType(ProtoTypeCode code, DataFormat format, ref Type type, out int modelKey)
        {
            int num;
            modelKey = -1;
            if (Helpers.IsEnum(type))
            {
                modelKey = this.GetKey(ref type);
                return WireType.Variant;
            }
            switch (code)
            {
                case ProtoTypeCode.Boolean:
                case ProtoTypeCode.Char:
                case ProtoTypeCode.SByte:
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.UInt16:
                case ProtoTypeCode.Int32:
                case ProtoTypeCode.UInt32:
                    return ((format == DataFormat.FixedSize) ? WireType.Fixed32 : WireType.Variant);

                case ProtoTypeCode.Int64:
                case ProtoTypeCode.UInt64:
                    return ((format == DataFormat.FixedSize) ? WireType.Fixed64 : WireType.Variant);

                case ProtoTypeCode.Single:
                    return WireType.Fixed32;

                case ProtoTypeCode.Double:
                    return WireType.Fixed64;

                case ProtoTypeCode.Decimal:
                case ProtoTypeCode.DateTime:
                case ProtoTypeCode.String:
                    break;

                case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                    goto TR_0001;

                default:
                    if ((code - ProtoTypeCode.TimeSpan) <= ProtoTypeCode.Boolean)
                    {
                        break;
                    }
                    goto TR_0001;
            }
            return WireType.String;
        TR_0001:
            modelKey = num = this.GetKey(ref type);
            return ((num < 0) ? WireType.None : WireType.String);
        }

        public bool IsDefined(Type type) => 
            (this.GetKey(ref type) >= 0);

        protected internal Type MapType(Type type) => 
            this.MapType(type, true);

        protected internal virtual Type MapType(Type type, bool demand) => 
            type;

        private bool PrepareDeserialize(object value, ref Type type)
        {
            if (type == null)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("type");
                }
                type = this.MapType(value.GetType());
            }
            bool flag = true;
            Type underlyingType = Helpers.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
                flag = false;
            }
            return flag;
        }

        internal static Type ResolveKnownType(string name, TypeModel model, Assembly assembly)
        {
            Type type2;
            if (Helpers.IsNullOrEmpty(name))
            {
                return null;
            }
            try
            {
                Type type = Type.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }
            catch
            {
            }
            try
            {
                int index = name.IndexOf(',');
                string str = ((index > 0) ? name.Substring(0, index) : name).Trim();
                if (assembly == null)
                {
                    assembly = Assembly.GetCallingAssembly();
                }
                Type type3 = assembly?.GetType(str);
                if (type3 == null)
                {
                    goto TR_0001;
                }
                else
                {
                    type2 = type3;
                }
            }
            catch
            {
                goto TR_0001;
            }
            return type2;
        TR_0001:
            return null;
        }

        internal static MethodInfo ResolveListAdd(TypeModel model, Type listType, Type itemType, out bool isList)
        {
            Type c = listType;
            isList = model.MapType(ilist).IsAssignableFrom(c);
            Type[] types = new Type[] { itemType };
            MethodInfo info = Helpers.GetInstanceMethod(c, "Add", types);
            if (info == null)
            {
                Type declaringType = model.MapType(typeof(ICollection<>)).MakeGenericType(types);
                if (declaringType.IsAssignableFrom(c))
                {
                    info = Helpers.GetInstanceMethod(declaringType, "Add", types);
                }
            }
            if (info == null)
            {
                foreach (Type type3 in c.GetInterfaces())
                {
                    if (((type3.Name == "IProducerConsumerCollection`1") && type3.IsGenericType) && (type3.GetGenericTypeDefinition().FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1"))
                    {
                        info = Helpers.GetInstanceMethod(type3, "TryAdd", types);
                        if (info != null)
                        {
                            break;
                        }
                    }
                }
            }
            if (info == null)
            {
                types[0] = model.MapType(typeof(object));
                info = Helpers.GetInstanceMethod(c, "Add", types);
            }
            if ((info == null) & isList)
            {
                info = Helpers.GetInstanceMethod(model.MapType(ilist), "Add", types);
            }
            return info;
        }

        protected internal static Type ResolveProxies(Type type)
        {
            if (type != null)
            {
                if (type.IsGenericParameter)
                {
                    return null;
                }
                Type underlyingType = Helpers.GetUnderlyingType(type);
                if (underlyingType != null)
                {
                    return underlyingType;
                }
                string fullName = type.FullName;
                if ((fullName != null) && fullName.StartsWith("System.Data.Entity.DynamicProxies."))
                {
                    return type.BaseType;
                }
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    string str2 = interfaces[i].FullName;
                    if (((str2 == "NHibernate.Proxy.INHibernateProxy") || (str2 == "NHibernate.Proxy.DynamicProxy.IProxy")) || (str2 == "NHibernate.Intercept.IFieldInterceptorAccessor"))
                    {
                        return type.BaseType;
                    }
                }
            }
            return null;
        }

        public void Serialize(ProtoWriter dest, object value)
        {
            dest.CheckDepthFlushlock();
            dest.SetRootObject(value);
            this.SerializeCore(dest, value);
            dest.CheckDepthFlushlock();
            ProtoWriter.Flush(dest);
        }

        public void Serialize(Stream dest, object value)
        {
            this.Serialize(dest, value, null);
        }

        protected internal abstract void Serialize(int key, object value, ProtoWriter dest);
        public void Serialize(Stream dest, object value, SerializationContext context)
        {
            using (ProtoWriter writer = new ProtoWriter(dest, this, context))
            {
                writer.SetRootObject(value);
                this.SerializeCore(writer, value);
                writer.Close();
            }
        }

        private void SerializeCore(ProtoWriter writer, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type type = value.GetType();
            int key = this.GetKey(ref type);
            if (key >= 0)
            {
                this.Serialize(key, value, writer);
            }
            else if (!this.TrySerializeAuxiliaryType(writer, type, DataFormat.Default, 1, value, false))
            {
                ThrowUnexpectedType(type);
            }
        }

        internal static string SerializeType(TypeModel model, Type type)
        {
            TypeFormatEventHandler handler;
            if ((model != null) && ((handler = model.DynamicTypeFormatting) != null))
            {
                TypeFormatEventArgs args = new TypeFormatEventArgs(type);
                handler(model, args);
                if (!Helpers.IsNullOrEmpty(args.FormattedName))
                {
                    return args.FormattedName;
                }
            }
            return type.AssemblyQualifiedName;
        }

        public void SerializeWithLengthPrefix(Stream dest, object value, Type type, PrefixStyle style, int fieldNumber)
        {
            this.SerializeWithLengthPrefix(dest, value, type, style, fieldNumber, null);
        }

        public void SerializeWithLengthPrefix(Stream dest, object value, Type type, PrefixStyle style, int fieldNumber, SerializationContext context)
        {
            if (type == null)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                type = this.MapType(value.GetType());
            }
            int key = this.GetKey(ref type);
            using (ProtoWriter writer = new ProtoWriter(dest, this, context))
            {
                if (style == PrefixStyle.None)
                {
                    this.Serialize(key, value, writer);
                }
                else
                {
                    if ((style - 1) > PrefixStyle.Fixed32)
                    {
                        throw new ArgumentOutOfRangeException("style");
                    }
                    ProtoWriter.WriteObject(value, key, writer, style, fieldNumber);
                }
                writer.Close();
            }
        }

        public static void ThrowCannotCreateInstance(Type type)
        {
            throw new ProtoException("No parameterless constructor found for " + type.Name);
        }

        protected internal static void ThrowUnexpectedSubtype(Type expected, Type actual)
        {
            if (expected != ResolveProxies(actual))
            {
                throw new InvalidOperationException("Unexpected sub-type: " + actual.FullName);
            }
        }

        protected internal static void ThrowUnexpectedType(Type type)
        {
            string str = (type == null) ? "(unknown)" : type.FullName;
            if (type != null)
            {
                Type baseType = type.BaseType;
                if (((baseType != null) && baseType.IsGenericType) && (baseType.GetGenericTypeDefinition().Name == "GeneratedMessage`2"))
                {
                    throw new InvalidOperationException("Are you mixing protobuf-net and protobuf-csharp-port? See http://stackoverflow.com/q/11564914; type: " + str);
                }
            }
            throw new InvalidOperationException("Type is not expected, and no contract can be inferred: " + str);
        }

        internal bool TryDeserializeAuxiliaryType(ProtoReader reader, DataFormat format, int tag, Type type, ref object value, bool skipOtherFields, bool asListItem, bool autoCreate, bool insideList)
        {
            int num;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Type itemType = null;
            ProtoTypeCode typeCode = Helpers.GetTypeCode(type);
            WireType wireType = this.GetWireType(typeCode, format, ref type, out num);
            bool flag = false;
            if (wireType == WireType.None)
            {
                itemType = GetListItemType(this, type);
                if (((itemType == null) && (type.IsArray && (type.GetArrayRank() == 1))) && (type != typeof(byte[])))
                {
                    itemType = type.GetElementType();
                }
                if (itemType != null)
                {
                    if (insideList)
                    {
                        throw CreateNestedListsNotSupported();
                    }
                    flag = this.TryDeserializeList(this, reader, format, tag, type, itemType, ref value);
                    if (!flag & autoCreate)
                    {
                        value = CreateListInstance(type, itemType);
                    }
                    return flag;
                }
                ThrowUnexpectedType(type);
            }
            while (true)
            {
                if (!(flag & asListItem))
                {
                    int num2 = reader.ReadFieldHeader();
                    if (num2 > 0)
                    {
                        if (num2 != tag)
                        {
                            if (!skipOtherFields)
                            {
                                object[] objArray1 = new object[] { "Expected field ", tag, ", but found ", num2 };
                                throw ProtoReader.AddErrorData(new InvalidOperationException(string.Concat(objArray1)), reader);
                            }
                            reader.SkipField();
                            continue;
                        }
                        flag = true;
                        reader.Hint(wireType);
                        if (num >= 0)
                        {
                            if ((wireType - 2) > WireType.Fixed64)
                            {
                                value = this.Deserialize(num, value, reader);
                                continue;
                            }
                            SubItemToken token = ProtoReader.StartSubItem(reader);
                            value = this.Deserialize(num, value, reader);
                            ProtoReader.EndSubItem(token, reader);
                            continue;
                        }
                        switch (typeCode)
                        {
                            case ProtoTypeCode.Boolean:
                            {
                                value = reader.ReadBoolean();
                                continue;
                            }
                            case ProtoTypeCode.Char:
                            {
                                value = (char) reader.ReadUInt16();
                                continue;
                            }
                            case ProtoTypeCode.SByte:
                            {
                                value = reader.ReadSByte();
                                continue;
                            }
                            case ProtoTypeCode.Byte:
                            {
                                value = reader.ReadByte();
                                continue;
                            }
                            case ProtoTypeCode.Int16:
                            {
                                value = reader.ReadInt16();
                                continue;
                            }
                            case ProtoTypeCode.UInt16:
                            {
                                value = reader.ReadUInt16();
                                continue;
                            }
                            case ProtoTypeCode.Int32:
                            {
                                value = reader.ReadInt32();
                                continue;
                            }
                            case ProtoTypeCode.UInt32:
                            {
                                value = reader.ReadUInt32();
                                continue;
                            }
                            case ProtoTypeCode.Int64:
                            {
                                value = reader.ReadInt64();
                                continue;
                            }
                            case ProtoTypeCode.UInt64:
                            {
                                value = reader.ReadUInt64();
                                continue;
                            }
                            case ProtoTypeCode.Single:
                            {
                                value = reader.ReadSingle();
                                continue;
                            }
                            case ProtoTypeCode.Double:
                            {
                                value = reader.ReadDouble();
                                continue;
                            }
                            case ProtoTypeCode.Decimal:
                            {
                                value = BclHelpers.ReadDecimal(reader);
                                continue;
                            }
                            case ProtoTypeCode.DateTime:
                            {
                                value = BclHelpers.ReadDateTime(reader);
                                continue;
                            }
                            case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                            {
                                continue;
                            }
                            case ProtoTypeCode.String:
                            {
                                value = reader.ReadString();
                                continue;
                            }
                        }
                        switch (typeCode)
                        {
                            case ProtoTypeCode.TimeSpan:
                            {
                                value = BclHelpers.ReadTimeSpan(reader);
                                continue;
                            }
                            case ProtoTypeCode.ByteArray:
                            {
                                value = ProtoReader.AppendBytes((byte[]) value, reader);
                                continue;
                            }
                            case ProtoTypeCode.Guid:
                            {
                                value = BclHelpers.ReadGuid(reader);
                                continue;
                            }
                            case ProtoTypeCode.Uri:
                            {
                                value = new Uri(reader.ReadString());
                                continue;
                            }
                        }
                        continue;
                    }
                }
                if (((!flag && !asListItem) & autoCreate) && (type != typeof(string)))
                {
                    value = Activator.CreateInstance(type);
                }
                return flag;
            }
        }

        private bool TryDeserializeList(TypeModel model, ProtoReader reader, DataFormat format, int tag, Type listType, Type itemType, ref object value)
        {
            bool flag;
            MethodInfo info = ResolveListAdd(model, listType, itemType, out flag);
            if (info == null)
            {
                throw new NotSupportedException("Unknown list variant: " + listType.FullName);
            }
            bool flag2 = false;
            object obj2 = null;
            IList list = value as IList;
            object[] parameters = flag ? null : new object[1];
            BasicList list2 = listType.IsArray ? new BasicList() : null;
            while (this.TryDeserializeAuxiliaryType(reader, format, tag, itemType, ref obj2, true, true, true, true))
            {
                flag2 = true;
                if ((value == null) && (list2 == null))
                {
                    value = CreateListInstance(listType, itemType);
                    list = value as IList;
                }
                if (list != null)
                {
                    list.Add(obj2);
                }
                else if (list2 != null)
                {
                    list2.Add(obj2);
                }
                else
                {
                    parameters[0] = obj2;
                    info.Invoke(value, parameters);
                }
                obj2 = null;
            }
            if (list2 != null)
            {
                Array array;
                if (value == null)
                {
                    array = Array.CreateInstance(itemType, list2.Count);
                    list2.CopyTo(array, 0);
                    value = array;
                }
                else if (list2.Count != 0)
                {
                    Array sourceArray = (Array) value;
                    array = Array.CreateInstance(itemType, (int) (sourceArray.Length + list2.Count));
                    Array.Copy(sourceArray, array, sourceArray.Length);
                    list2.CopyTo(array, sourceArray.Length);
                    value = array;
                }
            }
            return flag2;
        }

        internal bool TrySerializeAuxiliaryType(ProtoWriter writer, Type type, DataFormat format, int tag, object value, bool isInsideList)
        {
            int num;
            if (type == null)
            {
                type = value.GetType();
            }
            ProtoTypeCode typeCode = Helpers.GetTypeCode(type);
            WireType wireType = this.GetWireType(typeCode, format, ref type, out num);
            if (num >= 0)
            {
                if (Helpers.IsEnum(type))
                {
                    this.Serialize(num, value, writer);
                    return true;
                }
                ProtoWriter.WriteFieldHeader(tag, wireType, writer);
                if (wireType == WireType.None)
                {
                    throw ProtoWriter.CreateException(writer);
                }
                if ((wireType - 2) > WireType.Fixed64)
                {
                    this.Serialize(num, value, writer);
                    return true;
                }
                SubItemToken token = ProtoWriter.StartSubItem(value, writer);
                this.Serialize(num, value, writer);
                ProtoWriter.EndSubItem(token, writer);
                return true;
            }
            if (wireType != WireType.None)
            {
                ProtoWriter.WriteFieldHeader(tag, wireType, writer);
            }
            switch (typeCode)
            {
                case ProtoTypeCode.Boolean:
                    ProtoWriter.WriteBoolean((bool) value, writer);
                    return true;

                case ProtoTypeCode.Char:
                    ProtoWriter.WriteUInt16((char) value, writer);
                    return true;

                case ProtoTypeCode.SByte:
                    ProtoWriter.WriteSByte((sbyte) value, writer);
                    return true;

                case ProtoTypeCode.Byte:
                    ProtoWriter.WriteByte((byte) value, writer);
                    return true;

                case ProtoTypeCode.Int16:
                    ProtoWriter.WriteInt16((short) value, writer);
                    return true;

                case ProtoTypeCode.UInt16:
                    ProtoWriter.WriteUInt16((ushort) value, writer);
                    return true;

                case ProtoTypeCode.Int32:
                    ProtoWriter.WriteInt32((int) value, writer);
                    return true;

                case ProtoTypeCode.UInt32:
                    ProtoWriter.WriteUInt32((uint) value, writer);
                    return true;

                case ProtoTypeCode.Int64:
                    ProtoWriter.WriteInt64((long) value, writer);
                    return true;

                case ProtoTypeCode.UInt64:
                    ProtoWriter.WriteUInt64((ulong) value, writer);
                    return true;

                case ProtoTypeCode.Single:
                    ProtoWriter.WriteSingle((float) value, writer);
                    return true;

                case ProtoTypeCode.Double:
                    ProtoWriter.WriteDouble((double) value, writer);
                    return true;

                case ProtoTypeCode.Decimal:
                    BclHelpers.WriteDecimal((decimal) value, writer);
                    return true;

                case ProtoTypeCode.DateTime:
                    BclHelpers.WriteDateTime((DateTime) value, writer);
                    return true;

                case (ProtoTypeCode.DateTime | ProtoTypeCode.Unknown):
                    break;

                case ProtoTypeCode.String:
                    ProtoWriter.WriteString((string) value, writer);
                    return true;

                default:
                    switch (typeCode)
                    {
                        case ProtoTypeCode.TimeSpan:
                            BclHelpers.WriteTimeSpan((TimeSpan) value, writer);
                            return true;

                        case ProtoTypeCode.ByteArray:
                            ProtoWriter.WriteBytes((byte[]) value, writer);
                            return true;

                        case ProtoTypeCode.Guid:
                            BclHelpers.WriteGuid((Guid) value, writer);
                            return true;

                        case ProtoTypeCode.Uri:
                            ProtoWriter.WriteString(((Uri) value).AbsoluteUri, writer);
                            return true;

                        default:
                            break;
                    }
                    break;
            }
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                return false;
            }
            if (isInsideList)
            {
                throw CreateNestedListsNotSupported();
            }
            foreach (object obj2 in enumerable)
            {
                if (obj2 == null)
                {
                    throw new NullReferenceException();
                }
                if (!this.TrySerializeAuxiliaryType(writer, null, format, tag, obj2, true))
                {
                    ThrowUnexpectedType(obj2.GetType());
                }
            }
            return true;
        }

        internal protected enum CallbackType
        {
            BeforeSerialize,
            AfterSerialize,
            BeforeDeserialize,
            AfterDeserialize
        }

        private class DeserializeItemsIterator : IEnumerator, IEnumerable
        {
            private bool haveObject = true;
            private object current;
            private readonly Stream source;
            private readonly Type type;
            private readonly PrefixStyle style;
            private readonly int expectedField;
            private readonly Serializer.TypeResolver resolver;
            private readonly TypeModel model;
            private readonly SerializationContext context;

            public DeserializeItemsIterator(TypeModel model, Stream source, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, SerializationContext context)
            {
                this.source = source;
                this.type = type;
                this.style = style;
                this.expectedField = expectedField;
                this.resolver = resolver;
                this.model = model;
                this.context = context;
            }

            public bool MoveNext()
            {
                if (this.haveObject)
                {
                    int num;
                    this.current = this.model.DeserializeWithLengthPrefix(this.source, null, this.type, this.style, this.expectedField, this.resolver, out num, out this.haveObject, this.context);
                }
                return this.haveObject;
            }

            IEnumerator IEnumerable.GetEnumerator() => 
                this;

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            public object Current =>
                this.current;
        }

        private class DeserializeItemsIterator<T> : TypeModel.DeserializeItemsIterator, IEnumerator<T>, IDisposable, IEnumerator, IEnumerable<T>, IEnumerable
        {
            public DeserializeItemsIterator(TypeModel model, Stream source, PrefixStyle style, int expectedField, SerializationContext context) : base(model, source, model.MapType(typeof(T)), style, expectedField, null, context)
            {
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
                this;

            void IDisposable.Dispose()
            {
            }

            public T Current =>
                ((T) base.Current);
        }

        internal sealed class Formatter : IFormatter
        {
            private readonly TypeModel model;
            private readonly Type type;
            private SerializationBinder binder;
            private StreamingContext context;
            private ISurrogateSelector surrogateSelector;

            internal Formatter(TypeModel model, Type type)
            {
                if (model == null)
                {
                    throw new ArgumentNullException("model");
                }
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }
                this.model = model;
                this.type = type;
            }

            public object Deserialize(Stream source) => 
                this.model.Deserialize(source, null, this.type, -1, this.Context);

            public void Serialize(Stream destination, object graph)
            {
                this.model.Serialize(destination, graph, this.Context);
            }

            public SerializationBinder Binder
            {
                get => 
                    this.binder;
                set => 
                    (this.binder = value);
            }

            public StreamingContext Context
            {
                get => 
                    this.context;
                set => 
                    (this.context = value);
            }

            public ISurrogateSelector SurrogateSelector
            {
                get => 
                    this.surrogateSelector;
                set => 
                    (this.surrogateSelector = value);
            }
        }
    }
}

