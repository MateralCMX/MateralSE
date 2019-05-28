namespace VRage
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class FastResourceLock : IDisposable, IResourceLock
    {
        private const int LockOwned = 1;
        private const int LockExclusiveWaking = 2;
        private const int LockSharedOwnersShift = 2;
        private const int LockSharedOwnersMask = 0x3ff;
        private const int LockSharedOwnersIncrement = 4;
        private const int LockSharedWaitersShift = 12;
        private const int LockSharedWaitersMask = 0x3ff;
        private const int LockSharedWaitersIncrement = 0x1000;
        private const int LockExclusiveWaitersShift = 0x16;
        private const int LockExclusiveWaitersMask = 0x3ff;
        private const int LockExclusiveWaitersIncrement = 0x400000;
        private const int ExclusiveMask = -4194302;
        private static readonly int SpinCount = VRage.NativeMethods.SpinCount;
        private int _value = 0;
        private IntPtr _sharedWakeEvent;
        private IntPtr _exclusiveWakeEvent;

        [DebuggerStepThrough]
        public void AcquireExclusive()
        {
            int num2 = 0;
            while (true)
            {
                int comparand = this._value;
                if ((comparand & 3) == 0)
                {
                    if (Interlocked.CompareExchange(ref this._value, comparand + 1, comparand) == comparand)
                    {
                        return;
                    }
                }
                else if (num2 >= SpinCount)
                {
                    this.EnsureEventCreated(ref this._exclusiveWakeEvent);
                    if (Interlocked.CompareExchange(ref this._value, comparand + 0x400000, comparand) == comparand)
                    {
                        VRage.NativeMethods.WaitForSingleObject(this._exclusiveWakeEvent, -1);
                        while (true)
                        {
                            comparand = this._value;
                            if (Interlocked.CompareExchange(ref this._value, (comparand + 1) - 2, comparand) == comparand)
                            {
                                return;
                            }
                        }
                    }
                }
                num2++;
            }
        }

        [DebuggerStepThrough]
        public void AcquireShared()
        {
            int num2 = 0;
            while (true)
            {
                int comparand = this._value;
                if ((comparand & -4190209) == 0)
                {
                    if (Interlocked.CompareExchange(ref this._value, (comparand + 1) + 4, comparand) == comparand)
                    {
                        return;
                    }
                }
                else if ((((comparand & 1) != 0) && (((comparand >> 2) & 0x3ff) != 0)) && ((comparand & -4194302) == 0))
                {
                    if (Interlocked.CompareExchange(ref this._value, comparand + 4, comparand) == comparand)
                    {
                        return;
                    }
                }
                else if (num2 >= SpinCount)
                {
                    this.EnsureEventCreated(ref this._sharedWakeEvent);
                    if (Interlocked.CompareExchange(ref this._value, comparand + 0x1000, comparand) == comparand)
                    {
                        if (VRage.NativeMethods.WaitForSingleObject(this._sharedWakeEvent, -1) != 0)
                        {
                        }
                        continue;
                    }
                }
                num2++;
            }
        }

        public void ConvertExclusiveToShared()
        {
            while (true)
            {
                int comparand = this._value;
                int releaseCount = (comparand >> 12) & 0x3ff;
                if (Interlocked.CompareExchange(ref this._value, (comparand + 4) & -4190209, comparand) == comparand)
                {
                    if (releaseCount != 0)
                    {
                        VRage.NativeMethods.ReleaseSemaphore(this._sharedWakeEvent, releaseCount, IntPtr.Zero);
                    }
                    return;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this._sharedWakeEvent != IntPtr.Zero)
            {
                VRage.NativeMethods.CloseHandle(this._sharedWakeEvent);
                this._sharedWakeEvent = IntPtr.Zero;
            }
            if (this._exclusiveWakeEvent != IntPtr.Zero)
            {
                VRage.NativeMethods.CloseHandle(this._exclusiveWakeEvent);
                this._exclusiveWakeEvent = IntPtr.Zero;
            }
        }

        private void EnsureEventCreated(ref IntPtr handle)
        {
            if (Thread.VolatileRead(ref handle) == IntPtr.Zero)
            {
                IntPtr ptr = VRage.NativeMethods.CreateSemaphore(IntPtr.Zero, 0, 0x7fffffff, null);
                if (Interlocked.CompareExchange(ref handle, ptr, IntPtr.Zero) != IntPtr.Zero)
                {
                    VRage.NativeMethods.CloseHandle(ptr);
                }
            }
        }

        ~FastResourceLock()
        {
            this.Dispose(false);
        }

        public Statistics GetStatistics() => 
            new Statistics();

        public void ReleaseExclusive()
        {
            while (true)
            {
                int comparand = this._value;
                if (((comparand >> 0x16) & 0x3ff) != 0)
                {
                    if (Interlocked.CompareExchange(ref this._value, ((comparand - 1) + 2) - 0x400000, comparand) == comparand)
                    {
                        VRage.NativeMethods.ReleaseSemaphore(this._exclusiveWakeEvent, 1, IntPtr.Zero);
                        return;
                    }
                    continue;
                }
                int releaseCount = (comparand >> 12) & 0x3ff;
                if (Interlocked.CompareExchange(ref this._value, comparand & -4190210, comparand) == comparand)
                {
                    if (releaseCount != 0)
                    {
                        VRage.NativeMethods.ReleaseSemaphore(this._sharedWakeEvent, releaseCount, IntPtr.Zero);
                    }
                    return;
                }
            }
        }

        public void ReleaseShared()
        {
            while (true)
            {
                int comparand = this._value;
                int num2 = (comparand >> 2) & 0x3ff;
                if (num2 > 1)
                {
                    if (Interlocked.CompareExchange(ref this._value, comparand - 4, comparand) == comparand)
                    {
                        return;
                    }
                    continue;
                }
                if (((comparand >> 0x16) & 0x3ff) == 0)
                {
                    if (Interlocked.CompareExchange(ref this._value, (comparand - 1) - 4, comparand) == comparand)
                    {
                        return;
                    }
                    continue;
                }
                if (Interlocked.CompareExchange(ref this._value, (((comparand - 1) + 2) - 4) - 0x400000, comparand) == comparand)
                {
                    VRage.NativeMethods.ReleaseSemaphore(this._exclusiveWakeEvent, 1, IntPtr.Zero);
                    return;
                }
            }
        }

        [DebuggerStepThrough]
        public void SpinAcquireExclusive()
        {
            while (true)
            {
                int comparand = this._value;
                if (((comparand & 3) == 0) && (Interlocked.CompareExchange(ref this._value, comparand + 1, comparand) == comparand))
                {
                    return;
                }
                if (VRage.NativeMethods.SpinEnabled)
                {
                    Thread.SpinWait(8);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }
        }

        [DebuggerStepThrough]
        public void SpinAcquireShared()
        {
            while (true)
            {
                int comparand = this._value;
                if ((comparand & -4194302) == 0)
                {
                    if ((comparand & 1) == 0)
                    {
                        if (Interlocked.CompareExchange(ref this._value, (comparand + 1) + 4, comparand) == comparand)
                        {
                            return;
                        }
                    }
                    else if ((((comparand >> 2) & 0x3ff) != 0) && (Interlocked.CompareExchange(ref this._value, comparand + 4, comparand) == comparand))
                    {
                        return;
                    }
                }
                if (VRage.NativeMethods.SpinEnabled)
                {
                    Thread.SpinWait(8);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }
        }

        [DebuggerStepThrough]
        public void SpinConvertSharedToExclusive()
        {
            while (true)
            {
                int comparand = this._value;
                if ((((comparand >> 2) & 0x3ff) == 1) && (Interlocked.CompareExchange(ref this._value, comparand - 4, comparand) == comparand))
                {
                    return;
                }
                if (VRage.NativeMethods.SpinEnabled)
                {
                    Thread.SpinWait(8);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }
        }

        public bool TryAcquireExclusive()
        {
            int comparand = this._value;
            return (((comparand & 3) == 0) ? (Interlocked.CompareExchange(ref this._value, comparand + 1, comparand) == comparand) : false);
        }

        public bool TryAcquireShared()
        {
            int comparand = this._value;
            return (((comparand & -4194302) == 0) ? (((comparand & 1) != 0) ? ((((comparand >> 2) & 0x3ff) != 0) && (Interlocked.CompareExchange(ref this._value, comparand + 4, comparand) == comparand)) : (Interlocked.CompareExchange(ref this._value, (comparand + 1) + 4, comparand) == comparand)) : false);
        }

        public bool TryConvertSharedToExclusive()
        {
            while (true)
            {
                int comparand = this._value;
                if (((comparand >> 2) & 0x3ff) != 1)
                {
                    return false;
                }
                if (Interlocked.CompareExchange(ref this._value, comparand - 4, comparand) == comparand)
                {
                    return true;
                }
            }
        }

        public int ExclusiveWaiters =>
            ((this._value >> 0x16) & 0x3ff);

        public bool Owned =>
            ((this._value & 1) != 0);

        public int SharedOwners =>
            ((this._value >> 2) & 0x3ff);

        public int SharedWaiters =>
            ((this._value >> 12) & 0x3ff);

        [StructLayout(LayoutKind.Sequential)]
        public struct Statistics
        {
            public int AcqExcl;
            public int AcqShrd;
            public int AcqExclCont;
            public int AcqShrdCont;
            public int AcqExclSlp;
            public int AcqShrdSlp;
            public int PeakExclWtrsCount;
            public int PeakShrdWtrsCount;
        }
    }
}

