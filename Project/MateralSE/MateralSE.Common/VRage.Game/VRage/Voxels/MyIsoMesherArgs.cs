namespace VRage.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyIsoMesherArgs
    {
        public IMyStorage Storage;
        public MyCellCoord GeometryCell;
    }
}

