namespace Sandbox.Game.EntityComponents
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_ShipSoundComponent), true)]
    public class MyShipSoundComponent : MyEntityComponentBase
    {
        private static Dictionary<MyDefinitionId, MyShipSoundsDefinition> m_categories = new Dictionary<MyDefinitionId, MyShipSoundsDefinition>();
        private static MyShipSoundSystemDefinition m_definition = new MyShipSoundSystemDefinition();
        private bool m_initialized;
        private bool m_shouldPlay2D;
        private bool m_shouldPlay2DChanged;
        private bool m_insideShip;
        private float m_distanceToShip = float.MaxValue;
        public bool ShipHasChanged = true;
        private VRage.Game.Entity.MyEntity m_shipSoundSource;
        private MyCubeGrid m_shipGrid;
        private MyEntityThrustComponent m_shipThrusters;
        private MyGridWheelSystem m_shipWheels;
        private bool m_isDebris = true;
        private MyDefinitionId m_shipCategory;
        private MyShipSoundsDefinition m_groupData;
        private bool m_categoryChange;
        private bool m_forceSoundCheck;
        private float m_wheelVolumeModifierEngine;
        private float m_wheelVolumeModifierWheels;
        private HashSet<MySlimBlock> m_detectedBlocks = new HashSet<MySlimBlock>();
        private ShipStateEnum m_shipState;
        private float m_shipEngineModifier;
        private float m_singleSoundsModifier = 1f;
        private bool m_playingSpeedUpOrDown;
        private MyEntity3DSoundEmitter[] m_emitters = new MyEntity3DSoundEmitter[Enum.GetNames(typeof(ShipEmitters)).Length];
        private float[] m_thrusterVolumes;
        private float[] m_thrusterVolumeTargets;
        private bool m_singleThrusterTypeShip;
        private static MyStringHash m_thrusterIon = MyStringHash.GetOrCompute("Ion");
        private static MyStringHash m_thrusterHydrogen = MyStringHash.GetOrCompute("Hydrogen");
        private static MyStringHash m_thrusterAtmospheric = MyStringHash.GetOrCompute("Atmospheric");
        private static MyStringHash m_crossfade = MyStringHash.GetOrCompute("CrossFade");
        private static MyStringHash m_fadeOut = MyStringHash.GetOrCompute("FadeOut");
        private float[] m_timers = new float[Enum.GetNames(typeof(ShipTimers)).Length];
        private float m_lastFrameShipSpeed;
        private int m_speedChange = 15;
        private float m_shipCurrentPower;
        private float m_shipCurrentPowerTarget;
        private const float POWER_CHANGE_SPEED_UP = 0.006666667f;
        private const float POWER_CHANGE_SPEED_DOWN = 0.01f;
        private bool m_lastWheelUpdateStart;
        private bool m_lastWheelUpdateStop;
        private DateTime m_lastContactWithGround = DateTime.UtcNow;
        private bool m_shipWheelsAction;

        public MyShipSoundComponent()
        {
            for (int i = 0; i < this.m_emitters.Length; i++)
            {
                this.m_emitters[i] = null;
            }
            for (int j = 0; j < this.m_timers.Length; j++)
            {
                this.m_timers[j] = 0f;
            }
        }

        public static void ActualizeGroups()
        {
            foreach (MyShipSoundsDefinition definition in m_categories.Values)
            {
                definition.WheelsSpeedCompensation = m_definition.FullSpeed / definition.WheelsFullSpeed;
            }
        }

        public static void AddShipSounds(MyShipSoundsDefinition shipSoundGroup)
        {
            if (m_categories.ContainsKey(shipSoundGroup.Id))
            {
                m_categories.Remove(shipSoundGroup.Id);
            }
            m_categories.Add(shipSoundGroup.Id, shipSoundGroup);
        }

        private void CalculateShipCategory()
        {
            bool isDebris = this.m_isDebris;
            MyDefinitionId shipCategory = this.m_shipCategory;
            if ((this.m_shipThrusters == null) && ((this.m_shipWheels == null) || (this.m_shipWheels.WheelCount <= 0)))
            {
                this.m_isDebris = true;
            }
            else
            {
                bool flag2 = false;
                foreach (MyCubeBlock block in this.m_shipGrid.GetFatBlocks())
                {
                    if (block is MyShipController)
                    {
                        if ((this.m_shipGrid.MainCockpit == null) && (this.m_shipGrid.GridSizeEnum == MyCubeSize.Small))
                        {
                            this.m_shipSoundSource = block;
                        }
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                {
                    this.m_isDebris = true;
                }
                else
                {
                    int currentMass = this.m_shipGrid.GetCurrentMass();
                    float minValue = float.MinValue;
                    MyDefinitionId? nullable = null;
                    foreach (MyMotorSuspension suspension in this.m_shipWheels.Wheels)
                    {
                        if (suspension.BlockDefinition.SoundDefinitionId != null)
                        {
                            nullable = new MyDefinitionId?(suspension.BlockDefinition.SoundDefinitionId.Value);
                        }
                    }
                    foreach (MyShipSoundsDefinition definition in m_categories.Values)
                    {
                        if (((definition.MinWeight < currentMass) && ((definition.AllowSmallGrid && (this.m_shipGrid.GridSizeEnum == MyCubeSize.Small)) || (definition.AllowLargeGrid && (this.m_shipGrid.GridSizeEnum == MyCubeSize.Large)))) && ((minValue == float.MinValue) || (definition.MinWeight > minValue)))
                        {
                            minValue = definition.MinWeight;
                            this.m_shipCategory = definition.Id;
                            this.m_groupData = definition;
                        }
                        if ((nullable != null) && definition.Id.Equals(nullable.Value))
                        {
                            minValue = definition.MinWeight;
                            this.m_shipCategory = definition.Id;
                            this.m_groupData = definition;
                            break;
                        }
                    }
                    this.m_isDebris = minValue == float.MinValue;
                }
            }
            if (this.m_groupData == null)
            {
                this.m_isDebris = true;
            }
            if ((shipCategory != this.m_shipCategory) || (this.m_isDebris != isDebris))
            {
                this.m_categoryChange = true;
                if (!this.m_isDebris)
                {
                    for (int i = 0; i < this.m_emitters.Length; i++)
                    {
                        if (this.m_emitters[i].IsPlaying && this.m_emitters[i].Loop)
                        {
                            this.m_emitters[i].StopSound(true, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < this.m_emitters.Length; i++)
                    {
                        if (this.m_emitters[i].IsPlaying && this.m_emitters[i].Loop)
                        {
                            if ((i == 8) || (i == 9))
                            {
                                this.m_emitters[i].StopSound(ReferenceEquals(this.m_shipWheels, null), true);
                            }
                            else
                            {
                                this.m_emitters[i].StopSound(ReferenceEquals(this.m_shipThrusters, null), true);
                            }
                        }
                    }
                }
            }
            if (this.m_isDebris)
            {
                this.SetGridSounds(false);
            }
            else
            {
                this.SetGridSounds(true);
            }
        }

        private void CalculateThrusterComposition()
        {
            if (this.m_shipThrusters == null)
            {
                this.m_thrusterVolumeTargets[0] = 0f;
                this.m_thrusterVolumeTargets[1] = 0f;
                this.m_thrusterVolumeTargets[2] = 0f;
            }
            else
            {
                int num1;
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                foreach (MyThrust thrust in this.m_shipGrid.GetFatBlocks<MyThrust>())
                {
                    if (thrust != null)
                    {
                        if (thrust.BlockDefinition.ThrusterType == m_thrusterHydrogen)
                        {
                            num2 += thrust.CurrentStrength * ((Math.Abs(thrust.ThrustForce.X) + Math.Abs(thrust.ThrustForce.Y)) + Math.Abs(thrust.ThrustForce.Z));
                            flag3 = flag3 || (thrust.IsFunctional && thrust.Enabled);
                            continue;
                        }
                        if (thrust.BlockDefinition.ThrusterType == m_thrusterAtmospheric)
                        {
                            num3 += thrust.CurrentStrength * ((Math.Abs(thrust.ThrustForce.X) + Math.Abs(thrust.ThrustForce.Y)) + Math.Abs(thrust.ThrustForce.Z));
                            flag2 = flag2 || (thrust.IsFunctional && thrust.Enabled);
                            continue;
                        }
                        num += thrust.CurrentStrength * ((Math.Abs(thrust.ThrustForce.X) + Math.Abs(thrust.ThrustForce.Y)) + Math.Abs(thrust.ThrustForce.Z));
                        flag = flag || (thrust.IsFunctional && thrust.Enabled);
                    }
                }
                this.ShipHasChanged = false;
                if ((flag & flag2) || (flag & flag3))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !(flag3 & flag2);
                }
                this.m_singleThrusterTypeShip = (bool) num1;
                if (this.m_singleThrusterTypeShip)
                {
                    this.m_thrusterVolumeTargets[0] = flag ? 1f : 0f;
                    this.m_thrusterVolumeTargets[1] = flag3 ? 1f : 0f;
                    this.m_thrusterVolumeTargets[2] = flag2 ? 1f : 0f;
                    if ((!flag && !flag3) && !flag2)
                    {
                        this.ShipHasChanged = true;
                    }
                }
                else if (((num + num2) + num3) > 0f)
                {
                    float num4 = (num2 + num) + num3;
                    num = (num > 0f) ? ((this.m_groupData.ThrusterCompositionMinVolume_c + (num / num4)) / (1f + this.m_groupData.ThrusterCompositionMinVolume_c)) : 0f;
                    num2 = (num2 > 0f) ? ((this.m_groupData.ThrusterCompositionMinVolume_c + (num2 / num4)) / (1f + this.m_groupData.ThrusterCompositionMinVolume_c)) : 0f;
                    num3 = (num3 > 0f) ? ((this.m_groupData.ThrusterCompositionMinVolume_c + (num3 / num4)) / (1f + this.m_groupData.ThrusterCompositionMinVolume_c)) : 0f;
                    this.m_thrusterVolumeTargets[0] = num;
                    this.m_thrusterVolumeTargets[1] = num2;
                    this.m_thrusterVolumeTargets[2] = num3;
                }
                if ((this.m_thrusterVolumes[0] <= 0f) && this.m_emitters[2].IsPlaying)
                {
                    this.m_emitters[2].StopSound(false, true);
                    this.m_emitters[5].StopSound(false, true);
                }
                if ((this.m_thrusterVolumes[1] <= 0f) && this.m_emitters[3].IsPlaying)
                {
                    this.m_emitters[3].StopSound(false, true);
                    this.m_emitters[6].StopSound(false, true);
                }
                if ((this.m_thrusterVolumes[2] <= 0f) && this.m_emitters[4].IsPlaying)
                {
                    this.m_emitters[4].StopSound(false, true);
                    this.m_emitters[7].StopSound(false, true);
                }
                if ((((this.m_thrusterVolumeTargets[0] > 0f) && !this.m_emitters[2].IsPlaying) || ((this.m_thrusterVolumeTargets[1] > 0f) && !this.m_emitters[3].IsPlaying)) || ((this.m_thrusterVolumeTargets[2] > 0f) && !this.m_emitters[4].IsPlaying))
                {
                    this.m_forceSoundCheck = true;
                }
            }
        }

        private float CalculateVolumeFromSpeed(float speedRatio, ref List<MyTuple<float, float>> pairs)
        {
            float num = 1f;
            if (pairs.Count > 0)
            {
                num = pairs[pairs.Count - 1].Item2;
            }
            int num2 = 1;
            while (true)
            {
                if (num2 < pairs.Count)
                {
                    if (speedRatio >= pairs[num2].Item1)
                    {
                        num2++;
                        continue;
                    }
                    num = pairs[num2 - 1].Item2 + ((pairs[num2].Item2 - pairs[num2 - 1].Item2) * ((speedRatio - pairs[num2 - 1].Item1) / (pairs[num2].Item1 - pairs[num2 - 1].Item1)));
                }
                return num;
            }
        }

        public static void ClearShipSounds()
        {
            m_categories.Clear();
        }

        public void DestroyComponent()
        {
            for (int i = 0; i < this.m_emitters.Length; i++)
            {
                if (this.m_emitters[i] != null)
                {
                    this.m_emitters[i].StopSound(true, true);
                    this.m_emitters[i] = null;
                }
            }
            this.m_shipGrid = null;
            this.m_shipThrusters = null;
            this.m_shipWheels = null;
        }

        private void FadeOutSound(ShipEmitters emitter = 1, int duration = 0x7d0)
        {
            if (this.m_emitters[(int) emitter].IsPlaying)
            {
                IMyAudioEffect effect = MyAudio.Static.ApplyEffect(this.m_emitters[(int) emitter].Sound, m_fadeOut, new MyCueId[0], new float?((float) duration), false);
                this.m_emitters[(int) emitter].Sound = effect.OutputSound;
            }
            if (emitter == ShipEmitters.SingleSounds)
            {
                this.m_playingSpeedUpOrDown = false;
            }
        }

        private MySoundPair GetShipSound(ShipSystemSoundsEnum sound)
        {
            MyShipSoundsDefinition definition;
            MySoundPair pair;
            return (!this.m_isDebris ? (!m_categories.TryGetValue(this.m_shipCategory, out definition) ? MySoundPair.Empty : (!definition.Sounds.TryGetValue(sound, out pair) ? MySoundPair.Empty : pair)) : MySoundPair.Empty);
        }

        public bool InitComponent(MyCubeGrid shipGrid)
        {
            if ((shipGrid.GridSizeEnum == MyCubeSize.Small) && !MyFakes.ENABLE_NEW_SMALL_SHIP_SOUNDS)
            {
                return false;
            }
            if ((shipGrid.GridSizeEnum == MyCubeSize.Large) && !MyFakes.ENABLE_NEW_LARGE_SHIP_SOUNDS)
            {
                return false;
            }
            this.m_shipGrid = shipGrid;
            this.m_shipThrusters = this.m_shipGrid.Components.Get<MyEntityThrustComponent>();
            this.m_shipWheels = this.m_shipGrid.GridSystems.WheelSystem;
            this.m_thrusterVolumes = new float[Enum.GetNames(typeof(ShipThrusters)).Length];
            this.m_thrusterVolumeTargets = new float[Enum.GetNames(typeof(ShipThrusters)).Length];
            for (int i = 1; i < this.m_thrusterVolumes.Length; i++)
            {
                this.m_thrusterVolumes[i] = 0f;
                this.m_thrusterVolumeTargets[i] = 0f;
            }
            this.m_thrusterVolumes[0] = 1f;
            this.m_thrusterVolumeTargets[0] = 1f;
            for (int j = 0; j < this.m_emitters.Length; j++)
            {
                this.m_emitters[j] = new MyEntity3DSoundEmitter(this.m_shipGrid, true, 1f);
                this.m_emitters[j].Force2D = this.m_shouldPlay2D;
                this.m_emitters[j].Force3D = !this.m_shouldPlay2D;
            }
            this.m_initialized = true;
            return true;
        }

        private void m_shipWheels_OnMotorUnregister(MyCubeGrid obj)
        {
            if (obj.HasShipSoundEvents)
            {
                obj.HasShipSoundEvents = false;
                this.RotorGrid_OnClosing(obj);
            }
        }

        private void PlayShipSound(ShipEmitters emitter, ShipSystemSoundsEnum sound, bool checkIfAlreadyPlaying = true, bool stopPrevious = true, bool useForce2D = true, bool useFadeOut = false)
        {
            MySoundPair shipSound = this.GetShipSound(sound);
            if ((!ReferenceEquals(shipSound, MySoundPair.Empty) && (this.m_emitters[(int) emitter] != null)) && ((!checkIfAlreadyPlaying || !this.m_emitters[(int) emitter].IsPlaying) || !ReferenceEquals(this.m_emitters[(int) emitter].SoundPair, shipSound)))
            {
                if (this.m_emitters[(int) emitter].IsPlaying & useFadeOut)
                {
                    MyCueId[] cueIds = new MyCueId[] { shipSound.SoundId };
                    IMyAudioEffect effect = MyAudio.Static.ApplyEffect(this.m_emitters[(int) emitter].Sound, MyStringHash.GetOrCompute("CrossFade"), cueIds, new float?((float) 0x5dc), false);
                    this.m_emitters[(int) emitter].Sound = effect.OutputSound;
                }
                else
                {
                    bool? nullable = null;
                    this.m_emitters[(int) emitter].PlaySound(shipSound, stopPrevious, false, useForce2D && this.m_shouldPlay2D, false, false, nullable);
                }
            }
        }

        private void PlayThrusterPushSound(float shipSpeed, float volume, MySoundPair soundPair, MyEntity3DSoundEmitter emiter)
        {
            if ((this.m_shipThrusters == null) || (this.m_shipThrusters.ControlThrust.LengthSquared() < 1f))
            {
                if (emiter.IsPlaying)
                {
                    emiter.VolumeMultiplier *= 0.5f;
                    if (emiter.VolumeMultiplier < 0.1f)
                    {
                        emiter.StopSound(true, true);
                    }
                }
            }
            else if (emiter.IsPlaying)
            {
                emiter.VolumeMultiplier *= 0.995f;
            }
            else
            {
                emiter.VolumeMultiplier = volume;
                bool? nullable = null;
                emiter.PlaySound(soundPair, true, true, this.m_shouldPlay2D, false, false, nullable);
            }
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent A_0)
        {
            this.m_lastContactWithGround = DateTime.UtcNow;
        }

        private void RotorGrid_OnClosing(VRage.Game.Entity.MyEntity obj)
        {
            if (obj.Physics != null)
            {
                obj.Physics.RigidBody.ContactPointCallback -= new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                obj.OnClose -= new Action<VRage.Game.Entity.MyEntity>(this.RotorGrid_OnClosing);
            }
        }

        public static void SetDefinition(MyShipSoundSystemDefinition def)
        {
            m_definition = def;
        }

        private void SetGridSounds(bool silent)
        {
            foreach (MyCubeBlock block in this.m_shipGrid.GetFatBlocks())
            {
                if (!block.BlockDefinition.SilenceableByShipSoundSystem)
                {
                    continue;
                }
                if (block.IsSilenced != silent)
                {
                    block.SilenceInChange = true;
                    block.IsSilenced = silent;
                    if (!block.SilenceInChange)
                    {
                        block.UsedUpdateEveryFrame = (block.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE;
                        block.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    }
                }
            }
        }

        public unsafe void Update()
        {
            bool flag;
            float num;
            float num2;
            ShipStateEnum shipState;
            if (!this.m_initialized)
            {
                return;
            }
            else if (((this.m_shipGrid.Physics != null) && (!this.m_shipGrid.IsStatic && (((this.m_shipThrusters != null) || (this.m_shipWheels != null)) && (this.m_distanceToShip < m_definition.MaxUpdateRange_sq)))) && (this.m_groupData != null))
            {
                float single1;
                if (this.m_shipWheels != null)
                {
                    using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.m_shipWheels.Wheels.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyWheel topBlock = enumerator.Current.TopBlock as MyWheel;
                            if ((topBlock != null) && (topBlock.LastContactTime > this.m_lastContactWithGround))
                            {
                                this.m_lastContactWithGround = topBlock.LastContactTime;
                            }
                        }
                    }
                }
                flag = (DateTime.UtcNow - this.m_lastContactWithGround).TotalSeconds <= 0.20000000298023224;
                if (flag)
                {
                    single1 = (this.m_shipGrid.Physics.LinearVelocity * this.m_groupData.WheelsSpeedCompensation).Length();
                }
                else
                {
                    single1 = this.m_shipGrid.Physics.LinearVelocity.Length();
                }
                num2 = Math.Min((float) (single1 / m_definition.FullSpeed), (float) 1f);
                if (!MySandboxGame.Config.ShipSoundsAreBasedOnSpeed)
                {
                    num = this.m_shipCurrentPower * m_definition.FullSpeed;
                }
                shipState = this.m_shipState;
                if (this.m_shipGrid.GridSystems.ResourceDistributor.ResourceState == MyResourceStateEnum.NoPower)
                {
                    goto TR_00BB;
                }
                else if (this.m_isDebris || (((this.m_shipThrusters == null) || (this.m_shipThrusters.ThrustCount <= 0)) && ((this.m_shipWheels == null) || (this.m_shipWheels.WheelCount <= 0))))
                {
                    goto TR_00BB;
                }
            }
            else
            {
                return;
            }
            this.m_shipState = (num >= m_definition.SpeedThreshold1) ? ((num >= m_definition.SpeedThreshold2) ? ShipStateEnum.Fast : ShipStateEnum.Medium) : ShipStateEnum.Slow;
        TR_00BA:
            if (!MySandboxGame.Config.ShipSoundsAreBasedOnSpeed)
            {
                this.m_shipCurrentPowerTarget = 0f;
                if (flag)
                {
                    if ((this.m_shipWheels != null) && (this.m_shipWheels.WheelCount > 0))
                    {
                        if (Math.Abs(this.m_shipWheels.AngularVelocity.Z) >= 0.9f)
                        {
                            this.m_shipCurrentPowerTarget = 1f;
                        }
                        else if (this.m_shipGrid.Physics.LinearVelocity.LengthSquared() > 5f)
                        {
                            this.m_shipCurrentPowerTarget = 0.33f;
                        }
                    }
                }
                else if (this.m_shipThrusters != null)
                {
                    if (this.m_shipThrusters.FinalThrust.LengthSquared() >= 100f)
                    {
                        this.m_shipCurrentPowerTarget = 1f;
                    }
                    else if (((this.m_shipGrid.Physics.Gravity == Vector3.Zero) || !this.m_shipThrusters.DampenersEnabled) || (this.m_shipGrid.Physics.LinearVelocity.LengthSquared() >= 4f))
                    {
                        this.m_shipCurrentPowerTarget = 0f;
                    }
                    else
                    {
                        this.m_shipCurrentPowerTarget = 0.33f;
                    }
                }
                if (this.m_shipCurrentPower < this.m_shipCurrentPowerTarget)
                {
                    this.m_shipCurrentPower = Math.Min(this.m_shipCurrentPower + 0.006666667f, this.m_shipCurrentPowerTarget);
                }
                else if (this.m_shipCurrentPower > this.m_shipCurrentPowerTarget)
                {
                    this.m_shipCurrentPower = Math.Max(this.m_shipCurrentPower - 0.01f, this.m_shipCurrentPowerTarget);
                }
            }
            bool flag2 = this.m_shouldPlay2D;
            if (this.m_shipGrid.GridSizeEnum != MyCubeSize.Large)
            {
                if (((MySession.Static.ControlledEntity != null) && (!MySession.Static.IsCameraUserControlledSpectator() && (MySession.Static.ControlledEntity.Entity != null))) && ReferenceEquals(MySession.Static.ControlledEntity.Entity.Parent, this.m_shipGrid))
                {
                    int num18;
                    if ((!(MySession.Static.ControlledEntity.Entity is MyCockpit) || !(MySession.Static.ControlledEntity.Entity as MyCockpit).IsInFirstPersonView) && ((!(MySession.Static.ControlledEntity.Entity is MyRemoteControl) || ((MySession.Static.LocalCharacter == null) || !(MySession.Static.LocalCharacter.IsUsing is MyCockpit))) || !ReferenceEquals((MySession.Static.LocalCharacter.IsUsing as MyCockpit).Parent, this.m_shipGrid)))
                    {
                        num18 = !(MySession.Static.CameraController is MyCameraBlock) ? 0 : ((int) ReferenceEquals((MySession.Static.CameraController as MyCameraBlock).Parent, this.m_shipGrid));
                    }
                    else
                    {
                        num18 = 1;
                    }
                    this.m_shouldPlay2D = (bool) num18;
                }
                else
                {
                    this.m_shouldPlay2D = false;
                }
            }
            else
            {
                int num1;
                int num16;
                if (!this.m_insideShip || (MySession.Static.ControlledEntity == null))
                {
                    num1 = 0;
                }
                else if ((MySession.Static.ControlledEntity.Entity is MyCockpit) || (MySession.Static.ControlledEntity.Entity is MyRemoteControl))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) (MySession.Static.ControlledEntity.Entity is MyShipController);
                }
                this.m_shouldPlay2D = (bool) num16;
                if (this.m_shouldPlay2D)
                {
                    int num17;
                    MyCubeBlock entity = MySession.Static.ControlledEntity.Entity as MyCubeBlock;
                    if ((entity == null) || (entity.CubeGrid == null))
                    {
                        num17 = 0;
                    }
                    else
                    {
                        num17 = (int) (entity.CubeGrid.GridSizeEnum == MyCubeSize.Large);
                    }
                    this.m_shouldPlay2D &= num17;
                }
            }
            this.m_shouldPlay2DChanged = flag2 != this.m_shouldPlay2D;
            int index = 0;
            while (true)
            {
                if (index >= this.m_thrusterVolumes.Length)
                {
                    if (flag)
                    {
                        this.m_wheelVolumeModifierEngine = Math.Min((float) (this.m_wheelVolumeModifierEngine + 0.01f), (float) 1f);
                        this.m_wheelVolumeModifierWheels = Math.Min((float) (this.m_wheelVolumeModifierWheels + 0.03f), (float) 1f);
                    }
                    else
                    {
                        this.m_wheelVolumeModifierEngine = Math.Max((float) (this.m_wheelVolumeModifierEngine - 0.005f), (float) 0f);
                        this.m_wheelVolumeModifierWheels = Math.Max((float) (this.m_wheelVolumeModifierWheels - 0.03f), (float) 0f);
                    }
                    if (((this.m_shipState != shipState) || this.m_categoryChange) || this.m_forceSoundCheck)
                    {
                        if (this.m_shipState == ShipStateEnum.NoPower)
                        {
                            if (this.m_shipState != shipState)
                            {
                                int num4 = 0;
                                while (true)
                                {
                                    if (num4 >= this.m_emitters.Length)
                                    {
                                        this.m_emitters[1].VolumeMultiplier = 1f;
                                        this.PlayShipSound(ShipEmitters.SingleSounds, ShipSystemSoundsEnum.EnginesEnd, true, true, true, false);
                                        break;
                                    }
                                    this.m_emitters[num4].StopSound(false, true);
                                    num4++;
                                }
                            }
                        }
                        else
                        {
                            if (this.m_shipState == ShipStateEnum.Slow)
                            {
                                this.PlayShipSound(ShipEmitters.MainSound, ShipSystemSoundsEnum.MainLoopSlow, true, true, true, false);
                            }
                            else if (this.m_shipState == ShipStateEnum.Medium)
                            {
                                this.PlayShipSound(ShipEmitters.MainSound, ShipSystemSoundsEnum.MainLoopMedium, true, true, true, false);
                            }
                            else if (this.m_shipState == ShipStateEnum.Fast)
                            {
                                this.PlayShipSound(ShipEmitters.MainSound, ShipSystemSoundsEnum.MainLoopFast, true, true, true, false);
                            }
                            this.PlayShipSound(ShipEmitters.ShipEngine, ShipSystemSoundsEnum.ShipEngine, true, true, true, false);
                            this.PlayShipSound(ShipEmitters.ShipIdle, ShipSystemSoundsEnum.ShipIdle, true, true, true, false);
                            if (this.m_thrusterVolumes[0] > 0f)
                            {
                                this.PlayShipSound(ShipEmitters.IonThrusters, ShipSystemSoundsEnum.IonThrusters, true, true, true, false);
                                this.PlayShipSound(ShipEmitters.IonThrustersIdle, ShipSystemSoundsEnum.IonThrustersIdle, true, true, true, false);
                            }
                            if (this.m_thrusterVolumes[1] > 0f)
                            {
                                this.PlayShipSound(ShipEmitters.HydrogenThrusters, ShipSystemSoundsEnum.HydrogenThrusters, true, true, true, false);
                                this.PlayShipSound(ShipEmitters.HydrogenThrustersIdle, ShipSystemSoundsEnum.HydrogenThrustersIdle, true, true, true, false);
                            }
                            if (this.m_thrusterVolumes[2] > 0f)
                            {
                                if (this.m_shipState == ShipStateEnum.Slow)
                                {
                                    this.PlayShipSound(ShipEmitters.AtmosphericThrusters, ShipSystemSoundsEnum.AtmoThrustersSlow, true, true, true, false);
                                }
                                else if (this.m_shipState == ShipStateEnum.Medium)
                                {
                                    this.PlayShipSound(ShipEmitters.AtmosphericThrusters, ShipSystemSoundsEnum.AtmoThrustersMedium, true, true, true, false);
                                }
                                else if (this.m_shipState == ShipStateEnum.Fast)
                                {
                                    this.PlayShipSound(ShipEmitters.AtmosphericThrusters, ShipSystemSoundsEnum.AtmoThrustersFast, true, true, true, false);
                                }
                                this.PlayShipSound(ShipEmitters.AtmosphericThrustersIdle, ShipSystemSoundsEnum.AtmoThrustersIdle, true, true, true, false);
                            }
                            if (this.m_shipWheels.WheelCount > 0)
                            {
                                this.PlayShipSound(ShipEmitters.WheelsMain, ShipSystemSoundsEnum.WheelsEngineRun, true, true, true, false);
                                this.PlayShipSound(ShipEmitters.WheelsSecondary, ShipSystemSoundsEnum.WheelsSecondary, true, true, true, false);
                            }
                            if (shipState == ShipStateEnum.NoPower)
                            {
                                this.m_emitters[1].VolumeMultiplier = 1f;
                                this.PlayShipSound(ShipEmitters.SingleSounds, ShipSystemSoundsEnum.EnginesStart, true, true, true, false);
                            }
                        }
                        this.m_categoryChange = false;
                        this.m_forceSoundCheck = false;
                    }
                    if (this.m_shouldPlay2DChanged)
                    {
                        int num5 = 0;
                        while (true)
                        {
                            if (num5 >= this.m_emitters.Length)
                            {
                                this.m_shouldPlay2DChanged = false;
                                break;
                            }
                            this.m_emitters[num5].Force2D = this.m_shouldPlay2D;
                            this.m_emitters[num5].Force3D = !this.m_shouldPlay2D;
                            if ((this.m_emitters[num5].IsPlaying && (this.m_emitters[num5].Plays2D != this.m_shouldPlay2D)) && this.m_emitters[num5].Loop)
                            {
                                this.m_emitters[num5].StopSound(true, true);
                                bool? nullable = null;
                                this.m_emitters[num5].PlaySound(this.m_emitters[num5].SoundPair, true, true, this.m_shouldPlay2D, false, false, nullable);
                            }
                            num5++;
                        }
                    }
                    if (this.m_shipState == ShipStateEnum.NoPower)
                    {
                        if (this.m_shipEngineModifier > 0f)
                        {
                            this.m_shipEngineModifier = Math.Max((float) 0f, (float) (this.m_shipEngineModifier - (0.01666667f / this.m_groupData.EngineTimeToTurnOff)));
                        }
                    }
                    else
                    {
                        if (this.m_shipEngineModifier < 1f)
                        {
                            this.m_shipEngineModifier = Math.Min((float) 1f, (float) (this.m_shipEngineModifier + (0.01666667f / this.m_groupData.EngineTimeToTurnOn)));
                        }
                        float speedRatio = Math.Min((float) (num / m_definition.FullSpeed), (float) 1f);
                        float num7 = (this.CalculateVolumeFromSpeed(speedRatio, ref this.m_groupData.EngineVolumes) * this.m_shipEngineModifier) * this.m_singleSoundsModifier;
                        float num9 = 1f;
                        if (this.m_emitters[0].IsPlaying)
                        {
                            this.m_emitters[0].VolumeMultiplier = num7;
                            float semitones = this.m_groupData.EnginePitchRangeInSemitones_h + (this.m_groupData.EnginePitchRangeInSemitones * speedRatio);
                            this.m_emitters[0].Sound.FrequencyRatio = MyAudio.Static.SemitonesToFrequencyRatio(semitones);
                        }
                        float num8 = Math.Max((float) (Math.Min(this.CalculateVolumeFromSpeed(speedRatio, ref this.m_groupData.ThrusterVolumes), 1f) - (this.m_wheelVolumeModifierEngine * this.m_groupData.WheelsLowerThrusterVolumeBy)), (float) 0f);
                        num9 = (MyMath.Clamp(1.2f - (num8 * 3f), 0f, 1f) * this.m_shipEngineModifier) * this.m_singleSoundsModifier;
                        num8 *= this.m_shipEngineModifier * this.m_singleSoundsModifier;
                        this.m_emitters[11].VolumeMultiplier = MySandboxGame.Config.ShipSoundsAreBasedOnSpeed ? Math.Max((float) 0f, (float) (num7 - num9)) : num2;
                        this.m_emitters[10].VolumeMultiplier = ((MySandboxGame.Config.ShipSoundsAreBasedOnSpeed ? num9 : MyMath.Clamp(1.2f - (num2 * 3f), 0f, 1f)) * this.m_shipEngineModifier) * this.m_singleSoundsModifier;
                        float num10 = MyAudio.Static.SemitonesToFrequencyRatio(this.m_groupData.ThrusterPitchRangeInSemitones_h + (this.m_groupData.ThrusterPitchRangeInSemitones * num8));
                        if (this.m_emitters[2].IsPlaying)
                        {
                            float volume = this.m_thrusterVolumes[0];
                            this.m_emitters[2].VolumeMultiplier = num8 * volume;
                            this.m_emitters[2].Sound.FrequencyRatio = num10;
                            this.PlayThrusterPushSound(num, volume, this.GetShipSound(ShipSystemSoundsEnum.IonThrusterPush), this.m_emitters[12]);
                        }
                        if (this.m_emitters[5].IsPlaying)
                        {
                            this.m_emitters[5].VolumeMultiplier = num9 * this.m_thrusterVolumes[0];
                        }
                        if (this.m_emitters[3].IsPlaying)
                        {
                            float volume = this.m_thrusterVolumes[1];
                            this.m_emitters[3].VolumeMultiplier = num8 * volume;
                            this.m_emitters[3].Sound.FrequencyRatio = num10;
                            this.PlayThrusterPushSound(num, volume, this.GetShipSound(ShipSystemSoundsEnum.HydrogenThrusterPush), this.m_emitters[13]);
                        }
                        if (this.m_emitters[6].IsPlaying)
                        {
                            this.m_emitters[6].VolumeMultiplier = num9 * this.m_thrusterVolumes[1];
                        }
                        if (this.m_emitters[4].IsPlaying)
                        {
                            this.m_emitters[4].VolumeMultiplier = num8 * this.m_thrusterVolumes[2];
                            this.m_emitters[4].Sound.FrequencyRatio = num10;
                        }
                        if (this.m_emitters[7].IsPlaying)
                        {
                            this.m_emitters[7].VolumeMultiplier = num9 * this.m_thrusterVolumes[2];
                        }
                        if (this.m_emitters[8].IsPlaying)
                        {
                            this.m_emitters[0].VolumeMultiplier = Math.Max((float) (num7 - (this.m_wheelVolumeModifierEngine * this.m_groupData.WheelsLowerThrusterVolumeBy)), (float) 0f);
                            this.m_emitters[8].VolumeMultiplier = (num8 * this.m_wheelVolumeModifierEngine) * this.m_singleSoundsModifier;
                            this.m_emitters[8].Sound.FrequencyRatio = num10;
                            this.m_emitters[9].VolumeMultiplier = ((this.CalculateVolumeFromSpeed(speedRatio, ref this.m_groupData.WheelsVolumes) * this.m_shipEngineModifier) * this.m_wheelVolumeModifierWheels) * this.m_singleSoundsModifier;
                        }
                        float num11 = 0.5f + (num8 / 2f);
                        this.m_playingSpeedUpOrDown = this.m_playingSpeedUpOrDown && this.m_emitters[1].IsPlaying;
                        if (((this.m_speedChange < 20) || (this.m_timers[0] > 0f)) || (this.m_wheelVolumeModifierEngine > 0f))
                        {
                            if (((this.m_speedChange <= 15) && this.m_emitters[1].IsPlaying) && this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedUp)))
                            {
                                this.FadeOutSound(ShipEmitters.SingleSounds, 0x3e8);
                            }
                        }
                        else
                        {
                            this.m_timers[0] = (this.m_shipGrid.GridSizeEnum == MyCubeSize.Large) ? 8f : 1f;
                            if (this.m_emitters[1].IsPlaying && this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedDown)))
                            {
                                this.FadeOutSound(ShipEmitters.SingleSounds, 0x3e8);
                            }
                            this.m_emitters[1].VolumeMultiplier = num11;
                            this.PlayShipSound(ShipEmitters.SingleSounds, ShipSystemSoundsEnum.EnginesSpeedUp, false, false, true, false);
                            this.m_playingSpeedUpOrDown = true;
                        }
                        if (((this.m_speedChange > 10) || (this.m_timers[1] > 0f)) || (this.m_wheelVolumeModifierEngine > 0f))
                        {
                            if (((this.m_speedChange >= 15) && this.m_emitters[1].IsPlaying) && this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedDown)))
                            {
                                this.FadeOutSound(ShipEmitters.SingleSounds, 0x3e8);
                            }
                        }
                        else
                        {
                            this.m_timers[1] = (this.m_shipGrid.GridSizeEnum == MyCubeSize.Large) ? 8f : 2f;
                            if (this.m_emitters[1].IsPlaying && this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedUp)))
                            {
                                this.FadeOutSound(ShipEmitters.SingleSounds, 0x3e8);
                            }
                            this.m_emitters[1].VolumeMultiplier = num11;
                            this.PlayShipSound(ShipEmitters.SingleSounds, ShipSystemSoundsEnum.EnginesSpeedDown, false, false, true, false);
                            this.m_playingSpeedUpOrDown = true;
                        }
                        float speedDownSoundChangeVolumeTo = 1f;
                        if (this.m_playingSpeedUpOrDown && this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedDown)))
                        {
                            speedDownSoundChangeVolumeTo = this.m_groupData.SpeedDownSoundChangeVolumeTo;
                        }
                        if (this.m_playingSpeedUpOrDown && this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedUp)))
                        {
                            speedDownSoundChangeVolumeTo = this.m_groupData.SpeedUpSoundChangeVolumeTo;
                        }
                        if (this.m_singleSoundsModifier < speedDownSoundChangeVolumeTo)
                        {
                            this.m_singleSoundsModifier = Math.Min(this.m_singleSoundsModifier + this.m_groupData.SpeedUpDownChangeSpeed, speedDownSoundChangeVolumeTo);
                        }
                        else if (this.m_singleSoundsModifier > speedDownSoundChangeVolumeTo)
                        {
                            this.m_singleSoundsModifier = Math.Max(this.m_singleSoundsModifier - this.m_groupData.SpeedUpDownChangeSpeed, speedDownSoundChangeVolumeTo);
                        }
                        if (this.m_emitters[1].IsPlaying && (this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedDown)) || this.m_emitters[1].SoundPair.Equals(this.GetShipSound(ShipSystemSoundsEnum.EnginesSpeedUp))))
                        {
                            this.m_emitters[1].VolumeMultiplier = num11;
                        }
                    }
                    if ((this.m_shipThrusters != null) && (this.m_shipThrusters.ThrustCount <= 0))
                    {
                        this.m_shipThrusters = null;
                    }
                    if ((Math.Abs((float) (num - this.m_lastFrameShipSpeed)) > 0.01f) && (num >= 3f))
                    {
                        this.m_speedChange = (int) MyMath.Clamp((float) (this.m_speedChange + ((num > this.m_lastFrameShipSpeed) ? 1 : -1)), 0f, 30f);
                    }
                    else if (this.m_speedChange != 15)
                    {
                        this.m_speedChange += (this.m_speedChange > 15) ? -1 : 1;
                    }
                    if ((num >= this.m_lastFrameShipSpeed) && (this.m_timers[1] > 0f))
                    {
                        float* singlePtr1 = (float*) ref this.m_timers[1];
                        singlePtr1[0] -= 0.01666667f;
                    }
                    if ((num <= this.m_lastFrameShipSpeed) && (this.m_timers[0] > 0f))
                    {
                        float* singlePtr2 = (float*) ref this.m_timers[0];
                        singlePtr2[0] -= 0.01666667f;
                    }
                    this.m_lastFrameShipSpeed = num;
                    break;
                }
                if (this.m_thrusterVolumes[index] < this.m_thrusterVolumeTargets[index])
                {
                    this.m_thrusterVolumes[index] = Math.Min(this.m_thrusterVolumes[index] + this.m_groupData.ThrusterCompositionChangeSpeed, this.m_thrusterVolumeTargets[index]);
                }
                else if (this.m_thrusterVolumes[index] > this.m_thrusterVolumeTargets[index])
                {
                    this.m_thrusterVolumes[index] = Math.Max(this.m_thrusterVolumes[index] - this.m_groupData.ThrusterCompositionChangeSpeed, this.m_thrusterVolumeTargets[index]);
                }
                index++;
            }
            return;
        TR_00BB:
            this.m_shipState = ShipStateEnum.NoPower;
            goto TR_00BA;
        }

        public void Update100()
        {
            float maxValue;
            float single2;
            if ((!this.m_initialized || ((this.m_shipGrid == null) || (m_definition == null))) || (this.m_shipGrid.Physics == null))
            {
                maxValue = float.MaxValue;
            }
            else if (!this.m_shouldPlay2D)
            {
                maxValue = (float) this.m_shipGrid.PositionComp.WorldAABB.DistanceSquared(MySector.MainCamera.Position);
            }
            else
            {
                maxValue = 0f;
            }
            this.m_distanceToShip = single2;
            this.UpdateCategory();
            this.UpdateSounds();
            this.UpdateWheels();
        }

        private void UpdateCategory()
        {
            if ((this.m_initialized && ((this.m_shipGrid != null) && ((this.m_shipGrid.Physics != null) && (!this.m_shipGrid.IsStatic && (m_definition != null))))) && (this.m_distanceToShip < m_definition.MaxUpdateRange_sq))
            {
                if (this.m_shipThrusters == null)
                {
                    this.m_shipThrusters = this.m_shipGrid.Components.Get<MyEntityThrustComponent>();
                }
                if (this.m_shipWheels == null)
                {
                    this.m_shipWheels = this.m_shipGrid.GridSystems.WheelSystem;
                }
                this.CalculateShipCategory();
                if ((!this.m_isDebris && (this.m_shipState != ShipStateEnum.NoPower)) && ((!this.m_singleThrusterTypeShip || (this.ShipHasChanged || ((this.m_shipThrusters == null) || (this.m_shipThrusters.FinalThrust == Vector3.Zero)))) || ((this.m_shipWheels != null) && this.m_shipWheels.HasWorkingWheels(false))))
                {
                    this.CalculateThrusterComposition();
                }
                if (this.m_shipSoundSource == null)
                {
                    this.m_shipSoundSource = this.m_shipGrid;
                }
                if ((this.m_shipGrid.MainCockpit != null) && (this.m_shipGrid.GridSizeEnum == MyCubeSize.Small))
                {
                    this.m_shipSoundSource = this.m_shipGrid.MainCockpit;
                }
                if (((this.m_shipGrid.GridSizeEnum == MyCubeSize.Large) && (MySession.Static != null)) && (MySession.Static.LocalCharacter != null))
                {
                    if ((MySession.Static.LocalCharacter.ReverbDetectorComp == null) || (MySession.Static.Settings.RealisticSound && ((MySession.Static.LocalCharacter.AtmosphereDetectorComp == null) || (!MySession.Static.LocalCharacter.AtmosphereDetectorComp.InAtmosphere && !MySession.Static.LocalCharacter.AtmosphereDetectorComp.InShipOrStation))))
                    {
                        this.m_insideShip = false;
                    }
                    else
                    {
                        this.m_insideShip = MySession.Static.LocalCharacter.ReverbDetectorComp.Grids > 0;
                    }
                }
                if (this.m_groupData != null)
                {
                    this.m_shipGrid.MarkForUpdate();
                }
            }
        }

        private void UpdateSounds()
        {
            for (int i = 0; i < this.m_emitters.Length; i++)
            {
                if (this.m_emitters[i] != null)
                {
                    this.m_emitters[i].Entity = this.m_shipSoundSource;
                    this.m_emitters[i].Update();
                }
            }
        }

        private void UpdateWheels()
        {
            if (((this.m_shipGrid != null) && ((this.m_shipGrid.Physics != null) && (this.m_shipWheels != null))) && (this.m_shipWheels.WheelCount > 0))
            {
                bool flag = (this.m_distanceToShip < m_definition.WheelsCallbackRangeCreate_sq) && !this.m_isDebris;
                bool flag2 = (this.m_distanceToShip > m_definition.WheelsCallbackRangeRemove_sq) || this.m_isDebris;
                if ((flag | flag2) && ((this.m_lastWheelUpdateStart != flag) || (this.m_lastWheelUpdateStop != flag2)))
                {
                    foreach (MyMotorSuspension suspension in this.m_shipWheels.Wheels)
                    {
                        if (suspension == null)
                        {
                            continue;
                        }
                        if ((suspension.RotorGrid != null) && ((suspension.RotorGrid.Physics != null) && (suspension.RotorGrid.Physics.RigidBody != null)))
                        {
                            if (!suspension.RotorGrid.HasShipSoundEvents & flag)
                            {
                                suspension.RotorGrid.Physics.RigidBody.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                                suspension.RotorGrid.Physics.RigidBody.CallbackLimit = 1;
                                suspension.RotorGrid.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.RotorGrid_OnClosing);
                                suspension.RotorGrid.HasShipSoundEvents = true;
                                continue;
                            }
                            if (suspension.RotorGrid.HasShipSoundEvents & flag2)
                            {
                                suspension.RotorGrid.HasShipSoundEvents = false;
                                suspension.RotorGrid.Physics.RigidBody.ContactPointCallback -= new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                                suspension.RotorGrid.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.RotorGrid_OnClosing);
                            }
                        }
                    }
                    this.m_lastWheelUpdateStart = flag;
                    this.m_lastWheelUpdateStop = flag2;
                    if (flag && !this.m_shipWheelsAction)
                    {
                        this.m_shipWheels.OnMotorUnregister += new Action<MyCubeGrid>(this.m_shipWheels_OnMotorUnregister);
                        this.m_shipWheelsAction = true;
                    }
                    else if (flag2 && this.m_shipWheelsAction)
                    {
                        this.m_shipWheels.OnMotorUnregister -= new Action<MyCubeGrid>(this.m_shipWheels_OnMotorUnregister);
                        this.m_shipWheelsAction = false;
                    }
                }
            }
        }

        public bool NeedsPerFrameUpdate
        {
            get
            {
                if ((!this.m_initialized || ((this.m_shipGrid.Physics == null) || this.m_shipGrid.IsStatic)) || ((this.m_shipThrusters == null) && (this.m_shipWheels == null)))
                {
                    return false;
                }
                return ((this.m_distanceToShip < m_definition.MaxUpdateRange_sq) && (this.m_groupData != null));
            }
        }

        public override string ComponentTypeDebugString =>
            "ShipSoundSystem";

        private enum ShipEmitters
        {
            MainSound,
            SingleSounds,
            IonThrusters,
            HydrogenThrusters,
            AtmosphericThrusters,
            IonThrustersIdle,
            HydrogenThrustersIdle,
            AtmosphericThrustersIdle,
            WheelsMain,
            WheelsSecondary,
            ShipIdle,
            ShipEngine,
            IonThrusterSpeedUp,
            HydrogenThrusterSpeedUp
        }

        private enum ShipStateEnum
        {
            NoPower,
            Slow,
            Medium,
            Fast
        }

        private enum ShipThrusters
        {
            Ion,
            Hydrogen,
            Atmospheric
        }

        private enum ShipTimers
        {
            SpeedUp,
            SpeedDown
        }
    }
}

