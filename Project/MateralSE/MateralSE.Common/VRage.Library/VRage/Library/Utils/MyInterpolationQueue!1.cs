namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public class MyInterpolationQueue<T>
    {
        private MyQueue<Item<T>> m_queue;
        private InterpolationHandler<T> m_interpolator;
        private MyTimeSpan m_lastTimeStamp;

        public MyInterpolationQueue(int defaultCapacity, InterpolationHandler<T> interpolator)
        {
            this.m_lastTimeStamp = MyTimeSpan.Zero;
            this.m_queue = new MyQueue<Item<T>>(defaultCapacity);
            this.m_interpolator = interpolator;
        }

        public void AddSample(ref T item, MyTimeSpan sampleTimestamp)
        {
            if (sampleTimestamp >= this.m_lastTimeStamp)
            {
                if ((sampleTimestamp == this.m_lastTimeStamp) && (this.m_queue.Count > 0))
                {
                    this.m_queue[this.m_queue.Count - 1] = new Item<T>(item, sampleTimestamp);
                }
                else
                {
                    this.m_queue.Enqueue(new Item<T>(item, sampleTimestamp));
                    this.m_lastTimeStamp = sampleTimestamp;
                }
            }
        }

        public void Clear()
        {
            this.m_queue.Clear();
            this.m_lastTimeStamp = MyTimeSpan.Zero;
        }

        public void DiscardOld(MyTimeSpan currentTimestamp)
        {
            int num = -1;
            for (int i = 0; (i < this.m_queue.Count) && (this.m_queue[i].Timestamp < currentTimestamp); i++)
            {
                num++;
            }
            for (int j = 0; (j < num) && (this.m_queue.Count > 2); j++)
            {
                this.m_queue.Dequeue();
            }
        }

        public float Interpolate(MyTimeSpan currentTimestamp, out T result)
        {
            this.DiscardOld(currentTimestamp);
            if (this.m_queue.Count <= 1)
            {
                result = this.m_queue[0].Userdata;
                return 0f;
            }
            Item<T> item = this.m_queue[0];
            Item<T> item2 = this.m_queue[1];
            float interpolator = (float) ((currentTimestamp - item.Timestamp).Seconds / (item2.Timestamp - item.Timestamp).Seconds);
            this.m_interpolator(item.Userdata, item2.Userdata, interpolator, out result);
            return interpolator;
        }

        public MyTimeSpan LastSample =>
            this.m_lastTimeStamp;

        public int Count =>
            this.m_queue.Count;

        [StructLayout(LayoutKind.Sequential)]
        private struct Item
        {
            public T Userdata;
            public MyTimeSpan Timestamp;
            public Item(T userdata, MyTimeSpan timespan)
            {
                this.Userdata = userdata;
                this.Timestamp = timespan;
            }
        }
    }
}

