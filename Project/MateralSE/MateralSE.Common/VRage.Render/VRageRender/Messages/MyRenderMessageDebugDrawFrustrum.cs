namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawFrustrum : MyDebugRenderMessage
    {
        public BoundingFrustumD Frustum;
        public VRageMath.Color Color;
        public float Alpha;
        public bool DepthRead;
        public bool Smooth;
        public bool Cull;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawFrustrum;
    }
}

