namespace VRage.Render.Scene
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyEnvironment
    {
        public MyTimeSpan FrameTime;
        public float LastFrameDelta;
        public Vector3D CameraPosition;
    }
}

