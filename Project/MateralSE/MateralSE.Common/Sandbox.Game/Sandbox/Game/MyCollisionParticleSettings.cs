namespace Sandbox.Game
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCollisionParticleSettings
    {
        public string LargeGridClose;
        public string LargeGridDistant;
        public string SmallGridClose;
        public string SmallGridDistant;
        public string VoxelCollision;
        public float CloseDistanceSq;
        public float Scale;
    }
}

