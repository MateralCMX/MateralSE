namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public static class MySessionComponentMapping
    {
        private static Dictionary<Type, MyObjectBuilderType> m_objectBuilderTypeByType = new Dictionary<Type, MyObjectBuilderType>();
        private static Dictionary<MyObjectBuilderType, Type> m_typeByObjectBuilderType = new Dictionary<MyObjectBuilderType, Type>();
        private static Dictionary<Type, MyObjectBuilder_SessionComponent> m_sessionObjectBuilderByType = new Dictionary<Type, MyObjectBuilder_SessionComponent>();

        public static void Clear()
        {
            m_objectBuilderTypeByType.Clear();
            m_typeByObjectBuilderType.Clear();
            m_sessionObjectBuilderByType.Clear();
        }

        public static Dictionary<Type, MyObjectBuilder_SessionComponent> GetMappedSessionObjectBuilders(List<MyObjectBuilder_SessionComponent> objectBuilders)
        {
            m_sessionObjectBuilderByType.Clear();
            foreach (MyObjectBuilder_SessionComponent component in objectBuilders)
            {
                if (m_typeByObjectBuilderType.ContainsKey(component.GetType()))
                {
                    Type type = m_typeByObjectBuilderType[component.GetType()];
                    m_sessionObjectBuilderByType[type] = component;
                }
            }
            return m_sessionObjectBuilderByType;
        }

        public static bool Map(Type type, MyObjectBuilderType objectBuilderType)
        {
            if (!type.IsSubclassOf(typeof(MySessionComponentBase)))
            {
                return false;
            }
            if (m_objectBuilderTypeByType.ContainsKey(type))
            {
                return false;
            }
            m_objectBuilderTypeByType.Add(type, objectBuilderType);
            if (m_typeByObjectBuilderType.ContainsKey(objectBuilderType))
            {
                return false;
            }
            m_typeByObjectBuilderType.Add(objectBuilderType, type);
            return true;
        }

        public static MyObjectBuilderType TryGetMappedObjectBuilderType(Type type)
        {
            MyObjectBuilderType type2 = 0;
            m_objectBuilderTypeByType.TryGetValue(type, out type2);
            return type2;
        }

        public static Type TryGetMappedSessionComponentType(MyObjectBuilderType objectBuilderType)
        {
            Type type = null;
            m_typeByObjectBuilderType.TryGetValue(objectBuilderType, out type);
            return type;
        }
    }
}

