namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUpdateLodImmediately : MyRenderMessageBase
    {
        public uint Id;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateLodImmediately;
    }
}

