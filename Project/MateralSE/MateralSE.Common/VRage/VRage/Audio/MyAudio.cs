namespace VRage.Audio
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Collections;

    public class MyAudio
    {
        public static readonly int MAX_SAMPLE_RATE = 0xbb80;

        public static void LoadData(MyAudioInitParams initParams, ListReader<MySoundData> sounds, ListReader<MyAudioEffect> effects)
        {
            Static = initParams.Instance;
            OnSoundError = initParams.OnSoundError;
            Static.LoadData(initParams, sounds, effects);
        }

        public static void ReloadData(ListReader<MySoundData> sounds, ListReader<MyAudioEffect> effects)
        {
            Static.ReloadData(sounds, effects);
        }

        public static void UnloadData()
        {
            if (Static != null)
            {
                Static.UnloadData();
                Static = null;
            }
        }

        public static MySoundErrorDelegate OnSoundError
        {
            [CompilerGenerated]
            get => 
                <OnSoundError>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<OnSoundError>k__BackingField = value);
        }

        public static IMyAudio Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }
    }
}

