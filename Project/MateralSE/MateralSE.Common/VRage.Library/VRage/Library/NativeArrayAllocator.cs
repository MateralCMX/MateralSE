namespace VRage.Library
{
    using System;
    using VRage.Collections;

    public class NativeArrayAllocator : IMyElementAllocator<NativeArray>
    {
        public NativeArray Allocate(int size) => 
            new NativeArray(size);

        public void Dispose(NativeArray instance)
        {
            instance.Dispose();
        }

        public int GetBucketId(NativeArray instance) => 
            instance.Size;

        public int GetBytes(NativeArray instance) => 
            instance.Size;

        public void Init(NativeArray instance)
        {
        }
    }
}

