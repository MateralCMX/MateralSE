namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDrawSpriteAtlas : MySpriteDrawRenderMessage
    {
        public string Texture;
        public Vector2 Position;
        public Vector2 TextureOffset;
        public Vector2 TextureSize;
        public Vector2 RightVector;
        public Vector2 Scale;
        public VRageMath.Color Color;
        public Vector2 HalfSize;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawSpriteAtlas;
    }
}

