namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Runtime.InteropServices;
    using VRageRender;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyInstanceInfo
    {
        public MyInstanceFlagsEnum Flags;
        public float MaxViewDistance;
        public MyInstanceInfo(MyInstanceFlagsEnum flags, float maxViewDistance)
        {
            this.Flags = flags;
            this.MaxViewDistance = maxViewDistance;
        }
    }
}

