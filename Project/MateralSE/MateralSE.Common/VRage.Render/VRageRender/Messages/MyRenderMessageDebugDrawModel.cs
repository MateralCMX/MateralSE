namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawModel : MyDebugRenderMessage
    {
        public string Model;
        public MatrixD WorldMatrix;
        public VRageMath.Color Color;
        public bool DepthRead;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawModel;
    }
}

