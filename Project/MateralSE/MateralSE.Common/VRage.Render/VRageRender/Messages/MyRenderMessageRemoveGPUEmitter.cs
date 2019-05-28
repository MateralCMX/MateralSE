namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageRemoveGPUEmitter : MyRenderMessageBase
    {
        public uint GID;
        public bool Instant;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RemoveGPUEmitter;
    }
}

