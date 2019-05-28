namespace VRageMath
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyMovingAverage
    {
        private readonly Queue<float> m_queue = new Queue<float>();
        private readonly int m_windowSize;
        private int m_enqueueCounter;
        private readonly int m_enqueueCountToReset;

        public MyMovingAverage(int windowSize, int enqueueCountToReset = 0x3e8)
        {
            this.m_windowSize = windowSize;
            this.m_enqueueCountToReset = enqueueCountToReset;
        }

        public void Enqueue(float value)
        {
            this.m_queue.Enqueue(value);
            this.m_enqueueCounter++;
            if (this.m_enqueueCounter <= this.m_enqueueCountToReset)
            {
                this.Sum += value;
            }
            else
            {
                this.m_enqueueCounter = 0;
                this.UpdateSum();
            }
            while (this.m_queue.Count > this.m_windowSize)
            {
                float num = this.m_queue.Dequeue();
                this.Sum -= num;
            }
        }

        public void Reset()
        {
            this.Sum = 0.0;
            this.m_queue.Clear();
        }

        private void UpdateSum()
        {
            this.Sum = 0.0;
            foreach (float num in this.m_queue)
            {
                this.Sum += num;
            }
        }

        public float Avg =>
            ((this.m_queue.Count > 0) ? (((float) this.Sum) / ((float) this.m_queue.Count)) : 0f);

        public double Sum { get; private set; }
    }
}

