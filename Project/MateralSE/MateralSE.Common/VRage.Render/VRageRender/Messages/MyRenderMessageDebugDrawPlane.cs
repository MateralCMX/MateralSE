namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawPlane : MyDebugRenderMessage
    {
        public Vector3D Position;
        public Vector3 Normal;
        public VRageMath.Color Color;
        public bool DepthRead;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawPlane;
    }
}

