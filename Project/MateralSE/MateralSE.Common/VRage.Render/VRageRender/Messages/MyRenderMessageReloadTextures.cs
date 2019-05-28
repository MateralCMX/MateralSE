namespace VRageRender.Messages
{
    public class MyRenderMessageReloadTextures : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ReloadTextures;
    }
}

