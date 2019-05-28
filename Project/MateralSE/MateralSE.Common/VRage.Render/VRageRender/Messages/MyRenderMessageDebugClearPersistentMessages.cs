namespace VRageRender.Messages
{
    public class MyRenderMessageDebugClearPersistentMessages : MyDebugRenderMessage
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.DebugDraw;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugClearPersistentMessages;
    }
}

