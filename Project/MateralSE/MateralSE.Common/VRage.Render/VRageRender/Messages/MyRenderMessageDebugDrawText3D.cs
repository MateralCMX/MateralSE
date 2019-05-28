namespace VRageRender.Messages
{
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyRenderMessageDebugDrawText3D : MyDebugRenderMessage
    {
        public Vector3D Coord;
        public string Text;
        public VRageMath.Color Color;
        public float Scale;
        public bool DepthRead;
        public float? ClipDistance;
        public MyGuiDrawAlignEnum Align;
        public int CustomViewProjection;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawText3D;
    }
}

