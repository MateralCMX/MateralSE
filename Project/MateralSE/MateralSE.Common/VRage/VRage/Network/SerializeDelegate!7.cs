namespace VRage.Network
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Library.Collections;

    public delegate void SerializeDelegate<T1, T2, T3, T4, T5, T6, T7>(T1 inst, BitStream stream, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5, ref T6 arg6, ref T7 arg7);
}

