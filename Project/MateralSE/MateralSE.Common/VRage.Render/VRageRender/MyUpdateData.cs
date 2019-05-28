namespace VRageRender
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Library.Threading;
    using VRage.Library.Utils;

    internal class MyUpdateData
    {
        private bool MY_FAKE__ENABLE_FRAMES_OVERFLOW_BARRIER = true;
        private const int OVERFLOW_THRESHOLD = 0x3e8;
        private ManualResetEvent m_overflowGate = new ManualResetEvent(true);
        private MyConcurrentPool<MyUpdateFrame> m_frameDataPool = new MyConcurrentPool<MyUpdateFrame>(5, null, 0x2710, null);
        private MyConcurrentQueue<MyUpdateFrame> m_updateDataQueue = new MyConcurrentQueue<MyUpdateFrame>(5);

        public MyUpdateData()
        {
            this.CurrentUpdateFrame = this.m_frameDataPool.Get();
        }

        public void CommitUpdateFrame(SpinLockRef heldLock)
        {
            MyTimeSpan updateTimestamp = this.CurrentUpdateFrame.UpdateTimestamp;
            this.CurrentUpdateFrame.Processed = false;
            if (this.MY_FAKE__ENABLE_FRAMES_OVERFLOW_BARRIER && (this.m_updateDataQueue.Count > 0x3e8))
            {
                if (heldLock != null)
                {
                    heldLock.Exit();
                }
                this.m_overflowGate.Reset();
                this.m_overflowGate.WaitOne();
                if (heldLock != null)
                {
                    heldLock.Enter();
                }
            }
            this.m_updateDataQueue.Enqueue(this.CurrentUpdateFrame);
            this.CurrentUpdateFrame = this.m_frameDataPool.Get();
            this.CurrentUpdateFrame.UpdateTimestamp = updateTimestamp;
        }

        public MyUpdateFrame GetRenderFrame(out bool isPreFrame)
        {
            MyUpdateFrame frame;
            if (this.m_updateDataQueue.Count > 1)
            {
                isPreFrame = true;
                return this.m_updateDataQueue.Dequeue();
            }
            isPreFrame = false;
            this.m_overflowGate.Set();
            return (this.m_updateDataQueue.TryPeek(out frame) ? frame : null);
        }

        public void ReturnPreFrame(MyUpdateFrame frame)
        {
            this.m_frameDataPool.Return(frame);
        }

        public MyUpdateFrame CurrentUpdateFrame { get; private set; }
    }
}

