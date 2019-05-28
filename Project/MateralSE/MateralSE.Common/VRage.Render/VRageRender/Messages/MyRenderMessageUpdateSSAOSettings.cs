namespace VRageRender.Messages
{
    using VRageRender;

    public class MyRenderMessageUpdateSSAOSettings : MyRenderMessageBase
    {
        public MySSAOSettings Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateSSAOSettings;
    }
}

