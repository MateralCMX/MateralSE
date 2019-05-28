namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRopeData
    {
        public float MaxRopeLength;
        public float MinRopeLength;
        public float MinRopeLengthFromDummySizes;
        public float? MinRopeLengthStatic;
        public float CurrentRopeLength;
        public long HookEntityIdA;
        public long HookEntityIdB;
        public MyRopeDefinition Definition;
    }
}

