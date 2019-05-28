namespace VRageRender.Messages
{
    public class MyRenderMessageReloadModels : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ReloadModels;
    }
}

