namespace Sandbox.Game.World
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    public class MyGlobalEventFactory
    {
        private static readonly Dictionary<MyDefinitionId, MethodInfo> m_typesToHandlers = new Dictionary<MyDefinitionId, MethodInfo>();
        private static MyObjectFactory<MyEventTypeAttribute, MyGlobalEventBase> m_globalEventFactory = new MyObjectFactory<MyEventTypeAttribute, MyGlobalEventBase>();

        static MyGlobalEventFactory()
        {
            RegisterEventTypesAndHandlers(Assembly.GetAssembly(typeof(MyGlobalEventBase)));
            RegisterEventTypesAndHandlers(MyPlugins.GameAssembly);
            RegisterEventTypesAndHandlers(MyPlugins.SandboxAssembly);
        }

        public static MyGlobalEventBase CreateEvent(MyDefinitionId id)
        {
            MyGlobalEventDefinition eventDefinition = MyDefinitionManager.Static.GetEventDefinition(id);
            if (eventDefinition == null)
            {
                return null;
            }
            MyGlobalEventBase base2 = m_globalEventFactory.CreateInstance(id.TypeId);
            if (base2 != null)
            {
                base2.InitFromDefinition(eventDefinition);
            }
            return base2;
        }

        public static EventDataType CreateEvent<EventDataType>(MyDefinitionId id) where EventDataType: MyGlobalEventBase, new()
        {
            MyGlobalEventDefinition eventDefinition = MyDefinitionManager.Static.GetEventDefinition(id);
            if (eventDefinition == null)
            {
                return default(EventDataType);
            }
            EventDataType local1 = Activator.CreateInstance<EventDataType>();
            local1.InitFromDefinition(eventDefinition);
            return local1;
        }

        public static MyGlobalEventBase CreateEvent(MyObjectBuilder_GlobalEventBase ob)
        {
            if (ob.DefinitionId != null)
            {
                if (ob.DefinitionId.Value.TypeId == MyObjectBuilderType.Invalid)
                {
                    return CreateEventObsolete(ob);
                }
                ob.SubtypeName = ob.DefinitionId.Value.SubtypeName;
            }
            if (MyDefinitionManager.Static.GetEventDefinition(ob.GetId()) == null)
            {
                return null;
            }
            MyGlobalEventBase base1 = CreateEvent(ob.GetId());
            base1.Init(ob);
            return base1;
        }

        private static MyGlobalEventBase CreateEventObsolete(MyObjectBuilder_GlobalEventBase ob)
        {
            MyGlobalEventBase base1 = CreateEvent(GetEventDefinitionObsolete(ob.EventType));
            base1.SetActivationTime(TimeSpan.FromMilliseconds((double) ob.ActivationTimeMs));
            base1.Enabled = ob.Enabled;
            return base1;
        }

        private static MyDefinitionId GetEventDefinitionObsolete(MyGlobalEventTypeEnum eventType)
        {
            if ((eventType == MyGlobalEventTypeEnum.SpawnNeutralShip) || (eventType == MyGlobalEventTypeEnum.SpawnCargoShip))
            {
                return new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip");
            }
            return ((eventType != MyGlobalEventTypeEnum.MeteorWave) ? ((eventType != MyGlobalEventTypeEnum.April2014) ? new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase)) : new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "April2014")) : new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "MeteorWave"));
        }

        public static MethodInfo GetEventHandler(MyDefinitionId eventDefinitionId)
        {
            MethodInfo info = null;
            m_typesToHandlers.TryGetValue(eventDefinitionId, out info);
            return info;
        }

        private static void RegisterEventTypesAndHandlers(Assembly assembly)
        {
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                int index = 0;
                while (index < types.Length)
                {
                    MethodInfo[] methods = types[index].GetMethods();
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= methods.Length)
                        {
                            index++;
                            break;
                        }
                        MethodInfo handler = methods[num2];
                        if (handler.IsPublic && handler.IsStatic)
                        {
                            object[] customAttributes = handler.GetCustomAttributes(typeof(MyGlobalEventHandler), false);
                            if ((customAttributes != null) && (customAttributes.Length != 0))
                            {
                                object[] objArray2 = customAttributes;
                                for (int i = 0; i < objArray2.Length; i++)
                                {
                                    RegisterHandler(((MyGlobalEventHandler) objArray2[i]).EventDefinitionId, handler);
                                }
                            }
                        }
                        num2++;
                    }
                }
                m_globalEventFactory.RegisterFromAssembly(assembly);
            }
        }

        private static void RegisterHandler(MyDefinitionId eventDefinitionId, MethodInfo handler)
        {
            m_typesToHandlers[eventDefinitionId] = handler;
        }
    }
}

