namespace VRage.Collections
{
    using System;

    public interface IMyElementAllocator<T> where T: class
    {
        T Allocate(int bucketId);
        void Dispose(T instance);
        int GetBucketId(T instance);
        int GetBytes(T instance);
        void Init(T instance);
    }
}

