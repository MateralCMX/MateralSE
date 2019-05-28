namespace VRageRender.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public interface IMyLodController
    {
        event Action<IMyLodController> Loaded;

        void BindToActor(IMyVoxelActor actor);
        void DebugDraw(ref MatrixD cameraMatrix);
        void InvalidateAll();
        void InvalidateRange(Vector3I min, Vector3I max);
        void Unload();
        void Update(ref MatrixD view, BoundingFrustumD viewFrustum, float farClipping);

        IEnumerable<IMyVoxelActorCell> Cells { get; }

        IMyVoxelActor Actor { get; }

        Vector3I Size { get; }

        IMyVoxelRenderDataProcessorProvider VoxelRenderDataProcessorProvider { get; set; }

        float? SpherizeRadius { get; }

        Vector3D SpherizePosition { get; }
    }
}

