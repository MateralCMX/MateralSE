namespace VRageRender.Messages
{
    public class MyRenderMessageReloadEffects : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ReloadEffects;
    }
}

