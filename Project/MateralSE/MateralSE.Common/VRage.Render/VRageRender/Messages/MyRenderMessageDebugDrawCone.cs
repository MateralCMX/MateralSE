namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDebugDrawCone : MyDebugRenderMessage
    {
        public Vector3D Translation;
        public Vector3D DirectionVector;
        public Vector3D BaseVector;
        public VRageMath.Color Color;
        public bool DepthRead;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawCone;
    }
}

