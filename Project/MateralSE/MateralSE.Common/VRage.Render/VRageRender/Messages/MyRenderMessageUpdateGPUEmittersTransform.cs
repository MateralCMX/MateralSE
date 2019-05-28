namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessageUpdateGPUEmittersTransform : MyRenderMessageBase
    {
        public List<MyGPUEmitterTransformUpdate> Emitters = new List<MyGPUEmitterTransformUpdate>();

        public override void Close()
        {
            base.Close();
            this.Emitters.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateGPUEmittersTransform;
    }
}

