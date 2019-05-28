namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class CachingDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        private Dictionary<K, V> m_dictionary;
        private List<KeyValuePair<K, V>> m_additionsAndModifications;
        private List<K> m_removals;
        private static K m_keyToCompare;
        private static Predicate<K> m_keyEquals;
        private static Predicate<KeyValuePair<K, V>> m_keyValueEquals;

        static CachingDictionary()
        {
            CachingDictionary<K, V>.m_keyEquals = new Predicate<K>(CachingDictionary<K, V>.KeyEquals);
            CachingDictionary<K, V>.m_keyValueEquals = new Predicate<KeyValuePair<K, V>>(CachingDictionary<K, V>.KeyValueEquals);
        }

        public CachingDictionary()
        {
            this.m_dictionary = new Dictionary<K, V>();
            this.m_additionsAndModifications = new List<KeyValuePair<K, V>>();
            this.m_removals = new List<K>();
        }

        public void Add(K key, V value, bool immediate = false)
        {
            if (immediate)
            {
                this.m_dictionary[key] = value;
            }
            else
            {
                this.m_additionsAndModifications.Add(new KeyValuePair<K, V>(key, value));
                CachingDictionary<K, V>.m_keyToCompare = key;
                this.m_removals.RemoveAll(CachingDictionary<K, V>.m_keyEquals);
            }
        }

        public void ApplyAdditionsAndModifications()
        {
            foreach (KeyValuePair<K, V> pair in this.m_additionsAndModifications)
            {
                this.m_dictionary[pair.Key] = pair.Value;
            }
            this.m_additionsAndModifications.Clear();
        }

        public void ApplyChanges()
        {
            this.ApplyAdditionsAndModifications();
            this.ApplyRemovals();
        }

        public void ApplyRemovals()
        {
            foreach (K local in this.m_removals)
            {
                this.m_dictionary.Remove(local);
            }
            this.m_removals.Clear();
        }

        public void Clear()
        {
            this.m_dictionary.Clear();
            this.m_additionsAndModifications.Clear();
            this.m_removals.Clear();
        }

        public bool ContainsKey(K key) => 
            this.m_dictionary.ContainsKey(key);

        public Dictionary<K, V>.Enumerator GetEnumerator() => 
            this.m_dictionary.GetEnumerator();

        public bool HasChanges() => 
            ((this.m_additionsAndModifications.Count > 0) || (this.m_removals.Count > 0));

        private static bool KeyEquals(K key) => 
            EqualityComparer<K>.Default.Equals(key, CachingDictionary<K, V>.m_keyToCompare);

        private static bool KeyValueEquals(KeyValuePair<K, V> keyValue) => 
            EqualityComparer<K>.Default.Equals(keyValue.Key, CachingDictionary<K, V>.m_keyToCompare);

        public void Remove(K key, bool immediate = false)
        {
            if (immediate)
            {
                this.m_dictionary.Remove(key);
            }
            else
            {
                this.m_removals.Add(key);
                CachingDictionary<K, V>.m_keyToCompare = key;
                this.m_additionsAndModifications.RemoveAll(CachingDictionary<K, V>.m_keyValueEquals);
            }
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGetValue(K key, out V value) => 
            this.m_dictionary.TryGetValue(key, out value);

        private static K KeyToCompare
        {
            set => 
                (CachingDictionary<K, V>.m_keyToCompare = value);
        }

        public DictionaryReader<K, V> Reader =>
            this.m_dictionary;

        public V this[K key]
        {
            get => 
                this.m_dictionary[key];
            set => 
                this.Add(key, value, false);
        }

        public Dictionary<K, V>.KeyCollection Keys =>
            this.m_dictionary.Keys;

        public Dictionary<K, V>.ValueCollection Values =>
            this.m_dictionary.Values;
    }
}

