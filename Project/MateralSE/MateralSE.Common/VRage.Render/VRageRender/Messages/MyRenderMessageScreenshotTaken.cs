namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageScreenshotTaken : MyRenderMessageBase
    {
        public bool Success;
        public string Filename;
        public bool ShowNotification;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ScreenshotTaken;
    }
}

