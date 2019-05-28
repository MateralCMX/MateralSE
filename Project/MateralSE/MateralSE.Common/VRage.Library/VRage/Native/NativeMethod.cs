namespace VRage.Native
{
    using System;

    public static class NativeMethod
    {
        public static unsafe IntPtr CalculateAddress(IntPtr instance, int methodOffset) => 
            (*(((IntPtr*) instance.ToPointer())) + (methodOffset * sizeof(void*)));
    }
}

