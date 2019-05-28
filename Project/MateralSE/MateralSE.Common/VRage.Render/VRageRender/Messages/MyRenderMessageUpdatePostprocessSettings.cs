namespace VRageRender.Messages
{
    using VRageRender;

    public class MyRenderMessageUpdatePostprocessSettings : MyRenderMessageBase
    {
        public MyPostprocessSettings Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdatePostprocessSettings;
    }
}

