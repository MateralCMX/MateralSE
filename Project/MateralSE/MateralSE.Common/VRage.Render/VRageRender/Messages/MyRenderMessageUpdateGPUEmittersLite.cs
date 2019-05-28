namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessageUpdateGPUEmittersLite : MyRenderMessageBase
    {
        public List<MyGPUEmitterLite> Emitters = new List<MyGPUEmitterLite>();

        public override void Close()
        {
            base.Close();
            this.Emitters.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateGPUEmittersLite;
    }
}

