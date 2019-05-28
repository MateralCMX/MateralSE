namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyIndexArray<T>
    {
        private T[] m_internalArray;
        public float MinimumGrowFactor;

        public MyIndexArray(int defaultCapacity = 0)
        {
            this.MinimumGrowFactor = 2f;
            this.m_internalArray = (defaultCapacity > 0) ? new T[defaultCapacity] : EmptyArray<T>.Value;
        }

        public void Clear()
        {
            Array.Clear(this.m_internalArray, 0, this.m_internalArray.Length);
        }

        public void ClearItem(int index)
        {
            this.m_internalArray[index] = default(T);
        }

        public void TrimExcess(float minimumShrinkFactor = 0.5f, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            int index = this.m_internalArray.Length - 1;
            while (true)
            {
                if (index >= 0)
                {
                    T y = default(T);
                    if (comparer.Equals(this.m_internalArray[index], y))
                    {
                        index--;
                        continue;
                    }
                }
                int newSize = index + 1;
                if (newSize <= (this.m_internalArray.Length * minimumShrinkFactor))
                {
                    Array.Resize<T>(ref this.m_internalArray, newSize);
                }
                return;
            }
        }

        public T[] InternalArray =>
            this.m_internalArray;

        public int Length =>
            this.m_internalArray.Length;

        public T this[int index]
        {
            get
            {
                if (index < this.m_internalArray.Length)
                {
                    return this.m_internalArray[index];
                }
                return default(T);
            }
            set
            {
                int length = this.m_internalArray.Length;
                if (index >= length)
                {
                    int newSize = Math.Max((int) Math.Ceiling((double) (this.MinimumGrowFactor * length)), index + 1);
                    Array.Resize<T>(ref this.m_internalArray, newSize);
                }
                this.m_internalArray[index] = value;
            }
        }
    }
}

