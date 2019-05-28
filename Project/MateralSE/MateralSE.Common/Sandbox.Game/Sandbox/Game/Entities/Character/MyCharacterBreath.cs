namespace Sandbox.Game.Entities.Character
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyCharacterBreath
    {
        private readonly string BREATH_CALM = "PlayVocBreath1L";
        private readonly string BREATH_HEAVY = "PlayVocBreath2L";
        private readonly string OXYGEN_CHOKE_NORMAL = "PlayChokeA";
        private readonly string OXYGEN_CHOKE_LOW = "PlayChokeB";
        private readonly string OXYGEN_CHOKE_CRITICAL = "PlayChokeC";
        private const float CHOKE_TRESHOLD_LOW = 55f;
        private const float CHOKE_TRESHOLD_CRITICAL = 25f;
        private const float STAMINA_DRAIN_TIME_RUN = 25f;
        private const float STAMINA_DRAIN_TIME_SPRINT = 8f;
        private const float STAMINA_RECOVERY_EXHAUSTED_TO_CALM = 5f;
        private const float STAMINA_RECOVERY_CALM_TO_ZERO = 15f;
        private const float STAMINA_AMOUNT_RUN = 0.01f;
        private const float STAMINA_AMOUNT_SPRINT = 0.03125f;
        private const float STAMINA_AMOUNT_MAX = 20f;
        private IMySourceVoice m_sound;
        private MyCharacter m_character;
        private MyTimeSpan m_lastChange;
        private State m_state;
        private float m_staminaDepletion;
        private MySoundPair m_breathCalm;
        private MySoundPair m_breathHeavy;
        private MySoundPair m_oxygenChokeNormal;
        private MySoundPair m_oxygenChokeLow;
        private MySoundPair m_oxygenChokeCritical;

        public MyCharacterBreath(MyCharacter character)
        {
            this.CurrentState = State.NoBreath;
            this.m_character = character;
            string cueName = string.IsNullOrEmpty(character.Definition.BreathCalmSoundName) ? this.BREATH_CALM : character.Definition.BreathCalmSoundName;
            this.m_breathCalm = new MySoundPair(cueName, true);
            string str2 = string.IsNullOrEmpty(character.Definition.BreathHeavySoundName) ? this.BREATH_HEAVY : character.Definition.BreathHeavySoundName;
            this.m_breathHeavy = new MySoundPair(str2, true);
            string str3 = string.IsNullOrEmpty(character.Definition.OxygenChokeNormalSoundName) ? this.OXYGEN_CHOKE_NORMAL : character.Definition.OxygenChokeNormalSoundName;
            this.m_oxygenChokeNormal = new MySoundPair(str3, true);
            string str4 = string.IsNullOrEmpty(character.Definition.OxygenChokeLowSoundName) ? this.OXYGEN_CHOKE_LOW : character.Definition.OxygenChokeLowSoundName;
            this.m_oxygenChokeLow = new MySoundPair(str4, true);
            string str5 = string.IsNullOrEmpty(character.Definition.OxygenChokeCriticalSoundName) ? this.OXYGEN_CHOKE_CRITICAL : character.Definition.OxygenChokeCriticalSoundName;
            this.m_oxygenChokeCritical = new MySoundPair(str5, true);
        }

        public void Close()
        {
            if (this.m_sound != null)
            {
                this.m_sound.Stop(true);
            }
        }

        public void ForceUpdate()
        {
            if (((this.m_character != null) && ((this.m_character.StatComp != null) && ((this.m_character.StatComp.Health != null) && (MySession.Static != null)))) && ReferenceEquals(MySession.Static.LocalCharacter, this.m_character))
            {
                this.SetHealth(this.m_character.StatComp.Health.Value);
            }
        }

        private void PlaySound(MyCueId soundId, bool useCrossfade)
        {
            if (((this.m_sound != null) && this.m_sound.IsPlaying) & useCrossfade)
            {
                MyCueId[] cueIds = new MyCueId[] { soundId };
                IMyAudioEffect effect = MyAudio.Static.ApplyEffect(this.m_sound, MyStringHash.GetOrCompute("CrossFade"), cueIds, new float?((float) 0x7d0), false);
                this.m_sound = effect.OutputSound;
            }
            else
            {
                if (this.m_sound != null)
                {
                    this.m_sound.Stop(true);
                }
                this.m_sound = MyAudio.Static.PlaySound(soundId, null, MySoundDimensions.D2, false, false);
            }
        }

        private void SetHealth(float health)
        {
            if (health <= 0f)
            {
                this.CurrentState = State.NoBreath;
            }
            this.Update(true);
        }

        public void Update(bool force = false)
        {
            if (MySession.Static == null)
            {
                return;
            }
            else if (ReferenceEquals(MySession.Static.LocalCharacter, this.m_character))
            {
                this.m_staminaDepletion = (this.CurrentState != State.Heated) ? ((this.CurrentState != State.VeryHeated) ? Math.Max((float) (this.m_staminaDepletion - 0.01666667f), (float) 0f) : Math.Min((float) (this.m_staminaDepletion + 0.03125f), (float) 20f)) : Math.Min((float) (this.m_staminaDepletion + 0.01f), (float) 20f);
                if (this.CurrentState == State.NoBreath)
                {
                    if (this.m_sound != null)
                    {
                        this.m_sound.Stop(false);
                        this.m_sound = null;
                    }
                    return;
                }
                float num = this.m_character.StatComp.Health.Value;
                if (this.CurrentState != State.Choking)
                {
                    if (((this.CurrentState == State.Calm) || (this.CurrentState == State.Heated)) || (this.CurrentState == State.VeryHeated))
                    {
                        if ((this.m_staminaDepletion < 15f) && (num > 20f))
                        {
                            if (!this.m_breathCalm.SoundId.IsNull && (((this.m_sound == null) || !this.m_sound.IsPlaying) || (this.m_sound.CueEnum != this.m_breathCalm.SoundId)))
                            {
                                this.PlaySound(this.m_breathCalm.SoundId, true);
                                return;
                            }
                            if (((this.m_sound != null) && this.m_sound.IsPlaying) && this.m_breathCalm.SoundId.IsNull)
                            {
                                this.m_sound.Stop(true);
                                return;
                            }
                            return;
                        }
                        if (!this.m_breathHeavy.SoundId.IsNull && (((this.m_sound == null) || !this.m_sound.IsPlaying) || (this.m_sound.CueEnum != this.m_breathHeavy.SoundId)))
                        {
                            this.PlaySound(this.m_breathHeavy.SoundId, true);
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if ((num >= 55f) && (((this.m_sound == null) || !this.m_sound.IsPlaying) || (this.m_sound.CueEnum != this.m_oxygenChokeNormal.SoundId)))
                    {
                        this.PlaySound(this.m_oxygenChokeNormal.SoundId, false);
                        return;
                    }
                    if (((num >= 25f) && (num < 55f)) && (((this.m_sound == null) || !this.m_sound.IsPlaying) || (this.m_sound.CueEnum != this.m_oxygenChokeLow.SoundId)))
                    {
                        this.PlaySound(this.m_oxygenChokeLow.SoundId, false);
                        return;
                    }
                    if (((num > 0f) && (num < 25f)) && (((this.m_sound == null) || !this.m_sound.IsPlaying) || (this.m_sound.CueEnum != this.m_oxygenChokeCritical.SoundId)))
                    {
                        this.PlaySound(this.m_oxygenChokeCritical.SoundId, false);
                    }
                    return;
                }
            }
            else
            {
                return;
            }
            if (((this.m_sound != null) && this.m_sound.IsPlaying) && this.m_breathHeavy.SoundId.IsNull)
            {
                this.m_sound.Stop(true);
            }
        }

        public State CurrentState
        {
            get => 
                this.m_state;
            set => 
                (this.m_state = value);
        }

        public enum State
        {
            Calm,
            Heated,
            VeryHeated,
            NoBreath,
            Choking
        }
    }
}

