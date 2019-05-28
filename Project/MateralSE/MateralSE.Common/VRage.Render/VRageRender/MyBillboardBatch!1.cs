namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using VRage.Generics;

    public class MyBillboardBatch<T> where T: MyBillboard, new()
    {
        public readonly List<T> Billboards;
        public readonly MyObjectsPool<T> Pool;
        public readonly Dictionary<int, MyBillboardViewProjection> Matrices;

        public MyBillboardBatch()
        {
            this.Billboards = new List<T>(0xbb8);
            this.Pool = new MyObjectsPool<T>(0xbb8, null);
            this.Matrices = new Dictionary<int, MyBillboardViewProjection>(10);
        }

        public void Clear()
        {
            this.Billboards.Clear();
            this.Pool.DeallocateAll();
        }
    }
}

