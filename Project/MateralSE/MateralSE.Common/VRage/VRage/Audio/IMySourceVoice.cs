namespace VRage.Audio
{
    using System;
    using System.Runtime.InteropServices;

    public interface IMySourceVoice
    {
        void Cleanup();
        void Pause();
        void Resume();
        void SetVolume(float value);
        void Start(bool skipIntro, bool skipToEnd = false);
        void StartBuffered();
        void Stop(bool force = false);
        void SubmitBuffer(byte[] buffer, int size);

        Action StoppedPlaying { get; set; }

        bool IsPlaying { get; }

        float FrequencyRatio { get; set; }

        bool IsLoopable { get; }

        MyCueId CueEnum { get; }

        bool IsBuffered { get; }

        bool IsPaused { get; }

        float Volume { get; }

        float VolumeMultiplier { get; set; }
    }
}

