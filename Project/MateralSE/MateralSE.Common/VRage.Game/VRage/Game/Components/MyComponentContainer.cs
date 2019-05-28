namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Utils;

    public class MyComponentContainer
    {
        private readonly Dictionary<Type, MyComponentBase> m_components = new Dictionary<Type, MyComponentBase>();
        private static List<KeyValuePair<Type, MyComponentBase>> m_tmpComponents;
        [ThreadStatic]
        private static List<KeyValuePair<Type, MyComponentBase>> m_tmpSerializedComponents;

        public void Add<T>(T component) where T: MyComponentBase
        {
            Type type = typeof(T);
            this.Add(type, component);
        }

        public void Add(Type type, MyComponentBase component)
        {
            if (typeof(MyComponentBase).IsAssignableFrom(type) && ((component == null) || type.IsAssignableFrom(component.GetType())))
            {
                MyComponentBase base2;
                Type componentType = MyComponentTypeFactory.GetComponentType(type);
                if (componentType != null)
                {
                    bool flag1 = componentType != type;
                }
                if (this.m_components.TryGetValue(type, out base2))
                {
                    if (base2 is IMyComponentAggregate)
                    {
                        (base2 as IMyComponentAggregate).AddComponent(component);
                        return;
                    }
                    if (component is IMyComponentAggregate)
                    {
                        this.Remove(type);
                        (component as IMyComponentAggregate).AddComponent(base2);
                        this.m_components[type] = component;
                        component.SetContainer(this);
                        this.OnComponentAdded(type, component);
                        return;
                    }
                }
                this.Remove(type);
                if (component != null)
                {
                    this.m_components[type] = component;
                    component.SetContainer(this);
                    this.OnComponentAdded(type, component);
                }
            }
        }

        public void Clear()
        {
            if (this.m_components.Count > 0)
            {
                using (MyUtils.ClearCollectionToken<List<KeyValuePair<Type, MyComponentBase>>, KeyValuePair<Type, MyComponentBase>> token = MyUtils.ReuseCollection<KeyValuePair<Type, MyComponentBase>>(ref m_tmpComponents))
                {
                    List<KeyValuePair<Type, MyComponentBase>> collection = token.Collection;
                    foreach (KeyValuePair<Type, MyComponentBase> pair in this.m_components)
                    {
                        collection.Add(pair);
                        pair.Value.SetContainer(null);
                    }
                    this.m_components.Clear();
                    foreach (KeyValuePair<Type, MyComponentBase> pair2 in collection)
                    {
                        this.OnComponentRemoved(pair2.Key, pair2.Value);
                    }
                }
            }
        }

        public bool Contains(Type type)
        {
            using (Dictionary<Type, MyComponentBase>.KeyCollection.Enumerator enumerator = this.m_components.Keys.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Type current = enumerator.Current;
                    if (type.IsAssignableFrom(current))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Deserialize(MyObjectBuilder_ComponentContainer builder)
        {
            if ((builder != null) && (builder.Components != null))
            {
                foreach (MyObjectBuilder_ComponentContainer.ComponentData data in builder.Components)
                {
                    MyComponentBase component = null;
                    Type createdInstanceType = MyComponentFactory.GetCreatedInstanceType(data.Component.TypeId);
                    Type type = MyComponentTypeFactory.GetType(data.TypeId);
                    Type componentType = MyComponentTypeFactory.GetComponentType(createdInstanceType);
                    if (componentType != null)
                    {
                        type = componentType;
                    }
                    bool flag = this.TryGet(type, out component);
                    if ((flag && (createdInstanceType != component.GetType())) && (createdInstanceType != typeof(MyHierarchyComponentBase)))
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        component = MyComponentFactory.CreateInstanceByTypeId(data.Component.TypeId);
                    }
                    component.Deserialize(data.Component);
                    if (!flag)
                    {
                        this.Add(type, component);
                    }
                }
            }
        }

        public T Get<T>() where T: MyComponentBase
        {
            MyComponentBase base2;
            this.m_components.TryGetValue(typeof(T), out base2);
            return (T) base2;
        }

        public Dictionary<Type, MyComponentBase>.KeyCollection GetComponentTypes() => 
            this.m_components.Keys;

        public Dictionary<Type, MyComponentBase>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_components.Values.GetEnumerator();

        public bool Has<T>() where T: MyComponentBase => 
            this.m_components.ContainsKey(typeof(T));

        public virtual void Init(MyContainerDefinition definition)
        {
        }

        public void OnAddedToScene()
        {
            foreach (KeyValuePair<Type, MyComponentBase> pair in this.m_components)
            {
                pair.Value.OnAddedToScene();
            }
        }

        protected virtual void OnComponentAdded(Type t, MyComponentBase component)
        {
        }

        protected virtual void OnComponentRemoved(Type t, MyComponentBase component)
        {
        }

        public void OnRemovedFromScene()
        {
            foreach (KeyValuePair<Type, MyComponentBase> pair in this.m_components)
            {
                pair.Value.OnRemovedFromScene();
            }
        }

        public void Remove<T>() where T: MyComponentBase
        {
            Type t = typeof(T);
            this.Remove(t);
        }

        public void Remove(Type t)
        {
            MyComponentBase base2;
            if (this.m_components.TryGetValue(t, out base2))
            {
                this.RemoveComponentInternal(t, base2);
            }
        }

        public void Remove(Type t, MyComponentBase component)
        {
            MyComponentBase base2 = null;
            this.m_components.TryGetValue(t, out base2);
            if (base2 != null)
            {
                IMyComponentAggregate aggregate = base2 as IMyComponentAggregate;
                if (aggregate == null)
                {
                    this.RemoveComponentInternal(t, component);
                }
                else
                {
                    aggregate.RemoveComponent(component);
                }
            }
        }

        private void RemoveComponentInternal(Type t, MyComponentBase c)
        {
            c.SetContainer(null);
            this.m_components.Remove(t);
            this.OnComponentRemoved(t, c);
        }

        public MyObjectBuilder_ComponentContainer Serialize(bool copy = false)
        {
            MyObjectBuilder_ComponentContainer container2;
            using (MyUtils.ClearRangeToken<KeyValuePair<Type, MyComponentBase>> token = MyUtils.ReuseCollectionNested<KeyValuePair<Type, MyComponentBase>>(ref m_tmpSerializedComponents))
            {
                foreach (KeyValuePair<Type, MyComponentBase> pair in this.m_components)
                {
                    if (pair.Value.IsSerialized())
                    {
                        token.Add(pair);
                    }
                }
                if (token.Collection.Count == 0)
                {
                    container2 = null;
                }
                else
                {
                    MyObjectBuilder_ComponentContainer container = new MyObjectBuilder_ComponentContainer();
                    foreach (KeyValuePair<Type, MyComponentBase> pair2 in token)
                    {
                        MyObjectBuilder_ComponentBase base2 = pair2.Value.Serialize(copy);
                        if (base2 != null)
                        {
                            MyObjectBuilder_ComponentContainer.ComponentData item = new MyObjectBuilder_ComponentContainer.ComponentData();
                            item.TypeId = pair2.Key.Name;
                            item.Component = base2;
                            container.Components.Add(item);
                        }
                    }
                    container2 = container;
                }
            }
            return container2;
        }

        public bool TryGet<T>(out T component) where T: MyComponentBase
        {
            MyComponentBase base2;
            bool flag1 = this.m_components.TryGetValue(typeof(T), out base2);
            component = (T) base2;
            return flag1;
        }

        public bool TryGet(Type type, out MyComponentBase component) => 
            this.m_components.TryGetValue(type, out component);
    }
}

