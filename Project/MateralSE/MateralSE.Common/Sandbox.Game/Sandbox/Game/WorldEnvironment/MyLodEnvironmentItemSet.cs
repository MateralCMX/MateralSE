namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyLodEnvironmentItemSet
    {
        public List<int> Items;
        [FixedBuffer(typeof(int), 0x10)]
        public <LodOffsets>e__FixedBuffer LodOffsets;
        [StructLayout(LayoutKind.Sequential, Size=0x40), CompilerGenerated, UnsafeValueType]
        public struct <LodOffsets>e__FixedBuffer
        {
            public int FixedElementField;
        }
    }
}

