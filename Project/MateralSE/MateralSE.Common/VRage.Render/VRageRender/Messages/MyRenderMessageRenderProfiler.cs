namespace VRageRender.Messages
{
    using System;
    using VRage.Profiler;

    public class MyRenderMessageRenderProfiler : MyRenderMessageBase
    {
        public RenderProfilerCommand Command;
        public int Index;
        public string Value;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.RenderProfiler;
    }
}

