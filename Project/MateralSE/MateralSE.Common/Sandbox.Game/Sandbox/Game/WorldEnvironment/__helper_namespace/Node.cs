namespace Sandbox.Game.WorldEnvironment.__helper_namespace
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Node
    {
        [FixedBuffer(typeof(int), 4)]
        public <Children>e__FixedBuffer Children;
        public int Lod;
        [StructLayout(LayoutKind.Sequential, Size=0x10), CompilerGenerated, UnsafeValueType]
        public struct <Children>e__FixedBuffer
        {
            public int FixedElementField;
        }
    }
}

