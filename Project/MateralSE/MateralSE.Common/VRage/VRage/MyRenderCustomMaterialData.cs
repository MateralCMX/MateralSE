namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderCustomMaterialData
    {
        public int Index;
        public float Mass;
        public float Strength;
        public string Diffuse;
        public string Normal;
        public float SpecularShininess;
        public float SpecularPower;
    }
}

