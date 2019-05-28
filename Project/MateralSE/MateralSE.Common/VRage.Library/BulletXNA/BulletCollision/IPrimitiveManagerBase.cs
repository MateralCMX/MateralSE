namespace BulletXNA.BulletCollision
{
    using System;
    using System.Runtime.InteropServices;

    public interface IPrimitiveManagerBase
    {
        void Cleanup();
        void GetPrimitiveBox(int prim_index, out AABB primbox);
        int GetPrimitiveCount();
        void GetPrimitiveTriangle(int prim_index, PrimitiveTriangle triangle);
        bool IsTrimesh();
    }
}

