namespace VRageRender.Messages
{
    public class MyRenderMessageUnloadData : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UnloadData;
    }
}

