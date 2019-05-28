namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyEntity3DSoundEmitter : IMy3DSoundEmitter
    {
        internal static readonly ConcurrentDictionary<MyCueId, LastTimePlayingData> LastTimePlaying = new ConcurrentDictionary<MyCueId, LastTimePlayingData>();
        private static List<MyEntity3DSoundEmitter> m_entityEmitters = new List<MyEntity3DSoundEmitter>();
        private static int m_lastUpdate = -2147483648;
        private static MyStringHash m_effectHasHelmetInOxygen = MyStringHash.GetOrCompute("LowPassHelmet");
        private static MyStringHash m_effectNoHelmetNoOxygen = MyStringHash.GetOrCompute("LowPassNoHelmetNoOxy");
        private static MyStringHash m_effectEnclosedCockpitInSpace = MyStringHash.GetOrCompute("LowPassCockpitNoOxy");
        private static MyStringHash m_effectEnclosedCockpitInAir = MyStringHash.GetOrCompute("LowPassCockpit");
        private MyCueId m_cueEnum;
        private readonly MyCueId myEmptyCueId;
        private MySoundPair m_soundPair;
        private IMySourceVoice m_sound;
        private IMySourceVoice m_secondarySound;
        private MyCueId m_secondaryCueEnum;
        private float m_secondaryVolumeRatio;
        private bool m_secondaryEnabled;
        private float m_secondaryBaseVolume;
        private float m_baseVolume;
        private VRage.Game.Entity.MyEntity m_entity;
        private Vector3? m_position;
        private Vector3? m_velocity;
        private List<MyCueId> m_soundsQueue;
        private bool m_playing2D;
        private bool m_usesDistanceSounds;
        private bool m_useRealisticByDefault;
        private bool m_alwaysHearOnRealistic;
        private MyCueId m_closeSoundCueId;
        private MySoundPair m_closeSoundSoundPair;
        private bool m_realistic;
        private float m_volumeMultiplier;
        private bool m_volumeChanging;
        private MySoundData m_lastSoundData;
        private FastResourceLock m_lastSoundDataLock;
        private MyStringHash m_activeEffect;
        private int m_lastPlayedWaveNumber;
        private float? m_customVolume;
        public Dictionary<int, ConcurrentCachingList<Delegate>> EmitterMethods;
        [CompilerGenerated]
        private Action<MyEntity3DSoundEmitter> StoppedPlaying;
        public bool CanPlayLoopSounds;

        public event Action<MyEntity3DSoundEmitter> StoppedPlaying
        {
            [CompilerGenerated] add
            {
                Action<MyEntity3DSoundEmitter> stoppedPlaying = this.StoppedPlaying;
                while (true)
                {
                    Action<MyEntity3DSoundEmitter> a = stoppedPlaying;
                    Action<MyEntity3DSoundEmitter> action3 = (Action<MyEntity3DSoundEmitter>) Delegate.Combine(a, value);
                    stoppedPlaying = Interlocked.CompareExchange<Action<MyEntity3DSoundEmitter>>(ref this.StoppedPlaying, action3, a);
                    if (ReferenceEquals(stoppedPlaying, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity3DSoundEmitter> stoppedPlaying = this.StoppedPlaying;
                while (true)
                {
                    Action<MyEntity3DSoundEmitter> source = stoppedPlaying;
                    Action<MyEntity3DSoundEmitter> action3 = (Action<MyEntity3DSoundEmitter>) Delegate.Remove(source, value);
                    stoppedPlaying = Interlocked.CompareExchange<Action<MyEntity3DSoundEmitter>>(ref this.StoppedPlaying, action3, source);
                    if (ReferenceEquals(stoppedPlaying, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyEntity3DSoundEmitter(VRage.Game.Entity.MyEntity entity, bool useStaticList = false, float dopplerScaler = 1f)
        {
            int num1;
            int num2;
            this.m_cueEnum = new MyCueId(MyStringHash.NullOrEmpty);
            this.myEmptyCueId = new MyCueId(MyStringHash.NullOrEmpty);
            this.m_soundPair = MySoundPair.Empty;
            this.m_secondaryCueEnum = new MyCueId(MyStringHash.NullOrEmpty);
            this.m_secondaryBaseVolume = 1f;
            this.m_baseVolume = 1f;
            this.m_soundsQueue = new List<MyCueId>();
            this.m_closeSoundCueId = new MyCueId(MyStringHash.NullOrEmpty);
            this.m_closeSoundSoundPair = MySoundPair.Empty;
            this.m_volumeMultiplier = 1f;
            this.m_lastSoundDataLock = new FastResourceLock();
            this.m_activeEffect = MyStringHash.NullOrEmpty;
            this.m_lastPlayedWaveNumber = -1;
            this.EmitterMethods = new Dictionary<int, ConcurrentCachingList<Delegate>>();
            this.CanPlayLoopSounds = true;
            this.m_entity = entity;
            this.DopplerScaler = dopplerScaler;
            foreach (object obj2 in Enum.GetValues(typeof(MethodsEnum)))
            {
                this.EmitterMethods.Add((int) obj2, new ConcurrentCachingList<Delegate>());
            }
            this.EmitterMethods[1].Add(new Func<bool>(this.IsControlledEntity));
            if (((MySession.Static != null) && MySession.Static.Settings.RealisticSound) && MyFakes.ENABLE_NEW_SOUNDS)
            {
                this.EmitterMethods[0].Add(new Func<bool>(this.IsInAtmosphere));
                this.EmitterMethods[0].Add(new Func<bool>(this.IsCurrentWeapon));
                this.EmitterMethods[0].Add(new Func<bool>(this.IsOnSameGrid));
                this.EmitterMethods[0].Add(new Func<bool>(this.IsControlledEntity));
                this.EmitterMethods[1].Add(new Func<bool>(this.IsCurrentWeapon));
                this.EmitterMethods[2].Add(new Func<MySoundPair, MyCueId>(this.SelectCue));
                this.EmitterMethods[3].Add(new Func<MyStringHash>(this.SelectEffect));
            }
            this.UpdateEmitterMethods();
            if ((MySession.Static == null) || !MySession.Static.Settings.RealisticSound)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) MyFakes.ENABLE_NEW_SOUNDS;
            }
            this.m_useRealisticByDefault = (bool) num1;
            if ((MySession.Static == null) || !MySession.Static.Settings.RealisticSound)
            {
                num2 = 0;
            }
            else
            {
                num2 = (int) MyFakes.ENABLE_NEW_SOUNDS;
            }
            if ((((num2 & useStaticList) != 0) && (entity != null)) && MyFakes.ENABLE_NEW_SOUNDS_QUICK_UPDATE)
            {
                List<MyEntity3DSoundEmitter> entityEmitters = m_entityEmitters;
                lock (entityEmitters)
                {
                    m_entityEmitters.Add(this);
                }
            }
        }

        private bool CanHearSound()
        {
            bool flag = this.EmitterMethods[0].Count == 0;
            if ((MySession.Static.Settings.RealisticSound && MyFakes.ENABLE_NEW_SOUNDS) && this.m_alwaysHearOnRealistic)
            {
                flag = true;
            }
            foreach (Func<bool> func in this.EmitterMethods[0])
            {
                if (func != null)
                {
                    flag |= func();
                    if (flag)
                    {
                        break;
                    }
                }
            }
            return (this.IsCloseEnough() & flag);
        }

        private MyCueId CheckDistanceSounds(MyCueId soundId)
        {
            if (!soundId.IsNull)
            {
                using (this.m_lastSoundDataLock.AcquireExclusiveUsing())
                {
                    int num2;
                    if (((this.m_lastSoundData != null) && (this.m_lastSoundData.DistantSounds != null)) && (this.m_lastSoundData.DistantSounds.Count > 0))
                    {
                        float num = this.SourcePosition.LengthSquared();
                        num2 = -1;
                        this.m_usesDistanceSounds = true;
                        this.m_secondaryEnabled = false;
                        for (int i = 0; i < this.m_lastSoundData.DistantSounds.Count; i++)
                        {
                            float num3 = this.m_lastSoundData.DistantSounds[i].Distance * this.m_lastSoundData.DistantSounds[i].Distance;
                            if (num > num3)
                            {
                                num2 = i;
                            }
                            else
                            {
                                float num4 = (this.m_lastSoundData.DistantSounds[i].DistanceCrossfade >= 0f) ? (this.m_lastSoundData.DistantSounds[i].DistanceCrossfade * this.m_lastSoundData.DistantSounds[i].DistanceCrossfade) : float.MaxValue;
                                if (num <= num4)
                                {
                                    break;
                                }
                                this.m_secondaryVolumeRatio = (num - num4) / (num3 - num4);
                                this.m_secondaryEnabled = true;
                                MySoundPair objA = new MySoundPair(this.m_lastSoundData.DistantSounds[i].Sound, true);
                                if (!ReferenceEquals(objA, MySoundPair.Empty))
                                {
                                    this.m_secondaryCueEnum = this.SelectCue(objA);
                                }
                                else if (num2 >= 0)
                                {
                                    this.m_secondaryCueEnum = new MyCueId(MyStringHash.GetOrCompute(this.m_lastSoundData.DistantSounds[num2].Sound));
                                }
                                else
                                {
                                    this.m_secondaryEnabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.m_usesDistanceSounds = false;
                        goto TR_0002;
                    }
                    if (num2 < 0)
                    {
                        this.m_soundPair = this.m_closeSoundSoundPair;
                    }
                    else
                    {
                        MySoundPair objA = new MySoundPair(this.m_lastSoundData.DistantSounds[num2].Sound, true);
                        if (ReferenceEquals(objA, MySoundPair.Empty))
                        {
                            soundId = new MyCueId(MyStringHash.GetOrCompute(this.m_lastSoundData.DistantSounds[num2].Sound));
                        }
                        else
                        {
                            this.m_soundPair = objA;
                            soundId = this.SelectCue(this.m_soundPair);
                        }
                    }
                }
            }
        TR_0002:
            if (!this.m_secondaryEnabled)
            {
                this.m_secondaryCueEnum = this.myEmptyCueId;
            }
            return soundId;
        }

        private bool CheckForSynchronizedSounds()
        {
            if ((this.m_lastSoundData != null) && (this.m_lastSoundData.PreventSynchronization >= 0))
            {
                LastTimePlayingData data;
                bool flag = LastTimePlaying.TryGetValue(this.SoundId, out data);
                if (!flag)
                {
                    data.LastTime = 0;
                    data.Emitter = this;
                    LastTimePlaying.TryAdd(this.SoundId, data);
                }
                int sessionTotalFrames = MyFpsManager.GetSessionTotalFrames();
                if (((sessionTotalFrames - data.LastTime) <= this.m_lastSoundData.PreventSynchronization) & flag)
                {
                    MyAudio.Static.GetListenerPosition();
                    if (this.SourcePosition.LengthSquared() > data.Emitter.SourcePosition.LengthSquared())
                    {
                        return false;
                    }
                }
                data.LastTime = sessionTotalFrames;
                data.Emitter = this;
                LastTimePlaying[this.SoundId] = data;
            }
            return true;
        }

        public void Cleanup()
        {
            if (this.Sound != null)
            {
                this.Sound.Cleanup();
                this.Sound = null;
            }
            if (this.m_secondarySound != null)
            {
                this.m_secondarySound.Cleanup();
                this.m_secondarySound = null;
            }
        }

        public static void ClearEntityEmitters()
        {
            List<MyEntity3DSoundEmitter> entityEmitters = m_entityEmitters;
            lock (entityEmitters)
            {
                m_entityEmitters.Clear();
            }
        }

        public bool FastUpdate(bool silenced)
        {
            if (silenced)
            {
                this.VolumeMultiplier = Math.Max((float) 0f, (float) (this.m_volumeMultiplier - 0.01f));
                return (this.m_volumeMultiplier != 0f);
            }
            this.VolumeMultiplier = Math.Min((float) 1f, (float) (this.m_volumeMultiplier + 0.01f));
            return (this.m_volumeMultiplier != 1f);
        }

        private bool IsBeingWelded()
        {
            if (MySession.Static == null)
            {
                return false;
            }
            if (MySession.Static.ControlledEntity == null)
            {
                return false;
            }
            MyCharacter entity = MySession.Static.ControlledEntity.Entity as MyCharacter;
            if (entity == null)
            {
                return false;
            }
            MyEngineerToolBase currentWeapon = entity.CurrentWeapon as MyEngineerToolBase;
            if (currentWeapon == null)
            {
                return false;
            }
            MyCubeGrid targetGrid = currentWeapon.GetTargetGrid();
            MyCubeBlock objB = this.Entity as MyCubeBlock;
            if (((targetGrid == null) || ((objB == null) || !ReferenceEquals(targetGrid, objB.CubeGrid))) || !currentWeapon.HasHitBlock)
            {
                return false;
            }
            MySlimBlock cubeBlock = targetGrid.GetCubeBlock(currentWeapon.TargetCube);
            return ((cubeBlock != null) ? (ReferenceEquals(cubeBlock.FatBlock, objB) && currentWeapon.IsShooting) : false);
        }

        private bool IsCloseEnough() => 
            (this.m_playing2D || MyAudio.Static.SourceIsCloseEnoughToPlaySound(this.SourcePosition, this.SoundId, this.CustomMaxDistance));

        private bool IsControlledEntity() => 
            ((MySession.Static.ControlledEntity != null) && ReferenceEquals(this.m_entity, MySession.Static.ControlledEntity.Entity));

        private bool IsCurrentWeapon() => 
            ((this.Entity is IMyHandheldGunObject<MyDeviceBase>) && ((MySession.Static.ControlledEntity != null) && ((MySession.Static.ControlledEntity.Entity is MyCharacter) && ReferenceEquals((MySession.Static.ControlledEntity.Entity as MyCharacter).CurrentWeapon, this.Entity))));

        private bool IsInAtmosphere() => 
            ((MySession.Static.LocalCharacter != null) && ((MySession.Static.LocalCharacter.AtmosphereDetectorComp != null) && MySession.Static.LocalCharacter.AtmosphereDetectorComp.InAtmosphere));

        private bool IsInTerminal() => 
            (MyGuiScreenTerminal.IsOpen && ((MyGuiScreenTerminal.InteractedEntity != null) && ReferenceEquals(MyGuiScreenTerminal.InteractedEntity, this.Entity)));

        private bool IsOnSameGrid()
        {
            MyCubeGrid cubeGrid;
            MyCubeGrid grid2;
            List<IMyEntity>.Enumerator enumerator;
            bool flag;
            switch (this.Entity)
            {
                case (null):
                    goto TR_0000;
                    break;

                case (this.Entity.EntityId == 0):
                    goto TR_0000;
                    break;

                case (MyCubeBlock _):
                    goto TR_001E;
                    break;

                case (MyCubeGrid _):
                    goto TR_001E;
                    break;

                case (MyVoxelBase _):
                    if ((MySession.Static.ControlledEntity != null) && (MySession.Static.ControlledEntity.Entity is MyCockpit))
                    {
                        return false;
                    }
                    if ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.SoundComp != null))
                    {
                        if (ReferenceEquals(MySession.Static.LocalCharacter.SoundComp.StandingOnVoxel, this.Entity as MyVoxelBase))
                        {
                            return true;
                        }
                        if (MySession.Static.LocalCharacter.SoundComp.StandingOnGrid != null)
                        {
                            if (MySession.Static.LocalCharacter.SoundComp.StandingOnGrid.IsStatic)
                            {
                                return true;
                            }
                            using (enumerator = MySession.Static.LocalCharacter.SoundComp.StandingOnGrid.GridSystems.LandingSystem.GetAttachedEntities().GetEnumerator())
                            {
                                while (true)
                                {
                                    if (!enumerator.MoveNext())
                                    {
                                        break;
                                    }
                                    IMyEntity current = enumerator.Current;
                                    if ((current is MyVoxelBase) && ReferenceEquals(current as MyVoxelBase, this.Entity as MyVoxelBase))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    goto TR_0023;
                    break;

                default:
                    goto TR_0023;
                    break;
            }
            goto TR_0023;
        TR_0000:
            return false;
        TR_0001:
            return false;
        TR_0003:
            return ((cubeGrid != null) ? (!ReferenceEquals(cubeGrid, grid2) ? MyCubeGridGroups.Static.Physical.HasSameGroup(cubeGrid, grid2) : true) : false);
        TR_001E:
            cubeGrid = null;
            if ((MySession.Static.ControlledEntity != null) && (MySession.Static.ControlledEntity.Entity is MyCockpit))
            {
                cubeGrid = (MySession.Static.ControlledEntity.Entity as MyCockpit).CubeGrid;
            }
            else if ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.SoundComp != null))
            {
                cubeGrid = MySession.Static.LocalCharacter.SoundComp.StandingOnGrid;
            }
            if (cubeGrid == null)
            {
                if (MySession.Static.LocalCharacter == null)
                {
                    goto TR_0001;
                }
                else if (MySession.Static.LocalCharacter.AtmosphereDetectorComp != null)
                {
                    if (MySession.Static.LocalCharacter.AtmosphereDetectorComp.InShipOrStation)
                    {
                        Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>((long) MySession.Static.LocalCharacter.OxygenSourceGridEntityId, out cubeGrid, false);
                    }
                }
                else
                {
                    goto TR_0001;
                }
            }
            grid2 = (this.Entity is MyCubeBlock) ? (this.Entity as MyCubeBlock).CubeGrid : (this.Entity as MyCubeGrid);
            if (cubeGrid != null)
            {
                goto TR_0003;
            }
            else
            {
                if (((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.SoundComp != null)) && (MySession.Static.LocalCharacter.SoundComp.StandingOnVoxel != null))
                {
                    if (grid2.IsStatic)
                    {
                        return true;
                    }
                    using (enumerator = grid2.GridSystems.LandingSystem.GetAttachedEntities().GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            IMyEntity current = enumerator.Current;
                            if ((current is MyVoxelBase) && ReferenceEquals(current as MyVoxelBase, MySession.Static.LocalCharacter.SoundComp.StandingOnVoxel))
                            {
                                return true;
                            }
                        }
                    }
                }
                goto TR_0003;
            }
            return flag;
        TR_0023:
            return false;
        }

        private bool IsThereAir() => 
            ((MySession.Static.LocalCharacter != null) && ((MySession.Static.LocalCharacter.AtmosphereDetectorComp != null) && !MySession.Static.LocalCharacter.AtmosphereDetectorComp.InVoid));

        private void OnStopPlaying()
        {
            if (this.StoppedPlaying != null)
            {
                this.StoppedPlaying(this);
            }
        }

        public void PlaySingleSound(MyCueId soundId, bool stopPrevious = false, bool skipIntro = false, bool? force3D = new bool?())
        {
            if (this.m_cueEnum != soundId)
            {
                this.PlaySoundWithDistance(soundId, stopPrevious, skipIntro, false, true, false, false, force3D);
            }
        }

        public void PlaySingleSound(MySoundPair soundId, bool stopPrevious = false, bool skipIntro = false, bool skipToEnd = false, bool? force3D = new bool?())
        {
            this.m_closeSoundSoundPair = soundId;
            this.m_soundPair = soundId;
            MyCueId id = this.m_useRealisticByDefault ? soundId.Realistic : soundId.Arcade;
            if (this.EmitterMethods[2].Count > 0)
            {
                id = ((Func<MySoundPair, MyCueId>) this.EmitterMethods[2][0])(soundId);
            }
            if (!this.m_cueEnum.Equals(id))
            {
                this.PlaySoundWithDistance(id, stopPrevious, skipIntro, false, true, false, skipToEnd, force3D);
            }
        }

        public void PlaySound(byte[] buffer, int size, int sampleRate, float volume = 1f, float maxDistance = 0f, MySoundDimensions dimension = 1)
        {
            this.CustomMaxDistance = new float?(maxDistance);
            this.CustomVolume = new float?(volume);
            if (this.Sound == null)
            {
                this.Sound = MyAudio.Static.GetSound(this, sampleRate, 1, dimension);
            }
            if (this.Sound != null)
            {
                this.Sound.SubmitBuffer(buffer, size);
                if (!this.Sound.IsPlaying)
                {
                    this.Sound.StartBuffered();
                }
            }
        }

        public void PlaySound(MySoundPair soundId, bool stopPrevious = false, bool skipIntro = false, bool force2D = false, bool alwaysHearOnRealistic = false, bool skipToEnd = false, bool? force3D = new bool?())
        {
            this.m_closeSoundSoundPair = soundId;
            this.m_soundPair = soundId;
            MyCueId id = this.m_useRealisticByDefault ? soundId.Realistic : soundId.Arcade;
            if (this.EmitterMethods[2].Count > 0)
            {
                id = ((Func<MySoundPair, MyCueId>) this.EmitterMethods[2][0])(soundId);
            }
            bool? nullable = force3D;
            this.PlaySoundWithDistance(id, stopPrevious, skipIntro, force2D, true, alwaysHearOnRealistic, skipToEnd, nullable);
        }

        private void PlaySoundInternal(bool skipIntro = false, bool skipToEnd = false, bool force2D = false, bool alwaysHearOnRealistic = false, bool? force3D = new bool?())
        {
            this.Force2D = force2D;
            if (force3D != null)
            {
                this.Force3D = force3D.Value;
            }
            this.m_alwaysHearOnRealistic = alwaysHearOnRealistic;
            this.Loop = false;
            if (!this.SoundId.IsNull && this.CheckForSynchronizedSounds())
            {
                bool flag1;
                int canPlayLoopSounds;
                if (!this.ShouldPlay2D() || this.Force3D)
                {
                    flag1 = this.Force2D;
                }
                else
                {
                    flag1 = true;
                }
                this.m_playing2D = flag1;
                if (!MyAudio.Static.IsLoopable(this.SoundId) || skipToEnd)
                {
                    canPlayLoopSounds = 0;
                }
                else
                {
                    canPlayLoopSounds = (int) this.CanPlayLoopSounds;
                }
                this.Loop = (bool) canPlayLoopSounds;
                if (this.Loop && (MySession.Static.ElapsedPlayTime.TotalSeconds < 6.0))
                {
                    skipIntro = true;
                }
                if (this.m_playing2D)
                {
                    this.Sound = MyAudio.Static.PlaySound(this.m_closeSoundCueId, this, MySoundDimensions.D2, skipIntro, skipToEnd);
                }
                else if (this.CanHearSound())
                {
                    this.Sound = MyAudio.Static.PlaySound(this.SoundId, this, MySoundDimensions.D3, skipIntro, skipToEnd);
                }
            }
            if ((this.Sound == null) || !this.Sound.IsPlaying)
            {
                this.OnStopPlaying();
            }
            else
            {
                if (((MyMusicController.Static != null) && ((this.m_lastSoundData != null) && (this.m_lastSoundData.DynamicMusicCategory != MyStringId.NullOrEmpty))) && (this.m_lastSoundData.DynamicMusicAmount > 0))
                {
                    MyMusicController.Static.IncreaseCategory(this.m_lastSoundData.DynamicMusicCategory, this.m_lastSoundData.DynamicMusicAmount);
                }
                this.m_baseVolume = this.Sound.Volume;
                this.Sound.SetVolume(this.Sound.Volume * this.RealisticVolumeChange);
                if (this.m_secondaryEnabled)
                {
                    MyCueId secondaryCueEnum = this.m_secondaryCueEnum;
                    this.m_secondarySound = MyAudio.Static.PlaySound(this.m_secondaryCueEnum, this, MySoundDimensions.D3, skipIntro, skipToEnd);
                    if (this.Sound == null)
                    {
                        return;
                    }
                    if (this.m_secondarySound != null)
                    {
                        this.m_secondaryBaseVolume = this.m_secondarySound.Volume;
                        this.Sound.SetVolume((this.RealisticVolumeChange * this.m_baseVolume) * (1f - this.m_secondaryVolumeRatio));
                        this.m_secondarySound.SetVolume((this.RealisticVolumeChange * this.m_secondaryBaseVolume) * this.m_secondaryVolumeRatio);
                        this.m_secondarySound.VolumeMultiplier = this.m_volumeMultiplier;
                    }
                }
                this.Sound.VolumeMultiplier = this.m_volumeMultiplier;
                this.Sound.StoppedPlaying = new Action(this.OnStopPlaying);
                if (this.EmitterMethods[3].Count > 0)
                {
                    this.m_activeEffect = MyStringHash.NullOrEmpty;
                    MyStringHash hash = ((Func<MyStringHash>) this.EmitterMethods[3][0])();
                    if (hash != MyStringHash.NullOrEmpty)
                    {
                        float? duration = null;
                        IMyAudioEffect effect = MyAudio.Static.ApplyEffect(this.Sound, hash, null, duration, false);
                        if (effect != null)
                        {
                            this.Sound = effect.OutputSound;
                            this.m_activeEffect = hash;
                        }
                    }
                }
            }
        }

        public void PlaySoundWithDistance(MyCueId soundId, bool stopPrevious = false, bool skipIntro = false, bool force2D = false, bool useDistanceCheck = true, bool alwaysHearOnRealistic = false, bool skipToEnd = false, bool? force3D = new bool?())
        {
            this.m_lastSoundData = MyAudio.Static.GetCue(soundId);
            if (useDistanceCheck)
            {
                this.m_closeSoundCueId = soundId;
                MyCueId id1 = this.CheckDistanceSounds(soundId);
                soundId = id1;
            }
            bool usesDistanceSounds = this.m_usesDistanceSounds;
            if (this.Sound != null)
            {
                if (stopPrevious)
                {
                    this.StopSound(true, true);
                }
                else if (this.Loop)
                {
                    IMySourceVoice sound = this.Sound;
                    this.StopSound(true, true);
                    this.m_soundsQueue.Add(sound.CueEnum);
                }
            }
            if (this.m_secondarySound != null)
            {
                this.m_secondarySound.Stop(true);
            }
            this.SoundId = soundId;
            bool flag2 = force2D;
            this.PlaySoundInternal(skipIntro | skipToEnd, skipToEnd, flag2, alwaysHearOnRealistic, force3D);
            this.m_usesDistanceSounds = usesDistanceSounds;
        }

        public static void PreloadSound(MySoundPair soundId)
        {
            IMySourceVoice voice = MyAudio.Static.GetSound(soundId.SoundId, null, MySoundDimensions.D2);
            if (voice != null)
            {
                voice.Start(false, false);
                voice.Stop(true);
            }
        }

        private MyCueId SelectCue(MySoundPair sound)
        {
            int isPressurized;
            if (!this.m_useRealisticByDefault)
            {
                this.m_realistic = false;
                return sound.Arcade;
            }
            if (this.m_lastSoundData == null)
            {
                this.m_lastSoundData = MyAudio.Static.GetCue(sound.Realistic);
            }
            if ((this.m_lastSoundData != null) && this.m_lastSoundData.AlwaysUseOneMode)
            {
                this.m_realistic = true;
                return sound.Realistic;
            }
            MyCockpit cockpit = (MySession.Static.LocalCharacter != null) ? (MySession.Static.LocalCharacter.Parent as MyCockpit) : null;
            if ((cockpit == null) || (cockpit.CubeGrid.GridSizeEnum != MyCubeSize.Large))
            {
                isPressurized = 0;
            }
            else
            {
                isPressurized = (int) cockpit.BlockDefinition.IsPressurized;
            }
            bool flag = (bool) isPressurized;
            if (this.IsThereAir() | flag)
            {
                this.m_realistic = false;
                return sound.Arcade;
            }
            this.m_realistic = true;
            return sound.Realistic;
        }

        private MyStringHash SelectEffect()
        {
            int isPressurized;
            if ((this.m_lastSoundData != null) && !this.m_lastSoundData.ModifiableByHelmetFilters)
            {
                return MyStringHash.NullOrEmpty;
            }
            if (((MySession.Static == null) || ((MySession.Static.LocalCharacter == null) || ((MySession.Static.LocalCharacter.OxygenComponent == null) || !MyFakes.ENABLE_NEW_SOUNDS))) || !MySession.Static.Settings.RealisticSound)
            {
                return MyStringHash.NullOrEmpty;
            }
            bool flag = this.IsThereAir();
            MyCockpit parent = MySession.Static.LocalCharacter.Parent as MyCockpit;
            if ((parent == null) || (parent.BlockDefinition == null))
            {
                isPressurized = 0;
            }
            else
            {
                isPressurized = (int) parent.BlockDefinition.IsPressurized;
            }
            bool flag2 = (bool) isPressurized;
            if (flag & flag2)
            {
                return m_effectEnclosedCockpitInAir;
            }
            if (((!flag & flag2) && (parent.CubeGrid != null)) && (parent.CubeGrid.GridSizeEnum == MyCubeSize.Large))
            {
                return m_effectEnclosedCockpitInSpace;
            }
            if (MySession.Static.LocalCharacter.OxygenComponent.HelmetEnabled & flag)
            {
                return m_effectHasHelmetInOxygen;
            }
            if (((this.m_lastSoundData == null) || !MySession.Static.LocalCharacter.OxygenComponent.HelmetEnabled) || flag)
            {
                if ((!MySession.Static.LocalCharacter.OxygenComponent.HelmetEnabled && !flag) && (((parent == null) || (parent.BlockDefinition == null)) || !parent.BlockDefinition.IsPressurized))
                {
                    return m_effectNoHelmetNoOxygen;
                }
                if (((this.m_lastSoundData == null) || ((parent == null) || ((parent.BlockDefinition == null) || (!parent.BlockDefinition.IsPressurized || (parent.CubeGrid == null))))) || (parent.CubeGrid.GridSizeEnum != MyCubeSize.Small))
                {
                    return MyStringHash.NullOrEmpty;
                }
            }
            return this.m_lastSoundData.RealisticFilter;
        }

        public void SetPosition(Vector3D? position)
        {
            if (position != null)
            {
                this.m_position = new Vector3?(position.Value - MySector.MainCamera.Position);
            }
            else
            {
                Vector3? nullable1;
                Vector3D? nullable = position;
                if (nullable != null)
                {
                    nullable1 = new Vector3?(nullable.GetValueOrDefault());
                }
                else
                {
                    nullable1 = null;
                }
                this.m_position = nullable1;
            }
        }

        public void SetVelocity(Vector3? velocity)
        {
            this.m_velocity = velocity;
        }

        private bool ShouldPlay2D()
        {
            bool flag = this.EmitterMethods[1].Count == 0;
            foreach (Delegate delegate2 in this.EmitterMethods[1])
            {
                if (delegate2 != null)
                {
                    flag |= ((Func<bool>) delegate2)();
                }
            }
            return flag;
        }

        public void StopSound(bool forced, bool cleanUp = true)
        {
            this.m_usesDistanceSounds = false;
            if (this.Sound == null)
            {
                if (cleanUp)
                {
                    this.Loop = false;
                    this.SoundId = this.myEmptyCueId;
                }
            }
            else
            {
                bool? nullable;
                this.Sound.Stop(forced);
                if (this.Loop && !forced)
                {
                    nullable = null;
                    this.PlaySoundInternal(true, true, false, false, nullable);
                }
                if (this.m_soundsQueue.Count == 0)
                {
                    this.Sound = null;
                    if (cleanUp)
                    {
                        this.Loop = false;
                        this.SoundId = this.myEmptyCueId;
                    }
                }
                else if (cleanUp)
                {
                    this.SoundId = this.m_soundsQueue[0];
                    nullable = null;
                    this.PlaySoundInternal(true, false, false, false, nullable);
                    this.m_soundsQueue.RemoveAt(0);
                }
            }
            if (this.m_secondarySound != null)
            {
                this.m_secondarySound.Stop(true);
            }
        }

        public void Update()
        {
            this.UpdateEmitterMethods();
            bool flag = (this.Sound != null) && this.Sound.IsPlaying;
            if (!this.CanHearSound())
            {
                if (flag)
                {
                    this.StopSound(true, false);
                    this.Sound = null;
                }
            }
            else
            {
                bool? nullable;
                if (!flag && this.Loop)
                {
                    nullable = null;
                    this.PlaySound(this.m_closeSoundSoundPair, true, true, false, false, false, nullable);
                }
                else if (((flag && this.Loop) && (this.m_playing2D != this.ShouldPlay2D())) && ((this.Force2D && !this.m_playing2D) || (this.Force3D && this.m_playing2D)))
                {
                    this.StopSound(true, false);
                    nullable = null;
                    this.PlaySound(this.m_closeSoundSoundPair, true, true, false, false, false, nullable);
                }
                else if ((flag && (this.Loop && !this.m_playing2D)) && this.m_usesDistanceSounds)
                {
                    MyCueId id = this.m_secondaryEnabled ? this.m_secondaryCueEnum : this.myEmptyCueId;
                    MyCueId soundId = this.CheckDistanceSounds(this.m_closeSoundCueId);
                    if ((soundId != this.m_cueEnum) || (id != this.m_secondaryCueEnum))
                    {
                        nullable = null;
                        this.PlaySoundWithDistance(soundId, true, true, false, false, false, false, nullable);
                    }
                    else if (this.m_secondaryEnabled)
                    {
                        if (this.Sound != null)
                        {
                            this.Sound.SetVolume((this.RealisticVolumeChange * this.m_baseVolume) * (1f - this.m_secondaryVolumeRatio));
                        }
                        if (this.m_secondarySound != null)
                        {
                            this.m_secondarySound.SetVolume((this.RealisticVolumeChange * this.m_secondaryBaseVolume) * this.m_secondaryVolumeRatio);
                        }
                    }
                }
                if (flag && this.Loop)
                {
                    MyCueId soundId = this.SelectCue(this.m_soundPair);
                    if (!soundId.Equals(this.m_cueEnum))
                    {
                        nullable = null;
                        this.PlaySoundWithDistance(soundId, true, true, false, true, false, false, nullable);
                    }
                    MyStringHash hash = this.SelectEffect();
                    if (this.m_activeEffect != hash)
                    {
                        nullable = null;
                        this.PlaySoundWithDistance(soundId, true, true, false, true, false, false, nullable);
                    }
                }
            }
        }

        private void UpdateEmitterMethods()
        {
            using (Dictionary<int, ConcurrentCachingList<Delegate>>.ValueCollection.Enumerator enumerator = this.EmitterMethods.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ApplyChanges();
                }
            }
        }

        public static void UpdateEntityEmitters(bool removeUnused, bool updatePlaying, bool updateNotPlaying)
        {
            int sessionTotalFrames = MyFpsManager.GetSessionTotalFrames();
            if ((sessionTotalFrames != 0) && (Math.Abs((int) (m_lastUpdate - sessionTotalFrames)) >= 5))
            {
                m_lastUpdate = sessionTotalFrames;
                List<MyEntity3DSoundEmitter> entityEmitters = m_entityEmitters;
                lock (entityEmitters)
                {
                    for (int i = 0; i < m_entityEmitters.Count; i++)
                    {
                        if (((m_entityEmitters[i] != null) && (m_entityEmitters[i].Entity != null)) && !m_entityEmitters[i].Entity.Closed)
                        {
                            if ((m_entityEmitters[i].IsPlaying & updatePlaying) || (!m_entityEmitters[i].IsPlaying & updateNotPlaying))
                            {
                                m_entityEmitters[i].Update();
                            }
                        }
                        else if (removeUnused)
                        {
                            m_entityEmitters.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }

        public void UpdateSoundOcclusion()
        {
            if ((MyFakes.ENABLE_SOUND_OCCLUSION && !this.m_playing2D) && (MySector.MainCamera != null))
            {
                Vector3 position = (Vector3) MySector.MainCamera.Position;
                LineD ed = new LineD(this.m_entity.PositionComp.WorldAABB.Center, position);
                if (MyPhysics.CastRay(ed.From, ed.To, 30) == null)
                {
                    this.VolumeMultiplier = 1f;
                }
                else if (this.VolumeMultiplier > 0.2f)
                {
                    this.VolumeMultiplier = 0.2f;
                }
            }
        }

        bool IMy3DSoundEmitter.Realistic =>
            this.m_realistic;

        public bool Loop { get; private set; }

        public bool IsPlaying =>
            ((this.Sound != null) && this.Sound.IsPlaying);

        public MyCueId SoundId
        {
            get => 
                this.m_cueEnum;
            set
            {
                if (this.m_cueEnum != value)
                {
                    this.m_cueEnum = value;
                    if (this.m_cueEnum.Hash == MyStringHash.GetOrCompute("None"))
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        public MySoundData LastSoundData =>
            this.m_lastSoundData;

        private float RealisticVolumeChange
        {
            get
            {
                if (!this.m_realistic || (this.m_lastSoundData == null))
                {
                    return 1f;
                }
                return this.m_lastSoundData.RealisticVolumeChange;
            }
        }

        public float VolumeMultiplier
        {
            get => 
                this.m_volumeMultiplier;
            set
            {
                this.m_volumeMultiplier = value;
                if (this.Sound != null)
                {
                    this.Sound.VolumeMultiplier = this.m_volumeMultiplier;
                }
            }
        }

        public MySoundPair SoundPair =>
            this.m_closeSoundSoundPair;

        public IMySourceVoice Sound
        {
            get => 
                this.m_sound;
            set => 
                (this.m_sound = value);
        }

        public Vector3 SourcePosition
        {
            get
            {
                if (this.m_position != null)
                {
                    return this.m_position.Value;
                }
                if ((this.m_entity == null) || (MySector.MainCamera == null))
                {
                    return Vector3.Zero;
                }
                return (Vector3) (this.m_entity.WorldMatrix.Translation - MySector.MainCamera.Position);
            }
        }

        public Vector3 Velocity
        {
            get
            {
                if (this.m_velocity != null)
                {
                    return this.m_velocity.Value;
                }
                if (this.m_entity != null)
                {
                    if (this.m_entity.Physics != null)
                    {
                        return this.m_entity.Physics.LinearVelocity;
                    }
                    if ((this.m_entity.Parent != null) && (this.m_entity.Parent.Physics != null))
                    {
                        return this.m_entity.Parent.Physics.LinearVelocity;
                    }
                }
                return Vector3.Zero;
            }
        }

        public VRage.Game.Entity.MyEntity Entity
        {
            get => 
                this.m_entity;
            set => 
                (this.m_entity = value);
        }

        public float? CustomMaxDistance { get; set; }

        public float? CustomVolume
        {
            get => 
                this.m_customVolume;
            set
            {
                this.m_customVolume = value;
                if ((this.m_customVolume != null) && (this.Sound != null))
                {
                    this.Sound.SetVolume(this.RealisticVolumeChange * this.m_customVolume.Value);
                }
            }
        }

        public bool Force3D { get; set; }

        public bool Force2D { get; set; }

        public bool Plays2D =>
            this.m_playing2D;

        public int SourceChannels { get; set; }

        int IMy3DSoundEmitter.LastPlayedWaveNumber
        {
            get => 
                this.m_lastPlayedWaveNumber;
            set => 
                (this.m_lastPlayedWaveNumber = value);
        }

        public float DopplerScaler { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LastTimePlayingData
        {
            public int LastTime;
            public MyEntity3DSoundEmitter Emitter;
        }

        public enum MethodsEnum
        {
            CanHear,
            ShouldPlay2D,
            CueType,
            ImplicitEffect
        }
    }
}

