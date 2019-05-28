namespace VRageRender.Voxels
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyClipmap
    {
        void BindToActor(IMyVoxelActor actor);
        void DebugDraw();
        void InvalidateAll();
        void InvalidateRange(Vector3I min, Vector3I max);
        void Unload();
        void Update(ref MatrixD view, BoundingFrustumD viewFrustum, float farClipping);

        IEnumerable<IMyVoxelActorCell> Cells { get; }

        IMyVoxelActor Actor { get; }

        Vector3I Size { get; }
    }
}

