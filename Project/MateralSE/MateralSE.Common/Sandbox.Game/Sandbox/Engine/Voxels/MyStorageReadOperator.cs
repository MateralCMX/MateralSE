namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyStorageReadOperator : IVoxelOperator
    {
        private readonly MyStorageData m_data;
        public VoxelOperatorFlags Flags =>
            VoxelOperatorFlags.Read;
        public MyStorageReadOperator(MyStorageData data)
        {
            this.m_data = data;
        }

        public void Op(ref Vector3I position, MyStorageDataTypeEnum dataType, ref byte inData)
        {
            this.m_data.Set(dataType, ref position, inData);
        }
    }
}

