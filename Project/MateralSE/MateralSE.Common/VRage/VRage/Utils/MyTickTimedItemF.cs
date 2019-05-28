namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTickTimedItemF
    {
        private float m_storage;
        private int m_ticksLeft;
        public float Get()
        {
            if (this.m_ticksLeft <= 0)
            {
                return 0f;
            }
            this.m_ticksLeft--;
            return this.m_storage;
        }

        public bool TryGet(out float outStoredItem)
        {
            if (this.m_ticksLeft <= 0)
            {
                outStoredItem = 0f;
                return false;
            }
            outStoredItem = this.m_storage;
            this.m_ticksLeft--;
            return true;
        }

        public void Set(int itemTickTimeout, float item)
        {
            this.m_storage = item;
            this.m_ticksLeft = itemTickTimeout;
        }
    }
}

