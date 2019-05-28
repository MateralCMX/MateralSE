namespace VRageRender.Messages
{
    public class MyRenderMessageUpdateRenderLight : MyRenderMessageBase
    {
        public UpdateRenderLightData Data;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderLight;
    }
}

