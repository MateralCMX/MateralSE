namespace VRageRender.Messages
{
    using VRageRender;

    public class MyRenderMessageVideoAdaptersResponse : MyRenderMessageBase
    {
        public MyAdapterInfo[] Adapters;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.VideoAdaptersResponse;
    }
}

