namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawAABB : MyDebugRenderMessage
    {
        public BoundingBoxD AABB;
        public VRageMath.Color Color;
        public float Alpha;
        public float Scale;
        public bool DepthRead;
        public bool Shaded;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawAABB;
    }
}

