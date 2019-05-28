namespace VRage.Library.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Threading;

    public class SequenceIdGenerator
    {
        private uint m_maxId;
        private Queue<Item> m_reuseQueue;
        private int m_protecionCount;
        private uint m_reuseProtectionTime;
        private Func<uint> m_timeFunc;
        private SpinLockRef m_lock = new SpinLockRef();

        public SequenceIdGenerator(int reuseProtectionCount = 0x800, uint reuseProtectionTime = 60, Func<uint> timeFunc = null)
        {
            this.m_reuseQueue = new Queue<Item>(reuseProtectionCount);
            this.m_protecionCount = Math.Max(0, reuseProtectionCount);
            this.m_reuseProtectionTime = reuseProtectionTime;
            this.m_timeFunc = timeFunc;
        }

        private bool CheckFirstItemTime()
        {
            if (this.m_timeFunc == null)
            {
                return true;
            }
            uint num = this.m_timeFunc();
            uint time = this.m_reuseQueue.Peek().Time;
            if (num >= time)
            {
                return ((time + this.m_reuseProtectionTime) < num);
            }
            int count = this.m_reuseQueue.Count;
            for (int i = 0; i < count; i++)
            {
                Item item = this.m_reuseQueue.Dequeue();
                item.Time = num;
                this.m_reuseQueue.Enqueue(item);
            }
            return false;
        }

        public static SequenceIdGenerator CreateWithStopwatch(TimeSpan reuseProtectionTime, int reuseProtectionCount = 0x800)
        {
            Stopwatch sw = Stopwatch.StartNew();
            return ((reuseProtectionTime.TotalSeconds <= 5.0) ? ((reuseProtectionTime.TotalMilliseconds <= 500.0) ? ((reuseProtectionTime.TotalMilliseconds <= 50.0) ? new SequenceIdGenerator(reuseProtectionCount, (uint) reuseProtectionTime.TotalMilliseconds, () => (uint) sw.Elapsed.TotalMilliseconds) : new SequenceIdGenerator(reuseProtectionCount, (uint) (reuseProtectionTime.TotalSeconds * 100.0), () => (uint) (sw.Elapsed.TotalSeconds * 100.0))) : new SequenceIdGenerator(reuseProtectionCount, (uint) (reuseProtectionTime.TotalSeconds * 10.0), () => (uint) (sw.Elapsed.TotalSeconds * 10.0))) : new SequenceIdGenerator(reuseProtectionCount, (uint) reuseProtectionTime.TotalSeconds, () => (uint) sw.Elapsed.TotalSeconds));
        }

        public uint NextId()
        {
            uint id;
            using (this.m_lock.Acquire())
            {
                if ((this.m_reuseQueue.Count > this.m_protecionCount) && this.CheckFirstItemTime())
                {
                    id = this.m_reuseQueue.Dequeue().Id;
                }
                else
                {
                    uint num2 = this.m_maxId + 1;
                    this.m_maxId = num2;
                    id = num2;
                }
            }
            return id;
        }

        public void Reserve(uint reservedIdCount)
        {
            if (this.m_maxId != 0)
            {
                throw new InvalidOperationException("Reserve can be called only once and before any IDs are generated.");
            }
            this.m_maxId = reservedIdCount;
            this.ReservedCount = reservedIdCount;
        }

        public void Return(uint id)
        {
            using (this.m_lock.Acquire())
            {
                this.m_reuseQueue.Enqueue(new Item(id, this.m_timeFunc()));
            }
        }

        public int WaitingInQueue =>
            this.m_reuseQueue.Count;

        public uint ReservedCount { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct Item
        {
            public uint Id;
            public uint Time;
            public Item(uint id, uint time)
            {
                this.Id = id;
                this.Time = time;
            }
        }
    }
}

