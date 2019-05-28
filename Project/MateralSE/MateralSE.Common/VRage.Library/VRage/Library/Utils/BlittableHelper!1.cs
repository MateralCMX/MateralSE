namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.InteropServices;

    public static class BlittableHelper<T>
    {
        public static readonly bool IsBlittable;

        static BlittableHelper()
        {
            try
            {
                T local = default(T);
                if (local != null)
                {
                    local = default(T);
                    GCHandle.Alloc(local, GCHandleType.Pinned).Free();
                    BlittableHelper<T>.IsBlittable = true;
                }
            }
            catch
            {
            }
        }
    }
}

