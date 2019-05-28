namespace VRage.Audio
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyAudioInitParams
    {
        public IMyAudio Instance;
        public bool SimulateNoSoundCard;
        public bool DisablePooling;
        public MySoundErrorDelegate OnSoundError;
    }
}

