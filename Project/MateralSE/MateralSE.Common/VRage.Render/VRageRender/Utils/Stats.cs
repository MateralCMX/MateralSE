namespace VRageRender.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Stats;

    public static class Stats
    {
        public static readonly MyStats Timing = new MyStats();
        public static readonly MyStats Generic = MyRenderStats.Generic;
        public static readonly MyStats Network = new MyStats();
        public static MyPerAppLifetime PerAppLifetime;

        static Stats()
        {
            MyStats[] stats = new MyStats[] { Timing, Generic, Network };
            MyRenderStats.SetColumn(MyRenderStats.ColumnEnum.Left, stats);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyPerAppLifetime
        {
            public int MyModelsCount;
            public int MyModelsMeshesCount;
            public int MyModelsVertexesCount;
            public int MyModelsTrianglesCount;
        }
    }
}

