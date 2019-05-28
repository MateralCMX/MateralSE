namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    public static class Serializer
    {
        private const string ProtoBinaryField = "proto";
        public const int ListItemTag = 1;

        public static TTo ChangeType<TFrom, TTo>(TFrom instance)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize<TFrom>(stream, instance);
                stream.Position = 0L;
                return Deserialize<TTo>(stream);
            }
        }

        public static IFormatter CreateFormatter<T>() => 
            RuntimeTypeModel.Default.CreateFormatter(typeof(T));

        public static T DeepClone<T>(T instance) => 
            ((instance == null) ? instance : ((T) RuntimeTypeModel.Default.DeepClone(instance)));

        public static T Deserialize<T>(Stream source) => 
            ((T) RuntimeTypeModel.Default.Deserialize(source, null, typeof(T)));

        public static IEnumerable<T> DeserializeItems<T>(Stream source, PrefixStyle style, int fieldNumber) => 
            RuntimeTypeModel.Default.DeserializeItems<T>(source, style, fieldNumber);

        public static T DeserializeWithLengthPrefix<T>(Stream source, PrefixStyle style) => 
            DeserializeWithLengthPrefix<T>(source, style, 0);

        public static T DeserializeWithLengthPrefix<T>(Stream source, PrefixStyle style, int fieldNumber)
        {
            RuntimeTypeModel model = RuntimeTypeModel.Default;
            return (T) model.DeserializeWithLengthPrefix(source, null, model.MapType(typeof(T)), style, fieldNumber);
        }

        public static void FlushPool()
        {
            BufferPool.Flush();
        }

        public static string GetProto<T>() => 
            RuntimeTypeModel.Default.GetSchema(RuntimeTypeModel.Default.MapType(typeof(T)));

        public static T Merge<T>(Stream source, T instance) => 
            ((T) RuntimeTypeModel.Default.Deserialize(source, instance, typeof(T)));

        public static void Merge<T>(SerializationInfo info, T instance) where T: class, ISerializable
        {
            Merge<T>(info, new StreamingContext(StreamingContextStates.Persistence), instance);
        }

        public static void Merge<T>(XmlReader reader, T instance) where T: IXmlSerializable
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            byte[] buffer = new byte[0x1000];
            using (MemoryStream stream = new MemoryStream())
            {
                int depth = reader.Depth;
                goto TR_000D;
            TR_0005:
                stream.Position = 0L;
                Merge<T>(stream, instance);
                return;
            TR_000D:
                while (true)
                {
                    if (!reader.Read())
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        if (reader.Depth > depth)
                        {
                            if (reader.NodeType != XmlNodeType.Text)
                            {
                                continue;
                            }
                            do
                            {
                                int count = reader.ReadContentAsBase64(buffer, 0, 0x1000);
                                if (count > 0)
                                {
                                    stream.Write(buffer, 0, count);
                                    continue;
                                }
                            }
                            while (reader.Depth > depth);
                        }
                        goto TR_0005;
                    }
                }
                goto TR_000D;
            }
        }

        public static void Merge<T>(SerializationInfo info, StreamingContext context, T instance) where T: class, ISerializable
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (instance.GetType() != typeof(T))
            {
                throw new ArgumentException("Incorrect type", "instance");
            }
            using (MemoryStream stream = new MemoryStream((byte[]) info.GetValue("proto", typeof(byte[]))))
            {
                if (((T) RuntimeTypeModel.Default.Deserialize(stream, instance, typeof(T), context)) != instance)
                {
                    throw new ProtoException("Deserialization changed the instance; cannot succeed.");
                }
            }
        }

        public static T MergeWithLengthPrefix<T>(Stream source, T instance, PrefixStyle style)
        {
            RuntimeTypeModel model = RuntimeTypeModel.Default;
            return (T) model.DeserializeWithLengthPrefix(source, instance, model.MapType(typeof(T)), style, 0);
        }

        public static void PrepareSerializer<T>()
        {
            RuntimeTypeModel model = RuntimeTypeModel.Default;
            model[model.MapType(typeof(T))].CompileInPlace();
        }

        public static void Serialize<T>(Stream destination, T instance)
        {
            if (instance != null)
            {
                RuntimeTypeModel.Default.Serialize(destination, instance);
            }
        }

        public static void Serialize<T>(SerializationInfo info, T instance) where T: class, ISerializable
        {
            Serialize<T>(info, new StreamingContext(StreamingContextStates.Persistence), instance);
        }

        public static void Serialize<T>(XmlWriter writer, T instance) where T: IXmlSerializable
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize<T>(stream, instance);
                writer.WriteBase64(stream.GetBuffer(), 0, (int) stream.Length);
            }
        }

        public static void Serialize<T>(SerializationInfo info, StreamingContext context, T instance) where T: class, ISerializable
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (instance.GetType() != typeof(T))
            {
                throw new ArgumentException("Incorrect type", "instance");
            }
            using (MemoryStream stream = new MemoryStream())
            {
                RuntimeTypeModel.Default.Serialize(stream, instance, context);
                info.AddValue("proto", stream.ToArray());
            }
        }

        public static void SerializeWithLengthPrefix<T>(Stream destination, T instance, PrefixStyle style)
        {
            SerializeWithLengthPrefix<T>(destination, instance, style, 0);
        }

        public static void SerializeWithLengthPrefix<T>(Stream destination, T instance, PrefixStyle style, int fieldNumber)
        {
            RuntimeTypeModel model = RuntimeTypeModel.Default;
            model.SerializeWithLengthPrefix(destination, instance, model.MapType(typeof(T)), style, fieldNumber);
        }

        public static bool TryReadLengthPrefix(Stream source, PrefixStyle style, out int length)
        {
            int num;
            int num2;
            length = ProtoReader.ReadLengthPrefix(source, false, style, out num, out num2);
            return (num2 > 0);
        }

        public static bool TryReadLengthPrefix(byte[] buffer, int index, int count, PrefixStyle style, out int length)
        {
            using (Stream stream = new MemoryStream(buffer, index, count))
            {
                return TryReadLengthPrefix(stream, style, out length);
            }
        }

        public static class GlobalOptions
        {
            [Obsolete("Please use RuntimeTypeModel.Default.InferTagFromNameDefault instead (or on a per-model basis)", false)]
            public static bool InferTagFromName
            {
                get => 
                    RuntimeTypeModel.Default.InferTagFromNameDefault;
                set => 
                    (RuntimeTypeModel.Default.InferTagFromNameDefault = value);
            }
        }

        public static class NonGeneric
        {
            public static bool CanSerialize(Type type) => 
                RuntimeTypeModel.Default.IsDefined(type);

            public static object DeepClone(object instance) => 
                ((instance == null) ? null : RuntimeTypeModel.Default.DeepClone(instance));

            public static object Deserialize(Type type, Stream source) => 
                RuntimeTypeModel.Default.Deserialize(source, null, type);

            public static object Merge(Stream source, object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }
                return RuntimeTypeModel.Default.Deserialize(source, instance, instance.GetType(), (SerializationContext) null);
            }

            public static void Serialize(Stream dest, object instance)
            {
                if (instance != null)
                {
                    RuntimeTypeModel.Default.Serialize(dest, instance);
                }
            }

            public static void SerializeWithLengthPrefix(Stream destination, object instance, PrefixStyle style, int fieldNumber)
            {
                RuntimeTypeModel model = RuntimeTypeModel.Default;
                model.SerializeWithLengthPrefix(destination, instance, model.MapType(instance.GetType()), style, fieldNumber);
            }

            public static bool TryDeserializeWithLengthPrefix(Stream source, PrefixStyle style, Serializer.TypeResolver resolver, out object value)
            {
                value = RuntimeTypeModel.Default.DeserializeWithLengthPrefix(source, null, null, style, 0, resolver);
                return (value != null);
            }
        }

        public delegate Type TypeResolver(int fieldNumber);
    }
}

