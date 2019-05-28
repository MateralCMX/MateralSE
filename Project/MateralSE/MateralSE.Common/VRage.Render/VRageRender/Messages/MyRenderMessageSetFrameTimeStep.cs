namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageSetFrameTimeStep : MyRenderMessageBase
    {
        public float TimeStepInSeconds;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.SetFrameTimeStep;
    }
}

