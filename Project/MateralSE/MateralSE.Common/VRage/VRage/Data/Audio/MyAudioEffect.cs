namespace VRage.Data.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyAudioEffect
    {
        public int ResultEmitterIdx;
        public List<List<SoundEffect>> SoundsEffects = new List<List<SoundEffect>>();
        public MyStringHash EffectId;

        public enum FilterType
        {
            LowPass,
            BandPass,
            HighPass,
            Notch,
            None
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SoundEffect
        {
            public Curve VolumeCurve;
            public float Duration;
            public MyAudioEffect.FilterType Filter;
            public float Frequency;
            public bool StopAfter;
            public float OneOverQ;
        }
    }
}

