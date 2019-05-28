namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageRemoveRenderObject : MyRenderMessageBase
    {
        public uint ID;
        public bool FadeOut;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RemoveRenderObject;
    }
}

