namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public interface IMyDebugDrawBatchAabb : IDisposable
    {
        void Add(ref BoundingBoxD aabb, Color? color = new Color?());
    }
}

