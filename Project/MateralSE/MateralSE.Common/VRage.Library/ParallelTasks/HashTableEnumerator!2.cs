namespace ParallelTasks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class HashTableEnumerator<TKey, TData> : IEnumerator<KeyValuePair<TKey, TData>>, IDisposable, IEnumerator
    {
        private int currentIndex;
        private Hashtable<TKey, TData> table;

        public HashTableEnumerator(Hashtable<TKey, TData> table)
        {
            this.currentIndex = -1;
            this.table = table;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            while (true)
            {
                this.currentIndex++;
                if (this.table.array.Length <= this.currentIndex)
                {
                    return false;
                }
                HashtableNode<TKey, TData> node = this.table.array[this.currentIndex];
                if (node.Token == HashtableToken.Used)
                {
                    this.Current = new KeyValuePair<TKey, TData>(node.Key, node.Data);
                    return true;
                }
            }
        }

        public void Reset()
        {
            this.currentIndex = -1;
        }

        public KeyValuePair<TKey, TData> Current { get; private set; }

        object IEnumerator.Current =>
            this.Current;
    }
}

