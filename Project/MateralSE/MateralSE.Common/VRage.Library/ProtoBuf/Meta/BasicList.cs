namespace ProtoBuf.Meta
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class BasicList : IEnumerable
    {
        private static readonly Node nil = new Node(null, 0);
        protected Node head = nil;

        public int Add(object value)
        {
            Node node;
            this.head = node = this.head.Append(value);
            return (node.Length - 1);
        }

        internal bool Contains(object value)
        {
            using (IEnumerator enumerator = this.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    object current = enumerator.Current;
                    if (Equals(current, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CopyTo(Array array, int offset)
        {
            this.head.CopyTo(array, offset);
        }

        internal static BasicList GetContiguousGroups(int[] keys, object[] values)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Length < keys.Length)
            {
                throw new ArgumentException("Not all keys are covered by values", "values");
            }
            BasicList list = new BasicList();
            Group group = null;
            for (int i = 0; i < keys.Length; i++)
            {
                if ((i == 0) || (keys[i] != keys[i - 1]))
                {
                    group = null;
                }
                if (group == null)
                {
                    group = new Group(keys[i]);
                    list.Add(group);
                }
                group.Items.Add(values[i]);
            }
            return list;
        }

        public IEnumerator GetEnumerator() => 
            new NodeEnumerator(this.head);

        internal int IndexOf(IPredicate predicate) => 
            this.head.IndexOf(predicate);

        internal int IndexOfReference(object instance) => 
            this.head.IndexOfReference(instance);

        public void Trim()
        {
            this.head = this.head.Trim();
        }

        public object TryGet(int index) => 
            this.head.TryGet(index);

        public object this[int index] =>
            this.head[index];

        public int Count =>
            this.head.Length;

        internal class Group
        {
            public readonly int First;
            public readonly BasicList Items;

            public Group(int first)
            {
                this.First = first;
                this.Items = new BasicList();
            }
        }

        internal interface IPredicate
        {
            bool IsMatch(object obj);
        }

        protected sealed class Node
        {
            private readonly object[] data;
            private int length;

            internal Node(object[] data, int length)
            {
                this.data = data;
                this.length = length;
            }

            public BasicList.Node Append(object value)
            {
                object[] data;
                int length = this.length + 1;
                if (this.data == null)
                {
                    data = new object[10];
                }
                else if (this.length != this.data.Length)
                {
                    data = this.data;
                }
                else
                {
                    data = new object[this.data.Length * 2];
                    Array.Copy(this.data, data, this.length);
                }
                data[this.length] = value;
                return new BasicList.Node(data, length);
            }

            internal void CopyTo(Array array, int offset)
            {
                if (this.length > 0)
                {
                    Array.Copy(this.data, 0, array, offset, this.length);
                }
            }

            internal int IndexOf(BasicList.IPredicate predicate)
            {
                for (int i = 0; i < this.length; i++)
                {
                    if (predicate.IsMatch(this.data[i]))
                    {
                        return i;
                    }
                }
                return -1;
            }

            internal int IndexOfReference(object instance)
            {
                for (int i = 0; i < this.length; i++)
                {
                    if (instance == this.data[i])
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void RemoveLastWithMutate()
            {
                if (this.length == 0)
                {
                    throw new InvalidOperationException();
                }
                this.length--;
            }

            public BasicList.Node Trim()
            {
                if ((this.length == 0) || (this.length == this.data.Length))
                {
                    return this;
                }
                object[] destinationArray = new object[this.length];
                Array.Copy(this.data, destinationArray, this.length);
                return new BasicList.Node(destinationArray, this.length);
            }

            public object TryGet(int index)
            {
                if ((index < 0) || (index >= this.length))
                {
                    return null;
                }
                return this.data[index];
            }

            public object this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.length))
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    return this.data[index];
                }
                set
                {
                    if ((index < 0) || (index >= this.length))
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    this.data[index] = value;
                }
            }

            public int Length =>
                this.length;
        }

        private sealed class NodeEnumerator : IEnumerator
        {
            private int position = -1;
            private readonly BasicList.Node node;

            public NodeEnumerator(BasicList.Node node)
            {
                this.node = node;
            }

            public bool MoveNext()
            {
                int length = this.node.Length;
                if (this.position > length)
                {
                    return false;
                }
                int num2 = this.position + 1;
                this.position = num2;
                return (num2 < length);
            }

            void IEnumerator.Reset()
            {
                this.position = -1;
            }

            public object Current =>
                this.node[this.position];
        }
    }
}

