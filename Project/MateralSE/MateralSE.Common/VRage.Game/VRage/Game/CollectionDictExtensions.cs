namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class CollectionDictExtensions
    {
        public static IEnumerable<TVal> GetOrEmpty<TKey, TValCol, TVal>(this Dictionary<TKey, TValCol> self, TKey key) where TValCol: IEnumerable<TVal>
        {
            TValCol local;
            return (self.TryGetValue(key, out local) ? ((IEnumerable<TVal>) local) : Enumerable.Empty<TVal>());
        }

        public static IEnumerable<TVal> GetOrEmpty<TKey, TKey2, TVal>(this Dictionary<TKey, Dictionary<TKey2, TVal>> self, TKey key)
        {
            Dictionary<TKey2, TVal> dictionary;
            return (self.TryGetValue(key, out dictionary) ? ((IEnumerable<TVal>) dictionary.Values) : Enumerable.Empty<TVal>());
        }
    }
}

