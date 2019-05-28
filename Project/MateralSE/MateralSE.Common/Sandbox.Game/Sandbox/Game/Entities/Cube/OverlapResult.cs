namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    internal struct OverlapResult
    {
        public Vector3I Position;
        public MyCubeBlock FatBlock;
        public MyBlockOrientation Orientation;
        public MyCubeBlockDefinition Definition;
    }
}

