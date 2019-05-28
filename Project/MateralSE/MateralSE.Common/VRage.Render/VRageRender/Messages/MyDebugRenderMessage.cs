namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class MyDebugRenderMessage : MyRenderMessageBase
    {
        protected MyDebugRenderMessage()
        {
        }

        public bool Persistent { get; set; }

        public override bool IsPersistent =>
            this.Persistent;

        public override MyRenderMessageType MessageClass =>
            (this.IsPersistent ? MyRenderMessageType.StateChangeOnce : MyRenderMessageType.DebugDraw);
    }
}

