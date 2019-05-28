namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public delegate void InterpolationHandler<T>(T item1, T item2, float interpolator, out T result);
}

