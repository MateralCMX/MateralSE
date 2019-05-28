namespace VRage.Collections
{
    using System;

    public class MyDistributedUpdater<V, T> where V: IReadOnlyList<T>, new()
    {
        private V m_list;
        private int m_updateInterval;
        private int m_updateIndex;

        public MyDistributedUpdater(int updateInterval)
        {
            this.m_list = Activator.CreateInstance<V>();
            this.m_updateInterval = updateInterval;
        }

        public void Iterate(Action<T> p)
        {
            for (int i = this.m_updateIndex; i < this.m_list.Count; i += this.m_updateInterval)
            {
                p(this.m_list[i]);
            }
        }

        public void Update()
        {
            this.m_updateIndex++;
            this.m_updateIndex = this.m_updateIndex % this.m_updateInterval;
        }

        public int UpdateInterval
        {
            set => 
                (this.m_updateInterval = value);
        }

        public V List =>
            this.m_list;
    }
}

