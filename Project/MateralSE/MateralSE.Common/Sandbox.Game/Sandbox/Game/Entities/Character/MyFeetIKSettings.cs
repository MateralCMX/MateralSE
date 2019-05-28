namespace Sandbox.Game.Entities.Character
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyFeetIKSettings
    {
        public bool Enabled;
        public float BelowReachableDistance;
        public float AboveReachableDistance;
        public float VerticalShiftUpGain;
        public float VerticalShiftDownGain;
        public Vector3 FootSize;
    }
}

