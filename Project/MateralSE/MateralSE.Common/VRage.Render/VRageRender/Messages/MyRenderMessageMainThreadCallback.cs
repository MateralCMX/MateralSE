namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyRenderMessageMainThreadCallback : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.MainThreadCallback;

        public Action Callback { get; set; }
    }
}

