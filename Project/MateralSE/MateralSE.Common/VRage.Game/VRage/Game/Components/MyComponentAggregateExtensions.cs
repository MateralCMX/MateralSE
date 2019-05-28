namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class MyComponentAggregateExtensions
    {
        public static void AddComponent(this IMyComponentAggregate aggregate, MyComponentBase component)
        {
            if (component.ContainerBase != null)
            {
                component.OnBeforeRemovedFromContainer();
            }
            aggregate.ChildList.AddComponent(component);
            component.SetContainer(aggregate.ContainerBase);
            aggregate.AfterComponentAdd(component);
        }

        public static void AttachComponent(this IMyComponentAggregate aggregate, MyComponentBase component)
        {
            aggregate.ChildList.AddComponent(component);
        }

        public static void DetachComponent(this IMyComponentAggregate aggregate, MyComponentBase component)
        {
            int componentIndex = aggregate.ChildList.GetComponentIndex(component);
            if (componentIndex != -1)
            {
                aggregate.ChildList.RemoveComponentAt(componentIndex);
            }
        }

        public static void GetComponentsFlattened(this IMyComponentAggregate aggregate, List<MyComponentBase> output)
        {
            foreach (MyComponentBase base2 in aggregate.ChildList.Reader)
            {
                IMyComponentAggregate aggregate2 = base2 as IMyComponentAggregate;
                if (aggregate2 != null)
                {
                    aggregate2.GetComponentsFlattened(output);
                    continue;
                }
                output.Add(base2);
            }
        }

        public static bool RemoveComponent(this IMyComponentAggregate aggregate, MyComponentBase component)
        {
            int componentIndex = aggregate.ChildList.GetComponentIndex(component);
            if (componentIndex != -1)
            {
                aggregate.BeforeComponentRemove(component);
                component.SetContainer(null);
                aggregate.ChildList.RemoveComponentAt(componentIndex);
                return true;
            }
            using (List<MyComponentBase>.Enumerator enumerator = aggregate.ChildList.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IMyComponentAggregate current = enumerator.Current as IMyComponentAggregate;
                    if ((current != null) && current.RemoveComponent(component))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

