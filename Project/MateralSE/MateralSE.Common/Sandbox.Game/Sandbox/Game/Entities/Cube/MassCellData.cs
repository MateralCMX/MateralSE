namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MassCellData
    {
        public HkMassElement MassElement;
        public float LastMass;
    }
}

