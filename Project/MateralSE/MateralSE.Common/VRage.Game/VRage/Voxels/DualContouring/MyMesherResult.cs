namespace VRage.Voxels.DualContouring
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Voxels;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyMesherResult
    {
        public readonly MyVoxelContentConstitution Constitution;
        public readonly VrVoxelMesh Mesh;
        public static MyMesherResult Empty;
        public bool MeshProduced =>
            (this.Mesh != null);
        internal MyMesherResult(VrVoxelMesh mesh)
        {
            this.Constitution = MyVoxelContentConstitution.Mixed;
            this.Mesh = mesh;
        }

        internal MyMesherResult(MyVoxelContentConstitution constitution)
        {
            this.Constitution = constitution;
            this.Mesh = null;
        }

        static MyMesherResult()
        {
            Empty = new MyMesherResult(MyVoxelContentConstitution.Empty);
        }
    }
}

