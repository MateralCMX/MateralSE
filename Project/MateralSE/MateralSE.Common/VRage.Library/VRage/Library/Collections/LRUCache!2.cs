namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;

    public class LRUCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private static HashSet<int> m_debugEntrySet;
        private int m_first;
        private int m_last;
        private readonly IEqualityComparer<TKey> m_comparer;
        private readonly Dictionary<TKey, int> m_index;
        private readonly CacheEntry<TKey, TValue>[] m_entries;
        private readonly FastResourceLock m_lock;
        public Action<TKey, TValue> OnItemDiscarded;
        private const int Null = -1;

        static LRUCache()
        {
            LRUCache<TKey, TValue>.m_debugEntrySet = new HashSet<int>();
        }

        public LRUCache(int cacheSize, IEqualityComparer<TKey> comparer = null)
        {
            this.m_lock = new FastResourceLock();
            this.m_comparer = comparer ?? EqualityComparer<TKey>.Default;
            this.m_entries = new CacheEntry<TKey, TValue>[cacheSize];
            this.m_index = new Dictionary<TKey, int>(cacheSize, this.m_comparer);
            this.ResetInternal();
        }

        private void AddFirst(int entryIndex)
        {
            this.m_entries[this.m_first].Prev = entryIndex;
            this.m_entries[entryIndex].Next = this.m_first;
            this.m_first = entryIndex;
        }

        [Conditional("__UNUSED__")]
        private void AssertConsistent()
        {
            int num = 0;
            while (num < 3)
            {
                int item = 0;
                while (true)
                {
                    if (item >= this.m_entries.Length)
                    {
                        switch (num)
                        {
                            case 0:
                                for (int i = this.m_first; i != -1; i = this.m_entries[i].Next)
                                {
                                    bool flag = LRUCache<TKey, TValue>.m_debugEntrySet.Remove(i);
                                }
                                break;

                            case 1:
                                for (int i = this.m_last; i != -1; i = this.m_entries[i].Prev)
                                {
                                    bool flag2 = LRUCache<TKey, TValue>.m_debugEntrySet.Remove(i);
                                }
                                break;

                            case 2:
                                foreach (KeyValuePair<TKey, int> pair in this.m_index)
                                {
                                    bool flag3 = LRUCache<TKey, TValue>.m_debugEntrySet.Remove(pair.Value);
                                }
                                LRUCache<TKey, TValue>.m_debugEntrySet.Clear();
                                break;

                            default:
                                break;
                        }
                        num++;
                        break;
                    }
                    LRUCache<TKey, TValue>.m_debugEntrySet.Add(item);
                    item++;
                }
            }
        }

        private void CleanEntry(int entryIndex)
        {
            if ((this.OnItemDiscarded != null) && (this.m_entries[entryIndex].Data != null))
            {
                this.OnItemDiscarded(this.m_entries[entryIndex].Key, this.m_entries[entryIndex].Data);
            }
            this.m_index.Remove(this.m_entries[entryIndex].Key);
            this.m_entries[entryIndex].Key = default(TKey);
            this.m_entries[entryIndex].Data = default(TValue);
        }

        [IteratorStateMachine(typeof(<GetEnumerator>d__31))]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            <GetEnumerator>d__31<TKey, TValue> d__1 = new <GetEnumerator>d__31<TKey, TValue>(0);
            d__1.<>4__this = (LRUCache<TKey, TValue>) this;
            return d__1;
        }

        public TValue Read(TKey key)
        {
            TValue data;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                try
                {
                    int num;
                    if (!this.m_index.TryGetValue(key, out num))
                    {
                        data = default(TValue);
                    }
                    else
                    {
                        if (num != this.m_first)
                        {
                            this.Remove(num);
                            this.AddFirst(num);
                        }
                        data = this.m_entries[num].Data;
                    }
                }
                finally
                {
                }
            }
            return data;
        }

        private void ReinsertLast(int entryIndex)
        {
            this.m_entries[this.m_last].Next = entryIndex;
            this.m_entries[entryIndex].Prev = this.m_last;
            this.m_entries[entryIndex].Next = -1;
            this.m_last = entryIndex;
        }

        public void Remove(TKey key)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                try
                {
                    int num;
                    if (this.m_index.TryGetValue(key, out num))
                    {
                        this.Remove(num);
                        this.CleanEntry(num);
                        this.ReinsertLast(num);
                    }
                }
                finally
                {
                }
            }
        }

        private void Remove(int entryIndex)
        {
            int prev = this.m_entries[entryIndex].Prev;
            int next = this.m_entries[entryIndex].Next;
            if (prev != -1)
            {
                this.m_entries[prev].Next = this.m_entries[entryIndex].Next;
            }
            else
            {
                this.m_first = this.m_entries[entryIndex].Next;
            }
            if (next != -1)
            {
                this.m_entries[next].Prev = this.m_entries[entryIndex].Prev;
            }
            else
            {
                this.m_last = this.m_entries[entryIndex].Prev;
            }
            this.m_entries[entryIndex].Prev = -1;
            this.m_entries[entryIndex].Next = -1;
        }

        private void RemoveLast()
        {
            int prev = this.m_entries[this.m_last].Prev;
            this.m_entries[prev].Next = -1;
            this.m_entries[this.m_last].Prev = -1;
            if ((this.OnItemDiscarded != null) && (this.m_entries[this.m_last].Data != null))
            {
                this.OnItemDiscarded(this.m_entries[this.m_last].Key, this.m_entries[this.m_last].Data);
            }
            this.m_last = prev;
        }

        public int RemoveWhere(Func<TKey, TValue, bool> predicate)
        {
            int num = 0;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                int first = this.m_first;
                while (first != -1)
                {
                    int index = first;
                    first = this.m_entries[index].Next;
                    if (predicate(this.m_entries[index].Key, this.m_entries[index].Data))
                    {
                        this.Remove(index);
                        this.CleanEntry(index);
                        this.ReinsertLast(index);
                        num++;
                    }
                }
            }
            return num;
        }

        public void Reset()
        {
            if (this.m_index.Count > 0)
            {
                if (this.OnItemDiscarded != null)
                {
                    for (int i = 0; i < this.m_entries.Length; i++)
                    {
                        if (this.m_entries[i].Data != null)
                        {
                            this.OnItemDiscarded(this.m_entries[i].Key, this.m_entries[i].Data);
                        }
                    }
                }
                this.ResetInternal();
            }
        }

        private void ResetInternal()
        {
            CacheEntry<TKey, TValue> entry;
            entry.Data = default(TValue);
            entry.Key = default(TKey);
            for (int i = 0; i < this.m_entries.Length; i++)
            {
                entry.Prev = i - 1;
                entry.Next = i + 1;
                this.m_entries[i] = entry;
            }
            this.m_first = 0;
            this.m_last = this.m_entries.Length - 1;
            this.m_entries[this.m_last].Next = -1;
            this.m_index.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryPeek(TKey key, out TValue value)
        {
            bool flag;
            using (this.m_lock.AcquireSharedUsing())
            {
                int num;
                if (this.m_index.TryGetValue(key, out num))
                {
                    value = this.m_entries[num].Data;
                    flag = true;
                }
                else
                {
                    value = default(TValue);
                    flag = false;
                }
            }
            return flag;
        }

        public bool TryRead(TKey key, out TValue value)
        {
            bool flag;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                try
                {
                    int num;
                    if (!this.m_index.TryGetValue(key, out num))
                    {
                        value = default(TValue);
                        flag = false;
                    }
                    else
                    {
                        if (num != this.m_first)
                        {
                            this.Remove(num);
                            this.AddFirst(num);
                        }
                        value = this.m_entries[num].Data;
                        flag = true;
                    }
                }
                finally
                {
                }
            }
            return flag;
        }

        public void Write(TKey key, TValue value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                int num;
                if (this.m_index.TryGetValue(key, out num))
                {
                    this.m_entries[num].Data = value;
                }
                else
                {
                    int last = this.m_last;
                    this.RemoveLast();
                    if (this.m_entries[last].Key != null)
                    {
                        this.m_index.Remove(this.m_entries[last].Key);
                    }
                    this.m_entries[last].Key = key;
                    this.m_entries[last].Data = value;
                    this.AddFirst(last);
                    this.m_index.Add(key, last);
                }
            }
        }

        public float Usage =>
            (((float) this.m_index.Count) / ((float) this.m_entries.Length));

        public int Count =>
            this.m_index.Count;

        public int Capacity =>
            this.m_entries.Length;

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__31 : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private KeyValuePair<TKey, TValue> <>2__current;
            public LRUCache<TKey, TValue> <>4__this;
            private FastResourceLockExtensions.MySharedLock <>7__wrap1;
            private Dictionary<TKey, int>.ValueCollection.Enumerator <>7__wrap2;

            [DebuggerHidden]
            public <GetEnumerator>d__31(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                this.<>7__wrap1.Dispose();
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                this.<>7__wrap2.Dispose();
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    LRUCache<TKey, TValue> cache = this.<>4__this;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = cache.m_lock.AcquireSharedUsing();
                        this.<>1__state = -3;
                        this.<>7__wrap2 = cache.m_index.Values.GetEnumerator();
                        this.<>1__state = -4;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -4;
                    }
                    else
                    {
                        return false;
                    }
                    if (this.<>7__wrap2.MoveNext())
                    {
                        int current = this.<>7__wrap2.Current;
                        this.<>2__current = new KeyValuePair<TKey, TValue>(cache.m_entries[current].Key, cache.m_entries[current].Data);
                        this.<>1__state = 1;
                        flag = true;
                    }
                    else
                    {
                        this.<>m__Finally2();
                        this.<>7__wrap2 = new Dictionary<TKey, int>.ValueCollection.Enumerator();
                        this.<>m__Finally1();
                        this.<>7__wrap1 = new FastResourceLockExtensions.MySharedLock();
                        flag = false;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if (((num - -4) <= 1) || (num == 1))
                {
                    try
                    {
                        if ((num == -4) || (num == 1))
                        {
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally2();
                            }
                        }
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("Prev={Prev}, Next={Next}, Key={Key}, Data={Data}")]
        private struct CacheEntry
        {
            public int Prev;
            public int Next;
            public TValue Data;
            public TKey Key;
        }
    }
}

