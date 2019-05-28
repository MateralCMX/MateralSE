namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void Setter<T, TMember>(ref T obj, ref TMember value);
}

