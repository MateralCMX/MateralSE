namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawOBB : MyDebugRenderMessage
    {
        public MatrixD Matrix;
        public VRageMath.Color Color;
        public float Alpha;
        public bool DepthRead;
        public bool Smooth;
        public bool Cull;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawOBB;
    }
}

