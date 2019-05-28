namespace Sandbox.Gui
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GridLength
    {
        public float Size;
        public GridUnitType UnitType;
        public GridLength(float size, GridUnitType unitType = 0)
        {
            this.Size = size;
            this.UnitType = unitType;
        }
    }
}

