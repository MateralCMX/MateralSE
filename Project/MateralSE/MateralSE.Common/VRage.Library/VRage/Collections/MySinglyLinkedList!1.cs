namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MySinglyLinkedList<V> : IList<V>, ICollection<V>, IEnumerable<V>, IEnumerable
    {
        private Node<V> m_rootNode;
        private Node<V> m_lastNode;
        private int m_count;

        public MySinglyLinkedList()
        {
            this.m_rootNode = null;
            this.m_lastNode = null;
            this.m_count = 0;
        }

        public void Add(V item)
        {
            if (this.m_lastNode == null)
            {
                this.Prepend(item);
            }
            else
            {
                this.m_lastNode.Next = new Node<V>(null, item);
                this.m_count++;
                this.m_lastNode = this.m_lastNode.Next;
            }
        }

        public void Append(V item)
        {
            this.Add(item);
        }

        public void Clear()
        {
            this.m_rootNode = null;
            this.m_lastNode = null;
            this.m_count = 0;
        }

        public bool Contains(V item)
        {
            using (Enumerator<V> enumerator = this.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    V current = enumerator.Current;
                    if (current.Equals(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CopyTo(V[] array, int arrayIndex)
        {
            foreach (V local in this)
            {
                array[arrayIndex] = local;
                arrayIndex++;
            }
        }

        public V First()
        {
            if (this.m_count == 0)
            {
                throw new InvalidOperationException();
            }
            return this.m_rootNode.Data;
        }

        public Enumerator<V> GetEnumerator() => 
            new Enumerator<V>((MySinglyLinkedList<V>) this);

        public int IndexOf(V item)
        {
            int num = 0;
            using (Enumerator<V> enumerator = this.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    V current = enumerator.Current;
                    if (!current.Equals(item))
                    {
                        num++;
                        continue;
                    }
                    return num;
                }
            }
            return -1;
        }

        public void Insert(int index, V item)
        {
            if ((index < 0) || (index > this.m_count))
            {
                throw new IndexOutOfRangeException();
            }
            if (index == 0)
            {
                this.Prepend(item);
            }
            else if (index == this.m_count)
            {
                this.Add(item);
            }
            else
            {
                Enumerator<V> enumerator = this.GetEnumerator();
                for (int i = 0; i < index; i++)
                {
                    enumerator.MoveNext();
                }
                Node<V> node = new Node<V>(enumerator.m_currentNode.Next, item);
                enumerator.m_currentNode.Next = node;
                this.m_count++;
            }
        }

        public V Last()
        {
            if (this.m_count == 0)
            {
                throw new InvalidOperationException();
            }
            return this.m_lastNode.Data;
        }

        public void Merge(MySinglyLinkedList<V> otherList)
        {
            if (this.m_lastNode == null)
            {
                this.m_rootNode = otherList.m_rootNode;
                this.m_lastNode = otherList.m_lastNode;
            }
            else if (otherList.m_lastNode != null)
            {
                this.m_lastNode.Next = otherList.m_rootNode;
                this.m_lastNode = otherList.m_lastNode;
            }
            this.m_count += otherList.m_count;
            otherList.m_count = 0;
            otherList.m_lastNode = null;
            otherList.m_rootNode = null;
        }

        public V PopFirst()
        {
            if (this.m_count == 0)
            {
                throw new InvalidOperationException();
            }
            Node<V> rootNode = this.m_rootNode;
            if (ReferenceEquals(rootNode, this.m_lastNode))
            {
                this.m_lastNode = null;
            }
            this.m_rootNode = rootNode.Next;
            this.m_count--;
            return rootNode.Data;
        }

        public void Prepend(V item)
        {
            this.m_rootNode = new Node<V>(this.m_rootNode, item);
            this.m_count++;
            if (this.m_count == 1)
            {
                this.m_lastNode = this.m_rootNode;
            }
        }

        public bool Remove(V item)
        {
            Node<V> rootNode = this.m_rootNode;
            if (rootNode != null)
            {
                if (this.m_rootNode.Data.Equals(item))
                {
                    this.m_rootNode = this.m_rootNode.Next;
                    this.m_count--;
                    if (this.m_count == 0)
                    {
                        this.m_lastNode = null;
                    }
                    return true;
                }
                for (Node<V> node2 = rootNode.Next; node2 != null; node2 = node2.Next)
                {
                    if (node2.Data.Equals(item))
                    {
                        rootNode.Next = node2.Next;
                        this.m_count--;
                        if (ReferenceEquals(node2, this.m_lastNode))
                        {
                            this.m_lastNode = rootNode;
                        }
                        return true;
                    }
                    rootNode = node2;
                }
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if ((index < 0) || (index >= this.m_count))
            {
                throw new IndexOutOfRangeException();
            }
            if (index == 0)
            {
                this.m_rootNode = this.m_rootNode.Next;
                this.m_count--;
                if (this.m_count == 0)
                {
                    this.m_lastNode = null;
                }
            }
            else
            {
                Enumerator<V> enumerator = this.GetEnumerator();
                for (int i = 0; i < index; i++)
                {
                    enumerator.MoveNext();
                }
                enumerator.m_currentNode.Next = enumerator.m_currentNode.Next.Next;
                this.m_count--;
                if (this.m_count == index)
                {
                    this.m_lastNode = enumerator.m_currentNode;
                }
            }
        }

        public void Reverse()
        {
            if (this.m_count > 1)
            {
                Node<V> next;
                Node<V> node = null;
                for (Node<V> node2 = this.m_rootNode; !ReferenceEquals(node2, this.m_lastNode); node2 = next)
                {
                    next = node2.Next;
                    node2.Next = node;
                    node = node2;
                }
                Node<V> rootNode = this.m_rootNode;
                this.m_rootNode = this.m_lastNode;
                this.m_lastNode = rootNode;
            }
        }

        public MySinglyLinkedList<V> Split(Enumerator<V> newLastPosition, int newCount = -1)
        {
            if (newCount == -1)
            {
                newCount = 1;
                for (Node<V> node = this.m_rootNode; !ReferenceEquals(node, newLastPosition.m_currentNode); node = node.Next)
                {
                    newCount++;
                }
            }
            MySinglyLinkedList<V> list = new MySinglyLinkedList<V> {
                m_rootNode = newLastPosition.m_currentNode.Next
            };
            list.m_lastNode = (list.m_rootNode == null) ? null : this.m_lastNode;
            list.m_count = this.m_count - newCount;
            this.m_lastNode = newLastPosition.m_currentNode;
            this.m_lastNode.Next = null;
            this.m_count = newCount;
            return list;
        }

        IEnumerator<V> IEnumerable<V>.GetEnumerator() => 
            new Enumerator<V>((MySinglyLinkedList<V>) this);

        IEnumerator IEnumerable.GetEnumerator() => 
            new Enumerator<V>((MySinglyLinkedList<V>) this);

        public bool VerifyConsistency()
        {
            bool flag = true;
            if (this.m_lastNode == null)
            {
                int num1;
                if (!flag || (this.m_rootNode != null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (this.m_count == 0);
                }
                flag = (bool) num1;
            }
            if (this.m_rootNode == null)
            {
                int num2;
                if (!flag || (this.m_lastNode != null))
                {
                    num2 = 0;
                }
                else
                {
                    num2 = (int) (this.m_count == 0);
                }
                flag = (bool) num2;
            }
            if (ReferenceEquals(this.m_rootNode, this.m_lastNode))
            {
                flag = flag && ((this.m_rootNode == null) || (this.m_count == 1));
            }
            int num = 0;
            Node<V> rootNode = this.m_rootNode;
            while (rootNode != null)
            {
                rootNode = rootNode.Next;
                num++;
                flag = flag && (num <= this.m_count);
            }
            return (flag && (num == this.m_count));
        }

        public V this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_count))
                {
                    throw new IndexOutOfRangeException();
                }
                Enumerator<V> enumerator = this.GetEnumerator();
                for (int i = -1; i < index; i++)
                {
                    enumerator.MoveNext();
                }
                return enumerator.Current;
            }
            set
            {
                if ((index < 0) || (index >= this.m_count))
                {
                    throw new IndexOutOfRangeException();
                }
                Enumerator<V> enumerator = this.GetEnumerator();
                for (int i = -1; i < index; i++)
                {
                    enumerator.MoveNext();
                }
                enumerator.m_currentNode.Data = value;
            }
        }

        public int Count =>
            this.m_count;

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<V>, IDisposable, IEnumerator
        {
            internal MySinglyLinkedList<V>.Node m_previousNode;
            internal MySinglyLinkedList<V>.Node m_currentNode;
            internal MySinglyLinkedList<V> m_list;
            public Enumerator(MySinglyLinkedList<V> parentList)
            {
                this.m_list = parentList;
                this.m_currentNode = null;
                this.m_previousNode = null;
            }

            public V Current =>
                this.m_currentNode.Data;
            public bool HasCurrent =>
                (this.m_currentNode != null);
            public void Dispose()
            {
            }

            object IEnumerator.Current =>
                this.m_currentNode.Data;
            public bool MoveNext()
            {
                if (this.m_currentNode != null)
                {
                    this.m_previousNode = this.m_currentNode;
                    this.m_currentNode = this.m_currentNode.Next;
                }
                else
                {
                    if (this.m_previousNode != null)
                    {
                        return false;
                    }
                    this.m_currentNode = this.m_list.m_rootNode;
                    this.m_previousNode = null;
                }
                return (this.m_currentNode != null);
            }

            public V RemoveCurrent()
            {
                if (this.m_currentNode == null)
                {
                    throw new InvalidOperationException();
                }
                if (this.m_previousNode == null)
                {
                    this.m_currentNode = this.m_currentNode.Next;
                    return this.m_list.PopFirst();
                }
                this.m_previousNode.Next = this.m_currentNode.Next;
                if (ReferenceEquals(this.m_list.m_lastNode, this.m_currentNode))
                {
                    this.m_list.m_lastNode = this.m_previousNode;
                }
                MySinglyLinkedList<V>.Node currentNode = this.m_currentNode;
                this.m_currentNode = this.m_currentNode.Next;
                this.m_list.m_count--;
                return currentNode.Data;
            }

            public void InsertBeforeCurrent(V toInsert)
            {
                MySinglyLinkedList<V>.Node node = new MySinglyLinkedList<V>.Node(this.m_currentNode, toInsert);
                if (this.m_currentNode != null)
                {
                    if (this.m_previousNode == null)
                    {
                        this.m_list.m_rootNode = node;
                    }
                    else
                    {
                        this.m_previousNode.Next = node;
                    }
                }
                else if (this.m_previousNode != null)
                {
                    this.m_previousNode.Next = node;
                    this.m_list.m_lastNode = node;
                }
                else
                {
                    if (this.m_list.m_count != 0)
                    {
                        throw new InvalidOperationException("Inserting into a MySinglyLinkedList using an uninitialized enumerator!");
                    }
                    this.m_list.m_rootNode = node;
                    this.m_list.m_lastNode = node;
                }
                this.m_previousNode = node;
                this.m_list.m_count++;
            }

            public void Reset()
            {
                this.m_currentNode = null;
                this.m_previousNode = null;
            }
        }

        internal class Node
        {
            public MySinglyLinkedList<V>.Node Next;
            public V Data;

            public Node(MySinglyLinkedList<V>.Node next, V data)
            {
                this.Next = next;
                this.Data = data;
            }
        }
    }
}

