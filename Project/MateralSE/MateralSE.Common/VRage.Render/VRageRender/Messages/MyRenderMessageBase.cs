namespace VRageRender.Messages
{
    using System;
    using VRageRender;

    public abstract class MyRenderMessageBase
    {
        private int m_ref;

        protected MyRenderMessageBase()
        {
        }

        public void AddRef()
        {
            this.m_ref++;
        }

        public virtual void Close()
        {
            this.m_ref = 0;
        }

        public void Dispose()
        {
            this.m_ref--;
            if (this.m_ref == -1)
            {
                MyRenderProxy.MessagePool.Return(this);
            }
        }

        public virtual void Init()
        {
        }

        public abstract MyRenderMessageType MessageClass { get; }

        public abstract MyRenderMessageEnum MessageType { get; }

        public virtual bool IsPersistent =>
            false;
    }
}

