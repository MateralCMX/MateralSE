namespace VRageRender.Messages
{
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyRenderMessageDrawSpriteNormalized : MySpriteDrawRenderMessage
    {
        public string Texture;
        public Vector2 NormalizedCoord;
        public Vector2 NormalizedSize;
        public VRageMath.Color Color;
        public MyGuiDrawAlignEnum DrawAlign;
        public float Rotation;
        public Vector2 RightVector;
        public float Scale;
        public Vector2? OriginNormalized;
        public float RotationSpeed;
        public bool WaitTillLoaded;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawSpriteNormalized;
    }
}

