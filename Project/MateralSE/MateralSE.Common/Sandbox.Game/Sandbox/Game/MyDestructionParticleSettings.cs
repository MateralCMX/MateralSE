namespace Sandbox.Game
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDestructionParticleSettings
    {
        public string DestructionSmokeLarge;
        public string DestructionHit;
        public float CloseDistanceSq;
        public float Scale;
    }
}

