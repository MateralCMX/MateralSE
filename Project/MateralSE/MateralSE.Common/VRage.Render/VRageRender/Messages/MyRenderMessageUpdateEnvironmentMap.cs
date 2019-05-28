namespace VRageRender.Messages
{
    public class MyRenderMessageUpdateEnvironmentMap : MyRenderMessageBase
    {
        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.Draw;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateEnvironmentMap;
    }
}

