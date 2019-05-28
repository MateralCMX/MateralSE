namespace VRageRender.Profiler
{
    using System;
    using VRage.Profiler;

    public class MyNullRenderProfiler : MyRenderProfiler
    {
        protected override void Draw(MyProfiler drawProfiler, int lastFrameIndex, int frameToDraw)
        {
        }
    }
}

