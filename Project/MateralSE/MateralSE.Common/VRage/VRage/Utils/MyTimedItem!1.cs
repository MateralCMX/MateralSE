namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTimedItem<T>
    {
        private T m_storage;
        private int m_setTime;
        private int m_timeout;
        public T Get(int currentTime, bool autoRefreshTimeout)
        {
            if (currentTime >= (this.m_setTime + this.m_timeout))
            {
                return default(T);
            }
            if (autoRefreshTimeout)
            {
                this.m_setTime = currentTime + this.m_timeout;
            }
            return this.m_storage;
        }

        public bool TryGet(int currentTime, bool autoRefreshTimeout, out T outStoredItem)
        {
            if (currentTime >= (this.m_setTime + this.m_timeout))
            {
                outStoredItem = default(T);
                return false;
            }
            if (autoRefreshTimeout)
            {
                this.m_setTime = currentTime + this.m_timeout;
            }
            outStoredItem = this.m_storage;
            return true;
        }

        public void Set(int currentTime, int itemTimeout, T item)
        {
            this.m_setTime = currentTime;
            this.m_timeout = itemTimeout;
            this.m_storage = item;
        }
    }
}

