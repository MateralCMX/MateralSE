namespace VRageRender.Messages
{
    using System;
    using VRage.Library.Utils;
    using VRageMath;

    public class MyRenderMessageSetCameraViewMatrix : MyRenderMessageBase
    {
        public MatrixD ViewMatrix;
        public Matrix ProjectionMatrix;
        public Matrix ProjectionFarMatrix;
        public float FOV;
        public float FOVForSkybox;
        public float NearPlane;
        public float FarPlane;
        public float FarFarPlane;
        public float ProjectionOffsetX;
        public float ProjectionOffsetY;
        public Vector3D CameraPosition;
        public int LastMomentUpdateIndex;
        public MyTimeSpan UpdateTime;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeEvery;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetCameraViewMatrix;
    }
}

