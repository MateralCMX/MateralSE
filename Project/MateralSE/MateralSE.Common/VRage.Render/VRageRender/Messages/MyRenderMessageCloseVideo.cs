namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageCloseVideo : MyRenderMessageBase
    {
        public uint ID;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CloseVideo;
    }
}

