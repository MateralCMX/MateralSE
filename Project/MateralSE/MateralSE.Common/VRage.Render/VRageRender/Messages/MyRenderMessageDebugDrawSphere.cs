namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawSphere : MyDebugRenderMessage
    {
        public Vector3D Position;
        public float Radius;
        public VRageMath.Color Color;
        public float Alpha;
        public float? ClipDistance;
        public bool DepthRead;
        public bool Smooth;
        public bool Cull;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawSphere;
    }
}

