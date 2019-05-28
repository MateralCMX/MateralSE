namespace VRage.Audio
{
    using System;

    public interface IMyAudioEffect
    {
        void SetPosition(float msecs);
        void SetPositionRelative(float position);
        void Update(int stepInMsec);

        bool AutoUpdate { get; set; }

        IMySourceVoice OutputSound { get; }

        bool Finished { get; }
    }
}

