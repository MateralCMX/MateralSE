namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetLightShadowIgnore : MyRenderMessageBase
    {
        public uint ID;
        public uint ID2;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetLightShadowIgnore;
    }
}

