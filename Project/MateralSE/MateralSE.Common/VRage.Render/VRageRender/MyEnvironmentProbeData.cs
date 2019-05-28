namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct MyEnvironmentProbeData
    {
        [StructDefault]
        public static readonly MyEnvironmentProbeData Default;
        public float DrawDistance;
        public float TimeOut;
        public float AmbientScale;
        public float AmbientMinClamp;
        public float AmbientMaxClamp;
        static MyEnvironmentProbeData()
        {
            MyEnvironmentProbeData data = new MyEnvironmentProbeData {
                DrawDistance = 100f,
                AmbientMaxClamp = 0.3f,
                AmbientMinClamp = 0.02f,
                AmbientScale = 0.1f,
                TimeOut = 1f
            };
            Default = data;
        }
        private static class Defaults
        {
            public const float DrawDistance = 100f;
            public const float AmbientMaxClamp = 0.3f;
            public const float AmbientMinClamp = 0.02f;
            public const float AmbientScale = 0.1f;
            public const float TimeOut = 1f;
        }
    }
}

