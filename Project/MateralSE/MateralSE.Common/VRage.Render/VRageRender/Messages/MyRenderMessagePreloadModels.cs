namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;

    public class MyRenderMessagePreloadModels : MyRenderMessageBase
    {
        public List<string> Models;
        public bool ForInstancedComponent;

        public override void Close()
        {
            base.Close();
            this.Models.Clear();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.PreloadModels;
    }
}

