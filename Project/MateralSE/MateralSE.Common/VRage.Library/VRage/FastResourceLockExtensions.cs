namespace VRage
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class FastResourceLockExtensions
    {
        [DebuggerStepThrough]
        public static MyOwnedExclusiveLock AcquireExclusiveRecursiveUsing(this FastResourceLock lockObject, Ref<int> ownerField)
        {
            if (!lockObject.IsOwnedByCurrentThread(ownerField))
            {
                return new MyOwnedExclusiveLock(lockObject, ownerField);
            }
            return new MyOwnedExclusiveLock();
        }

        [DebuggerStepThrough]
        public static MyExclusiveLock AcquireExclusiveUsing(this FastResourceLock lockObject) => 
            new MyExclusiveLock(lockObject);

        [DebuggerStepThrough]
        public static MySharedLock AcquireSharedRecursiveUsing(this FastResourceLock lockObject, Ref<int> ownerField)
        {
            if (!lockObject.IsOwnedByCurrentThread(ownerField))
            {
                return new MySharedLock(lockObject);
            }
            return new MySharedLock();
        }

        [DebuggerStepThrough]
        public static MySharedLock AcquireSharedUsing(this FastResourceLock lockObject) => 
            new MySharedLock(lockObject);

        [DebuggerStepThrough]
        public static bool IsOwnedByCurrentThread(this FastResourceLock lockObject, Ref<int> ownerField) => 
            (lockObject.Owned && (ownerField.Value == Thread.CurrentThread.ManagedThreadId));

        [StructLayout(LayoutKind.Sequential)]
        public struct MyExclusiveLock : IDisposable
        {
            private readonly FastResourceLock m_lockObject;
            [DebuggerStepThrough]
            public MyExclusiveLock(FastResourceLock lockObject)
            {
                this.m_lockObject = lockObject;
                this.m_lockObject.AcquireExclusive();
            }

            public void Dispose()
            {
                this.m_lockObject.ReleaseExclusive();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyOwnedExclusiveLock : IDisposable
        {
            private Ref<int> m_owner;
            private FastResourceLockExtensions.MyExclusiveLock m_core;
            public MyOwnedExclusiveLock(FastResourceLock lockObject, Ref<int> ownerField)
            {
                this.m_owner = ownerField;
                this.m_core = new FastResourceLockExtensions.MyExclusiveLock(lockObject);
                this.m_owner.Value = Thread.CurrentThread.ManagedThreadId;
            }

            public void Dispose()
            {
                if (this.m_owner != null)
                {
                    this.m_owner.Value = -1;
                    this.m_core.Dispose();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MySharedLock : IDisposable
        {
            private readonly FastResourceLock m_lockObject;
            [DebuggerStepThrough]
            public MySharedLock(FastResourceLock lockObject)
            {
                this.m_lockObject = lockObject;
                this.m_lockObject.AcquireShared();
            }

            public void Dispose()
            {
                if (this.m_lockObject != null)
                {
                    this.m_lockObject.ReleaseShared();
                }
            }
        }
    }
}

