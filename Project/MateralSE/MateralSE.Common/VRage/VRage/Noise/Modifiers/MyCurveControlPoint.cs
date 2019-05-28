namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCurveControlPoint
    {
        public double Input;
        public double Output;
    }
}

