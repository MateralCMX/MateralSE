namespace VRage.Render.Scene
{
    using System;
    using VRageRender.Messages;

    public interface IMyDebugDraw
    {
        void Add(MyDebugRenderMessage message);
    }
}

