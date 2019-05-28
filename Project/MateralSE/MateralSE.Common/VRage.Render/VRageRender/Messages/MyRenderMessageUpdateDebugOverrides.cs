namespace VRageRender.Messages
{
    public class MyRenderMessageUpdateDebugOverrides : MyRenderMessageBase
    {
        public MyRenderDebugOverrides Overrides;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateDebugOverrides;
    }
}

