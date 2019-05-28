namespace VRage.Collections
{
    using System;

    public class MyConcurrentBufferPool<TElement> : MyConcurrentBucketPool<TElement> where TElement: class
    {
        public MyConcurrentBufferPool(string debugName, IMyElementAllocator<TElement> allocator) : base(debugName, allocator)
        {
        }

        public TElement Get(int bucketId) => 
            base.Get(MyConcurrentBufferPool<TElement>.GetNearestBiggerPowerOfTwo(bucketId));

        private static int GetNearestBiggerPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 0x10;
            v++;
            return v;
        }
    }
}

