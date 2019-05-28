namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTickTimedItem<T>
    {
        private T m_storage;
        private int m_ticksLeft;
        public T Get()
        {
            if (this.m_ticksLeft > 0)
            {
                this.m_ticksLeft--;
                return this.m_storage;
            }
            return default(T);
        }

        public bool TryGet(out T outStoredItem)
        {
            if (this.m_ticksLeft <= 0)
            {
                outStoredItem = default(T);
                return false;
            }
            this.m_ticksLeft--;
            outStoredItem = this.m_storage;
            return true;
        }

        public void Set(int itemTickTimeout, T item)
        {
            this.m_storage = item;
            this.m_ticksLeft = itemTickTimeout;
        }
    }
}

