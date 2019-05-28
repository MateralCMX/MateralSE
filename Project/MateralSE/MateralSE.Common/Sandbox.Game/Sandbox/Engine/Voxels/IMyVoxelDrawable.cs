namespace Sandbox.Engine.Voxels
{
    using VRage.Game.Components;
    using VRage.Game.Voxels;
    using VRage.Voxels;
    using VRageMath;

    public interface IMyVoxelDrawable
    {
        IMyStorage Storage { get; }

        Vector3I Size { get; }

        Vector3D PositionLeftBottomCorner { get; }

        Matrix Orientation { get; }

        Vector3I StorageMin { get; }

        MyRenderComponentBase Render { get; }

        MyClipmapScaleEnum ScaleGroup { get; }
    }
}

