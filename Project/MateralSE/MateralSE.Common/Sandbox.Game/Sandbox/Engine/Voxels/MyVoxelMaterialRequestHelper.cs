namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Runtime.InteropServices;

    internal class MyVoxelMaterialRequestHelper
    {
        [ThreadStatic]
        public static bool WantsOcclusion;
        [ThreadStatic]
        public static bool IsContouring;

        public static ContouringFlagsProxy StartContouring()
        {
            WantsOcclusion = true;
            IsContouring = true;
            return new ContouringFlagsProxy();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ContouringFlagsProxy : IDisposable
        {
            private bool oldState;
            public void Dispose()
            {
                MyVoxelMaterialRequestHelper.WantsOcclusion = false;
                MyVoxelMaterialRequestHelper.IsContouring = false;
            }
        }
    }
}

