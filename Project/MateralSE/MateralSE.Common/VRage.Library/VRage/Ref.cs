namespace VRage
{
    using System;
    using System.Runtime.CompilerServices;

    public static class Ref
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ref<T> Create<T>(T value) where T: struct
        {
            Ref<T> ref1 = new Ref<T>();
            ref1.Value = value;
            return ref1;
        }
    }
}

