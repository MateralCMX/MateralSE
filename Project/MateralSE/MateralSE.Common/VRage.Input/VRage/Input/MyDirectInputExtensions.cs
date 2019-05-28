namespace VRage.Input
{
    using SharpDX;
    using SharpDX.DirectInput;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Native;

    internal static class MyDirectInputExtensions
    {
        public static Result TryAcquire(this Device device) => 
            NativeCall<int>.Method(device.NativePointer, 7);
    }
}

