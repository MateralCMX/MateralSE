namespace VRageRender.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public interface IMyVoxelActorCell
    {
        void SetVisible(bool visible, bool notify = true);
        void UpdateMesh(ref MyVoxelRenderCellData data);

        Vector3D Offset { get; }

        int Lod { get; }

        bool Visible { get; }
    }
}

