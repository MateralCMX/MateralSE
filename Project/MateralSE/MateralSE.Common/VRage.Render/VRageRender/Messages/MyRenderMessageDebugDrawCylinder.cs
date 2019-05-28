namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawCylinder : MyDebugRenderMessage
    {
        public MatrixD Matrix;
        public VRageMath.Color Color;
        public float Alpha;
        public bool DepthRead;
        public bool Smooth;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawCylinder;
    }
}

