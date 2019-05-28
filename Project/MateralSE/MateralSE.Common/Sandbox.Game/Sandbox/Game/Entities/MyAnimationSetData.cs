namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyAnimationSetData
    {
        public float BlendTime;
        public string Area;
        public VRage.Game.AnimationSet AnimationSet;
    }
}

