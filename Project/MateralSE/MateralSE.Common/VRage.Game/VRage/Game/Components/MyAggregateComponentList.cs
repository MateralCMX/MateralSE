namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;

    public sealed class MyAggregateComponentList
    {
        private List<MyComponentBase> m_components = new List<MyComponentBase>();

        public void AddComponent(MyComponentBase component)
        {
            this.m_components.Add(component);
        }

        public bool Contains(MyComponentBase component)
        {
            if (this.m_components.Contains(component))
            {
                return true;
            }
            using (List<MyComponentBase>.Enumerator enumerator = this.m_components.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyComponentBase current = enumerator.Current;
                    if ((current is IMyComponentAggregate) && (current as IMyComponentAggregate).ChildList.Contains(component))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetComponentIndex(MyComponentBase component) => 
            this.m_components.IndexOf(component);

        public bool RemoveComponent(MyComponentBase component)
        {
            if (this.Contains(component))
            {
                component.OnBeforeRemovedFromContainer();
                if (this.m_components.Remove(component))
                {
                    return true;
                }
                using (List<MyComponentBase>.Enumerator enumerator = this.m_components.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyComponentBase current = enumerator.Current;
                        if ((current is IMyComponentAggregate) && (current as IMyComponentAggregate).ChildList.RemoveComponent(component))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void RemoveComponentAt(int index)
        {
            this.m_components.RemoveAtFast<MyComponentBase>(index);
        }

        public ListReader<MyComponentBase> Reader =>
            new ListReader<MyComponentBase>(this.m_components);
    }
}

