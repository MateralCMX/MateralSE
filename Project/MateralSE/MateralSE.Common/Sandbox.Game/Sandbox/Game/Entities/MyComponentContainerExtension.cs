namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public static class MyComponentContainerExtension
    {
        public static MyObjectBuilder_ComponentBase FindComponentBuilder(MyContainerDefinition.DefaultComponent component, MyObjectBuilder_ComponentContainer builder)
        {
            MyObjectBuilder_ComponentBase base2 = null;
            if ((builder != null) && component.IsValid())
            {
                null;
                if (!component.BuilderType.IsNull)
                {
                    MyObjectBuilder_ComponentContainer.ComponentData data = builder.Components.Find(x => x.Component.TypeId == component.BuilderType);
                    if (data != null)
                    {
                        base2 = data.Component;
                    }
                }
            }
            return base2;
        }

        public static void InitComponents(this MyComponentContainer container, MyObjectBuilderType type, MyStringHash subtypeName, MyObjectBuilder_ComponentContainer builder)
        {
            if (MyDefinitionManager.Static != null)
            {
                MyContainerDefinition definition = null;
                bool flag = ReferenceEquals(builder, null);
                if (TryGetContainerDefinition(type, subtypeName, out definition))
                {
                    container.Init(definition);
                    if (definition.DefaultComponents != null)
                    {
                        foreach (MyContainerDefinition.DefaultComponent component in definition.DefaultComponents)
                        {
                            MyComponentDefinitionBase componentDefinition = null;
                            MyObjectBuilder_ComponentBase base3 = FindComponentBuilder(component, builder);
                            bool flag2 = base3 != null;
                            Type type2 = null;
                            MyComponentBase base4 = null;
                            MyStringHash hash = subtypeName;
                            if (component.SubtypeId != null)
                            {
                                hash = component.SubtypeId.Value;
                            }
                            if (TryGetComponentDefinition(component.BuilderType, hash, out componentDefinition))
                            {
                                base4 = MyComponentFactory.CreateInstanceByTypeId(componentDefinition.Id.TypeId);
                                base4.Init(componentDefinition);
                            }
                            else if (component.IsValid())
                            {
                                base4 = component.BuilderType.IsNull ? MyComponentFactory.CreateInstanceByType(component.InstanceType) : MyComponentFactory.CreateInstanceByTypeId(component.BuilderType);
                            }
                            if (base4 != null)
                            {
                                Type componentType = MyComponentTypeFactory.GetComponentType(base4.GetType());
                                if (componentType != null)
                                {
                                    type2 = componentType;
                                }
                                else
                                {
                                    MyComponentDefinitionBase base1 = componentDefinition;
                                }
                            }
                            if ((type2 == null) && (base4 != null))
                            {
                                type2 = base4.GetType();
                            }
                            if (((base4 != null) && (type2 != null)) && ((flag | flag2) || component.ForceCreate))
                            {
                                if (base3 != null)
                                {
                                    base4.Deserialize(base3);
                                }
                                container.Add(type2, base4);
                            }
                        }
                    }
                }
                container.Deserialize(builder);
            }
        }

        public static bool TryAddComponent(long entityId, MyDefinitionId componentDefinitionId)
        {
            MyEntity entity;
            MyComponentDefinitionBase base2;
            if (!MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                return false;
            }
            if (TryGetComponentDefinition(componentDefinitionId.TypeId, componentDefinitionId.SubtypeId, out base2))
            {
                MyComponentBase component = MyComponentFactory.CreateInstanceByTypeId(base2.Id.TypeId);
                Type componentType = MyComponentTypeFactory.GetComponentType(component.GetType());
                if (componentType == null)
                {
                    return false;
                }
                component.Init(base2);
                entity.Components.Add(componentType, component);
            }
            return true;
        }

        public static bool TryAddComponent(long entityId, string instanceTypeStr, string componentTypeStr)
        {
            MyEntity entity;
            Type type = null;
            Type type2 = null;
            MyComponentDefinitionBase base3;
            try
            {
                type = Type.GetType(instanceTypeStr, true);
            }
            catch (Exception)
            {
            }
            try
            {
                type = Type.GetType(componentTypeStr, true);
            }
            catch (Exception)
            {
            }
            if (!MyEntities.TryGetEntityById(entityId, out entity, false) || (type == null))
            {
                return false;
            }
            MyComponentBase component = MyComponentFactory.CreateInstanceByType(type);
            if ((entity.DefinitionId != null) && TryGetComponentDefinition(component.GetType(), entity.DefinitionId.Value.SubtypeId, out base3))
            {
                component.Init(base3);
            }
            entity.Components.Add(type2, component);
            return true;
        }

        public static bool TryGetComponentDefinition(MyObjectBuilderType type, MyStringHash subtypeName, out MyComponentDefinitionBase componentDefinition)
        {
            componentDefinition = null;
            if (MyDefinitionManager.Static == null)
            {
                return false;
            }
            MyDefinitionId componentId = new MyDefinitionId(type, subtypeName);
            if (MyDefinitionManager.Static.TryGetEntityComponentDefinition(componentId, out componentDefinition))
            {
                return true;
            }
            if (subtypeName != MyStringHash.NullOrEmpty)
            {
                MyDefinitionId id3 = new MyDefinitionId(typeof(MyObjectBuilder_EntityBase), subtypeName);
                if (MyDefinitionManager.Static.TryGetEntityComponentDefinition(id3, out componentDefinition))
                {
                    return true;
                }
            }
            MyDefinitionId id2 = new MyDefinitionId(type);
            return MyDefinitionManager.Static.TryGetEntityComponentDefinition(id2, out componentDefinition);
        }

        public static bool TryGetContainerDefinition(MyObjectBuilderType type, MyStringHash subtypeName, out MyContainerDefinition definition)
        {
            definition = null;
            if (MyDefinitionManager.Static == null)
            {
                return false;
            }
            MyDefinitionId containerId = new MyDefinitionId(type, subtypeName);
            if (MyDefinitionManager.Static.TryGetContainerDefinition(containerId, out definition))
            {
                return true;
            }
            if (subtypeName != MyStringHash.NullOrEmpty)
            {
                MyDefinitionId id3 = new MyDefinitionId(typeof(MyObjectBuilder_EntityBase), subtypeName);
                if (MyDefinitionManager.Static.TryGetContainerDefinition(id3, out definition))
                {
                    return true;
                }
            }
            MyDefinitionId id2 = new MyDefinitionId(type);
            return MyDefinitionManager.Static.TryGetContainerDefinition(id2, out definition);
        }

        public static bool TryGetEntityComponentTypes(long entityId, out List<Type> components)
        {
            MyEntity entity;
            components = null;
            if (!MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                return false;
            }
            components = new List<Type>();
            foreach (Type type in entity.Components.GetComponentTypes())
            {
                if (type != null)
                {
                    components.Add(type);
                }
            }
            return (components.Count > 0);
        }

        public static bool TryRemoveComponent(long entityId, Type componentType)
        {
            MyEntity entity;
            if (!MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                return false;
            }
            entity.Components.Remove(componentType);
            return true;
        }
    }
}

