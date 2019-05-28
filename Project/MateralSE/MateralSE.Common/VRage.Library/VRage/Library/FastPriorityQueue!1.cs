namespace VRage.Library
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public sealed class FastPriorityQueue<T> : IEnumerable where T: FastPriorityQueue<T>.Node
    {
        private int m_numNodes;
        private T[] m_nodes;

        public FastPriorityQueue(int maxNodes)
        {
            this.m_numNodes = 0;
            this.m_nodes = new T[maxNodes + 1];
        }

        private void CascadeDown(T node)
        {
            int queueIndex = node.QueueIndex;
            int index = 2 * queueIndex;
            if (index <= this.m_numNodes)
            {
                int num3 = index + 1;
                T local = this.m_nodes[index];
                if (local.Priority >= node.Priority)
                {
                    if (num3 > this.m_numNodes)
                    {
                        return;
                    }
                    T local3 = this.m_nodes[num3];
                    if (local3.Priority >= node.Priority)
                    {
                        return;
                    }
                    local3.QueueIndex = queueIndex;
                    this.m_nodes[queueIndex] = local3;
                    queueIndex = num3;
                }
                else
                {
                    if (num3 > this.m_numNodes)
                    {
                        node.QueueIndex = index;
                        local.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = local;
                        this.m_nodes[index] = node;
                        return;
                    }
                    T local2 = this.m_nodes[num3];
                    if (local.Priority < local2.Priority)
                    {
                        local.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = local;
                        queueIndex = index;
                    }
                    else
                    {
                        local2.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = local2;
                        queueIndex = num3;
                    }
                }
                while (true)
                {
                    index = 2 * queueIndex;
                    if (index > this.m_numNodes)
                    {
                        node.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = node;
                        return;
                    }
                    num3 = index + 1;
                    local = this.m_nodes[index];
                    if (local.Priority >= node.Priority)
                    {
                        if (num3 > this.m_numNodes)
                        {
                            node.QueueIndex = queueIndex;
                            this.m_nodes[queueIndex] = node;
                            return;
                        }
                        T local5 = this.m_nodes[num3];
                        if (local5.Priority >= node.Priority)
                        {
                            node.QueueIndex = queueIndex;
                            this.m_nodes[queueIndex] = node;
                            return;
                        }
                        local5.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = local5;
                        queueIndex = num3;
                        continue;
                    }
                    if (num3 > this.m_numNodes)
                    {
                        node.QueueIndex = index;
                        local.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = local;
                        this.m_nodes[index] = node;
                        return;
                    }
                    T local4 = this.m_nodes[num3];
                    if (local.Priority < local4.Priority)
                    {
                        local.QueueIndex = queueIndex;
                        this.m_nodes[queueIndex] = local;
                        queueIndex = index;
                        continue;
                    }
                    local4.QueueIndex = queueIndex;
                    this.m_nodes[queueIndex] = local4;
                    queueIndex = num3;
                }
            }
        }

        private void CascadeUp(T node)
        {
            if (node.QueueIndex > 1)
            {
                int index = ((T) node).QueueIndex >> 1;
                T higher = this.m_nodes[index];
                if (!this.HasHigherOrEqualPriority(higher, node))
                {
                    this.m_nodes[node.QueueIndex] = higher;
                    higher.QueueIndex = node.QueueIndex;
                    node.QueueIndex = index;
                    while (true)
                    {
                        if (index > 1)
                        {
                            index = index >> 1;
                            T local2 = this.m_nodes[index];
                            if (!this.HasHigherOrEqualPriority(local2, node))
                            {
                                this.m_nodes[node.QueueIndex] = local2;
                                local2.QueueIndex = node.QueueIndex;
                                node.QueueIndex = index;
                                continue;
                            }
                        }
                        this.m_nodes[node.QueueIndex] = node;
                        return;
                    }
                }
            }
        }

        public void Clear()
        {
            Array.Clear(this.m_nodes, 1, this.m_numNodes);
            this.m_numNodes = 0;
        }

        public bool Contains(T node) => 
            (this.m_nodes[node.QueueIndex] == node);

        public T Dequeue()
        {
            T local = this.m_nodes[1];
            if (this.m_numNodes == 1)
            {
                T local3 = default(T);
                this.m_nodes[1] = local3;
                this.m_numNodes = 0;
                return local;
            }
            T node = this.m_nodes[this.m_numNodes];
            this.m_nodes[1] = node;
            node.QueueIndex = 1;
            this.m_nodes[this.m_numNodes] = default(T);
            this.m_numNodes--;
            this.CascadeDown(node);
            return local;
        }

        public void Enqueue(T node, long priority)
        {
            if (this.m_numNodes >= (this.m_nodes.Length - 1))
            {
                this.Resize((this.m_numNodes > 0) ? (this.m_numNodes * 2) : 0x10);
            }
            node.Priority = priority;
            this.m_numNodes++;
            this.m_nodes[this.m_numNodes] = node;
            node.QueueIndex = this.m_numNodes;
            this.CascadeUp(node);
        }

        [IteratorStateMachine(typeof(<GetEnumerator>d__21))]
        public IEnumerator<T> GetEnumerator()
        {
            <GetEnumerator>d__21<T> d__1 = new <GetEnumerator>d__21<T>(0);
            d__1.<>4__this = (FastPriorityQueue<T>) this;
            return d__1;
        }

        private bool HasHigherOrEqualPriority(T higher, T lower) => 
            (higher.Priority <= lower.Priority);

        public bool IsValidQueue()
        {
            for (int i = 1; i < this.m_nodes.Length; i++)
            {
                if (this.m_nodes[i] != null)
                {
                    int index = 2 * i;
                    if (((index < this.m_nodes.Length) && (this.m_nodes[index] != null)) && (this.m_nodes[index].Priority < this.m_nodes[i].Priority))
                    {
                        return false;
                    }
                    int num3 = index + 1;
                    if (((num3 < this.m_nodes.Length) && (this.m_nodes[num3] != null)) && (this.m_nodes[num3].Priority < this.m_nodes[i].Priority))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void OnNodeUpdated(T node)
        {
            int index = ((T) node).QueueIndex >> 1;
            if ((index <= 0) || (node.Priority >= this.m_nodes[index].Priority))
            {
                this.CascadeDown(node);
            }
            else
            {
                this.CascadeUp(node);
            }
        }

        public void Remove(T node)
        {
            if (node.QueueIndex == this.m_numNodes)
            {
                T local2 = default(T);
                this.m_nodes[this.m_numNodes] = local2;
                this.m_numNodes--;
            }
            else
            {
                T local = this.m_nodes[this.m_numNodes];
                this.m_nodes[node.QueueIndex] = local;
                local.QueueIndex = node.QueueIndex;
                this.m_nodes[this.m_numNodes] = default(T);
                this.m_numNodes--;
                this.OnNodeUpdated(local);
            }
        }

        public void Resize(int maxNodes)
        {
            T[] destinationArray = new T[maxNodes + 1];
            Array.Copy(this.m_nodes, destinationArray, (int) (Math.Min(maxNodes, this.m_numNodes) + 1));
            this.m_nodes = destinationArray;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public void UpdatePriority(T node, long priority)
        {
            node.Priority = priority;
            this.OnNodeUpdated(node);
        }

        public int Count =>
            this.m_numNodes;

        public int MaxSize =>
            (this.m_nodes.Length - 1);

        public T First =>
            this.m_nodes[1];

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__21 : IEnumerator<T>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private T <>2__current;
            public FastPriorityQueue<T> <>4__this;
            private int <i>5__2;

            [DebuggerHidden]
            public <GetEnumerator>d__21(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                FastPriorityQueue<T> queue = this.<>4__this;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<i>5__2 = 1;
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    int num2 = this.<i>5__2;
                    this.<i>5__2 = num2 + 1;
                }
                if (this.<i>5__2 > queue.m_numNodes)
                {
                    return false;
                }
                this.<>2__current = queue.m_nodes[this.<i>5__2];
                this.<>1__state = 1;
                return true;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        public class Node
        {
            internal long Priority;
            internal int QueueIndex;
        }
    }
}

