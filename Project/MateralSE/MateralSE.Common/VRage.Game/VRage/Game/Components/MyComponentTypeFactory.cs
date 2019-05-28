namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Plugins;
    using VRage.Utils;

    [PreloadRequired]
    public static class MyComponentTypeFactory
    {
        private static Dictionary<MyStringId, Type> m_idToType = new Dictionary<MyStringId, Type>(MyStringId.Comparer);
        private static Dictionary<Type, MyStringId> m_typeToId = new Dictionary<Type, MyStringId>();
        private static Dictionary<Type, Type> m_typeToContainerComponentType = new Dictionary<Type, Type>();

        static MyComponentTypeFactory()
        {
            RegisterFromAssembly(typeof(MyComponentTypeFactory).Assembly);
            RegisterFromAssembly(MyPlugins.SandboxAssembly);
            RegisterFromAssembly(MyPlugins.GameAssembly);
            RegisterFromAssembly(MyPlugins.SandboxGameAssembly);
            RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        private static void AddId(Type type, MyStringId id)
        {
            m_idToType[id] = type;
            m_typeToId[type] = id;
        }

        public static Type GetComponentType(Type type)
        {
            Type type2;
            return (!m_typeToContainerComponentType.TryGetValue(type, out type2) ? null : type2);
        }

        public static MyStringId GetId(Type type) => 
            m_typeToId[type];

        public static Type GetType(string typeId)
        {
            MyStringId id;
            if (!MyStringId.TryGet(typeId, out id))
            {
                throw new Exception("Unregistered component typeId! : " + typeId);
            }
            return m_idToType[id];
        }

        public static Type GetType(MyStringId id) => 
            m_idToType[id];

        private static void RegisterComponentTypeAttribute(Type type)
        {
            Type componentType = null;
            object[] customAttributes = type.GetCustomAttributes(typeof(MyComponentTypeAttribute), true);
            int index = 0;
            while (true)
            {
                if (index < customAttributes.Length)
                {
                    MyComponentTypeAttribute attribute = (MyComponentTypeAttribute) customAttributes[index];
                    if ((attribute.ComponentType == null) || (componentType != null))
                    {
                        index++;
                        continue;
                    }
                    componentType = attribute.ComponentType;
                }
                if (componentType != null)
                {
                    m_typeToContainerComponentType.Add(type, componentType);
                }
                return;
            }
        }

        private static void RegisterFromAssembly(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    RegisterFromAssembly(assemblyArray[i]);
                }
            }
        }

        private static void RegisterFromAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                Type type = typeof(MyComponentBase);
                foreach (Type type2 in assembly.GetTypes())
                {
                    if (type.IsAssignableFrom(type2))
                    {
                        AddId(type2, MyStringId.GetOrCompute(type2.Name));
                        RegisterComponentTypeAttribute(type2);
                    }
                }
            }
        }
    }
}

