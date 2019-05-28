namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawTriangle : MyDebugRenderMessage
    {
        public Vector3D Vertex0;
        public Vector3D Vertex1;
        public Vector3D Vertex2;
        public VRageMath.Color Color;
        public bool DepthRead;
        public bool Smooth;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawTriangle;
    }
}

