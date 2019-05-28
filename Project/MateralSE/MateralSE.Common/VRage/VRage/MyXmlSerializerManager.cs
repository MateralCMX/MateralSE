namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    public class MyXmlSerializerManager
    {
        private static readonly HashSet<Type> m_serializableBaseTypes = new HashSet<Type>();
        private static readonly Dictionary<Type, XmlSerializer> m_serializersByType = new Dictionary<Type, XmlSerializer>();
        private static readonly Dictionary<string, XmlSerializer> m_serializersBySerializedName = new Dictionary<string, XmlSerializer>();
        private static readonly Dictionary<Type, string> m_serializedNameByType = new Dictionary<Type, string>();
        private static HashSet<Assembly> m_registeredAssemblies = new HashSet<Assembly>();

        public static XmlSerializer GetOrCreateSerializer(Type type)
        {
            XmlSerializer serializer;
            if (!m_serializersByType.TryGetValue(type, out serializer))
            {
                serializer = RegisterType(type, true, true);
            }
            return serializer;
        }

        public static string GetSerializedName(Type type) => 
            m_serializedNameByType[type];

        public static XmlSerializer GetSerializer(string serializedName) => 
            m_serializersBySerializedName[serializedName];

        public static XmlSerializer GetSerializer(Type type) => 
            m_serializersByType[type];

        public static bool IsSerializerAvailable(string name) => 
            m_serializersBySerializedName.ContainsKey(name);

        public static void RegisterFromAssembly(Assembly assembly)
        {
            if ((assembly != null) && !m_registeredAssemblies.Contains(assembly))
            {
                m_registeredAssemblies.Add(assembly);
                foreach (Type type in assembly.GetTypes())
                {
                    try
                    {
                        if (!m_serializersByType.ContainsKey(type))
                        {
                            RegisterType(type, false, true);
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException("Error creating XML serializer for type " + type.Name, exception);
                    }
                }
            }
        }

        public static void RegisterSerializableBaseType(Type type)
        {
            m_serializableBaseTypes.Add(type);
        }

        public static void RegisterSerializer(Type type)
        {
            if (!m_serializersByType.ContainsKey(type))
            {
                RegisterType(type, true, false);
            }
        }

        private static XmlSerializer RegisterType(Type type, bool forceRegister = false, bool checkAttributes = true)
        {
            string key = null;
            if (checkAttributes)
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                if (customAttributes.Length != 0)
                {
                    XmlTypeAttribute attribute = (XmlTypeAttribute) customAttributes[0];
                    key = type.Name;
                    if (!string.IsNullOrEmpty(attribute.TypeName))
                    {
                        key = attribute.TypeName;
                    }
                }
                else
                {
                    using (HashSet<Type>.Enumerator enumerator = m_serializableBaseTypes.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.IsAssignableFrom(type))
                            {
                                key = type.Name;
                                break;
                            }
                        }
                    }
                }
            }
            if (key == null)
            {
                if (!forceRegister)
                {
                    return null;
                }
                key = type.Name;
            }
            XmlSerializer serializer = new XmlSerializer(type);
            m_serializersByType.Add(type, serializer);
            m_serializersBySerializedName.Add(key, serializer);
            m_serializedNameByType.Add(type, key);
            return serializer;
        }

        public static bool TryGetSerializer(string serializedName, out XmlSerializer serializer) => 
            m_serializersBySerializedName.TryGetValue(serializedName, out serializer);
    }
}

