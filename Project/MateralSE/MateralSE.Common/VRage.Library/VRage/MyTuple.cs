namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    public struct MyTuple
    {
        public static MyTuple<T1> Create<T1>(T1 arg1) => 
            new MyTuple<T1>(arg1);

        public static MyTuple<T1, T2> Create<T1, T2>(T1 arg1, T2 arg2) => 
            new MyTuple<T1, T2>(arg1, arg2);

        public static MyTuple<T1, T2, T3> Create<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) => 
            new MyTuple<T1, T2, T3>(arg1, arg2, arg3);

        public static MyTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => 
            new MyTuple<T1, T2, T3, T4>(arg1, arg2, arg3, arg4);

        public static MyTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => 
            new MyTuple<T1, T2, T3, T4, T5>(arg1, arg2, arg3, arg4, arg5);

        public static MyTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => 
            new MyTuple<T1, T2, T3, T4, T5, T6>(arg1, arg2, arg3, arg4, arg5, arg6);

        public static int CombineHashCodes(int h1, int h2) => 
            (((h1 << 5) + h1) ^ h2);

        public static int CombineHashCodes(int h1, int h2, int h3) => 
            CombineHashCodes(CombineHashCodes(h1, h2), h3);

        public static int CombineHashCodes(int h1, int h2, int h3, int h4) => 
            CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));

        public static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5) => 
            CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);

        public static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6) => 
            CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6));

        public static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7) => 
            CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7));

        public static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8) => 
            CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7, h8));
    }
}

