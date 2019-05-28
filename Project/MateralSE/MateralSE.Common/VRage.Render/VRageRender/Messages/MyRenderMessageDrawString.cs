namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDrawString : MySpriteDrawRenderMessage
    {
        public int FontIndex;
        public Vector2 ScreenCoord;
        public Color ColorMask;
        public string Text;
        public float ScreenScale;
        public float ScreenMaxWidth;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawString;
    }
}

