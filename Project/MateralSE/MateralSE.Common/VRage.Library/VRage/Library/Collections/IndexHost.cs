namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections.__helper_namespace;

    public class IndexHost
    {
        private readonly ComponentIndex NullIndex = new ComponentIndex(new List<Type>());
        private readonly Dictionary<List<Type>, WeakReference> m_indexes = new Dictionary<List<Type>, WeakReference>(new TypeListComparer());

        public IndexHost()
        {
            this.m_indexes[this.NullIndex.Types] = new WeakReference(this.NullIndex);
        }

        public ComponentIndex GetAfterInsert(ComponentIndex current, Type newType, out int insertionPoint)
        {
            List<Type> types = current.Types.ToList<Type>();
            insertionPoint = ~types.BinarySearch(newType, TypeComparer.Instance);
            types.Insert(insertionPoint, newType);
            return this.GetForTypes(types);
        }

        public ComponentIndex GetAfterRemove(ComponentIndex current, Type oldType, out int removalPoint)
        {
            List<Type> types = current.Types.ToList<Type>();
            removalPoint = current.Index[oldType];
            types.RemoveAt(removalPoint);
            return this.GetForTypes(types);
        }

        public ComponentIndex GetEmptyComponentIndex() => 
            this.NullIndex;

        private ComponentIndex GetForTypes(List<Type> types)
        {
            ComponentIndex target;
            WeakReference reference;
            if (this.m_indexes.TryGetValue(types, out reference) && reference.IsAlive)
            {
                target = (ComponentIndex) reference.Target;
            }
            else
            {
                if (reference == null)
                {
                    reference = new WeakReference(null);
                }
                target = new ComponentIndex(types);
                this.m_indexes[types] = reference;
            }
            return target;
        }
    }
}

