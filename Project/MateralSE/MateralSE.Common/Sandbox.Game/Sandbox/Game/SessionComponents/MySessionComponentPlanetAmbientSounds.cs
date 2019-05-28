namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game.Components;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class MySessionComponentPlanetAmbientSounds : MySessionComponentBase
    {
        private IMySourceVoice m_sound;
        private IMyAudioEffect m_effect;
        private readonly MyStringHash m_crossFade = MyStringHash.GetOrCompute("CrossFade");
        private readonly MyStringHash m_fadeIn = MyStringHash.GetOrCompute("FadeIn");
        private readonly MyStringHash m_fadeOut = MyStringHash.GetOrCompute("FadeOut");
        private MyPlanet m_nearestPlanet;
        private long m_nextPlanetRecalculation = -1L;
        private int m_planetRecalculationIntervalInSpace = 300;
        private int m_planetRecalculationIntervalOnPlanet = 300;
        private float m_volumeModifier = 1f;
        private static float m_volumeModifierTarget = 1f;
        private float m_volumeOriginal = 1f;
        private const float VOLUME_CHANGE_SPEED = 0.25f;
        public float VolumeModifierGlobal = 1f;
        private MyPlanetEnvironmentalSoundRule[] m_nearestSoundRules;
        private readonly MyPlanetEnvironmentalSoundRule[] m_emptySoundRules = new MyPlanetEnvironmentalSoundRule[0];

        private static MyPlanet FindNearestPlanet(Vector3D worldPosition)
        {
            BoundingBoxD box = new BoundingBoxD(worldPosition, worldPosition);
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref box);
            if ((closestPlanet == null) || (closestPlanet.AtmosphereAltitude <= Vector3D.Distance(worldPosition, closestPlanet.PositionComp.GetPosition())))
            {
                return closestPlanet;
            }
            return null;
        }

        private static bool FindSoundRuleIndex(float angleFromEquator, float height, float sunAngleFromZenith, MyPlanetEnvironmentalSoundRule[] soundRules, out int outRuleIndex)
        {
            outRuleIndex = -1;
            if (soundRules != null)
            {
                for (int i = 0; i < soundRules.Length; i++)
                {
                    if (soundRules[i].Check(angleFromEquator, height, sunAngleFromZenith))
                    {
                        outRuleIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public override void LoadData()
        {
            base.LoadData();
        }

        private void PlaySound(MyCueId sound)
        {
            if ((this.m_sound == null) || !this.m_sound.IsPlaying)
            {
                this.m_sound = MyAudio.Static.PlaySound(sound, null, MySoundDimensions.D2, false, false);
                if (!sound.IsNull)
                {
                    float? duration = null;
                    this.m_effect = MyAudio.Static.ApplyEffect(this.m_sound, this.m_fadeIn, null, duration, false);
                }
                if (this.m_effect != null)
                {
                    this.m_sound = this.m_effect.OutputSound;
                }
            }
            else if (((this.m_effect != null) && this.m_effect.Finished) && sound.IsNull)
            {
                this.m_sound.Stop(true);
            }
            else if (this.m_sound.CueEnum != sound)
            {
                if ((this.m_effect != null) && !this.m_effect.Finished)
                {
                    this.m_effect.AutoUpdate = true;
                }
                if (sound.IsNull)
                {
                    this.m_effect = MyAudio.Static.ApplyEffect(this.m_sound, this.m_fadeOut, null, 5000f, false);
                }
                else
                {
                    MyCueId[] cueIds = new MyCueId[] { sound };
                    this.m_effect = MyAudio.Static.ApplyEffect(this.m_sound, this.m_crossFade, cueIds, 5000f, false);
                }
                if ((this.m_effect != null) && !this.m_effect.Finished)
                {
                    this.m_effect.AutoUpdate = true;
                    this.m_sound = this.m_effect.OutputSound;
                }
            }
            if (this.m_sound != null)
            {
                MySoundData cue = MyAudio.Static.GetCue(sound);
                this.m_volumeOriginal = (cue != null) ? cue.Volume : 1f;
                this.m_sound.SetVolume((this.m_volumeOriginal * this.m_volumeModifier) * this.VolumeModifierGlobal);
            }
        }

        public static void SetAmbientOff()
        {
            m_volumeModifierTarget = 0f;
        }

        public static void SetAmbientOn()
        {
            m_volumeModifierTarget = 1f;
        }

        private void SetNearestPlanet(MyPlanet planet)
        {
            this.m_nearestPlanet = planet;
            if ((this.m_nearestPlanet != null) && (this.m_nearestPlanet.Generator != null))
            {
                this.m_nearestSoundRules = this.m_nearestPlanet.Generator.SoundRules ?? this.m_emptySoundRules;
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (this.m_sound != null)
            {
                this.m_sound.Stop(false);
            }
            this.m_nearestPlanet = null;
            this.m_nearestSoundRules = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (!Game.IsDedicated)
            {
                if (this.m_volumeModifier != m_volumeModifierTarget)
                {
                    this.m_volumeModifier = (this.m_volumeModifier >= m_volumeModifierTarget) ? MyMath.Clamp(this.m_volumeModifier - 0.004166667f, m_volumeModifierTarget, 1f) : MyMath.Clamp(this.m_volumeModifier + 0.004166667f, 0f, m_volumeModifierTarget);
                    if ((this.m_sound != null) && this.m_sound.IsPlaying)
                    {
                        this.m_sound.SetVolume((this.m_volumeOriginal * this.m_volumeModifier) * this.VolumeModifierGlobal);
                    }
                }
                long gameplayFrameCounter = MySession.Static.GameplayFrameCounter;
                if (gameplayFrameCounter >= this.m_nextPlanetRecalculation)
                {
                    this.Planet = FindNearestPlanet(MySector.MainCamera.Position);
                    this.m_nextPlanetRecalculation = (this.Planet != null) ? (gameplayFrameCounter + this.m_planetRecalculationIntervalOnPlanet) : (gameplayFrameCounter + this.m_planetRecalculationIntervalInSpace);
                }
                if (((this.Planet != null) && (this.Planet.Provider != null)) && ((!MyFakes.ENABLE_NEW_SOUNDS || !MySession.Static.Settings.RealisticSound) || this.Planet.HasAtmosphere))
                {
                    Vector3D vectord = MySector.MainCamera.Position - this.Planet.PositionComp.GetPosition();
                    double num2 = vectord.Length();
                    float height = this.Planet.Provider.Shape.DistanceToRatio((float) num2);
                    if (height >= 0f)
                    {
                        int num5;
                        Vector3D vectord2 = -vectord / num2;
                        if (FindSoundRuleIndex((float) -vectord2.Y, height, MySector.DirectionToSunNormalized.Dot((Vector3) -vectord2), this.m_nearestSoundRules, out num5))
                        {
                            this.PlaySound(new MyCueId(this.m_nearestSoundRules[num5].EnvironmentSound));
                        }
                        else
                        {
                            MyCueId sound = new MyCueId();
                            this.PlaySound(sound);
                        }
                    }
                }
                else if (this.m_sound != null)
                {
                    this.m_sound.Stop(true);
                }
            }
        }

        private MyPlanet Planet
        {
            get => 
                this.m_nearestPlanet;
            set => 
                this.SetNearestPlanet(value);
        }

        public override bool IsRequiredByGame =>
            (base.IsRequiredByGame && MyFakes.ENABLE_PLANETS);
    }
}

