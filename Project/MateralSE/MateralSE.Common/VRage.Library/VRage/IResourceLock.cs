namespace VRage
{
    using System;

    public interface IResourceLock
    {
        void AcquireExclusive();
        void AcquireShared();
        void ReleaseExclusive();
        void ReleaseShared();
        bool TryAcquireExclusive();
        bool TryAcquireShared();
    }
}

