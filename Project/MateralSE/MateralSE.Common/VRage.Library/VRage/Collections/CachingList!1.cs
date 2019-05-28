namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class CachingList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        private List<T> m_list;
        private List<T> m_toAdd;
        private List<T> m_toRemove;

        public CachingList()
        {
            this.m_list = new List<T>();
            this.m_toAdd = new List<T>();
            this.m_toRemove = new List<T>();
        }

        public CachingList(int capacity)
        {
            this.m_list = new List<T>();
            this.m_toAdd = new List<T>();
            this.m_toRemove = new List<T>();
            this.m_list = new List<T>(capacity);
        }

        public void Add(T entity)
        {
            if (this.m_toRemove.Contains(entity))
            {
                this.m_toRemove.Remove(entity);
            }
            else
            {
                this.m_toAdd.Add(entity);
            }
        }

        public void ApplyAdditions()
        {
            this.m_list.AddList<T>(this.m_toAdd);
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
                this.m_list.Remove(local);
            }
            this.m_toRemove.Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < this.m_list.Count; i++)
            {
                this.Remove(this.m_list[i], false);
            }
        }

        public void ClearImmediate()
        {
            this.m_toAdd.Clear();
            this.m_toRemove.Clear();
            this.m_list.Clear();
        }

        [Conditional("DEBUG")]
        public void DebugCheckEmpty()
        {
        }

        public List<T>.Enumerator GetEnumerator() => 
            this.m_list.GetEnumerator();

        public void Remove(T entity, bool immediate = false)
        {
            int index = this.m_toAdd.IndexOf(entity);
            if (index >= 0)
            {
                this.m_toAdd.RemoveAt(index);
            }
            else
            {
                this.m_toRemove.Add(entity);
            }
            if (immediate)
            {
                this.m_list.Remove(entity);
                this.m_toRemove.Remove(entity);
            }
        }

        public void RemoveAtImmediately(int index)
        {
            if ((index >= 0) && (index < this.m_list.Count))
            {
                this.m_list.RemoveAt(index);
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            this.m_list.Sort(comparer);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString() => 
            $"Count = {this.m_list.Count}; ToAdd = {this.m_toAdd.Count}; ToRemove = {this.m_toRemove.Count}";

        public int Count =>
            this.m_list.Count;

        public T this[int index] =>
            this.m_list[index];
    }
}

