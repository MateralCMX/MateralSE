namespace VRageRender.Messages
{
    using System;
    using VRageMath;

    public class MyRenderMessageDrawVideo : MyRenderMessageBase
    {
        public uint ID;
        public VRageMath.Rectangle Rectangle;
        public VRageMath.Color Color;
        public MyVideoRectangleFitMode FitMode;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.Draw;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DrawVideo;
    }
}

