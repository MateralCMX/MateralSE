namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawLine2D : MyDebugRenderMessage
    {
        public Vector2 PointFrom;
        public Vector2 PointTo;
        public Color ColorFrom;
        public Color ColorTo;
        public Matrix? Projection;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawLine2D;
    }
}

