namespace VRageRender.Messages
{
    using System.Threading;

    public class MyRenderMessageDebugWaitForPresent : MyRenderMessageBase
    {
        public EventWaitHandle WaitHandle;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.DebugDraw;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugWaitForPresent;
    }
}

