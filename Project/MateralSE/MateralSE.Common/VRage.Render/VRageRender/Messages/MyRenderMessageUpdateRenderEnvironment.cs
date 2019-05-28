namespace VRageRender.Messages
{
    using System;
    using VRageRender;

    public class MyRenderMessageUpdateRenderEnvironment : MyRenderMessageBase
    {
        public MyEnvironmentData Data;
        public bool ResetEyeAdaptation;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderEnvironment;
    }
}

