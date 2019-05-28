namespace Sandbox.Game
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCharacterMovementSettings
    {
        public float WalkAcceleration;
        public float WalkDecceleration;
        public float SprintAcceleration;
        public float SprintDecceleration;
    }
}

