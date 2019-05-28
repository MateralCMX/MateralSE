namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetMouseCapture : MyRenderMessageBase
    {
        public bool Capture;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetMouseCapture;
    }
}

