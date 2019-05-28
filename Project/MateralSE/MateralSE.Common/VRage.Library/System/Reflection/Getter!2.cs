namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public delegate void Getter<T, TMember>(ref T obj, out TMember value);
}

