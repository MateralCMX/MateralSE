namespace VRage.Render.Scene
{
    using System;
    using System.Collections.Generic;

    public class MyIDTracker<T> where T: class
    {
        private uint m_ID;
        private T m_value;
        private static readonly Dictionary<uint, MyIDTracker<T>> m_dict;

        static MyIDTracker()
        {
            MyIDTracker<T>.m_dict = new Dictionary<uint, MyIDTracker<T>>();
        }

        public MyIDTracker()
        {
            this.m_ID = uint.MaxValue;
        }

        internal void Clear()
        {
            this.Deregister();
        }

        internal void Deregister()
        {
            MyIDTracker<T>.m_dict.Remove(this.m_ID);
            this.m_ID = uint.MaxValue;
            this.m_value = default(T);
        }

        public static T FindByID(uint id)
        {
            MyIDTracker<T> tracker;
            if (MyIDTracker<T>.m_dict.TryGetValue(id, out tracker))
            {
                return tracker.m_value;
            }
            return default(T);
        }

        internal void Register(uint id, T val)
        {
            this.m_ID = id;
            this.m_value = val;
            MyIDTracker<T>.m_dict[id] = (MyIDTracker<T>) this;
        }

        internal uint ID =>
            this.m_ID;

        internal T Value =>
            this.m_value;

        public static int Count =>
            MyIDTracker<T>.m_dict.Count;
    }
}

