namespace VRage.Collections
{
    using System;

    public class MyConcurrentBucketPool<TElement, TAllocator> : MyConcurrentBucketPool<TElement> where TElement: class where TAllocator: IMyElementAllocator<TElement>, new()
    {
        public MyConcurrentBucketPool(string debugName) : base(debugName, Activator.CreateInstance<TAllocator>())
        {
        }
    }
}

