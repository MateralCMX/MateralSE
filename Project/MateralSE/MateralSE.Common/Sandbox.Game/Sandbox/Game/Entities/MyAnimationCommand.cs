namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyAnimationCommand
    {
        [Serialize(MyObjectFlags.DefaultZero)]
        public string AnimationSubtypeName;
        public MyPlaybackCommand PlaybackCommand;
        public MyBlendOption BlendOption;
        public MyFrameOption FrameOption;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string Area;
        public float BlendTime;
        public float TimeScale;
        public bool ExcludeLegsWhenMoving;
        public bool KeepContinuingAnimations;
    }
}

