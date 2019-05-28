namespace VRageRender.Messages
{
    public class MyRenderMessageCollectGarbage : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CollectGarbage;
    }
}

