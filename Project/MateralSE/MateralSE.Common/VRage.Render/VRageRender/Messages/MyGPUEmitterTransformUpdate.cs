namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGPUEmitterTransformUpdate
    {
        public uint GID;
        public Matrix3x3 Rotation;
        public Vector3D Position;
        public float Scale;
        public float ParticlesPerSecond;
        public float ParticlesPerFrame;
    }
}

