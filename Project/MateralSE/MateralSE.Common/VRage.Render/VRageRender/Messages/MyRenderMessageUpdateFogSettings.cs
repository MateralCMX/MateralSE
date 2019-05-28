namespace VRageRender.Messages
{
    public class MyRenderMessageUpdateFogSettings : MyRenderMessageBase
    {
        public MyRenderFogSettings Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateFogSettings;
    }
}

