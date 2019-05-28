namespace VRage.Collections
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class HeapItem<K>
    {
        protected HeapItem()
        {
        }

        public int HeapIndex { get; internal set; }

        public K HeapKey { get; internal set; }
    }
}

