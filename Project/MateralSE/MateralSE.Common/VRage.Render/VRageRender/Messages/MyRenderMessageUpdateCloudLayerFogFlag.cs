namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUpdateCloudLayerFogFlag : MyRenderMessageBase
    {
        public bool ShouldDrawFog;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateCloudLayerFogFlag;
    }
}

