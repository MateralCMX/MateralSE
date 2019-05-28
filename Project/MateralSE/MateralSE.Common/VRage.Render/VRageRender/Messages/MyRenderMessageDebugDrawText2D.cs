namespace VRageRender.Messages
{
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyRenderMessageDebugDrawText2D : MyDebugRenderMessage
    {
        public Vector2 Coord;
        public string Text;
        public VRageMath.Color Color;
        public float Scale;
        public MyGuiDrawAlignEnum Align;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawText2D;
    }
}

