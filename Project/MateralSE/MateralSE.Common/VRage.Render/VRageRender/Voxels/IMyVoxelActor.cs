namespace VRageRender.Voxels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public interface IMyVoxelActor
    {
        event VisibilityChange CellChange;

        event ActionRef<MatrixD> Move;

        void BeginBatch(MyVoxelActorTransitionMode? switchMode = new MyVoxelActorTransitionMode?());
        IMyVoxelActorCell CreateCell(Vector3D offset, int lod, bool notify = false);
        void DeleteCell(IMyVoxelActorCell cell, bool notify = false);
        void EndBatch(bool justLoaded);

        uint Id { get; }

        Vector3I Size { get; set; }

        MyVoxelActorTransitionMode TransitionMode { get; set; }

        bool IsBatching { get; }
    }
}

