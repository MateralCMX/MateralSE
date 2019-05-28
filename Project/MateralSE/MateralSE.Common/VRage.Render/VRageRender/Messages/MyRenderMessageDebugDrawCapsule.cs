namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawCapsule : MyDebugRenderMessage
    {
        public Vector3D P0;
        public Vector3D P1;
        public float Radius;
        public VRageMath.Color Color;
        public bool DepthRead;
        public bool Shaded;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawCapsule;
    }
}

