namespace Sandbox.Game
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyExplosionInfoSimplified
    {
        public float Damage;
        public Vector3D Center;
        public float Radius;
        public MyExplosionTypeEnum Type;
        public MyExplosionFlags Flags;
        public Vector3D VoxelCenter;
        public float ParticleScale;
        public Vector3 Velocity;
    }
}

