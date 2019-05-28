namespace VRage.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Threading;

    public class MyDebugHitCounter : IEnumerable<MyDebugHitCounter.Sample>, IEnumerable
    {
        public readonly MyQueue<Sample> History;
        private Sample current;
        private readonly uint m_sampleCycle;
        private readonly uint m_historyLength;
        private SpinLockRef m_lock = new SpinLockRef();

        public MyDebugHitCounter(uint cycleSize = 0x186a0)
        {
            this.m_sampleCycle = cycleSize;
            this.m_historyLength = 10;
            this.History = new MyQueue<Sample>((int) this.m_historyLength);
        }

        public void Cycle()
        {
            using (this.m_lock.Acquire())
            {
                if (this.History.Count >= this.m_historyLength)
                {
                    this.History.Dequeue();
                }
                this.History.Enqueue(this.current);
                this.current = new Sample();
            }
        }

        public void CycleWork()
        {
            if (this.current.Count > 0)
            {
                this.Cycle();
            }
        }

        public ConcurrentEnumerator<SpinLockRef.Token, Sample, IEnumerator<Sample>> GetEnumerator() => 
            ConcurrentEnumerator.Create<SpinLockRef.Token, Sample, IEnumerator<Sample>>(this.m_lock.Acquire(), this.GetEnumeratorInternal());

        [IteratorStateMachine(typeof(<GetEnumeratorInternal>d__16))]
        private IEnumerator<Sample> GetEnumeratorInternal()
        {
            <GetEnumeratorInternal>d__16 d__1 = new <GetEnumeratorInternal>d__16(0);
            d__1.<>4__this = this;
            return d__1;
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public unsafe void Hit()
        {
            using (this.m_lock.Acquire())
            {
                uint* numPtr1 = (uint*) ref this.current.Count;
                numPtr1[0]++;
            }
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public unsafe void Miss()
        {
            using (this.m_lock.Acquire())
            {
                uint* numPtr1 = (uint*) ref this.current.Cycle;
                numPtr1[0]++;
                if (this.current.Cycle == this.m_sampleCycle)
                {
                    this.Cycle();
                }
            }
        }

        IEnumerator<Sample> IEnumerable<Sample>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public float ValueAndCycle()
        {
            this.Cycle();
            return this.LastCycleHitRatio;
        }

        public float CurrentHitRatio
        {
            get
            {
                using (this.m_lock.Acquire())
                {
                    return this.current.Value;
                }
            }
        }

        public float LastCycleHitRatio
        {
            get
            {
                float num;
                using (this.m_lock.Acquire())
                {
                    if (this.History.Count > 1)
                    {
                        num = this.History[1].Value;
                    }
                    else
                    {
                        num = 0f;
                    }
                }
                return num;
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumeratorInternal>d__16 : IEnumerator<MyDebugHitCounter.Sample>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private MyDebugHitCounter.Sample <>2__current;
            public MyDebugHitCounter <>4__this;
            private MyQueue<MyDebugHitCounter.Sample>.Enumerator <>7__wrap1;

            [DebuggerHidden]
            public <GetEnumeratorInternal>d__16(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                this.<>7__wrap1.Dispose();
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    MyDebugHitCounter counter = this.<>4__this;
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>2__current = counter.current;
                            this.<>1__state = 1;
                            return true;

                        case 1:
                            this.<>1__state = -1;
                            this.<>7__wrap1 = counter.History.GetEnumerator();
                            this.<>1__state = -3;
                            break;

                        case 2:
                            this.<>1__state = -3;
                            break;

                        default:
                            return false;
                    }
                    if (!this.<>7__wrap1.MoveNext())
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap1 = new MyQueue<MyDebugHitCounter.Sample>.Enumerator();
                        flag = false;
                    }
                    else
                    {
                        MyDebugHitCounter.Sample current = this.<>7__wrap1.Current;
                        this.<>2__current = current;
                        this.<>1__state = 2;
                        flag = true;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 2))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            MyDebugHitCounter.Sample IEnumerator<MyDebugHitCounter.Sample>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Sample
        {
            public uint Count;
            public uint Cycle;
            public float Value =>
                (((float) this.Count) / ((float) this.Cycle));
            public override string ToString() => 
                this.Value.ToString();
        }
    }
}

