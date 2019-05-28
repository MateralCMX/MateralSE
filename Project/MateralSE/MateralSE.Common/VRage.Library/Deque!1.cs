using System;
using System.Runtime.CompilerServices;
using System.Threading;

internal class Deque<T>
{
    private const int INITIAL_SIZE = 0x20;
    private T[] m_array;
    private int m_mask;
    private volatile int m_headIndex;
    private volatile int m_tailIndex;
    private object m_foreignLock;

    public Deque()
    {
        this.m_array = new T[0x20];
        this.m_mask = 0x1f;
        this.m_foreignLock = new object();
    }

    public void Clear()
    {
        for (int i = 0; i < this.m_array.Length; i++)
        {
            this.m_array[i] = default(T);
        }
        this.m_headIndex = 0;
        this.m_tailIndex = 0;
    }

    public bool LocalPop(ref T obj)
    {
        bool flag2;
        object foreignLock = this.m_foreignLock;
        lock (foreignLock)
        {
            int tailIndex = this.m_tailIndex;
            if (this.m_headIndex >= tailIndex)
            {
                flag2 = false;
            }
            else
            {
                tailIndex--;
                Interlocked.Exchange(ref this.m_tailIndex, tailIndex);
                if (this.m_headIndex <= tailIndex)
                {
                    obj = this.m_array[tailIndex & this.m_mask];
                    flag2 = true;
                }
                else if (this.m_headIndex <= tailIndex)
                {
                    obj = this.m_array[tailIndex & this.m_mask];
                    flag2 = true;
                }
                else
                {
                    this.m_tailIndex = tailIndex + 1;
                    flag2 = false;
                }
            }
        }
        return flag2;
    }

    public void LocalPush(T obj)
    {
        object foreignLock = this.m_foreignLock;
        lock (foreignLock)
        {
            int tailIndex = this.m_tailIndex;
            if (tailIndex < (this.m_headIndex + this.m_mask))
            {
                this.m_array[tailIndex & this.m_mask] = obj;
                this.m_tailIndex = tailIndex + 1;
            }
            else
            {
                int headIndex = this.m_headIndex;
                int num3 = this.m_tailIndex - this.m_headIndex;
                if (num3 >= this.m_mask)
                {
                    T[] localArray = new T[this.m_array.Length << 1];
                    int index = 0;
                    while (true)
                    {
                        if (index >= num3)
                        {
                            this.m_array = localArray;
                            this.m_headIndex = 0;
                            this.m_tailIndex = tailIndex = num3;
                            this.m_mask = (this.m_mask << 1) | 1;
                            break;
                        }
                        localArray[index] = this.m_array[(index + headIndex) & this.m_mask];
                        index++;
                    }
                }
                this.m_array[tailIndex & this.m_mask] = obj;
                this.m_tailIndex = tailIndex + 1;
            }
        }
    }

    public bool TrySteal(ref T obj)
    {
        bool flag = false;
        try
        {
            if (Monitor.TryEnter(this.m_foreignLock))
            {
                bool flag2;
                int headIndex = this.m_headIndex;
                Interlocked.Exchange(ref this.m_headIndex, headIndex + 1);
                if (headIndex < this.m_tailIndex)
                {
                    obj = this.m_array[headIndex & this.m_mask];
                    flag2 = true;
                }
                else
                {
                    this.m_headIndex = headIndex;
                    flag2 = false;
                }
                return flag2;
            }
        }
        finally
        {
            if (flag)
            {
                Monitor.Exit(this.m_foreignLock);
            }
        }
        return false;
    }

    public bool IsEmpty =>
        (this.m_headIndex >= this.m_tailIndex);

    public int Count =>
        (this.m_tailIndex - this.m_headIndex);
}

