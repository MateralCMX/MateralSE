namespace VRage.ObjectBuilders
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using VRage;
    using VRage.FileSystem;
    using VRage.Library.Collections;
    using VRage.Serialization;
    using VRage.Utils;

    public class MyObjectBuilderSerializer
    {
        public static readonly string ProtobufferExtension = "B3";
        private static readonly bool ENABLE_PROTOBUFFERS_CLONING = true;
        private static MyObjectFactory<MyObjectBuilderDefinitionAttribute, MyObjectBuilder_Base> m_objectFactory;
        public static RuntimeTypeModel Serializer = ProtoBuf.Meta.TypeModel.Create();
        public static readonly MySerializeInfo Dynamic = new MySerializeInfo(MyObjectFlags.Dynamic, MyPrimitiveFlags.None, 0, new DynamicSerializerDelegate(MyObjectBuilderSerializer.SerializeDynamic), null, null);

        static MyObjectBuilderSerializer()
        {
            Serializer.UseImplicitZeroDefaults = false;
            m_objectFactory = new MyObjectFactory<MyObjectBuilderDefinitionAttribute, MyObjectBuilder_Base>();
        }

        public static MyObjectBuilder_Base Clone(MyObjectBuilder_Base toClone)
        {
            MyObjectBuilder_Base objectBuilder = null;
            using (MemoryStream stream = new MemoryStream())
            {
                if (ENABLE_PROTOBUFFERS_CLONING)
                {
                    Serializer.Serialize(stream, toClone);
                    stream.Position = 0L;
                    DeserializePB(stream, out objectBuilder, toClone.GetType());
                }
                else
                {
                    SerializeXMLInternal(stream, toClone, null);
                    stream.Position = 0L;
                    DeserializeXML(stream, out objectBuilder, toClone.GetType());
                }
            }
            return objectBuilder;
        }

        public static T CreateNewObject<T>() where T: MyObjectBuilder_Base, new() => 
            m_objectFactory.CreateInstance<T>();

        public static T CreateNewObject<T>(string subtypeName) where T: MyObjectBuilder_Base, new()
        {
            T local1 = CreateNewObject<T>();
            local1.SubtypeName = subtypeName;
            return local1;
        }

        public static MyObjectBuilder_Base CreateNewObject(MyObjectBuilderType type) => 
            m_objectFactory.CreateInstance(type);

        public static MyObjectBuilder_Base CreateNewObject(SerializableDefinitionId id) => 
            CreateNewObject(id.TypeId, id.SubtypeId);

        public static MyObjectBuilder_Base CreateNewObject(MyObjectBuilderType type, string subtypeName)
        {
            MyObjectBuilder_Base base1 = CreateNewObject(type);
            base1.SubtypeName = subtypeName;
            return base1;
        }

        public static bool DeserializeGZippedXML<T>(Stream reader, out T objectBuilder) where T: MyObjectBuilder_Base
        {
            objectBuilder = default(T);
            try
            {
                using (GZipStream stream = new GZipStream(reader, CompressionMode.Decompress))
                {
                    using (BufferedStream stream2 = new BufferedStream(stream, 0x8000))
                    {
                        XmlSerializer serializer = MyXmlSerializerManager.GetSerializer(typeof(T));
                        objectBuilder = (T) serializer.Deserialize(stream2);
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("ERROR: Exception during objectbuilder read! (xml): " + typeof(T).Name);
                MyLog.Default.WriteLine(exception);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return false;
            }
            return true;
        }

        public static bool DeserializePB<T>(Stream reader, out T objectBuilder) where T: MyObjectBuilder_Base
        {
            MyObjectBuilder_Base base2;
            bool flag1 = DeserializePB(reader, out base2, typeof(T));
            objectBuilder = (T) base2;
            return flag1;
        }

        public static bool DeserializePB<T>(string path, out T objectBuilder) where T: MyObjectBuilder_Base
        {
            ulong num;
            return DeserializePB<T>(path, out objectBuilder, out num);
        }

        internal static bool DeserializePB(Stream reader, out MyObjectBuilder_Base objectBuilder, Type builderType)
        {
            objectBuilder = null;
            try
            {
                objectBuilder = Serializer.Deserialize(reader, null, builderType) as MyObjectBuilder_Base;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("ERROR: Exception during objectbuilder read! (pb): " + builderType.Name);
                MyLog.Default.WriteLine(exception);
                return false;
            }
            return true;
        }

        public static bool DeserializePB<T>(string path, out T objectBuilder, out ulong fileSize) where T: MyObjectBuilder_Base
        {
            bool flag = false;
            fileSize = 0L;
            objectBuilder = default(T);
            using (Stream stream = MyFileSystem.OpenRead(path))
            {
                if (stream != null)
                {
                    using (Stream stream2 = stream.UnwrapGZip())
                    {
                        if (stream2 != null)
                        {
                            fileSize = (ulong) stream.Length;
                            flag = DeserializePB<T>(stream2, out objectBuilder);
                        }
                    }
                }
            }
            if (!flag)
            {
                MyLog.Default.WriteLine($"Failed to deserialize file '{path}'");
            }
            return flag;
        }

        public static bool DeserializeXML<T>(Stream reader, out T objectBuilder) where T: MyObjectBuilder_Base
        {
            MyObjectBuilder_Base base2;
            bool flag1 = DeserializeXML(reader, out base2, typeof(T));
            objectBuilder = (T) base2;
            return flag1;
        }

        public static bool DeserializeXML<T>(string path, out T objectBuilder) where T: MyObjectBuilder_Base
        {
            ulong num;
            return DeserializeXML<T>(path, out objectBuilder, out num);
        }

        public static bool DeserializeXML(Stream reader, out MyObjectBuilder_Base objectBuilder, Type builderType) => 
            DeserializeXML(reader, out objectBuilder, builderType, null);

        public static bool DeserializeXML(string path, out MyObjectBuilder_Base objectBuilder, Type builderType)
        {
            bool flag = false;
            objectBuilder = null;
            using (Stream stream = MyFileSystem.OpenRead(path))
            {
                if (stream != null)
                {
                    using (Stream stream2 = stream.UnwrapGZip())
                    {
                        if (stream2 != null)
                        {
                            flag = DeserializeXML(stream2, out objectBuilder, builderType);
                        }
                    }
                }
            }
            if (!flag)
            {
                MyLog.Default.WriteLine($"Failed to deserialize file '{path}'");
            }
            return flag;
        }

        public static bool DeserializeXML<T>(string path, out T objectBuilder, out ulong fileSize) where T: MyObjectBuilder_Base
        {
            bool flag = false;
            fileSize = 0L;
            objectBuilder = default(T);
            using (Stream stream = MyFileSystem.OpenRead(path))
            {
                if (stream != null)
                {
                    using (Stream stream2 = stream.UnwrapGZip())
                    {
                        if (stream2 != null)
                        {
                            fileSize = (ulong) stream.Length;
                            flag = DeserializeXML<T>(stream2, out objectBuilder);
                        }
                    }
                }
            }
            if (!flag)
            {
                MyLog.Default.WriteLine($"Failed to deserialize file '{path}'");
            }
            return flag;
        }

        internal static bool DeserializeXML(Stream reader, out MyObjectBuilder_Base objectBuilder, Type builderType, Dictionary<string, string> typeOverrideMap)
        {
            objectBuilder = null;
            try
            {
                XmlSerializer serializer = MyXmlSerializerManager.GetSerializer(builderType);
                XmlReaderSettings settings1 = new XmlReaderSettings();
                settings1.CheckCharacters = true;
                XmlReaderSettings settings = settings1;
                MyXmlTextReader xmlReader = new MyXmlTextReader(reader, settings) {
                    DefinitionTypeOverrideMap = typeOverrideMap
                };
                objectBuilder = (MyObjectBuilder_Base) serializer.Deserialize(xmlReader);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("ERROR: Exception during objectbuilder read! (xml): " + builderType.Name);
                MyLog.Default.WriteLine(exception);
                return false;
            }
            return true;
        }

        private static ushort Get16BitHash(string s)
        {
            using (MD5 md = MD5.Create())
            {
                return BitConverter.ToUInt16(md.ComputeHash(Encoding.UTF8.GetBytes(s)), 0);
            }
        }

        public static void LoadSerializers()
        {
            foreach (MyObjectBuilderDefinitionAttribute attribute in m_objectFactory.Attributes)
            {
                int fieldNumber = Get16BitHash(attribute.ProducedType.Name) + 0xffff;
                Serializer.Add(attribute.ProducedType.BaseType, true).AddSubType(fieldNumber, attribute.ProducedType);
            }
        }

        public static void RegisterFromAssembly(Assembly assembly)
        {
            m_objectFactory.RegisterFromAssembly(assembly);
        }

        public static void SerializeDynamic(BitStream stream, Type baseType, ref Type obj)
        {
            if (stream.Reading)
            {
                MyRuntimeObjectBuilderId id = new MyRuntimeObjectBuilderId(stream.ReadUInt16(0x10));
                obj = (Type) ((MyObjectBuilderType) id);
            }
            else
            {
                MyRuntimeObjectBuilderId id2 = (MyRuntimeObjectBuilderId) obj;
                stream.WriteUInt16(id2.Value, 0x10);
            }
        }

        private static void SerializeGZippedXMLInternal(Stream writeTo, MyObjectBuilder_Base objectBuilder, Type serializeAsType = null)
        {
            using (GZipStream stream = new GZipStream(writeTo, CompressionMode.Compress, true))
            {
                using (BufferedStream stream2 = new BufferedStream(stream, 0x8000))
                {
                    SerializeXMLInternal(stream2, objectBuilder, serializeAsType);
                }
            }
        }

        public static bool SerializePB(Stream stream, MyObjectBuilder_Base objectBuilder)
        {
            try
            {
                Serializer.Serialize(stream, objectBuilder);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception.ToString());
                return false;
            }
            return true;
        }

        public static bool SerializePB(string path, bool compress, MyObjectBuilder_Base objectBuilder)
        {
            ulong num;
            return SerializePB(path, compress, objectBuilder, out num);
        }

        public static bool SerializePB(string path, bool compress, MyObjectBuilder_Base objectBuilder, out ulong sizeInBytes)
        {
            try
            {
                using (Stream stream = MyFileSystem.OpenWrite(path, FileMode.Create))
                {
                    using (Stream stream2 = compress ? stream.WrapGZip(true) : stream)
                    {
                        long position = stream.Position;
                        Serializer.Serialize(stream2, objectBuilder);
                        sizeInBytes = (ulong) (stream.Position - position);
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error: " + path + " failed to serialize.");
                MyLog.Default.WriteLine(exception.ToString());
                sizeInBytes = 0L;
                return false;
            }
            return true;
        }

        public static bool SerializeXML(Stream writeTo, MyObjectBuilder_Base objectBuilder, XmlCompression compress = 0, Type serializeAsType = null)
        {
            try
            {
                if (compress == XmlCompression.Gzip)
                {
                    SerializeGZippedXMLInternal(writeTo, objectBuilder, serializeAsType);
                }
                else if (compress == XmlCompression.Uncompressed)
                {
                    SerializeXMLInternal(writeTo, objectBuilder, serializeAsType);
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error during serialization.");
                MyLog.Default.WriteLine(exception.ToString());
                return false;
            }
            return true;
        }

        public static bool SerializeXML(string path, bool compress, MyObjectBuilder_Base objectBuilder, Type serializeAsType = null)
        {
            ulong num;
            return SerializeXML(path, compress, objectBuilder, out num, serializeAsType);
        }

        public static bool SerializeXML(string path, bool compress, MyObjectBuilder_Base objectBuilder, out ulong sizeInBytes, Type serializeAsType = null)
        {
            try
            {
                using (Stream stream = MyFileSystem.OpenWrite(path, FileMode.Create))
                {
                    using (Stream stream2 = compress ? stream.WrapGZip(true) : stream)
                    {
                        long position = stream.Position;
                        MyXmlSerializerManager.GetSerializer(serializeAsType ?? objectBuilder.GetType()).Serialize(stream2, objectBuilder);
                        sizeInBytes = (ulong) (stream.Position - position);
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error: " + path + " failed to serialize.");
                MyLog.Default.WriteLine(exception.ToString());
                sizeInBytes = 0L;
                return false;
            }
            return true;
        }

        private static void SerializeXMLInternal(Stream writeTo, MyObjectBuilder_Base objectBuilder, Type serializeAsType = null)
        {
            MyXmlSerializerManager.GetSerializer(serializeAsType ?? objectBuilder.GetType()).Serialize(writeTo, objectBuilder);
        }

        public static void UnregisterAssembliesAndSerializers()
        {
            m_objectFactory = new MyObjectFactory<MyObjectBuilderDefinitionAttribute, MyObjectBuilder_Base>();
            Serializer = ProtoBuf.Meta.TypeModel.Create();
            Serializer.AutoAddMissingTypes = true;
            Serializer.UseImplicitZeroDefaults = false;
        }

        public enum XmlCompression
        {
            Uncompressed,
            Gzip
        }
    }
}

