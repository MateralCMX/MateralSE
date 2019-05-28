namespace VRageRender.Messages
{
    public class MyRenderMessageUpdateHBAO : MyRenderMessageBase
    {
        public MyHBAOData Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateHBAO;
    }
}

