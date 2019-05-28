namespace VRageRender.Messages
{
    using VRageRender;

    public class MyRenderMessageSwitchDeviceSettings : MyRenderMessageBase
    {
        public MyRenderDeviceSettings Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SwitchDeviceSettings;
    }
}

