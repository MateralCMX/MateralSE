namespace VRage.ObjectBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Reflection;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyObjectBuilderType
    {
        private const string LEGACY_TYPE_PREFIX = "MyObjectBuilder_";
        public static readonly MyObjectBuilderType Invalid;
        private readonly Type m_type;
        public static readonly ComparerType Comparer;
        private static Dictionary<string, MyObjectBuilderType> m_typeByName;
        private static Dictionary<string, MyObjectBuilderType> m_typeByLegacyName;
        private static Dictionary<MyRuntimeObjectBuilderId, MyObjectBuilderType> m_typeById;
        private static Dictionary<MyObjectBuilderType, MyRuntimeObjectBuilderId> m_idByType;
        private static ushort m_idCounter;
        private const int EXPECTED_TYPE_COUNT = 500;
        public MyObjectBuilderType(Type type)
        {
            this.m_type = type;
        }

        public bool IsNull =>
            (this.m_type == null);
        public static implicit operator MyObjectBuilderType(Type t) => 
            new MyObjectBuilderType(t);

        public static implicit operator Type(MyObjectBuilderType t) => 
            t.m_type;

        public static explicit operator MyRuntimeObjectBuilderId(MyObjectBuilderType t)
        {
            MyRuntimeObjectBuilderId id;
            if (!m_idByType.TryGetValue(t, out id))
            {
                id = new MyRuntimeObjectBuilderId();
            }
            return id;
        }

        public static explicit operator MyObjectBuilderType(MyRuntimeObjectBuilderId id) => 
            m_typeById[id];

        public static bool operator ==(MyObjectBuilderType lhs, MyObjectBuilderType rhs) => 
            (lhs.m_type == rhs.m_type);

        public static bool operator !=(MyObjectBuilderType lhs, MyObjectBuilderType rhs) => 
            (lhs.m_type != rhs.m_type);

        public static bool operator ==(Type lhs, MyObjectBuilderType rhs) => 
            (lhs == rhs.m_type);

        public static bool operator !=(Type lhs, MyObjectBuilderType rhs) => 
            (lhs != rhs.m_type);

        public static bool operator ==(MyObjectBuilderType lhs, Type rhs) => 
            (lhs.m_type == rhs);

        public static bool operator !=(MyObjectBuilderType lhs, Type rhs) => 
            (lhs.m_type != rhs);

        public override bool Equals(object obj) => 
            ((obj != null) && ((obj is MyObjectBuilderType) && this.Equals((MyObjectBuilderType) obj)));

        public bool Equals(MyObjectBuilderType type) => 
            (type.m_type == this.m_type);

        public override int GetHashCode() => 
            ((this.m_type != null) ? this.m_type.GetHashCode() : 0);

        public override string ToString() => 
            this.m_type?.Name;

        public static MyObjectBuilderType Parse(string value) => 
            m_typeByName[value];

        public static MyObjectBuilderType ParseBackwardsCompatible(string value)
        {
            MyObjectBuilderType type;
            return (!m_typeByName.TryGetValue(value, out type) ? m_typeByLegacyName[value] : type);
        }

        public static bool TryParse(string value, out MyObjectBuilderType result)
        {
            if (value != null)
            {
                return m_typeByName.TryGetValue(value, out result);
            }
            result = Invalid;
            return false;
        }

        static MyObjectBuilderType()
        {
            Invalid = new MyObjectBuilderType(null);
            Comparer = new ComparerType();
            m_typeByName = new Dictionary<string, MyObjectBuilderType>(500);
            m_typeByLegacyName = new Dictionary<string, MyObjectBuilderType>(500);
            m_typeById = new Dictionary<MyRuntimeObjectBuilderId, MyObjectBuilderType>(500, MyRuntimeObjectBuilderId.Comparer);
            m_idByType = new Dictionary<MyObjectBuilderType, MyRuntimeObjectBuilderId>(500, Comparer);
        }

        public static bool IsReady() => 
            (m_typeByName.Count > 0);

        public static void RegisterFromAssembly(Assembly assembly, bool registerLegacyNames = false)
        {
            if (assembly != null)
            {
                Type type = typeof(MyObjectBuilder_Base);
                Type[] types = assembly.GetTypes();
                Array.Sort<Type>(types, FullyQualifiedNameComparer.Default);
                foreach (Type type2 in types)
                {
                    if (type.IsAssignableFrom(type2) && !m_typeByName.ContainsKey(type2.Name))
                    {
                        MyObjectBuilderType type3 = new MyObjectBuilderType(type2);
                        MyRuntimeObjectBuilderId key = new MyRuntimeObjectBuilderId(m_idCounter = (ushort) (m_idCounter + 1));
                        m_typeById.Add(key, type3);
                        m_idByType.Add(type3, key);
                        m_typeByName.Add(type2.Name, type3);
                        if (registerLegacyNames && type2.Name.StartsWith("MyObjectBuilder_"))
                        {
                            RegisterLegacyName(type3, type2.Name.Substring("MyObjectBuilder_".Length));
                        }
                        object[] customAttributes = type2.GetCustomAttributes(typeof(MyObjectBuilderDefinitionAttribute), true);
                        if (customAttributes.Length != 0)
                        {
                            MyObjectBuilderDefinitionAttribute attribute = (MyObjectBuilderDefinitionAttribute) customAttributes[0];
                            if (!string.IsNullOrEmpty(attribute.LegacyName))
                            {
                                RegisterLegacyName(type3, attribute.LegacyName);
                            }
                        }
                    }
                }
            }
        }

        internal static void RegisterLegacyName(MyObjectBuilderType type, string legacyName)
        {
            m_typeByLegacyName.Add(legacyName, type);
        }

        internal static void RemapType(ref SerializableDefinitionId id, Dictionary<string, string> typeOverrideMap)
        {
            string str;
            bool flag = typeOverrideMap.TryGetValue(id.TypeIdString, out str);
            if (!flag && id.TypeIdString.StartsWith("MyObjectBuilder_"))
            {
                flag = typeOverrideMap.TryGetValue(id.TypeIdString.Substring("MyObjectBuilder_".Length), out str);
            }
            if (flag)
            {
                id.TypeIdString = str;
            }
        }

        public static void UnregisterAssemblies()
        {
            if (m_typeByLegacyName != null)
            {
                m_typeByLegacyName.Clear();
            }
            if (m_typeById != null)
            {
                m_typeById.Clear();
            }
            if (m_idByType != null)
            {
                m_idByType.Clear();
            }
            if (m_typeByName != null)
            {
                m_typeByName.Clear();
            }
            m_idCounter = 0;
        }
        public class ComparerType : IEqualityComparer<MyObjectBuilderType>
        {
            public bool Equals(MyObjectBuilderType x, MyObjectBuilderType y) => 
                (x == y);

            public int GetHashCode(MyObjectBuilderType obj) => 
                obj.GetHashCode();
        }
    }
}

