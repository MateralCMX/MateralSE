namespace VRage.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.Utils;
    using VRageMath;

    public interface IMyAudio
    {
        event Action<bool> VoiceChatEnabled;

        IMyAudioEffect ApplyEffect(IMySourceVoice input, MyStringHash effect, MyCueId[] cueIds = null, float? duration = new float?(), bool musicEffect = false);
        bool ApplyTransition(MyStringId transitionEnum, int priority = 0, MyStringId? category = new MyStringId?(), bool loop = true);
        void ChangeGlobalVolume(float level, float time);
        void EnableMasterLimiter(bool enable);
        ListReader<IMy3DSoundEmitter> Get3DSounds();
        Dictionary<MyStringId, List<MyCueId>> GetAllMusicCues();
        List<MyStringId> GetCategories();
        MySoundData GetCue(MyCueId cue);
        Vector3 GetListenerPosition();
        IMySourceVoice GetSound(MyCueId cueId, IMy3DSoundEmitter source = null, MySoundDimensions type = 0);
        IMySourceVoice GetSound(IMy3DSoundEmitter source, int sampleRate, int channels, MySoundDimensions dimension);
        int GetSoundInstancesTotal2D();
        int GetSoundInstancesTotal3D();
        int GetUpdating3DSoundsCount();
        bool HasAnyTransition();
        bool IsLoopable(MyCueId cueId);
        bool IsValidTransitionCategory(MyStringId transitionCategory, MyStringId musicCategory);
        void LoadData(MyAudioInitParams initParams, ListReader<MySoundData> cues, ListReader<MyAudioEffect> effects);
        void MuteHud(bool mute);
        void Pause();
        void PauseGameSounds();
        void PlayMusic(MyMusicTrack? track = new MyMusicTrack?(), int priorityForRandom = 0);
        IMySourceVoice PlayMusicCue(MyCueId musicCue, bool overrideMusicAllowed);
        IMySourceVoice PlaySound(MyCueId cueId, IMy3DSoundEmitter source = null, MySoundDimensions type = 0, bool skipIntro = false, bool skipToEnd = false);
        void ReloadData();
        void ReloadData(ListReader<MySoundData> cues, ListReader<MyAudioEffect> effects);
        void Resume();
        void ResumeGameSounds();
        float SemitonesToFrequencyRatio(float semitones);
        void SetReverbParameters(float diffusion, float roomSize);
        void SetSameSoundLimiter();
        bool SourceIsCloseEnoughToPlaySound(Vector3 position, MyCueId cueId, float? customMaxDistance = 0f);
        void StopMusic();
        void StopUpdatingAll3DCues();
        void UnloadData();
        void Update(int stepSizeInMS, Vector3 listenerPosition, Vector3 listenerUp, Vector3 listenerFront, Vector3 listenerVelocity);
        void WriteDebugInfo(StringBuilder sb);

        Dictionary<MyCueId, MySoundData>.ValueCollection CueDefinitions { get; }

        MySoundData SoloCue { get; set; }

        bool ApplyReverb { get; set; }

        float VolumeMusic { get; set; }

        float VolumeHud { get; set; }

        float VolumeGame { get; set; }

        float VolumeVoiceChat { get; set; }

        bool Mute { get; set; }

        bool MusicAllowed { get; set; }

        bool GameSoundIsPaused { get; }

        bool EnableVoiceChat { get; set; }

        bool UseVolumeLimiter { get; set; }

        bool UseSameSoundLimiter { get; set; }

        bool EnableReverb { get; set; }

        int SampleRate { get; }

        bool EnableDoppler { get; set; }
    }
}

