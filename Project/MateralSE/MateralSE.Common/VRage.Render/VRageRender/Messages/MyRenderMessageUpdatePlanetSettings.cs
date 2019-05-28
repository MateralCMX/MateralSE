namespace VRageRender.Messages
{
    public class MyRenderMessageUpdatePlanetSettings : MyRenderMessageBase
    {
        public MyRenderPlanetSettings Settings;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdatePlanetSettings;
    }
}

