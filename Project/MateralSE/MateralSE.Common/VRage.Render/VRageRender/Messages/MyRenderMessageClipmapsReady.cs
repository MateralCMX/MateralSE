namespace VRageRender.Messages
{
    internal class MyRenderMessageClipmapsReady : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.ClipmapsReady;
    }
}

