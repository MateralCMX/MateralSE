namespace VRageRender.Messages
{
    public class MyRenderMessageRebuildCullingStructure : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RebuildCullingStructure;
    }
}

