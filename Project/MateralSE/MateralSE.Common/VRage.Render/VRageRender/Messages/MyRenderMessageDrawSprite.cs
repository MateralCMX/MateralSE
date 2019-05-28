namespace VRageRender.Messages
{
    using System;
    using VRageMath;
    using VRageRender;

    public class MyRenderMessageDrawSprite : MySpriteDrawRenderMessage
    {
        public string Texture;
        public VRageMath.Color Color;
        public Rectangle? SourceRectangle;
        public RectangleF DestinationRectangle;
        public Vector2 Origin;
        public float Rotation;
        public Vector2 RightVector;
        public float Depth;
        public SpriteEffects Effects;
        public bool ScaleDestination;
        public bool WaitTillLoaded;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawSprite;
    }
}

