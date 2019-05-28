namespace LitJson
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class JsonMapper
    {
        private static int max_nesting_depth = 100;
        private static IFormatProvider datetime_format = DateTimeFormatInfo.InvariantInfo;
        private static IDictionary<Type, ExporterFunc> base_exporters_table = new Dictionary<Type, ExporterFunc>();
        private static IDictionary<Type, ExporterFunc> custom_exporters_table = new Dictionary<Type, ExporterFunc>();
        private static IDictionary<Type, IDictionary<Type, ImporterFunc>> base_importers_table = new Dictionary<Type, IDictionary<Type, ImporterFunc>>();
        private static IDictionary<Type, IDictionary<Type, ImporterFunc>> custom_importers_table = new Dictionary<Type, IDictionary<Type, ImporterFunc>>();
        private static IDictionary<Type, ArrayMetadata> array_metadata = new Dictionary<Type, ArrayMetadata>();
        private static readonly object array_metadata_lock = new object();
        private static IDictionary<Type, IDictionary<Type, MethodInfo>> conv_ops = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
        private static readonly object conv_ops_lock = new object();
        private static IDictionary<Type, ObjectMetadata> object_metadata = new Dictionary<Type, ObjectMetadata>();
        private static readonly object object_metadata_lock = new object();
        private static IDictionary<Type, IList<PropertyMetadata>> type_properties = new Dictionary<Type, IList<PropertyMetadata>>();
        private static readonly object type_properties_lock = new object();
        private static JsonWriter static_writer = new JsonWriter();
        private static readonly object static_writer_lock = new object();

        static JsonMapper()
        {
            RegisterBaseExporters();
            RegisterBaseImporters();
        }

        private static void AddArrayMetadata(Type type)
        {
            if (!array_metadata.ContainsKey(type))
            {
                ArrayMetadata metadata = new ArrayMetadata {
                    IsArray = type.IsArray
                };
                if (type.GetInterface("System.Collections.IList") != null)
                {
                    metadata.IsList = true;
                }
                foreach (PropertyInfo info in type.GetProperties())
                {
                    if (info.Name == "Item")
                    {
                        ParameterInfo[] indexParameters = info.GetIndexParameters();
                        if ((indexParameters.Length == 1) && (indexParameters[0].ParameterType == typeof(int)))
                        {
                            metadata.ElementType = info.PropertyType;
                        }
                    }
                }
                object obj2 = array_metadata_lock;
                lock (obj2)
                {
                    try
                    {
                        array_metadata.Add(type, metadata);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        private static void AddObjectMetadata(Type type)
        {
            if (!object_metadata.ContainsKey(type))
            {
                ObjectMetadata metadata = new ObjectMetadata();
                if (type.GetInterface("System.Collections.IDictionary") != null)
                {
                    metadata.IsDictionary = true;
                }
                metadata.Properties = new Dictionary<string, PropertyMetadata>();
                foreach (PropertyInfo info in type.GetProperties())
                {
                    if (info.Name != "Item")
                    {
                        PropertyMetadata metadata2 = new PropertyMetadata {
                            Info = info,
                            Type = info.PropertyType
                        };
                        metadata.Properties.Add(info.Name, metadata2);
                    }
                    else
                    {
                        ParameterInfo[] indexParameters = info.GetIndexParameters();
                        if ((indexParameters.Length == 1) && (indexParameters[0].ParameterType == typeof(string)))
                        {
                            metadata.ElementType = info.PropertyType;
                        }
                    }
                }
                foreach (FieldInfo info2 in type.GetFields())
                {
                    PropertyMetadata metadata3 = new PropertyMetadata {
                        Info = info2,
                        IsField = true,
                        Type = info2.FieldType
                    };
                    metadata.Properties.Add(info2.Name, metadata3);
                }
                object obj2 = object_metadata_lock;
                lock (obj2)
                {
                    try
                    {
                        object_metadata.Add(type, metadata);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        private static void AddTypeProperties(Type type)
        {
            if (!type_properties.ContainsKey(type))
            {
                IList<PropertyMetadata> list = new List<PropertyMetadata>();
                foreach (PropertyInfo info in type.GetProperties())
                {
                    if (info.Name != "Item")
                    {
                        PropertyMetadata item = new PropertyMetadata {
                            Info = info,
                            IsField = false
                        };
                        list.Add(item);
                    }
                }
                foreach (FieldInfo info2 in type.GetFields())
                {
                    PropertyMetadata item = new PropertyMetadata {
                        Info = info2,
                        IsField = true
                    };
                    list.Add(item);
                }
                object obj2 = type_properties_lock;
                lock (obj2)
                {
                    try
                    {
                        type_properties.Add(type, list);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        private static MethodInfo GetConvOp(Type t1, Type t2)
        {
            object obj2 = conv_ops_lock;
            lock (obj2)
            {
                if (!conv_ops.ContainsKey(t1))
                {
                    conv_ops.Add(t1, new Dictionary<Type, MethodInfo>());
                }
            }
            if (conv_ops[t1].ContainsKey(t2))
            {
                return conv_ops[t1][t2];
            }
            Type[] types = new Type[] { t2 };
            MethodInfo method = t1.GetMethod("op_Implicit", types);
            object obj3 = conv_ops_lock;
            lock (obj3)
            {
                try
                {
                    conv_ops[t1].Add(t2, method);
                }
                catch (ArgumentException)
                {
                    return conv_ops[t1][t2];
                }
            }
            return method;
        }

        private static void ReadSkip(JsonReader reader)
        {
            ToWrapper((WrapperFactory) (() => new JsonMockWrapper()), reader);
        }

        private static IJsonWrapper ReadValue(WrapperFactory factory, JsonReader reader)
        {
            reader.Read();
            if ((reader.Token == JsonToken.ArrayEnd) || (reader.Token == JsonToken.Null))
            {
                return null;
            }
            IJsonWrapper wrapper = factory();
            if (reader.Token == JsonToken.String)
            {
                wrapper.SetString((string) reader.Value);
                return wrapper;
            }
            if (reader.Token == JsonToken.Double)
            {
                wrapper.SetDouble((double) reader.Value);
                return wrapper;
            }
            if (reader.Token == JsonToken.Int)
            {
                wrapper.SetInt((int) reader.Value);
                return wrapper;
            }
            if (reader.Token == JsonToken.Long)
            {
                wrapper.SetLong((long) reader.Value);
                return wrapper;
            }
            if (reader.Token == JsonToken.Boolean)
            {
                wrapper.SetBoolean((bool) reader.Value);
                return wrapper;
            }
            if (reader.Token == JsonToken.ArrayStart)
            {
                wrapper.SetJsonType(JsonType.Array);
                while (true)
                {
                    IJsonWrapper wrapper2 = ReadValue(factory, reader);
                    if ((wrapper2 == null) && (reader.Token == JsonToken.ArrayEnd))
                    {
                        break;
                    }
                    wrapper.Add(wrapper2);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                wrapper.SetJsonType(JsonType.Object);
                while (true)
                {
                    reader.Read();
                    if (reader.Token == JsonToken.ObjectEnd)
                    {
                        break;
                    }
                    string str = (string) reader.Value;
                    wrapper[str] = ReadValue(factory, reader);
                }
            }
            return wrapper;
        }

        private static object ReadValue(Type inst_type, JsonReader reader)
        {
            reader.Read();
            if (reader.Token == JsonToken.ArrayEnd)
            {
                return null;
            }
            if (reader.Token == JsonToken.Null)
            {
                if (!inst_type.IsClass)
                {
                    throw new JsonException($"Can't assign null to an instance of type {inst_type}");
                }
                return null;
            }
            if (((reader.Token == JsonToken.Double) || ((reader.Token == JsonToken.Int) || ((reader.Token == JsonToken.Long) || (reader.Token == JsonToken.String)))) || (reader.Token == JsonToken.Boolean))
            {
                Type c = reader.Value.GetType();
                if (inst_type.IsAssignableFrom(c))
                {
                    return reader.Value;
                }
                if (custom_importers_table.ContainsKey(c) && custom_importers_table[c].ContainsKey(inst_type))
                {
                    return custom_importers_table[c][inst_type](reader.Value);
                }
                if (base_importers_table.ContainsKey(c) && base_importers_table[c].ContainsKey(inst_type))
                {
                    return base_importers_table[c][inst_type](reader.Value);
                }
                if (inst_type.IsEnum)
                {
                    return Enum.ToObject(inst_type, reader.Value);
                }
                MethodInfo convOp = GetConvOp(inst_type, c);
                if (convOp == null)
                {
                    throw new JsonException($"Can't assign value '{reader.Value}' (type {c}) to type {inst_type}");
                }
                object[] parameters = new object[] { reader.Value };
                return convOp.Invoke(null, parameters);
            }
            object obj2 = null;
            if (reader.Token != JsonToken.ArrayStart)
            {
                if (reader.Token == JsonToken.ObjectStart)
                {
                    AddObjectMetadata(inst_type);
                    ObjectMetadata metadata2 = object_metadata[inst_type];
                    obj2 = Activator.CreateInstance(inst_type);
                    while (true)
                    {
                        reader.Read();
                        if (reader.Token == JsonToken.ObjectEnd)
                        {
                            break;
                        }
                        string key = (string) reader.Value;
                        if (!metadata2.Properties.ContainsKey(key))
                        {
                            if (metadata2.IsDictionary)
                            {
                                ((IDictionary) obj2).Add(key, ReadValue(metadata2.ElementType, reader));
                                continue;
                            }
                            if (!reader.SkipNonMembers)
                            {
                                throw new JsonException($"The type {inst_type} doesn't have the property '{key}'");
                            }
                            ReadSkip(reader);
                            continue;
                        }
                        PropertyMetadata metadata3 = metadata2.Properties[key];
                        if (metadata3.IsField)
                        {
                            ((FieldInfo) metadata3.Info).SetValue(obj2, ReadValue(metadata3.Type, reader));
                            continue;
                        }
                        PropertyInfo info = (PropertyInfo) metadata3.Info;
                        if (info.CanWrite)
                        {
                            info.SetValue(obj2, ReadValue(metadata3.Type, reader), null);
                            continue;
                        }
                        ReadValue(metadata3.Type, reader);
                    }
                }
            }
            else
            {
                IList list;
                Type elementType;
                AddArrayMetadata(inst_type);
                ArrayMetadata metadata = array_metadata[inst_type];
                if (!metadata.IsArray && !metadata.IsList)
                {
                    throw new JsonException($"Type {inst_type} can't act as an array");
                }
                if (!metadata.IsArray)
                {
                    list = (IList) Activator.CreateInstance(inst_type);
                    elementType = metadata.ElementType;
                }
                else
                {
                    list = new ArrayList();
                    elementType = inst_type.GetElementType();
                }
                while (true)
                {
                    object obj3 = ReadValue(elementType, reader);
                    if ((obj3 == null) && (reader.Token == JsonToken.ArrayEnd))
                    {
                        if (!metadata.IsArray)
                        {
                            obj2 = list;
                        }
                        else
                        {
                            int count = list.Count;
                            obj2 = Array.CreateInstance(elementType, count);
                            for (int i = 0; i < count; i++)
                            {
                                ((Array) obj2).SetValue(list[i], i);
                            }
                        }
                        break;
                    }
                    list.Add(obj3);
                }
            }
            return obj2;
        }

        private static void RegisterBaseExporters()
        {
            base_exporters_table[typeof(byte)] = (obj, writer) => writer.Write(Convert.ToInt32((byte) obj));
            base_exporters_table[typeof(char)] = (obj, writer) => writer.Write(Convert.ToString((char) obj));
            base_exporters_table[typeof(DateTime)] = (obj, writer) => writer.Write(Convert.ToString((DateTime) obj, datetime_format));
            base_exporters_table[typeof(decimal)] = (obj, writer) => writer.Write((decimal) obj);
            base_exporters_table[typeof(sbyte)] = (obj, writer) => writer.Write(Convert.ToInt32((sbyte) obj));
            base_exporters_table[typeof(short)] = (obj, writer) => writer.Write(Convert.ToInt32((short) obj));
            base_exporters_table[typeof(ushort)] = (obj, writer) => writer.Write(Convert.ToInt32((ushort) obj));
            base_exporters_table[typeof(uint)] = (obj, writer) => writer.Write(Convert.ToUInt64((uint) obj));
            base_exporters_table[typeof(ulong)] = (obj, writer) => writer.Write((ulong) obj);
        }

        private static void RegisterBaseImporters()
        {
            ImporterFunc importer = (ImporterFunc) (input => Convert.ToByte((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(byte), importer);
            importer = (ImporterFunc) (input => Convert.ToUInt64((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(ulong), importer);
            importer = (ImporterFunc) (input => Convert.ToSByte((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(sbyte), importer);
            importer = (ImporterFunc) (input => Convert.ToInt16((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(short), importer);
            importer = (ImporterFunc) (input => Convert.ToUInt16((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(ushort), importer);
            importer = (ImporterFunc) (input => Convert.ToUInt32((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(uint), importer);
            importer = (ImporterFunc) (input => Convert.ToSingle((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(float), importer);
            importer = (ImporterFunc) (input => Convert.ToDouble((int) input));
            RegisterImporter(base_importers_table, typeof(int), typeof(double), importer);
            importer = (ImporterFunc) (input => Convert.ToDecimal((double) input));
            RegisterImporter(base_importers_table, typeof(double), typeof(decimal), importer);
            importer = (ImporterFunc) (input => Convert.ToUInt32((long) input));
            RegisterImporter(base_importers_table, typeof(long), typeof(uint), importer);
            importer = (ImporterFunc) (input => Convert.ToChar((string) input));
            RegisterImporter(base_importers_table, typeof(string), typeof(char), importer);
            importer = (ImporterFunc) (input => Convert.ToDateTime((string) input, datetime_format));
            RegisterImporter(base_importers_table, typeof(string), typeof(DateTime), importer);
        }

        public static void RegisterExporter<T>(ExporterFunc<T> exporter)
        {
            ExporterFunc func = (obj, writer) => exporter((T) obj, writer);
            custom_exporters_table[typeof(T)] = func;
        }

        public static void RegisterImporter<TJson, TValue>(ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc func = (ImporterFunc) (input => importer((TJson) input));
            RegisterImporter(custom_importers_table, typeof(TJson), typeof(TValue), func);
        }

        private static void RegisterImporter(IDictionary<Type, IDictionary<Type, ImporterFunc>> table, Type json_type, Type value_type, ImporterFunc importer)
        {
            if (!table.ContainsKey(json_type))
            {
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());
            }
            table[json_type][value_type] = importer;
        }

        public static string ToJson(object obj)
        {
            object obj2 = static_writer_lock;
            lock (obj2)
            {
                static_writer.Reset();
                WriteValue(obj, static_writer, true, 0);
                return static_writer.ToString();
            }
        }

        public static void ToJson(object obj, JsonWriter writer)
        {
            WriteValue(obj, writer, false, 0);
        }

        public static JsonData ToObject(JsonReader reader) => 
            ((JsonData) ToWrapper((WrapperFactory) (() => new JsonData()), reader));

        public static T ToObject<T>(JsonReader reader) => 
            ((T) ReadValue(typeof(T), reader));

        public static JsonData ToObject(TextReader reader)
        {
            JsonReader reader2 = new JsonReader(reader);
            return (JsonData) ToWrapper((WrapperFactory) (() => new JsonData()), reader2);
        }

        public static T ToObject<T>(TextReader reader)
        {
            JsonReader reader2 = new JsonReader(reader);
            return (T) ReadValue(typeof(T), reader2);
        }

        public static JsonData ToObject(string json) => 
            ((JsonData) ToWrapper((WrapperFactory) (() => new JsonData()), json));

        public static T ToObject<T>(string json)
        {
            JsonReader reader = new JsonReader(json);
            return (T) ReadValue(typeof(T), reader);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory, JsonReader reader) => 
            ReadValue(factory, reader);

        public static IJsonWrapper ToWrapper(WrapperFactory factory, string json)
        {
            JsonReader reader = new JsonReader(json);
            return ReadValue(factory, reader);
        }

        public static void UnregisterExporters()
        {
            custom_exporters_table.Clear();
        }

        public static void UnregisterImporters()
        {
            custom_importers_table.Clear();
        }

        private static void WriteValue(object obj, JsonWriter writer, bool writer_is_private, int depth)
        {
            if (depth > max_nesting_depth)
            {
                throw new JsonException($"Max allowed object depth reached while trying to export from type {obj.GetType()}");
            }
            if (obj == null)
            {
                writer.Write((string) null);
            }
            else if (obj is IJsonWrapper)
            {
                if (writer_is_private)
                {
                    writer.TextWriter.Write(((IJsonWrapper) obj).ToJson());
                }
                else
                {
                    ((IJsonWrapper) obj).ToJson(writer);
                }
            }
            else if (obj is string)
            {
                writer.Write((string) obj);
            }
            else if (obj is double)
            {
                writer.Write((double) obj);
            }
            else if (obj is int)
            {
                writer.Write((int) obj);
            }
            else if (obj as bool)
            {
                writer.Write((bool) obj);
            }
            else
            {
                switch (obj)
                {
                    case (long _):
                        writer.Write((long) obj);
                        break;

                    case (Array _):
                        writer.WriteArrayStart();
                        foreach (object obj2 in (Array) obj)
                        {
                            WriteValue(obj2, writer, writer_is_private, depth + 1);
                        }
                        writer.WriteArrayEnd();
                        break;

                    case (IList _):
                        writer.WriteArrayStart();
                        foreach (object obj3 in (IList) obj)
                        {
                            WriteValue(obj3, writer, writer_is_private, depth + 1);
                        }
                        writer.WriteArrayEnd();
                        break;

                    case (IDictionary _):
                        writer.WriteObjectStart();
                        foreach (DictionaryEntry entry in (IDictionary) obj)
                        {
                            writer.WritePropertyName((string) entry.Key);
                            WriteValue(entry.Value, writer, writer_is_private, depth + 1);
                        }
                        writer.WriteObjectEnd();
                        break;

                    default:
                    {
                        Type key = obj.GetType();
                        if (custom_exporters_table.ContainsKey(key))
                        {
                            custom_exporters_table[key](obj, writer);
                        }
                        else if (base_exporters_table.ContainsKey(key))
                        {
                            base_exporters_table[key](obj, writer);
                        }
                        else if (obj is Enum)
                        {
                            Type underlyingType = Enum.GetUnderlyingType(key);
                            if (((underlyingType == typeof(long)) || (underlyingType == typeof(uint))) || (underlyingType == typeof(ulong)))
                            {
                                writer.Write((ulong) obj);
                            }
                            else
                            {
                                writer.Write((int) obj);
                            }
                        }
                        else
                        {
                            AddTypeProperties(key);
                            IList<PropertyMetadata> list = type_properties[key];
                            writer.WriteObjectStart();
                            foreach (PropertyMetadata metadata in list)
                            {
                                if (metadata.IsField)
                                {
                                    writer.WritePropertyName(metadata.Info.Name);
                                    WriteValue(((FieldInfo) metadata.Info).GetValue(obj), writer, writer_is_private, depth + 1);
                                    continue;
                                }
                                PropertyInfo info = (PropertyInfo) metadata.Info;
                                if (info.CanRead)
                                {
                                    writer.WritePropertyName(metadata.Info.Name);
                                    WriteValue(info.GetValue(obj, null), writer, writer_is_private, depth + 1);
                                }
                            }
                            writer.WriteObjectEnd();
                        }
                        break;
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly JsonMapper.<>c <>9 = new JsonMapper.<>c();
            public static WrapperFactory <>9__23_0;
            public static ExporterFunc <>9__24_0;
            public static ExporterFunc <>9__24_1;
            public static ExporterFunc <>9__24_2;
            public static ExporterFunc <>9__24_3;
            public static ExporterFunc <>9__24_4;
            public static ExporterFunc <>9__24_5;
            public static ExporterFunc <>9__24_6;
            public static ExporterFunc <>9__24_7;
            public static ExporterFunc <>9__24_8;
            public static ImporterFunc <>9__25_0;
            public static ImporterFunc <>9__25_1;
            public static ImporterFunc <>9__25_2;
            public static ImporterFunc <>9__25_3;
            public static ImporterFunc <>9__25_4;
            public static ImporterFunc <>9__25_5;
            public static ImporterFunc <>9__25_6;
            public static ImporterFunc <>9__25_7;
            public static ImporterFunc <>9__25_8;
            public static ImporterFunc <>9__25_9;
            public static ImporterFunc <>9__25_10;
            public static ImporterFunc <>9__25_11;
            public static WrapperFactory <>9__30_0;
            public static WrapperFactory <>9__31_0;
            public static WrapperFactory <>9__32_0;

            internal IJsonWrapper <ReadSkip>b__23_0() => 
                new JsonMockWrapper();

            internal void <RegisterBaseExporters>b__24_0(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToInt32((byte) obj));
            }

            internal void <RegisterBaseExporters>b__24_1(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToString((char) obj));
            }

            internal void <RegisterBaseExporters>b__24_2(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToString((DateTime) obj, JsonMapper.datetime_format));
            }

            internal void <RegisterBaseExporters>b__24_3(object obj, JsonWriter writer)
            {
                writer.Write((decimal) obj);
            }

            internal void <RegisterBaseExporters>b__24_4(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToInt32((sbyte) obj));
            }

            internal void <RegisterBaseExporters>b__24_5(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToInt32((short) obj));
            }

            internal void <RegisterBaseExporters>b__24_6(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToInt32((ushort) obj));
            }

            internal void <RegisterBaseExporters>b__24_7(object obj, JsonWriter writer)
            {
                writer.Write(Convert.ToUInt64((uint) obj));
            }

            internal void <RegisterBaseExporters>b__24_8(object obj, JsonWriter writer)
            {
                writer.Write((ulong) obj);
            }

            internal object <RegisterBaseImporters>b__25_0(object input) => 
                Convert.ToByte((int) input);

            internal object <RegisterBaseImporters>b__25_1(object input) => 
                Convert.ToUInt64((int) input);

            internal object <RegisterBaseImporters>b__25_10(object input) => 
                Convert.ToChar((string) input);

            internal object <RegisterBaseImporters>b__25_11(object input) => 
                Convert.ToDateTime((string) input, JsonMapper.datetime_format);

            internal object <RegisterBaseImporters>b__25_2(object input) => 
                Convert.ToSByte((int) input);

            internal object <RegisterBaseImporters>b__25_3(object input) => 
                Convert.ToInt16((int) input);

            internal object <RegisterBaseImporters>b__25_4(object input) => 
                Convert.ToUInt16((int) input);

            internal object <RegisterBaseImporters>b__25_5(object input) => 
                Convert.ToUInt32((int) input);

            internal object <RegisterBaseImporters>b__25_6(object input) => 
                Convert.ToSingle((int) input);

            internal object <RegisterBaseImporters>b__25_7(object input) => 
                Convert.ToDouble((int) input);

            internal object <RegisterBaseImporters>b__25_8(object input) => 
                Convert.ToDecimal((double) input);

            internal object <RegisterBaseImporters>b__25_9(object input) => 
                Convert.ToUInt32((long) input);

            internal IJsonWrapper <ToObject>b__30_0() => 
                new JsonData();

            internal IJsonWrapper <ToObject>b__31_0() => 
                new JsonData();

            internal IJsonWrapper <ToObject>b__32_0() => 
                new JsonData();
        }
    }
}

