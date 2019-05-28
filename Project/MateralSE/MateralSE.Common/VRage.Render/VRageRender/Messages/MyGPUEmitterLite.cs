namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGPUEmitterLite
    {
        public uint GID;
        public float ParticlesPerSecond;
        public float ParticlesPerFrame;
    }
}

