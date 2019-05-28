namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Collections;

    public class MyIndexedComponentContainer<T> where T: class
    {
        private static readonly IndexHost Host;
        private ComponentIndex m_componentIndex;
        private readonly List<T> m_components;

        static MyIndexedComponentContainer()
        {
            MyIndexedComponentContainer<T>.Host = new IndexHost();
        }

        public MyIndexedComponentContainer()
        {
            this.m_components = new List<T>();
            this.m_componentIndex = MyIndexedComponentContainer<T>.Host.GetEmptyComponentIndex();
        }

        public MyIndexedComponentContainer(MyComponentContainerTemplate<T> template)
        {
            this.m_components = new List<T>();
            this.m_components.Capacity = template.Components.Count;
            for (int i = 0; i < template.Components.Count; i++)
            {
                Func<Type, T> func = template.Components[i];
                Type arg = template.Components.m_componentIndex.Types[i];
                this.m_components.Add(func(arg));
            }
            this.m_componentIndex = template.Components.m_componentIndex;
        }

        public void Add(Type slot, T component)
        {
            if (!this.m_componentIndex.Index.ContainsKey(slot))
            {
                int num;
                this.m_componentIndex = MyIndexedComponentContainer<T>.Host.GetAfterInsert(this.m_componentIndex, slot, out num);
                this.m_components.Insert(num, component);
            }
        }

        public void Clear()
        {
            this.m_components.Clear();
            this.m_componentIndex = MyIndexedComponentContainer<T>.Host.GetEmptyComponentIndex();
        }

        public bool Contains<TComponent>() where TComponent: T => 
            this.m_componentIndex.Index.ContainsKey(typeof(TComponent));

        public TComponent GetComponent<TComponent>() where TComponent: T => 
            this[typeof(TComponent)];

        public void Remove(Type slot)
        {
            if (this.m_componentIndex.Index.ContainsKey(slot))
            {
                int num;
                this.m_componentIndex = MyIndexedComponentContainer<T>.Host.GetAfterRemove(this.m_componentIndex, slot, out num);
                this.m_components.RemoveAt(num);
            }
        }

        public TComponent TryGetComponent<TComponent>() where TComponent: class, T => 
            this.TryGetComponent(typeof(TComponent));

        public T TryGetComponent(Type t)
        {
            int num;
            if (this.m_componentIndex.Index.TryGetValue(t, out num))
            {
                return this.m_components[num];
            }
            return default(T);
        }

        public ListReader<T> Components =>
            this.m_components;

        public T this[int index] =>
            this.m_components[index];

        public T this[Type type] =>
            this.m_components[this.m_componentIndex.Index[type]];

        public int Count =>
            this.m_components.Count;
    }
}

