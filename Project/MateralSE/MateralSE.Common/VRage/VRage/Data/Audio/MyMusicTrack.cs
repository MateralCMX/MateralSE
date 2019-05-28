namespace VRage.Data.Audio
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyMusicTrack
    {
        public MyStringId TransitionCategory;
        public MyStringId MusicCategory;
    }
}

