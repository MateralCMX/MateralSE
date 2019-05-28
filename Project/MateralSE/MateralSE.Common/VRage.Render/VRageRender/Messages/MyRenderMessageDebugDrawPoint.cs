namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawPoint : MyDebugRenderMessage
    {
        public Vector3D Position;
        public VRageMath.Color Color;
        public bool DepthRead;
        public float? ClipDistance;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawPoint;
    }
}

