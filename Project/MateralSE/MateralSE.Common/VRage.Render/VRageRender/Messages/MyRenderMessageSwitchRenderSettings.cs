namespace VRageRender.Messages
{
    using VRageRender;

    public class MyRenderMessageSwitchRenderSettings : MyRenderMessageBase
    {
        public MyRenderSettings Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SwitchRenderSettings;
    }
}

