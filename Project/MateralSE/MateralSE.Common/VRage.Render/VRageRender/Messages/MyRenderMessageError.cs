namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageError : MyRenderMessageBase
    {
        public string Callstack;
        public string Message;
        public bool ShouldTerminate;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.Error;
    }
}

