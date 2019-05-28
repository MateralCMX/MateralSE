namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageRenderOffscreenTexture : MyRenderMessageBase
    {
        public string OffscreenTexture;
        public Color? BackgroundColor;
        public Vector2 AspectRatio;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RenderOffscreenTexture;
    }
}

