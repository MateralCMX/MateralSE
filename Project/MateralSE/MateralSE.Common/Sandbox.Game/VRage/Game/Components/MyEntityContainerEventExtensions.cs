namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public static class MyEntityContainerEventExtensions
    {
        private static Dictionary<long, RegisteredEvents> RegisteredListeners = new Dictionary<long, RegisteredEvents>();
        private static Dictionary<MyComponentBase, List<long>> ExternalListeners = new Dictionary<MyComponentBase, List<long>>();
        private static HashSet<long> ExternalyListenedEntities = new HashSet<long>();
        private static List<RegisteredComponent> m_tmpList = new List<RegisteredComponent>();
        private static List<MyComponentBase> m_tmpCompList = new List<MyComponentBase>();
        private static bool ProcessingEvents;
        private static bool HasPostponedOperations;
        private static List<Tuple<MyEntityComponentBase, MyEntity, MyStringHash, EntityEventHandler>> PostponedRegistration = new List<Tuple<MyEntityComponentBase, MyEntity, MyStringHash, EntityEventHandler>>();
        private static List<Tuple<MyEntityComponentBase, MyEntity, MyStringHash>> PostponedUnregistration = new List<Tuple<MyEntityComponentBase, MyEntity, MyStringHash>>();
        private static List<long> PostPonedRegisteredListenersRemoval = new List<long>();
        private static int m_debugCounter;

        private static void AddPostponedListenerRemoval(long id)
        {
            PostPonedRegisteredListenersRemoval.Add(id);
            HasPostponedOperations = true;
        }

        private static void AddPostponedRegistration(MyEntityComponentBase component, MyEntity entity, MyStringHash eventType, EntityEventHandler handler)
        {
            PostponedRegistration.Add(new Tuple<MyEntityComponentBase, MyEntity, MyStringHash, EntityEventHandler>(component, entity, eventType, handler));
            HasPostponedOperations = true;
        }

        private static void AddPostponedUnregistration(MyEntityComponentBase component, MyEntity entity, MyStringHash eventType)
        {
            PostponedUnregistration.Add(new Tuple<MyEntityComponentBase, MyEntity, MyStringHash>(component, entity, eventType));
            HasPostponedOperations = true;
        }

        public static void InitEntityEvents()
        {
            RegisteredListeners = new Dictionary<long, RegisteredEvents>();
            ExternalListeners = new Dictionary<MyComponentBase, List<long>>();
            ExternalyListenedEntities = new HashSet<long>();
            PostponedRegistration = new List<Tuple<MyEntityComponentBase, MyEntity, MyStringHash, EntityEventHandler>>();
            PostponedUnregistration = new List<Tuple<MyEntityComponentBase, MyEntity, MyStringHash>>();
            ProcessingEvents = false;
            HasPostponedOperations = false;
        }

        private static void InvokeEventOnListeners(long entityId, MyStringHash eventType, EntityEventParams eventParams)
        {
            bool processingEvents = ProcessingEvents;
            if (processingEvents)
            {
                m_debugCounter++;
            }
            if (m_debugCounter <= 5)
            {
                ProcessingEvents = true;
                if (RegisteredListeners.ContainsKey(entityId) && RegisteredListeners[entityId].ContainsKey(eventType))
                {
                    foreach (RegisteredComponent component in RegisteredListeners[entityId][eventType])
                    {
                        try
                        {
                            component.Handler(eventParams);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                ProcessingEvents = processingEvents;
                if (!ProcessingEvents)
                {
                    m_debugCounter = 0;
                }
                if (HasPostponedOperations && !ProcessingEvents)
                {
                    ProcessPostponedRegistrations();
                }
            }
        }

        private static void ProcessPostponedRegistrations()
        {
            foreach (Tuple<MyEntityComponentBase, MyEntity, MyStringHash, EntityEventHandler> tuple in PostponedRegistration)
            {
                tuple.Item1.RegisterForEntityEvent(tuple.Item2, tuple.Item3, tuple.Item4);
            }
            foreach (Tuple<MyEntityComponentBase, MyEntity, MyStringHash> tuple2 in PostponedUnregistration)
            {
                tuple2.Item1.UnregisterForEntityEvent(tuple2.Item2, tuple2.Item3);
            }
            foreach (long num in PostPonedRegisteredListenersRemoval)
            {
                RegisteredListeners.Remove(num);
            }
            PostponedRegistration.Clear();
            PostponedUnregistration.Clear();
            PostPonedRegisteredListenersRemoval.Clear();
            HasPostponedOperations = false;
        }

        public static void RaiseEntityEvent(this MyEntityComponentBase component, MyStringHash eventType, EntityEventParams eventParams)
        {
            if (component.Entity != null)
            {
                InvokeEventOnListeners(component.Entity.EntityId, eventType, eventParams);
            }
        }

        public static void RaiseEntityEvent(this MyEntity entity, MyStringHash eventType, EntityEventParams eventParams)
        {
            if (entity.Components != null)
            {
                InvokeEventOnListeners(entity.EntityId, eventType, eventParams);
            }
        }

        public static void RaiseEntityEventOn(MyEntity entity, MyStringHash eventType, EntityEventParams eventParams)
        {
            if (entity.Components != null)
            {
                InvokeEventOnListeners(entity.EntityId, eventType, eventParams);
            }
        }

        private static void RegisteredComponentBeforeRemovedFromContainer(MyEntityComponentBase component)
        {
            component.BeforeRemovedFromContainer -= new Action<MyEntityComponentBase>(MyEntityContainerEventExtensions.RegisteredComponentBeforeRemovedFromContainer);
            if (component.Entity != null)
            {
                if (RegisteredListeners.ContainsKey(component.Entity.EntityId))
                {
                    m_tmpList.Clear();
                    foreach (KeyValuePair<MyStringHash, List<RegisteredComponent>> pair in RegisteredListeners[component.Entity.EntityId])
                    {
                        Predicate<RegisteredComponent> <>9__0;
                        Predicate<RegisteredComponent> match = <>9__0;
                        if (<>9__0 == null)
                        {
                            Predicate<RegisteredComponent> local1 = <>9__0;
                            match = <>9__0 = x => ReferenceEquals(x.Component, component);
                        }
                        pair.Value.RemoveAll(match);
                    }
                }
                if (ExternalListeners.ContainsKey(component))
                {
                    foreach (long num in ExternalListeners[component])
                    {
                        if (RegisteredListeners.ContainsKey(num))
                        {
                            foreach (KeyValuePair<MyStringHash, List<RegisteredComponent>> pair2 in RegisteredListeners[num])
                            {
                                Predicate<RegisteredComponent> <>9__1;
                                Predicate<RegisteredComponent> match = <>9__1;
                                if (<>9__1 == null)
                                {
                                    Predicate<RegisteredComponent> local2 = <>9__1;
                                    match = <>9__1 = x => ReferenceEquals(x.Component, component);
                                }
                                pair2.Value.RemoveAll(match);
                            }
                        }
                    }
                    ExternalListeners.Remove(component);
                }
            }
        }

        private static void RegisteredEntityOnClose(IMyEntity entity)
        {
            entity.OnClose -= new Action<IMyEntity>(MyEntityContainerEventExtensions.RegisteredEntityOnClose);
            if (RegisteredListeners.ContainsKey(entity.EntityId))
            {
                if (ProcessingEvents)
                {
                    AddPostponedListenerRemoval(entity.EntityId);
                }
                else
                {
                    RegisteredListeners.Remove(entity.EntityId);
                }
            }
            if (ExternalyListenedEntities.Contains(entity.EntityId))
            {
                ExternalyListenedEntities.Remove(entity.EntityId);
                m_tmpCompList.Clear();
                foreach (KeyValuePair<MyComponentBase, List<long>> pair in ExternalListeners)
                {
                    pair.Value.Remove(entity.EntityId);
                    if (pair.Value.Count == 0)
                    {
                        m_tmpCompList.Add(pair.Key);
                    }
                }
                foreach (MyComponentBase base2 in m_tmpCompList)
                {
                    ExternalListeners.Remove(base2);
                }
            }
        }

        public static void RegisterForEntityEvent(this MyEntityComponentBase component, MyStringHash eventType, EntityEventHandler handler)
        {
            if (ProcessingEvents)
            {
                AddPostponedRegistration(component, component.Entity as MyEntity, eventType, handler);
            }
            else if (component.Entity != null)
            {
                component.BeforeRemovedFromContainer += new Action<MyEntityComponentBase>(MyEntityContainerEventExtensions.RegisteredComponentBeforeRemovedFromContainer);
                component.Entity.OnClose += new Action<IMyEntity>(MyEntityContainerEventExtensions.RegisteredEntityOnClose);
                if (!RegisteredListeners.ContainsKey(component.Entity.EntityId))
                {
                    RegisteredListeners[component.Entity.EntityId] = new RegisteredEvents(eventType, component, handler);
                }
                else
                {
                    RegisteredEvents events = RegisteredListeners[component.Entity.EntityId];
                    if (!events.ContainsKey(eventType))
                    {
                        events[eventType] = new List<RegisteredComponent>();
                        events[eventType].Add(new RegisteredComponent(component, handler));
                    }
                    else if (events[eventType].Find(x => x.Handler == handler) == null)
                    {
                        events[eventType].Add(new RegisteredComponent(component, handler));
                    }
                }
            }
        }

        public static void RegisterForEntityEvent(this MyEntityComponentBase component, MyEntity entity, MyStringHash eventType, EntityEventHandler handler)
        {
            if (ProcessingEvents)
            {
                AddPostponedRegistration(component, entity, eventType, handler);
            }
            else if (ReferenceEquals(component.Entity, entity))
            {
                component.RegisterForEntityEvent(eventType, handler);
            }
            else if (entity != null)
            {
                component.BeforeRemovedFromContainer += new Action<MyEntityComponentBase>(MyEntityContainerEventExtensions.RegisteredComponentBeforeRemovedFromContainer);
                entity.OnClose += new Action<MyEntity>(MyEntityContainerEventExtensions.RegisteredEntityOnClose);
                if (!RegisteredListeners.ContainsKey(entity.EntityId))
                {
                    RegisteredListeners[entity.EntityId] = new RegisteredEvents(eventType, component, handler);
                }
                else
                {
                    RegisteredEvents events = RegisteredListeners[entity.EntityId];
                    if (!events.ContainsKey(eventType))
                    {
                        events[eventType] = new List<RegisteredComponent>();
                        events[eventType].Add(new RegisteredComponent(component, handler));
                    }
                    else if (events[eventType].Find(x => x.Handler == handler) == null)
                    {
                        events[eventType].Add(new RegisteredComponent(component, handler));
                    }
                }
                if (ExternalListeners.ContainsKey(component) && !ExternalListeners[component].Contains(entity.EntityId))
                {
                    ExternalListeners[component].Add(entity.EntityId);
                }
                else
                {
                    List<long> list1 = new List<long>();
                    list1.Add(entity.EntityId);
                    ExternalListeners[component] = list1;
                }
                ExternalyListenedEntities.Add(entity.EntityId);
            }
        }

        public static void UnregisterForEntityEvent(this MyEntityComponentBase component, MyEntity entity, MyStringHash eventType)
        {
            if (ProcessingEvents)
            {
                AddPostponedUnregistration(component, entity, eventType);
            }
            else if (entity != null)
            {
                bool flag = true;
                if (RegisteredListeners.ContainsKey(entity.EntityId))
                {
                    if (RegisteredListeners[entity.EntityId].ContainsKey(eventType))
                    {
                        RegisteredListeners[entity.EntityId][eventType].RemoveAll(x => ReferenceEquals(x.Component, component));
                        if (RegisteredListeners[entity.EntityId][eventType].Count == 0)
                        {
                            RegisteredListeners[entity.EntityId].Remove(eventType);
                        }
                    }
                    if (RegisteredListeners[entity.EntityId].Count == 0)
                    {
                        RegisteredListeners.Remove(entity.EntityId);
                        ExternalyListenedEntities.Remove(entity.EntityId);
                        flag = false;
                    }
                }
                if (ExternalListeners.ContainsKey(component) && ExternalListeners[component].Contains(entity.EntityId))
                {
                    ExternalListeners[component].Remove(entity.EntityId);
                    if (ExternalListeners[component].Count == 0)
                    {
                        ExternalListeners.Remove(component);
                    }
                }
                if (!flag)
                {
                    entity.OnClose -= new Action<MyEntity>(MyEntityContainerEventExtensions.RegisteredEntityOnClose);
                }
            }
        }

        public class ControlAcquiredParams : MyEntityContainerEventExtensions.EntityEventParams
        {
            public MyEntity Owner;

            public ControlAcquiredParams(MyEntity owner)
            {
                this.Owner = owner;
            }
        }

        public class ControlReleasedParams : MyEntityContainerEventExtensions.EntityEventParams
        {
            public MyEntity Owner;

            public ControlReleasedParams(MyEntity owner)
            {
                this.Owner = owner;
            }
        }

        public delegate void EntityEventHandler(MyEntityContainerEventExtensions.EntityEventParams eventParams);

        public class EntityEventParams
        {
        }

        public class HitParams : MyEntityContainerEventExtensions.EntityEventParams
        {
            public MyStringHash HitEntity;
            public MyStringHash HitAction;

            public HitParams(MyStringHash hitAction, MyStringHash hitEntity)
            {
                this.HitEntity = hitEntity;
                this.HitAction = hitAction;
            }
        }

        public class InventoryChangedParams : MyEntityContainerEventExtensions.EntityEventParams
        {
            public uint ItemId;
            public float Amount;
            public MyInventoryBase Inventory;

            public InventoryChangedParams(uint itemId, MyInventoryBase inventory, float amount)
            {
                this.ItemId = itemId;
                this.Inventory = inventory;
                this.Amount = amount;
            }
        }

        public class ModelChangedParams : MyEntityContainerEventExtensions.EntityEventParams
        {
            public Vector3 Size;
            public float Mass;
            public float Volume;
            public string Model;
            public string DisplayName;
            public string[] Icons;

            public ModelChangedParams(string model, Vector3 size, float mass, float volume, string displayName, string[] icons)
            {
                this.Model = model;
                this.Size = size;
                this.Mass = mass;
                this.Volume = volume;
                this.DisplayName = displayName;
                this.Icons = icons;
            }
        }

        private class RegisteredComponent
        {
            public MyComponentBase Component;
            public MyEntityContainerEventExtensions.EntityEventHandler Handler;

            public RegisteredComponent(MyComponentBase component, MyEntityContainerEventExtensions.EntityEventHandler handler)
            {
                this.Component = component;
                this.Handler = handler;
            }
        }

        private class RegisteredEvents : Dictionary<MyStringHash, List<MyEntityContainerEventExtensions.RegisteredComponent>>
        {
            public RegisteredEvents(MyStringHash eventType, MyComponentBase component, MyEntityContainerEventExtensions.EntityEventHandler handler)
            {
                base[eventType] = new List<MyEntityContainerEventExtensions.RegisteredComponent>();
                base[eventType].Add(new MyEntityContainerEventExtensions.RegisteredComponent(component, handler));
            }
        }
    }
}

