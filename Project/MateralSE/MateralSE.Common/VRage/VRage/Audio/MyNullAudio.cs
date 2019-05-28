namespace VRage.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.Utils;
    using VRageMath;

    public class MyNullAudio : IMyAudio
    {
        event Action<bool> IMyAudio.VoiceChatEnabled
        {
            add
            {
            }
            remove
            {
            }
        }

        public Vector3 GetListenerPosition() => 
            Vector3.Zero;

        IMyAudioEffect IMyAudio.ApplyEffect(IMySourceVoice input, MyStringHash effect, MyCueId[] cueIds, float? duration, bool musicEffect) => 
            null;

        bool IMyAudio.ApplyTransition(MyStringId transitionEnum, int priority, MyStringId? category, bool loop) => 
            false;

        void IMyAudio.ChangeGlobalVolume(float level, float time)
        {
        }

        void IMyAudio.EnableMasterLimiter(bool e)
        {
        }

        ListReader<IMy3DSoundEmitter> IMyAudio.Get3DSounds() => 
            0;

        Dictionary<MyStringId, List<MyCueId>> IMyAudio.GetAllMusicCues() => 
            null;

        List<MyStringId> IMyAudio.GetCategories() => 
            null;

        MySoundData IMyAudio.GetCue(MyCueId cue) => 
            null;

        IMySourceVoice IMyAudio.GetSound(MyCueId cue, IMy3DSoundEmitter source, MySoundDimensions type) => 
            null;

        IMySourceVoice IMyAudio.GetSound(IMy3DSoundEmitter source, int sampleRate, int channels, MySoundDimensions dimension) => 
            null;

        int IMyAudio.GetSoundInstancesTotal2D() => 
            0;

        int IMyAudio.GetSoundInstancesTotal3D() => 
            0;

        int IMyAudio.GetUpdating3DSoundsCount() => 
            0;

        bool IMyAudio.HasAnyTransition() => 
            false;

        bool IMyAudio.IsLoopable(MyCueId cueId) => 
            false;

        bool IMyAudio.IsValidTransitionCategory(MyStringId transitionCategory, MyStringId musicCategory) => 
            false;

        void IMyAudio.LoadData(MyAudioInitParams initParams, ListReader<MySoundData> sounds, ListReader<MyAudioEffect> effects)
        {
        }

        void IMyAudio.MuteHud(bool mute)
        {
        }

        void IMyAudio.Pause()
        {
        }

        void IMyAudio.PauseGameSounds()
        {
        }

        void IMyAudio.PlayMusic(MyMusicTrack? track, int priorityForRandom)
        {
        }

        IMySourceVoice IMyAudio.PlayMusicCue(MyCueId musicCue, bool overrideMusicAllowed) => 
            null;

        IMySourceVoice IMyAudio.PlaySound(MyCueId cue, IMy3DSoundEmitter source, MySoundDimensions type, bool skipIntro, bool skipToEnd) => 
            null;

        void IMyAudio.ReloadData()
        {
        }

        void IMyAudio.ReloadData(ListReader<MySoundData> sounds, ListReader<MyAudioEffect> effects)
        {
        }

        void IMyAudio.Resume()
        {
        }

        void IMyAudio.ResumeGameSounds()
        {
        }

        float IMyAudio.SemitonesToFrequencyRatio(float semitones) => 
            0f;

        void IMyAudio.SetReverbParameters(float diffusion, float roomSize)
        {
        }

        void IMyAudio.SetSameSoundLimiter()
        {
        }

        bool IMyAudio.SourceIsCloseEnoughToPlaySound(Vector3 position, MyCueId cueId, float? customMaxDistance) => 
            false;

        void IMyAudio.StopMusic()
        {
        }

        void IMyAudio.StopUpdatingAll3DCues()
        {
        }

        void IMyAudio.UnloadData()
        {
        }

        void IMyAudio.Update(int stepSizeInMS, Vector3 listenerPosition, Vector3 listenerUp, Vector3 listenerFront, Vector3 listenerVelocity)
        {
        }

        void IMyAudio.WriteDebugInfo(StringBuilder sb)
        {
        }

        Dictionary<MyCueId, MySoundData>.ValueCollection IMyAudio.CueDefinitions =>
            null;

        MySoundData IMyAudio.SoloCue { get; set; }

        bool IMyAudio.ApplyReverb
        {
            get => 
                false;
            set
            {
            }
        }

        int IMyAudio.SampleRate =>
            0;

        float IMyAudio.VolumeMusic { get; set; }

        float IMyAudio.VolumeHud
        {
            get => 
                0f;
            set
            {
            }
        }

        float IMyAudio.VolumeGame { get; set; }

        float IMyAudio.VolumeVoiceChat { get; set; }

        bool IMyAudio.GameSoundIsPaused =>
            true;

        bool IMyAudio.Mute
        {
            get => 
                true;
            set
            {
            }
        }

        bool IMyAudio.MusicAllowed
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyAudio.EnableVoiceChat
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyAudio.UseSameSoundLimiter
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyAudio.EnableReverb
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyAudio.EnableDoppler
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyAudio.UseVolumeLimiter
        {
            get => 
                false;
            set
            {
            }
        }
    }
}

