namespace System.Collections.Generic
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class DictionaryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static void AssertEmpty<K, V>(this Dictionary<K, V> collection)
        {
            if (collection.Count != 0)
            {
                collection.Clear();
            }
        }

        public static KeyValuePair<K, V> FirstPair<K, V>(this Dictionary<K, V> dictionary)
        {
            Dictionary<K, V>.Enumerator enumerator = dictionary.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static TValue GetOrAdd<TKey, TValue, TContext>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TContext context, Func<TContext, TKey, TValue> activator)
        {
            TValue orAdd;
            if (!dictionary.TryGetValue(key, out orAdd))
            {
                orAdd = dictionary.GetOrAdd(key, activator(context, key));
            }
            return orAdd;
        }

        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dictionary, K key)
        {
            V local;
            dictionary.TryGetValue(key, out local);
            return local;
        }

        public static V GetValueOrDefault<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V defaultValue)
        {
            V local;
            return (dictionary.TryGetValue(key, out local) ? local : defaultValue);
        }

        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dictionary, K key, V defaultValue)
        {
            V local;
            return (dictionary.TryGetValue(key, out local) ? local : defaultValue);
        }

        public static void Remove<K, V>(this ConcurrentDictionary<K, V> dictionary, K key)
        {
            V local;
            dictionary.TryRemove(key, out local);
        }
    }
}

