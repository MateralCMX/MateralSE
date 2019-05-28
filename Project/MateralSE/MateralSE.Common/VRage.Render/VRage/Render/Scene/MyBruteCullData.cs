namespace VRage.Render.Scene
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Render11.Common;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyBruteCullData
    {
        public MyCullAABB Aabb;
        public MyChildCullTreeData UserData;
    }
}

