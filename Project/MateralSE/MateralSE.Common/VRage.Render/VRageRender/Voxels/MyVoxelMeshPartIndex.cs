namespace VRageRender.Voxels
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVoxelMeshPartIndex
    {
        public MyVoxelMaterialTriple Materials;
        public int StartIndex;
        public int IndexCount;
        public override string ToString() => 
            $"{this.Materials} {this.StartIndex}:{this.IndexCount}";
    }
}

