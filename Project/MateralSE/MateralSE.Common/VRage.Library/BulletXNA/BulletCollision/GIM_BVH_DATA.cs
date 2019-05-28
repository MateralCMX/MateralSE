namespace BulletXNA.BulletCollision
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GIM_BVH_DATA
    {
        public AABB m_bound;
        public int m_data;
    }
}

