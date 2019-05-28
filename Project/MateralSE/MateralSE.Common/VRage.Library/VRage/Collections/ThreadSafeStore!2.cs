namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class ThreadSafeStore<TKey, TValue>
    {
        private readonly object m_lock;
        private Dictionary<TKey, TValue> m_store;
        private readonly Func<TKey, TValue> m_creator;

        public ThreadSafeStore(Func<TKey, TValue> creator)
        {
            this.m_lock = new object();
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }
            this.m_creator = creator;
            this.m_store = new Dictionary<TKey, TValue>();
        }

        private TValue AddValue(TKey key, Func<TKey, TValue> creator = null)
        {
            TValue local = (creator ?? this.m_creator)(key);
            object @lock = this.m_lock;
            lock (@lock)
            {
                if (this.m_store != null)
                {
                    TValue local3;
                    if (!this.m_store.TryGetValue(key, out local3))
                    {
                        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this.m_store) {
                            [key] = local
                        };
                        Thread.MemoryBarrier();
                        this.m_store = dictionary;
                    }
                    else
                    {
                        return local3;
                    }
                }
                else
                {
                    this.m_store = new Dictionary<TKey, TValue>();
                    this.m_store[key] = local;
                }
                return local;
            }
        }

        public TValue Get(TKey key)
        {
            TValue local;
            return (this.m_store.TryGetValue(key, out local) ? local : this.AddValue(key, null));
        }

        public TValue Get(TKey key, Func<TKey, TValue> creator)
        {
            TValue local;
            return (this.m_store.TryGetValue(key, out local) ? local : this.AddValue(key, creator));
        }
    }
}

