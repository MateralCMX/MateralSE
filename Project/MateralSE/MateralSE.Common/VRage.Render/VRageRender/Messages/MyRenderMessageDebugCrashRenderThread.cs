namespace VRageRender.Messages
{
    public class MyRenderMessageDebugCrashRenderThread : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugCrashRenderThread;
    }
}

