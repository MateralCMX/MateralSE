namespace VRageRender.Messages
{
    public class MyRenderMessageClearScreenDecals : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ClearDecals;
    }
}

