namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct MyTuple<T1>
    {
        public T1 Item1;
        public MyTuple(T1 item1)
        {
            this.Item1 = item1;
        }
    }
}

