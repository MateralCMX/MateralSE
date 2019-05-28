namespace VRage.ModAPI
{
    using System;

    [Flags]
    public enum VoxelOperatorFlags
    {
        Read = 1,
        Write = 2,
        WriteAll = 6,
        None = 0,
        ReadWrite = 3,
        Default = 3
    }
}

