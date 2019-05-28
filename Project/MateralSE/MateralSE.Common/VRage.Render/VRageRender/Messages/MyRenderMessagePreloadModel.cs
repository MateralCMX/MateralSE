namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessagePreloadModel : MyRenderMessageBase
    {
        public bool ForceOldPipeline;
        public string Name;
        public float Rescale = 1f;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.PreloadModel;
    }
}

