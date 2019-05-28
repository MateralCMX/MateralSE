namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public static class PoolManager
    {
        private static readonly ConcurrentDictionary<Type, IConcurrentPool> Pools = new ConcurrentDictionary<Type, IConcurrentPool>();

        public static TPooled Get<TPooled>() where TPooled: new()
        {
            IConcurrentPool pool;
            Type key = typeof(TPooled);
            if (!Pools.TryGetValue(key, out pool))
            {
                Pools[key] = pool = GetPool<TPooled>(key, 0);
            }
            return (TPooled) pool.Get();
        }

        public static ReturnHandle<TPooled> Get<TPooled>(out TPooled poolObject) where TPooled: new()
        {
            IConcurrentPool pool;
            Type key = typeof(TPooled);
            if (!Pools.TryGetValue(key, out pool))
            {
                Pools[key] = pool = GetPool<TPooled>(key, 0);
            }
            poolObject = (TPooled) pool.Get();
            return new ReturnHandle<TPooled>(poolObject);
        }

        private static IConcurrentPool GetPool<TPooled>(Type type, int preallocated = 0) where TPooled: new()
        {
            Type type2 = typeof(ICollection<>);
            foreach (Type type3 in type.GetInterfaces())
            {
                if (type3.IsGenericType && (type3.GetGenericTypeDefinition() == type2))
                {
                    Type[] typeArguments = new Type[] { type, type3.GetGenericArguments()[0] };
                    object[] args = new object[] { preallocated };
                    return (IConcurrentPool) Activator.CreateInstance(typeof(MyConcurrentCollectionPool<,>).MakeGenericType(typeArguments), args);
                }
            }
            return new MyConcurrentPool<TPooled>(preallocated, null, 0x2710, null);
        }

        public static void Preallocate<TPooled>(int size) where TPooled: new()
        {
            Type key = typeof(TPooled);
            if (!Pools.ContainsKey(key))
            {
                Pools[key] = GetPool<TPooled>(key, size);
            }
        }

        public static void Return<TPooled>(ref TPooled obj) where TPooled: new()
        {
            IConcurrentPool pool;
            Type key = typeof(TPooled);
            if (Pools.TryGetValue(key, out pool))
            {
                pool.Return((TPooled) obj);
            }
            obj = default(TPooled);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ReturnHandle<TObject> : IDisposable where TObject: new()
        {
            private TObject m_obj;
            public ReturnHandle(TObject data)
            {
                this = (PoolManager.ReturnHandle<>) new PoolManager.ReturnHandle<TObject>();
                this.m_obj = data;
            }

            public void Dispose()
            {
                PoolManager.Return<TObject>(ref this.m_obj);
            }
        }
    }
}

