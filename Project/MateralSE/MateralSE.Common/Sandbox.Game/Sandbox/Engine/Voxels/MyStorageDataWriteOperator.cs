namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyStorageDataWriteOperator : IVoxelOperator
    {
        private readonly MyStorageData m_data;
        public VoxelOperatorFlags Flags =>
            VoxelOperatorFlags.WriteAll;
        public MyStorageDataWriteOperator(MyStorageData data)
        {
            this.m_data = data;
        }

        public void Op(ref Vector3I position, MyStorageDataTypeEnum dataType, ref byte outData)
        {
            outData = this.m_data.Get(dataType, ref position);
        }
    }
}

