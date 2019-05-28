namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyResourceSourceInfo
    {
        public MyDefinitionId ResourceTypeId;
        public float DefinedOutput;
        public float ProductionToCapacityMultiplier;
        public bool IsInfiniteCapacity;
    }
}

