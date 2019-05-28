namespace VRageRender.Messages
{
    internal class MyRenderMessageVideoAdaptersRequest : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.VideoAdaptersRequest;
    }
}

