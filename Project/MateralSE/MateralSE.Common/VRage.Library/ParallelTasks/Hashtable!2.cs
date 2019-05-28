namespace ParallelTasks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Threading;

    public class Hashtable<TKey, TData> : IEnumerable<KeyValuePair<TKey, TData>>, IEnumerable
    {
        private static readonly EqualityComparer<TKey> KeyComparer;
        public volatile HashtableNode<TKey, TData>[] array;
        private SpinLock writeLock;
        private static readonly HashtableNode<TKey, TData> DeletedNode;

        static Hashtable()
        {
            Hashtable<TKey, TData>.KeyComparer = EqualityComparer<TKey>.Default;
            TKey key = default(TKey);
            TData data = default(TData);
            Hashtable<TKey, TData>.DeletedNode = new HashtableNode<TKey, TData>(key, data, HashtableToken.Deleted);
        }

        public Hashtable(int initialCapacity)
        {
            if (initialCapacity < 1)
            {
                throw new ArgumentOutOfRangeException("initialCapacity", "cannot be < 1");
            }
            this.array = new HashtableNode<TKey, TData>[initialCapacity];
            this.writeLock = new SpinLock();
        }

        public void Add(TKey key, TData data)
        {
            try
            {
                this.writeLock.Enter();
                if (!this.Insert(this.array, key, data))
                {
                    this.Resize();
                    this.Insert(this.array, key, data);
                }
            }
            finally
            {
                this.writeLock.Exit();
            }
        }

        private bool Find(TKey key, out HashtableNode<TKey, TData> node)
        {
            node = new HashtableNode<TKey, TData>();
            HashtableNode<TKey, TData>[] array = this.array;
            int num = Math.Abs(GetHashCode_HashTable<TKey>.GetHashCode(key)) % array.Length;
            int index = num;
            while (true)
            {
                HashtableNode<TKey, TData> node2 = array[index];
                if (node2.Token == HashtableToken.Empty)
                {
                    return false;
                }
                if ((node2.Token != HashtableToken.Deleted) && Hashtable<TKey, TData>.KeyComparer.Equals(key, node2.Key))
                {
                    node = node2;
                    return true;
                }
                index = (index + 1) % array.Length;
                if (index == num)
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TData>> GetEnumerator() => 
            new HashTableEnumerator<TKey, TData>((Hashtable<TKey, TData>) this);

        private bool Insert(HashtableNode<TKey, TData>[] table, TKey key, TData data)
        {
            int num = Math.Abs(GetHashCode_HashTable<TKey>.GetHashCode(key)) % table.Length;
            int index = num;
            bool flag = false;
            while (true)
            {
                HashtableNode<TKey, TData> node = table[index];
                if (((node.Token != HashtableToken.Empty) && (node.Token != HashtableToken.Deleted)) && !Hashtable<TKey, TData>.KeyComparer.Equals(key, node.Key))
                {
                    index = (index + 1) % table.Length;
                    if (index != num)
                    {
                        continue;
                    }
                }
                else
                {
                    table[index] = new HashtableNode<TKey, TData> { 
                        Key = key,
                        Data = data,
                        Token = HashtableToken.Used
                    };
                    flag = true;
                }
                return flag;
            }
        }

        public void Remove(TKey key)
        {
            try
            {
                this.writeLock.Enter();
                HashtableNode<TKey, TData>[] array = this.array;
                int num = Math.Abs(GetHashCode_HashTable<TKey>.GetHashCode(key)) % array.Length;
                int index = num;
                while (true)
                {
                    HashtableNode<TKey, TData> node = array[index];
                    if (node.Token != HashtableToken.Empty)
                    {
                        if ((node.Token == HashtableToken.Deleted) || !Hashtable<TKey, TData>.KeyComparer.Equals(key, node.Key))
                        {
                            index = (index + 1) % array.Length;
                        }
                        else
                        {
                            array[index] = Hashtable<TKey, TData>.DeletedNode;
                        }
                        if (index != num)
                        {
                            continue;
                        }
                    }
                    break;
                }
            }
            finally
            {
                this.writeLock.Exit();
            }
        }

        private void Resize()
        {
            HashtableNode<TKey, TData>[] table = new HashtableNode<TKey, TData>[this.array.Length * 2];
            for (int i = 0; i < this.array.Length; i++)
            {
                HashtableNode<TKey, TData> node = this.array[i];
                if (node.Token == HashtableToken.Used)
                {
                    this.Insert(table, node.Key, node.Data);
                }
            }
            this.array = table;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGet(TKey key, out TData data)
        {
            HashtableNode<TKey, TData> node;
            if (this.Find(key, out node))
            {
                data = node.Data;
                return true;
            }
            data = default(TData);
            return false;
        }

        public void UnsafeSet(TKey key, TData value)
        {
            bool flag = false;
            while (true)
            {
                HashtableNode<TKey, TData>[] array = this.array;
                int num = Math.Abs(GetHashCode_HashTable<TKey>.GetHashCode(key)) % array.Length;
                int index = num;
                while (true)
                {
                    HashtableNode<TKey, TData> node = array[index];
                    if (!Hashtable<TKey, TData>.KeyComparer.Equals(key, node.Key))
                    {
                        index = (index + 1) % array.Length;
                        if (index != num)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        array[index] = new HashtableNode<TKey, TData> { 
                            Key = key,
                            Data = value,
                            Token = HashtableToken.Used
                        };
                        flag = true;
                    }
                    if (array != this.array)
                    {
                        break;
                    }
                    if (!flag)
                    {
                        this.Add(key, value);
                    }
                    return;
                }
            }
        }
    }
}

