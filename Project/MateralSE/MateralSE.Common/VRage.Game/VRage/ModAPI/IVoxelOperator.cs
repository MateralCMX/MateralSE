namespace VRage.ModAPI
{
    using System;
    using VRage.Voxels;
    using VRageMath;

    public interface IVoxelOperator
    {
        void Op(ref Vector3I position, MyStorageDataTypeEnum dataType, ref byte inOutContent);

        VoxelOperatorFlags Flags { get; }
    }
}

