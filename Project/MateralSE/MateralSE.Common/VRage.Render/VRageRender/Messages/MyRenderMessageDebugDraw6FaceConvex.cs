namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDraw6FaceConvex : MyDebugRenderMessage
    {
        public Vector3D[] Vertices;
        public VRageMath.Color Color;
        public float Alpha;
        public bool DepthRead;
        public bool Fill;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDraw6FaceConvex;
    }
}

