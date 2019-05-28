namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawAxis : MyDebugRenderMessage
    {
        public MatrixD Matrix;
        public float AxisLength;
        public bool DepthRead;
        public bool SkipScale;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawAxis;
    }
}

