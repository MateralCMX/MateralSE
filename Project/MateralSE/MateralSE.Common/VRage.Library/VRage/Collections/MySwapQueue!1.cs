namespace VRage.Collections
{
    using System;
    using System.Threading;

    public class MySwapQueue<T> where T: class
    {
        private T m_read;
        private T m_write;
        private T m_waitingData;
        private T m_unusedData;

        public MySwapQueue(Func<T> factoryMethod) : this(factoryMethod(), factoryMethod(), factoryMethod())
        {
        }

        public MySwapQueue(T first, T second, T third)
        {
            this.m_read = first;
            this.m_write = second;
            this.m_unusedData = third;
            this.m_waitingData = default(T);
        }

        public void CommitWrite()
        {
            this.m_write = Interlocked.Exchange<T>(ref this.m_waitingData, this.m_write);
            if (this.m_write == null)
            {
                T local = default(T);
                this.m_write = Interlocked.Exchange<T>(ref this.m_unusedData, local);
            }
        }

        public bool RefreshRead()
        {
            T comparand = default(T);
            if (Interlocked.CompareExchange<T>(ref this.m_unusedData, this.m_read, comparand) != null)
            {
                return false;
            }
            comparand = default(T);
            this.m_read = Interlocked.Exchange<T>(ref this.m_waitingData, comparand);
            return true;
        }

        public T Read =>
            this.m_read;

        public T Write =>
            this.m_write;
    }
}

