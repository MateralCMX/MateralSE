namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDecalRenderInfo
    {
        public MyDecalFlags Flags;
        public Vector3D Position;
        public Vector3 Normal;
        public Vector4UByte BoneIndices;
        public Vector4 BoneWeights;
        public MyDecalBindingInfo? Binding;
        public uint[] RenderObjectIds;
        public MyStringHash Material;
        public MyStringHash Source;
    }
}

