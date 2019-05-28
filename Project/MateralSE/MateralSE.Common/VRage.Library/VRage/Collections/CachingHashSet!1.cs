namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CachingHashSet<T> : IEnumerable<T>, IEnumerable
    {
        private HashSet<T> m_hashSet;
        private HashSet<T> m_toAdd;
        private HashSet<T> m_toRemove;

        public CachingHashSet()
        {
            this.m_hashSet = new HashSet<T>();
            this.m_toAdd = new HashSet<T>();
            this.m_toRemove = new HashSet<T>();
        }

        public void Add(T item)
        {
            if (!this.m_toRemove.Remove(item) && !this.m_hashSet.Contains(item))
            {
                this.m_toAdd.Add(item);
            }
        }

        public void ApplyAdditions()
        {
            foreach (T local in this.m_toAdd)
            {
                this.m_hashSet.Add(local);
            }
            this.m_toAdd.Clear();
        }

        public void ApplyChanges()
        {
            this.ApplyAdditions();
            this.ApplyRemovals();
        }

        public void ApplyRemovals()
        {
            foreach (T local in this.m_toRemove)
            {
                this.m_hashSet.Remove(local);
            }
            this.m_toRemove.Clear();
        }

        public void Clear()
        {
            this.m_hashSet.Clear();
            this.m_toAdd.Clear();
            this.m_toRemove.Clear();
        }

        public bool Contains(T item) => 
            this.m_hashSet.Contains(item);

        public HashSet<T>.Enumerator GetEnumerator() => 
            this.m_hashSet.GetEnumerator();

        public void Remove(T item, bool immediate = false)
        {
            if (immediate)
            {
                this.m_toAdd.Remove(item);
                this.m_hashSet.Remove(item);
                this.m_toRemove.Remove(item);
            }
            else if (!this.m_toAdd.Remove(item) && this.m_hashSet.Contains(item))
            {
                this.m_toRemove.Add(item);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString() => 
            $"Count = {this.m_hashSet.Count}; ToAdd = {this.m_toAdd.Count}; ToRemove = {this.m_toRemove.Count}";

        public int Count =>
            this.m_hashSet.Count;
    }
}

