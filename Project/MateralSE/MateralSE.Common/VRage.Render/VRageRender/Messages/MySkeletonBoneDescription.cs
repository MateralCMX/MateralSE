namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MySkeletonBoneDescription
    {
        public Matrix SkinTransform;
        public int Parent;
    }
}

