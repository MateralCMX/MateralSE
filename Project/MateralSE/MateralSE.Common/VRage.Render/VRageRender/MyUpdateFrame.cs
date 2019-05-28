namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using VRage.Library.Threading;
    using VRage.Library.Utils;
    using VRageRender.Messages;

    public class MyUpdateFrame
    {
        public bool Processed;
        public MyTimeSpan UpdateTimestamp;
        public readonly List<MyRenderMessageBase> RenderInput = new List<MyRenderMessageBase>(0x800);
        private readonly SpinLockRef m_lock = new SpinLockRef();

        public void Enqueue(MyRenderMessageBase message)
        {
            using (this.m_lock.Acquire())
            {
                this.RenderInput.Add(message);
            }
        }
    }
}

