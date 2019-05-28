namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageChangeModel : MyRenderMessageBase
    {
        public uint ID;
        public string Model;
        public float Scale;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ChangeModel;
    }
}

