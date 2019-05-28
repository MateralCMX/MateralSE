namespace VRage.Game
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public abstract class MyDefinitionManagerBase
    {
        protected MyDefinitionSet m_definitions = new MyDefinitionSet();
        private static MyObjectFactory<MyDefinitionTypeAttribute, MyDefinitionBase> m_definitionFactory = new MyObjectFactory<MyDefinitionTypeAttribute, MyDefinitionBase>();
        protected static Dictionary<Type, MyDefinitionPostprocessor> m_postprocessorsByType = new Dictionary<Type, MyDefinitionPostprocessor>();
        protected static List<MyDefinitionPostprocessor> m_postProcessors = new List<MyDefinitionPostprocessor>();
        protected static HashSet<Assembly> m_registeredAssemblies = new HashSet<Assembly>();
        public static MyDefinitionManagerBase Static;
        private static readonly Dictionary<Type, HashSet<Type>> m_childDefinitionMap = new Dictionary<Type, HashSet<Type>>();
        private static ConcurrentDictionary<Type, Type> m_objectBuilderTypeCache = new ConcurrentDictionary<Type, Type>();
        private static HashSet<Assembly> m_registered = new HashSet<Assembly>();

        protected MyDefinitionManagerBase()
        {
        }

        public IEnumerable<T> GetAllDefinitions<T>() where T: MyDefinitionBase => 
            this.m_definitions.GetDefinitionsOfTypeAndSubtypes<T>();

        public T GetDefinition<T>(string subtypeId) where T: MyDefinitionBase => 
            this.m_definitions.GetDefinition<T>(MyStringHash.GetOrCompute(subtypeId));

        public T GetDefinition<T>(MyDefinitionId subtypeId) where T: MyDefinitionBase => 
            this.m_definitions.GetDefinition<T>(subtypeId);

        public T GetDefinition<T>(MyStringHash subtypeId) where T: MyDefinitionBase => 
            this.m_definitions.GetDefinition<T>(subtypeId);

        public IEnumerable<T> GetDefinitions<T>() where T: MyDefinitionBase => 
            this.m_definitions.GetDefinitionsOfType<T>();

        public abstract MyDefinitionSet GetLoadingSet();
        public static Type GetObjectBuilderType(Type defType)
        {
            Type objectBuilderType;
            if (!m_objectBuilderTypeCache.TryGetValue(defType, out objectBuilderType))
            {
                object[] customAttributes = defType.GetCustomAttributes(typeof(MyDefinitionTypeAttribute), false);
                int index = 0;
                if (index >= customAttributes.Length)
                {
                    return null;
                }
                objectBuilderType = ((MyDefinitionTypeAttribute) customAttributes[index]).ObjectBuilderType;
                m_objectBuilderTypeCache.TryAdd(defType, objectBuilderType);
            }
            return objectBuilderType;
        }

        public static MyObjectFactory<MyDefinitionTypeAttribute, MyDefinitionBase> GetObjectFactory() => 
            m_definitionFactory;

        public static MyDefinitionPostprocessor GetPostProcessor(Type obType)
        {
            MyDefinitionPostprocessor postprocessor;
            m_postprocessorsByType.TryGetValue(obType, out postprocessor);
            return postprocessor;
        }

        public HashSet<Type> GetSubtypes<T>()
        {
            HashSet<Type> set;
            m_childDefinitionMap.TryGetValue(typeof(T), out set);
            return set;
        }

        public static void RegisterTypesFromAssembly(Assembly assembly)
        {
            if ((assembly != null) && !m_registeredAssemblies.Contains(assembly))
            {
                m_registeredAssemblies.Add(assembly);
                if (!m_registered.Contains(assembly))
                {
                    m_registered.Add(assembly);
                    foreach (Type type in assembly.GetTypes())
                    {
                        object[] customAttributes = type.GetCustomAttributes(typeof(MyDefinitionTypeAttribute), false);
                        if (customAttributes.Length != 0)
                        {
                            if (!type.IsSubclassOf(typeof(MyDefinitionBase)) && (type != typeof(MyDefinitionBase)))
                            {
                                object[] args = new object[] { type.Name };
                                MyLog.Default.Error("Type {0} is not a definition.", args);
                            }
                            else
                            {
                                object[] objArray2 = customAttributes;
                                int index = 0;
                                while (true)
                                {
                                    if (index >= objArray2.Length)
                                    {
                                        Type key = type;
                                        while (key != typeof(MyDefinitionBase))
                                        {
                                            HashSet<Type> set;
                                            key = key.BaseType;
                                            if (!m_childDefinitionMap.TryGetValue(key, out set))
                                            {
                                                set = new HashSet<Type>();
                                                m_childDefinitionMap[key] = set;
                                                set.Add(key);
                                            }
                                            set.Add(type);
                                        }
                                        break;
                                    }
                                    MyDefinitionTypeAttribute descriptor = (MyDefinitionTypeAttribute) objArray2[index];
                                    m_definitionFactory.RegisterDescriptor(descriptor, type);
                                    MyDefinitionPostprocessor item = (MyDefinitionPostprocessor) Activator.CreateInstance(descriptor.PostProcessor);
                                    item.DefinitionType = descriptor.ObjectBuilderType;
                                    m_postProcessors.Add(item);
                                    m_postprocessorsByType.Add(descriptor.ObjectBuilderType, item);
                                    MyXmlSerializerManager.RegisterSerializer(descriptor.ObjectBuilderType);
                                    index++;
                                }
                            }
                        }
                    }
                    m_postProcessors.Sort(MyDefinitionPostprocessor.Comparer);
                }
            }
        }

        public bool TryGetDefinition<T>(MyStringHash subtypeId, out T def) where T: MyDefinitionBase
        {
            T local;
            def = local = this.m_definitions.GetDefinition<T>(subtypeId);
            return (local != null);
        }

        public MyDefinitionSet Definitions =>
            this.m_definitions;
    }
}

