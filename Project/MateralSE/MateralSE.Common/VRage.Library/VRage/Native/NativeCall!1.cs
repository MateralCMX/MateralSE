namespace VRage.Native
{
    using System;

    public static class NativeCall<TResult>
    {
        public static TResult Function(IntPtr address) => 
            NativeCallHelper<Func<IntPtr, TResult>>.Invoke(address);

        public static TResult Function<TArg1>(IntPtr address, TArg1 arg1) => 
            NativeCallHelper<Func<IntPtr, TArg1, TResult>>.Invoke(address, arg1);

        public static TResult Function<TArg1, TArg2>(IntPtr address, TArg1 arg1, TArg2 arg2) => 
            NativeCallHelper<Func<IntPtr, TArg1, TArg2, TResult>>.Invoke(address, arg1, arg2);

        public static TResult Function<TArg1, TArg2, TArg3>(IntPtr address, TArg1 arg1, TArg2 arg2, TArg3 arg3) => 
            NativeCallHelper<Func<IntPtr, TArg1, TArg2, TArg3, TResult>>.Invoke(address, arg1, arg2, arg3);

        public static TResult Function<TArg1, TArg2, TArg3, TArg4>(IntPtr address, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) => 
            NativeCallHelper<Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TResult>>.Invoke(address, arg1, arg2, arg3, arg4);

        public static TResult Function<TArg1, TArg2, TArg3, TArg4, TArg5>(IntPtr address, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) => 
            NativeCallHelper<Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>>.Invoke(address, arg1, arg2, arg3, arg4, arg5);

        public static TResult Function<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(IntPtr address, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) => 
            NativeCallHelper<Func<IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>>.Invoke(address, arg1, arg2, arg3, arg4, arg5, arg6);

        public static TResult Method(IntPtr instance, int methodOffset) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance);

        public static TResult Method<TArg1>(IntPtr instance, int methodOffset, TArg1 arg1) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TArg1, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance, arg1);

        public static TResult Method<TArg1, TArg2>(IntPtr instance, int methodOffset, TArg1 arg1, TArg2 arg2) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TArg1, TArg2, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance, arg1, arg2);

        public static TResult Method<TArg1, TArg2, TArg3>(IntPtr instance, int methodOffset, TArg1 arg1, TArg2 arg2, TArg3 arg3) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance, arg1, arg2, arg3);

        public static TResult Method<TArg1, TArg2, TArg3, TArg4>(IntPtr instance, int methodOffset, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance, arg1, arg2, arg3, arg4);

        public static TResult Method<TArg1, TArg2, TArg3, TArg4, TArg5>(IntPtr instance, int methodOffset, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance, arg1, arg2, arg3, arg4, arg5);

        public static TResult Method<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(IntPtr instance, int methodOffset, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) => 
            NativeCallHelper<Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>>.Invoke(NativeMethod.CalculateAddress(instance, methodOffset), instance, arg1, arg2, arg3, arg4, arg5, arg6);
    }
}

