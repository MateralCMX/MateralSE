namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetGravityProvider : MyRenderMessageBase
    {
        public Func<Vector3D, Vector3> CalculateGravityInPoint;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetGravityProvider;
    }
}

