namespace VRage.Collections
{
    using System;

    public class MyConcurrentBufferPool<TElement, TAllocator> : MyConcurrentBufferPool<TElement> where TElement: class where TAllocator: IMyElementAllocator<TElement>, new()
    {
        public MyConcurrentBufferPool(string debugName) : base(debugName, Activator.CreateInstance<TAllocator>())
        {
        }
    }
}

