namespace VRage.Collections
{
    using System;
    using VRage;

    public class MyConcurrentArrayBufferPool<TElement> : MyConcurrentBufferPool<TElement[], MyConcurrentArrayBufferPool<TElement>.ArrayAllocator>
    {
        public MyConcurrentArrayBufferPool(string debugName) : base(debugName)
        {
        }

        private static int SizeOf<T>() => 
            (typeof(T).IsValueType ? TypeExtensions.SizeOf<T>() : IntPtr.Size);

        public class ArrayAllocator : IMyElementAllocator<TElement[]>
        {
            private static readonly int ElementSize;

            static ArrayAllocator()
            {
                MyConcurrentArrayBufferPool<TElement>.ArrayAllocator.ElementSize = MyConcurrentArrayBufferPool<TElement>.SizeOf<TElement>();
            }

            public TElement[] Allocate(int size) => 
                new TElement[size];

            public void Dispose(TElement[] instance)
            {
            }

            public int GetBucketId(TElement[] instance) => 
                instance.Length;

            public int GetBytes(TElement[] instance) => 
                (MyConcurrentArrayBufferPool<TElement>.ArrayAllocator.ElementSize * instance.Length);

            public void Init(TElement[] item)
            {
            }
        }
    }
}

