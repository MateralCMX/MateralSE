namespace Sandbox.Game.Entities
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.EntityComponents.Interfaces;
    using VRage.ObjectBuilders;

    internal static class MyEntityFactory
    {
        private static MyObjectFactory<MyEntityTypeAttribute, MyEntity> m_objectFactory = new MyObjectFactory<MyEntityTypeAttribute, MyEntity>();
        private static readonly HashSet<Type> m_emptySet = new HashSet<Type>();

        public static void AddScriptGameLogic(MyEntity entity, MyObjectBuilderType builderType, string subTypeName = null)
        {
            MyScriptManager @static = MyScriptManager.Static;
            if ((@static != null) && (entity != null))
            {
                HashSet<Type> emptySet;
                if (subTypeName == null)
                {
                    emptySet = m_emptySet;
                }
                else
                {
                    Tuple<Type, string> key = new Tuple<Type, string>((Type) builderType, subTypeName);
                    emptySet = @static.SubEntityScripts.GetValueOrDefault<Tuple<Type, string>, HashSet<Type>>(key, m_emptySet);
                }
                HashSet<Type> first = @static.EntityScripts.GetValueOrDefault<Type, HashSet<Type>>((Type) builderType, m_emptySet);
                int capacity = emptySet.Count + first.Count;
                if (capacity != 0)
                {
                    List<MyGameLogicComponent> logicComponents = new List<MyGameLogicComponent>(capacity);
                    foreach (Type local1 in first.Concat<Type>(emptySet))
                    {
                        MyGameLogicComponent item = (MyGameLogicComponent) Activator.CreateInstance(local1);
                        MyEntityComponentDescriptor descriptor = (MyEntityComponentDescriptor) local1.GetCustomAttribute(typeof(MyEntityComponentDescriptor), false);
                        if (descriptor.EntityUpdate == null)
                        {
                            ((IMyGameLogicComponent) item).EntityUpdate = true;
                        }
                        else if (descriptor.EntityUpdate.Value)
                        {
                            ((IMyGameLogicComponent) item).EntityUpdate = true;
                        }
                        logicComponents.Add(item);
                    }
                    MyGameLogicComponent component = MyCompositeGameLogicComponent.Create(logicComponents, entity);
                    entity.GameLogic = component;
                }
            }
        }

        public static MyEntity CreateEntity(MyObjectBuilder_Base builder) => 
            CreateEntity(builder.TypeId, builder.SubtypeName);

        public static T CreateEntity<T>(MyObjectBuilder_Base builder) where T: MyEntity
        {
            T entity = m_objectFactory.CreateInstance<T>(builder.TypeId);
            AddScriptGameLogic(entity, builder.GetType(), builder.SubtypeName);
            MyEntities.RaiseEntityCreated(entity);
            return entity;
        }

        public static MyEntity CreateEntity(MyObjectBuilderType typeId, string subTypeName = null)
        {
            MyEntity entity = m_objectFactory.CreateInstance(typeId);
            AddScriptGameLogic(entity, typeId, subTypeName);
            MyEntities.RaiseEntityCreated(entity);
            return entity;
        }

        public static MyObjectBuilder_EntityBase CreateObjectBuilder(MyEntity entity) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_EntityBase>(entity);

        public static void RegisterDescriptor(MyEntityTypeAttribute descriptor, Type type)
        {
            if ((type != null) && (descriptor != null))
            {
                m_objectFactory.RegisterDescriptor(descriptor, type);
            }
        }

        public static void RegisterDescriptorsFromAssembly(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    RegisterDescriptorsFromAssembly(assemblyArray[i]);
                }
            }
        }

        public static void RegisterDescriptorsFromAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                m_objectFactory.RegisterFromAssembly(assembly);
            }
        }
    }
}

