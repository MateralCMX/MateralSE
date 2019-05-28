namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessageUpdateGPUEmitters : MyRenderMessageBase
    {
        public List<MyGPUEmitter> Emitters = new List<MyGPUEmitter>();

        public override void Close()
        {
            base.Close();
            this.Emitters.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateGPUEmitters;
    }
}

