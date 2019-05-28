namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CyclingOptions
    {
        public bool Enabled;
        public bool OnlySmallGrids;
        public bool OnlyLargeGrids;
    }
}

