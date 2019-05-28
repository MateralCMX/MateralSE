namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVoxelMiningDefinition
    {
        public string MinedOre;
        public int HitCount;
        public MyDefinitionId PhysicalItemId;
        public float RemovedRadius;
        public bool OnlyApplyMaterial;
    }
}

