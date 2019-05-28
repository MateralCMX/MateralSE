namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageCreateRenderLight : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderLight;
    }
}

