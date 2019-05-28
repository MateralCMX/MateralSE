namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    [MyComponentBuilder(typeof(MyObjectBuilder_CharacterSoundComponent), true)]
    public class MyCharacterSoundComponent : MyEntityComponentBase
    {
        private readonly Dictionary<int, MySoundPair> CharacterSounds = new Dictionary<int, MySoundPair>();
        private static readonly MySoundPair EmptySoundPair = new MySoundPair();
        private static MyStringHash LowPressure = MyStringHash.GetOrCompute("LowPressure");
        private List<MyEntity3DSoundEmitter> m_soundEmitters = new List<MyEntity3DSoundEmitter>(Enum.GetNames(typeof(MySoundEmitterEnum)).Length);
        private List<MyPhysics.HitInfo> m_hits = new List<MyPhysics.HitInfo>();
        private int m_lastScreamTime;
        private float m_jetpackSustainTimer;
        private float m_jetpackMinIdleTime;
        private const float JETPACK_TIME_BETWEEN_SOUNDS = 0.25f;
        private bool m_jumpReady;
        private const int SCREAM_DELAY_MS = 800;
        private const float DEFAULT_ANKLE_HEIGHT = 0.2f;
        private int m_lastStepTime;
        private const int m_stepMinimumDelayCrouch = 530;
        private const int m_stepMinimumDelayWalk = 440;
        private const int m_stepMinimumDelayRun = 290;
        private const int m_stepMinimumDelayRunStrafe = 330;
        private const int m_stepMinimumDelayRunSideBack = 400;
        private const int m_stepMinimumDelayRunForward = 330;
        private const int m_stepMinimumDelaySprint = 250;
        private MyCharacterMovementEnum m_lastUpdateMovementState;
        private MyCharacter m_character;
        private MyCubeGrid m_standingOnGrid;
        private int m_lastContactCounter;
        private MyVoxelBase m_standingOnVoxel;
        private MyStringHash m_characterPhysicalMaterial = MyMaterialType.CHARACTER;
        private bool m_isWalking;
        private const float WIND_SPEED_LOW = 40f;
        private const float WIND_SPEED_HIGH = 80f;
        private const float WIND_SPEED_DIFF = 40f;
        private const float WIND_CHANGE_SPEED = 0.008333334f;
        private float m_windVolume;
        private float m_windTargetVolume;
        private bool m_inAtmosphere = true;
        private MyEntity3DSoundEmitter m_windEmitter;
        private bool m_windSystem;
        private MyEntity3DSoundEmitter m_oxygenEmitter;
        private MyEntity3DSoundEmitter m_movementEmitter;
        private MyEntity3DSoundEmitter m_magneticBootsEmitter;
        private MySoundPair m_lastActionSound;
        private MySoundPair m_lastPrimarySound;
        private bool m_isFirstPerson;
        private bool m_isFirstPersonChanged;

        public MyCharacterSoundComponent()
        {
            string[] names = Enum.GetNames(typeof(MySoundEmitterEnum));
            for (int i = 0; i < names.Length; i++)
            {
                string text1 = names[i];
                this.m_soundEmitters.Add(new MyEntity3DSoundEmitter(base.Entity as MyEntity, false, 1f));
            }
            for (int j = 0; j < Enum.GetNames(typeof(CharacterSoundsEnum)).Length; j++)
            {
                this.CharacterSounds.Add(j, EmptySoundPair);
            }
            if ((MySession.Static != null) && (MySession.Static.Settings.EnableOxygen || MySession.Static.CreativeMode))
            {
                this.m_oxygenEmitter = new MyEntity3DSoundEmitter(base.Entity as MyEntity, false, 1f);
            }
        }

        public void CharacterDied()
        {
            if (this.m_windEmitter.IsPlaying)
            {
                this.m_windEmitter.StopSound(true, true);
            }
        }

        public void FindAndPlayStateSound()
        {
            if (this.m_isFirstPerson == ((MySession.Static.LocalCharacter != null) && MySession.Static.LocalCharacter.IsInFirstPersonView))
            {
                this.m_isFirstPersonChanged = false;
            }
            else
            {
                this.m_isFirstPerson = !this.m_isFirstPerson;
                this.m_isFirstPersonChanged = true;
            }
            if (this.m_character.Breath != null)
            {
                this.m_character.Breath.Update(false);
            }
            MySoundPair objA = this.SelectSound();
            this.UpdateBreath();
            if ((this.m_movementEmitter != null) && !this.CharacterSounds[0x10].Equals(MySoundPair.Empty))
            {
                if (this.m_isWalking && !this.m_movementEmitter.IsPlaying)
                {
                    this.m_movementEmitter.PlaySound(this.CharacterSounds[0x10], false, false, MyFakes.FORCE_CHARACTER_2D_SOUND, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                }
                if (!this.m_isWalking && this.m_movementEmitter.IsPlaying)
                {
                    this.m_movementEmitter.StopSound(false, true);
                }
            }
            MyEntity3DSoundEmitter emitter = this.m_soundEmitters[0];
            MyEntity3DSoundEmitter emitter2 = this.m_soundEmitters[4];
            MyEntity3DSoundEmitter walkEmitter = this.m_soundEmitters[2];
            if (this.m_isFirstPersonChanged)
            {
                emitter.StopSound(true, true);
                bool isPlaying = emitter2.IsPlaying;
                emitter2.StopSound(true, true);
                if (isPlaying)
                {
                    emitter2.PlaySound(this.CharacterSounds[1], false, true, false, false, false, new bool?(!this.m_isFirstPerson && !MyFakes.FORCE_CHARACTER_2D_SOUND));
                }
            }
            if ((emitter.Sound != null) && (emitter.LastSoundData != null))
            {
                float num = MathHelper.Clamp((float) (this.m_character.Physics.LinearVelocity.Length() / 7.5f), (float) 0.1f, (float) 1f);
                float num2 = emitter.LastSoundData.Volume * num;
                emitter.Sound.SetVolume(num2);
            }
            if (!(objA.Equals(emitter.SoundPair) && emitter.IsPlaying) && (!this.m_isWalking || this.m_character.Definition.LoopingFootsteps))
            {
                MyCharacter entity = base.Entity as MyCharacter;
                if (!ReferenceEquals(objA, EmptySoundPair) && (objA == this.CharacterSounds[2]))
                {
                    if (this.m_jetpackSustainTimer >= 0.25f)
                    {
                        if (emitter.Loop)
                        {
                            emitter.StopSound(true, true);
                        }
                        emitter.PlaySound(objA, false, false, false, false, false, new bool?(!this.m_isFirstPerson && !MyFakes.FORCE_CHARACTER_2D_SOUND));
                    }
                }
                else if ((!emitter2.IsPlaying && (!ReferenceEquals(objA, EmptySoundPair) && (entity != null))) && entity.JetpackRunning)
                {
                    if ((this.m_jetpackSustainTimer <= 0f) || (objA != this.CharacterSounds[1]))
                    {
                        emitter2.PlaySound(this.CharacterSounds[1], false, false, false, false, false, new bool?(!this.m_isFirstPerson && !MyFakes.FORCE_CHARACTER_2D_SOUND));
                    }
                }
                else if (ReferenceEquals(objA, EmptySoundPair))
                {
                    foreach (MyEntity3DSoundEmitter emitter4 in this.m_soundEmitters)
                    {
                        if (emitter4.Loop)
                        {
                            emitter4.StopSound(false, true);
                        }
                    }
                }
                else if (!ReferenceEquals(objA, this.m_lastPrimarySound) || ((objA != this.CharacterSounds[3]) && (objA != this.CharacterSounds[4])))
                {
                    if (emitter.Loop)
                    {
                        emitter.StopSound(false, true);
                    }
                    if (objA == this.CharacterSounds[2])
                    {
                        emitter.PlaySound(objA, true, false, false, false, false, new bool?(!this.m_isFirstPerson && !MyFakes.FORCE_CHARACTER_2D_SOUND));
                    }
                    else if (objA != this.CharacterSounds[1])
                    {
                        bool? nullable = null;
                        emitter.PlaySound(objA, true, false, false, false, false, nullable);
                    }
                }
            }
            else if ((!this.m_character.Definition.LoopingFootsteps && (walkEmitter != null)) && (objA != null))
            {
                this.IKFeetStepSounds(walkEmitter, objA, this.m_isWalking && this.m_character.IsMagneticBootsEnabled);
            }
            if (((this.m_character.JetpackComp == null) || (this.m_character.JetpackComp.IsFlying || !this.m_character.JetpackComp.DampenersEnabled)) || this.m_character.Physics.LinearVelocity.Equals(Vector3.Zero))
            {
                emitter.VolumeMultiplier = 1f;
            }
            else
            {
                float num3 = 0.98f;
                emitter.VolumeMultiplier *= num3;
            }
            this.m_lastPrimarySound = objA;
        }

        private void IKFeetStepSounds(MyEntity3DSoundEmitter walkEmitter, MySoundPair cueEnum, bool magneticBootsOn)
        {
            MyCharacterMovementEnum currentMovementState = this.m_character.GetCurrentMovementState();
            bool isCrouching = this.m_character.IsCrouching;
            if (currentMovementState.GetMode() == 3)
            {
                return;
            }
            if (currentMovementState.GetSpeed() != this.m_lastUpdateMovementState.GetSpeed())
            {
                walkEmitter.StopSound(true, true);
                this.m_lastStepTime = 0;
            }
            int num = 0x7fffffff;
            if (currentMovementState.GetDirection() == 0)
            {
                goto TR_001D;
            }
            else if (currentMovementState > MyCharacterMovementEnum.WalkingRightFront)
            {
                if (currentMovementState > MyCharacterMovementEnum.RunningLeftFront)
                {
                    if (currentMovementState > MyCharacterMovementEnum.RunningRightFront)
                    {
                        if (currentMovementState > MyCharacterMovementEnum.Sprinting)
                        {
                            if ((currentMovementState != MyCharacterMovementEnum.CrouchRotatingLeft) && (currentMovementState != MyCharacterMovementEnum.CrouchRotatingRight))
                            {
                                goto TR_001D;
                            }
                        }
                        else
                        {
                            if (currentMovementState != MyCharacterMovementEnum.RunningRightBack)
                            {
                                if (currentMovementState == MyCharacterMovementEnum.Sprinting)
                                {
                                    num = 250;
                                }
                                goto TR_001D;
                            }
                            goto TR_0040;
                        }
                    }
                    else
                    {
                        if (currentMovementState == MyCharacterMovementEnum.RunningLeftBack)
                        {
                            goto TR_0040;
                        }
                        else if (currentMovementState == MyCharacterMovementEnum.RunStrafingRight)
                        {
                            goto TR_003A;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.RunningRightFront)
                        {
                            goto TR_001D;
                        }
                        goto TR_0036;
                    }
                }
                else if (currentMovementState > MyCharacterMovementEnum.CrouchWalkingRightBack)
                {
                    if (currentMovementState > MyCharacterMovementEnum.Backrunning)
                    {
                        if (currentMovementState == MyCharacterMovementEnum.RunStrafingLeft)
                        {
                            goto TR_003A;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.RunningLeftFront)
                        {
                            goto TR_001D;
                        }
                    }
                    else if ((currentMovementState != MyCharacterMovementEnum.Running) && (currentMovementState != MyCharacterMovementEnum.Backrunning))
                    {
                        goto TR_001D;
                    }
                    goto TR_0036;
                }
                else if (currentMovementState != MyCharacterMovementEnum.CrouchWalkingRightFront)
                {
                    if (currentMovementState == MyCharacterMovementEnum.WalkingRightBack)
                    {
                        goto TR_0020;
                    }
                    else if (currentMovementState != MyCharacterMovementEnum.CrouchWalkingRightBack)
                    {
                        goto TR_001D;
                    }
                }
                goto TR_001E;
            }
            else if (currentMovementState > MyCharacterMovementEnum.CrouchStrafingLeft)
            {
                if (currentMovementState > MyCharacterMovementEnum.WalkingLeftBack)
                {
                    if (currentMovementState > MyCharacterMovementEnum.WalkStrafingRight)
                    {
                        if (currentMovementState == MyCharacterMovementEnum.CrouchStrafingRight)
                        {
                            goto TR_001E;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.WalkingRightFront)
                        {
                            goto TR_001D;
                        }
                    }
                    else if (currentMovementState == MyCharacterMovementEnum.CrouchWalkingLeftBack)
                    {
                        goto TR_001E;
                    }
                    else if (currentMovementState != MyCharacterMovementEnum.WalkStrafingRight)
                    {
                        goto TR_001D;
                    }
                    goto TR_0020;
                }
                else if (currentMovementState == MyCharacterMovementEnum.WalkingLeftFront)
                {
                    goto TR_0020;
                }
                else if (currentMovementState != MyCharacterMovementEnum.CrouchWalkingLeftFront)
                {
                    if (currentMovementState != MyCharacterMovementEnum.WalkingLeftBack)
                    {
                        goto TR_001D;
                    }
                    goto TR_0020;
                }
                goto TR_001E;
            }
            else if (currentMovementState > MyCharacterMovementEnum.CrouchWalking)
            {
                if (currentMovementState > MyCharacterMovementEnum.CrouchBackWalking)
                {
                    if (currentMovementState == MyCharacterMovementEnum.WalkStrafingLeft)
                    {
                        goto TR_0020;
                    }
                    else if (currentMovementState != MyCharacterMovementEnum.CrouchStrafingLeft)
                    {
                        goto TR_001D;
                    }
                }
                else if (currentMovementState == MyCharacterMovementEnum.BackWalking)
                {
                    goto TR_0020;
                }
                else if (currentMovementState != MyCharacterMovementEnum.CrouchBackWalking)
                {
                    goto TR_001D;
                }
                goto TR_001E;
            }
            else if (currentMovementState == MyCharacterMovementEnum.Crouching)
            {
                goto TR_001E;
            }
            else if (currentMovementState != MyCharacterMovementEnum.Walking)
            {
                if (currentMovementState != MyCharacterMovementEnum.CrouchWalking)
                {
                    goto TR_001D;
                }
                goto TR_001E;
            }
            goto TR_0020;
        TR_001D:
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastStepTime) >= num)
            {
                int num2;
                int num3;
                float y;
                MyFeetIKSettings settings;
                Vector3 center;
                Vector3 center;
                MyCharacterBone bone = this.m_character.AnimationController?.FindBone(this.m_character.Definition.LeftAnkleBoneName, out num2);
                MyCharacterBone bone2 = this.m_character.AnimationController?.FindBone(this.m_character.Definition.RightAnkleBoneName, out num3);
                if (bone == null)
                {
                    center = this.m_character.PositionComp.LocalAABB.Center;
                }
                else
                {
                    center = bone.AbsoluteTransform.Translation;
                }
                if (bone2 == null)
                {
                    center = this.m_character.PositionComp.LocalAABB.Center;
                }
                else
                {
                    center = bone2.AbsoluteTransform.Translation;
                }
                Vector3 vector = center;
                if ((this.m_character.Definition.FeetIKSettings == null) || !this.m_character.Definition.FeetIKSettings.TryGetValue(MyCharacterMovementEnum.Standing, out settings))
                {
                    y = 0.2f;
                }
                else
                {
                    y = settings.FootSize.Y;
                }
                float num5 = 0f;
                if (this.m_character.AnimationController != null)
                {
                    this.m_character.AnimationController.Variables.GetValue(MyAnimationVariableStorageHints.StrIdSpeed, out num5);
                }
                if (((center.Y - y) < this.m_character.PositionComp.LocalAABB.Min.Y) || ((vector.Y - y) < this.m_character.PositionComp.LocalAABB.Min.Y))
                {
                    if (num5 > 0f)
                    {
                        walkEmitter.PlaySound(cueEnum, false, false, false, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                        if (walkEmitter.Sound != null)
                        {
                            if (!magneticBootsOn)
                            {
                                walkEmitter.Sound.FrequencyRatio = 1f;
                            }
                            else
                            {
                                walkEmitter.Sound.FrequencyRatio *= 0.95f;
                                if ((this.m_magneticBootsEmitter != null) && (this.CharacterSounds[0x11] != MySoundPair.Empty))
                                {
                                    this.m_magneticBootsEmitter.PlaySound(this.CharacterSounds[0x11], false, false, false, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                                }
                            }
                        }
                    }
                    this.m_lastStepTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
            }
            this.m_lastUpdateMovementState = currentMovementState;
            return;
        TR_001E:
            num = 530;
            goto TR_001D;
        TR_0020:
            num = 440;
            goto TR_001D;
        TR_0036:
            num = 330;
            goto TR_001D;
        TR_003A:
            num = 330;
            goto TR_001D;
        TR_0040:
            num = 400;
            goto TR_001D;
        }

        private void InitSounds()
        {
            if (this.m_character.Definition.JumpSoundName != null)
            {
                this.CharacterSounds[0] = new MySoundPair(this.m_character.Definition.JumpSoundName, true);
            }
            if (this.m_character.Definition.JetpackIdleSoundName != null)
            {
                this.CharacterSounds[1] = new MySoundPair(this.m_character.Definition.JetpackIdleSoundName, true);
            }
            if (this.m_character.Definition.JetpackRunSoundName != null)
            {
                this.CharacterSounds[2] = new MySoundPair(this.m_character.Definition.JetpackRunSoundName, true);
            }
            if (this.m_character.Definition.CrouchDownSoundName != null)
            {
                this.CharacterSounds[3] = new MySoundPair(this.m_character.Definition.CrouchDownSoundName, true);
            }
            if (this.m_character.Definition.CrouchUpSoundName != null)
            {
                this.CharacterSounds[4] = new MySoundPair(this.m_character.Definition.CrouchUpSoundName, true);
            }
            if (this.m_character.Definition.PainSoundName != null)
            {
                this.CharacterSounds[5] = new MySoundPair(this.m_character.Definition.PainSoundName, true);
            }
            if (this.m_character.Definition.SuffocateSoundName != null)
            {
                this.CharacterSounds[6] = new MySoundPair(this.m_character.Definition.SuffocateSoundName, true);
            }
            if (this.m_character.Definition.DeathSoundName != null)
            {
                this.CharacterSounds[7] = new MySoundPair(this.m_character.Definition.DeathSoundName, true);
            }
            if (this.m_character.Definition.DeathBySuffocationSoundName != null)
            {
                this.CharacterSounds[8] = new MySoundPair(this.m_character.Definition.DeathBySuffocationSoundName, true);
            }
            if (this.m_character.Definition.IronsightActSoundName != null)
            {
                this.CharacterSounds[9] = new MySoundPair(this.m_character.Definition.IronsightActSoundName, true);
            }
            if (this.m_character.Definition.IronsightDeactSoundName != null)
            {
                this.CharacterSounds[10] = new MySoundPair(this.m_character.Definition.IronsightDeactSoundName, true);
            }
            if (this.m_character.Definition.FastFlySoundName != null)
            {
                this.m_windEmitter = new MyEntity3DSoundEmitter(base.Entity as MyEntity, false, 1f);
                this.m_windEmitter.Force3D = false;
                this.m_windSystem = true;
                this.CharacterSounds[11] = new MySoundPair(this.m_character.Definition.FastFlySoundName, true);
            }
            if (this.m_character.Definition.HelmetOxygenNormalSoundName != null)
            {
                this.CharacterSounds[12] = new MySoundPair(this.m_character.Definition.HelmetOxygenNormalSoundName, true);
            }
            if (this.m_character.Definition.HelmetOxygenLowSoundName != null)
            {
                this.CharacterSounds[13] = new MySoundPair(this.m_character.Definition.HelmetOxygenLowSoundName, true);
            }
            if (this.m_character.Definition.HelmetOxygenCriticalSoundName != null)
            {
                this.CharacterSounds[14] = new MySoundPair(this.m_character.Definition.HelmetOxygenCriticalSoundName, true);
            }
            if (this.m_character.Definition.HelmetOxygenNoneSoundName != null)
            {
                this.CharacterSounds[15] = new MySoundPair(this.m_character.Definition.HelmetOxygenNoneSoundName, true);
            }
            if (this.m_character.Definition.MovementSoundName != null)
            {
                this.CharacterSounds[0x10] = new MySoundPair(this.m_character.Definition.MovementSoundName, true);
                this.m_movementEmitter = new MyEntity3DSoundEmitter(base.Entity as MyEntity, false, 1f);
            }
            if ((!string.IsNullOrEmpty(this.m_character.Definition.MagnetBootsStepsSoundName) || (!string.IsNullOrEmpty(this.m_character.Definition.MagnetBootsStartSoundName) || !string.IsNullOrEmpty(this.m_character.Definition.MagnetBootsEndSoundName))) || !string.IsNullOrEmpty(this.m_character.Definition.MagnetBootsProximitySoundName))
            {
                this.CharacterSounds[0x11] = new MySoundPair(this.m_character.Definition.MagnetBootsStepsSoundName, true);
                this.CharacterSounds[0x12] = new MySoundPair(this.m_character.Definition.MagnetBootsStartSoundName, true);
                this.CharacterSounds[0x13] = new MySoundPair(this.m_character.Definition.MagnetBootsEndSoundName, true);
                this.CharacterSounds[20] = new MySoundPair(this.m_character.Definition.MagnetBootsProximitySoundName, true);
                this.m_magneticBootsEmitter = new MyEntity3DSoundEmitter(base.Entity as MyEntity, false, 1f);
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_character = base.Entity as MyCharacter;
            using (List<MyEntity3DSoundEmitter>.Enumerator enumerator = this.m_soundEmitters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Entity = base.Entity as MyEntity;
                }
            }
            if (this.m_windEmitter != null)
            {
                this.m_windEmitter.Entity = base.Entity as MyEntity;
            }
            if (this.m_oxygenEmitter != null)
            {
                this.m_oxygenEmitter.Entity = base.Entity as MyEntity;
            }
            this.m_lastUpdateMovementState = this.m_character.GetCurrentMovementState();
            this.m_characterPhysicalMaterial = MyStringHash.GetOrCompute(this.m_character.Definition.PhysicalMaterial);
            this.InitSounds();
        }

        public override void OnBeforeRemovedFromContainer()
        {
            this.StopStateSound(true);
            this.m_character = null;
            base.OnBeforeRemovedFromContainer();
        }

        public void PlayActionSound(MySoundPair actionSound, bool? force3D = new bool?())
        {
            this.m_lastActionSound = actionSound;
            this.m_soundEmitters[3].PlaySound(this.m_lastActionSound, false, false, false, false, false, force3D);
        }

        public void PlayDamageSound(float oldHealth)
        {
            if (MyFakes.ENABLE_NEW_SOUNDS && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastScreamTime) > 800))
            {
                bool? nullable;
                bool flag = false;
                if (ReferenceEquals(MySession.Static.LocalCharacter, base.Entity))
                {
                    flag = true;
                }
                this.m_lastScreamTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                if ((this.m_character.StatComp != null) && (this.m_character.StatComp.LastDamage.Type == LowPressure))
                {
                    nullable = null;
                    this.PlaySecondarySound(CharacterSoundsEnum.SUFFOCATE_SOUND, false, flag, nullable);
                }
                else
                {
                    nullable = null;
                    this.PlaySecondarySound(CharacterSoundsEnum.PAIN_SOUND, false, flag, nullable);
                }
            }
        }

        public void PlayDeathSound(MyStringHash damageType, bool stopPrevious = false)
        {
            bool? nullable;
            if (damageType == LowPressure)
            {
                nullable = null;
                this.m_soundEmitters[1].PlaySound(this.CharacterSounds[8], stopPrevious, false, false, false, false, nullable);
            }
            else
            {
                nullable = null;
                this.m_soundEmitters[1].PlaySound(this.CharacterSounds[7], stopPrevious, false, false, false, false, nullable);
            }
        }

        public void PlayFallSound()
        {
            MyStringHash hash = this.RayCastGround();
            if ((hash != MyStringHash.NullOrEmpty) && (MyMaterialPropertiesHelper.Static != null))
            {
                MySoundPair soundId = MyMaterialPropertiesHelper.Static.GetCollisionCue(MovementSoundType.Fall, this.m_characterPhysicalMaterial, hash);
                if (!soundId.SoundId.IsNull)
                {
                    MyEntity3DSoundEmitter emitter = this.m_soundEmitters[6];
                    if (emitter != null)
                    {
                        emitter.Entity = this.m_character;
                        emitter.PlaySound(soundId, false, false, false, true, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                    }
                }
            }
        }

        internal void PlayMagneticBootsEnd()
        {
            if ((this.m_magneticBootsEmitter != null) && (this.CharacterSounds[0x13] != MySoundPair.Empty))
            {
                this.m_magneticBootsEmitter.PlaySound(this.CharacterSounds[0x13], false, false, false, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
            }
        }

        internal void PlayMagneticBootsProximity()
        {
            if ((this.m_magneticBootsEmitter != null) && (this.CharacterSounds[20] != MySoundPair.Empty))
            {
                this.m_magneticBootsEmitter.PlaySound(this.CharacterSounds[20], false, false, false, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
            }
        }

        internal void PlayMagneticBootsStart()
        {
            if ((this.m_magneticBootsEmitter != null) && (this.CharacterSounds[0x12] != MySoundPair.Empty))
            {
                this.m_magneticBootsEmitter.PlaySound(this.CharacterSounds[0x12], false, false, false, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
            }
        }

        public void PlaySecondarySound(CharacterSoundsEnum soundEnum, bool stopPrevious = false, bool force2D = false, bool? force3D = new bool?())
        {
            this.m_soundEmitters[1].PlaySound(this.CharacterSounds[(int) soundEnum], stopPrevious, false, force2D, false, false, force3D);
        }

        public void Preload()
        {
            using (Dictionary<int, MySoundPair>.ValueCollection.Enumerator enumerator = this.CharacterSounds.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyEntity3DSoundEmitter.PreloadSound(enumerator.Current);
                }
            }
        }

        private MyStringHash RayCastGround()
        {
            MyStringHash materialAt = new MyStringHash();
            if (this.m_character != null)
            {
                float num = MyConstants.DEFAULT_GROUND_SEARCH_DISTANCE;
                Vector3D from = this.m_character.PositionComp.GetPosition() + (this.m_character.PositionComp.WorldMatrix.Up * 0.5);
                MyPhysics.CastRay(from, from + (this.m_character.PositionComp.WorldMatrix.Down * num), this.m_hits, 0x12);
                int num2 = 0;
                while ((num2 < this.m_hits.Count) && ((this.m_hits[num2].HkHitInfo.Body == null) || ReferenceEquals(this.m_hits[num2].HkHitInfo.GetHitEntity(), base.Entity.Components)))
                {
                    num2++;
                }
                if (this.m_hits.Count == 0)
                {
                    if (((this.m_standingOnGrid == null) && (this.m_standingOnVoxel == null)) || !this.ShouldUpdateSoundEmitters)
                    {
                        this.m_standingOnGrid = null;
                        this.m_standingOnVoxel = null;
                    }
                    else
                    {
                        this.m_standingOnGrid = null;
                        this.m_standingOnVoxel = null;
                        MyEntity3DSoundEmitter.UpdateEntityEmitters(true, true, false);
                    }
                }
                if (num2 < this.m_hits.Count)
                {
                    MyPhysics.HitInfo info = this.m_hits[num2];
                    IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                    if (Vector3D.DistanceSquared(info.Position, from) < (num * num))
                    {
                        MyCubeGrid objB = hitEntity as MyCubeGrid;
                        MyVoxelBase base2 = hitEntity as MyVoxelBase;
                        if ((((objB == null) || ReferenceEquals(this.m_standingOnGrid, objB)) && ((base2 == null) || ReferenceEquals(this.m_standingOnVoxel, base2))) || !this.ShouldUpdateSoundEmitters)
                        {
                            this.m_standingOnGrid = objB;
                            this.m_standingOnVoxel = base2;
                        }
                        else
                        {
                            this.m_standingOnGrid = objB;
                            this.m_standingOnVoxel = base2;
                            MyEntity3DSoundEmitter.UpdateEntityEmitters(true, true, true);
                        }
                        if ((objB != null) || (base2 != null))
                        {
                            this.m_jumpReady = true;
                        }
                        if ((objB != null) && (objB.Physics != null))
                        {
                            materialAt = objB.Physics.GetMaterialAt(info.Position + (this.m_character.PositionComp.WorldMatrix.Down * 0.10000000149011612));
                        }
                        else if (((base2 == null) || (base2.Storage == null)) || (base2.Storage.DataProvider == null))
                        {
                            if ((hitEntity != null) && (hitEntity.Physics != null))
                            {
                                materialAt = hitEntity.Physics.GetMaterialAt(info.Position + (this.m_character.PositionComp.WorldMatrix.Down * 0.10000000149011612));
                            }
                        }
                        else
                        {
                            MyVoxelMaterialDefinition materialAt = base2.GetMaterialAt(ref info.Position);
                            if (materialAt != null)
                            {
                                materialAt = materialAt.MaterialTypeNameHash;
                            }
                        }
                        if ((materialAt == MyStringHash.NullOrEmpty) && (hitEntity.Parent != null))
                        {
                            MyCubeGrid parent = hitEntity.Parent as MyCubeGrid;
                            MyCubeBlock block = hitEntity.Parent as MyCubeBlock;
                            if ((parent != null) && (parent.Physics != null))
                            {
                                materialAt = hitEntity.Parent.Physics.MaterialType;
                            }
                            else if (block != null)
                            {
                                materialAt = block.BlockDefinition.PhysicalMaterial.Id.SubtypeId;
                            }
                        }
                        if (materialAt == MyStringHash.NullOrEmpty)
                        {
                            materialAt = MyMaterialType.ROCK;
                        }
                    }
                }
                this.m_hits.Clear();
            }
            return materialAt;
        }

        private void ResetStandingSoundStates()
        {
            if (!MyFakes.ENABLE_REALISTIC_ON_TOUCH)
            {
                this.m_standingOnGrid = null;
                this.m_standingOnVoxel = null;
            }
            else if ((this.m_standingOnGrid == null) || (this.m_lastContactCounter >= 0))
            {
                this.m_lastContactCounter--;
            }
            else
            {
                this.m_standingOnGrid = null;
                MyEntity3DSoundEmitter.UpdateEntityEmitters(true, true, false);
            }
        }

        private MySoundPair SelectSound()
        {
            MySoundPair emptySoundPair = EmptySoundPair;
            MyStringHash orCompute = MyStringHash.GetOrCompute(this.m_character.Definition.PhysicalMaterial);
            this.m_isWalking = false;
            bool flag = false;
            MyCharacterMovementEnum currentMovementState = this.m_character.GetCurrentMovementState();
            if (currentMovementState > MyCharacterMovementEnum.CrouchStrafingRight)
            {
                if (currentMovementState > MyCharacterMovementEnum.Backrunning)
                {
                    if (currentMovementState > MyCharacterMovementEnum.RunningLeftBack)
                    {
                        if (currentMovementState > MyCharacterMovementEnum.RunningRightFront)
                        {
                            if (currentMovementState != MyCharacterMovementEnum.RunningRightBack)
                            {
                                if (currentMovementState == MyCharacterMovementEnum.Sprinting)
                                {
                                    if (this.m_character.Breath != null)
                                    {
                                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.VeryHeated;
                                    }
                                    emptySoundPair = MyMaterialPropertiesHelper.Static.GetCollisionCue(MovementSoundType.Sprint, orCompute, this.RayCastGround());
                                    this.m_isWalking = true;
                                }
                                goto TR_0004;
                            }
                        }
                        else if ((currentMovementState != MyCharacterMovementEnum.RunStrafingRight) && (currentMovementState != MyCharacterMovementEnum.RunningRightFront))
                        {
                            goto TR_0004;
                        }
                    }
                    else if (((currentMovementState != MyCharacterMovementEnum.RunStrafingLeft) && (currentMovementState != MyCharacterMovementEnum.RunningLeftFront)) && (currentMovementState != MyCharacterMovementEnum.RunningLeftBack))
                    {
                        goto TR_0004;
                    }
                    goto TR_0048;
                }
                else if (currentMovementState > MyCharacterMovementEnum.WalkingRightBack)
                {
                    if (currentMovementState != MyCharacterMovementEnum.CrouchWalkingRightBack)
                    {
                        if ((currentMovementState != MyCharacterMovementEnum.Running) && (currentMovementState != MyCharacterMovementEnum.Backrunning))
                        {
                            goto TR_0004;
                        }
                        goto TR_0048;
                    }
                }
                else
                {
                    if (currentMovementState != MyCharacterMovementEnum.WalkingRightFront)
                    {
                        if (currentMovementState == MyCharacterMovementEnum.CrouchWalkingRightFront)
                        {
                            goto TR_0007;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.WalkingRightBack)
                        {
                            goto TR_0004;
                        }
                    }
                    goto TR_000B;
                }
            }
            else if (currentMovementState > MyCharacterMovementEnum.WalkStrafingLeft)
            {
                if (currentMovementState > MyCharacterMovementEnum.CrouchWalkingLeftFront)
                {
                    if (currentMovementState > MyCharacterMovementEnum.CrouchWalkingLeftBack)
                    {
                        if (currentMovementState == MyCharacterMovementEnum.WalkStrafingRight)
                        {
                            goto TR_000B;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.CrouchStrafingRight)
                        {
                            goto TR_0004;
                        }
                    }
                    else if (currentMovementState == MyCharacterMovementEnum.WalkingLeftBack)
                    {
                        goto TR_000B;
                    }
                    else if (currentMovementState != MyCharacterMovementEnum.CrouchWalkingLeftBack)
                    {
                        goto TR_0004;
                    }
                    goto TR_0007;
                }
                else if (currentMovementState == MyCharacterMovementEnum.CrouchStrafingLeft)
                {
                    goto TR_0007;
                }
                else if (currentMovementState != MyCharacterMovementEnum.WalkingLeftFront)
                {
                    if (currentMovementState != MyCharacterMovementEnum.CrouchWalkingLeftFront)
                    {
                        goto TR_0004;
                    }
                    goto TR_0007;
                }
                goto TR_000B;
            }
            else if (currentMovementState > MyCharacterMovementEnum.CrouchWalking)
            {
                if (currentMovementState != MyCharacterMovementEnum.BackWalking)
                {
                    if (currentMovementState == MyCharacterMovementEnum.CrouchBackWalking)
                    {
                        goto TR_0007;
                    }
                    else if (currentMovementState != MyCharacterMovementEnum.WalkStrafingLeft)
                    {
                        goto TR_0004;
                    }
                }
                goto TR_000B;
            }
            else
            {
                switch (currentMovementState)
                {
                    case MyCharacterMovementEnum.Standing:
                    case MyCharacterMovementEnum.Crouching:
                    {
                        if (this.m_character.Breath != null)
                        {
                            this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
                        }
                        MyCharacterMovementEnum previousMovementState = this.m_character.GetPreviousMovementState();
                        MyCharacterMovementEnum enum4 = this.m_character.GetCurrentMovementState();
                        if ((previousMovementState != enum4) && ((previousMovementState == MyCharacterMovementEnum.Standing) || (previousMovementState == MyCharacterMovementEnum.Crouching)))
                        {
                            emptySoundPair = (enum4 == MyCharacterMovementEnum.Standing) ? this.CharacterSounds[4] : this.CharacterSounds[3];
                        }
                        this.RayCastGround();
                        goto TR_0004;
                    }
                    case MyCharacterMovementEnum.Sitting:
                        if (this.m_character.Breath != null)
                        {
                            this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
                        }
                        goto TR_0004;

                    case MyCharacterMovementEnum.Flying:
                        if (this.m_character.Breath != null)
                        {
                            this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
                        }
                        if (((this.m_character.JetpackComp == null) || (this.m_jetpackMinIdleTime > 0f)) || (this.m_character.JetpackComp.FinalThrust.LengthSquared() < 50000f))
                        {
                            emptySoundPair = this.CharacterSounds[1];
                            this.m_jetpackSustainTimer = Math.Max((float) 0f, (float) (this.m_jetpackSustainTimer - 0.01666667f));
                        }
                        else
                        {
                            emptySoundPair = this.CharacterSounds[2];
                            this.m_jetpackSustainTimer = Math.Min((float) 0.25f, (float) (this.m_jetpackSustainTimer + 0.01666667f));
                        }
                        this.m_jetpackMinIdleTime -= 0.01666667f;
                        if (((this.m_standingOnGrid != null) || (this.m_standingOnVoxel != null)) && this.ShouldUpdateSoundEmitters)
                        {
                            flag = true;
                        }
                        this.ResetStandingSoundStates();
                        goto TR_0004;

                    case MyCharacterMovementEnum.Falling:
                        if (this.m_character.Breath != null)
                        {
                            this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
                        }
                        if (((this.m_standingOnGrid != null) || (this.m_standingOnVoxel != null)) && this.ShouldUpdateSoundEmitters)
                        {
                            flag = true;
                        }
                        this.ResetStandingSoundStates();
                        goto TR_0004;

                    case MyCharacterMovementEnum.Jump:
                        if (this.m_jumpReady)
                        {
                            this.m_jumpReady = false;
                            this.m_character.SetPreviousMovementState(this.m_character.GetCurrentMovementState());
                            MyEntity3DSoundEmitter emitter = this.m_soundEmitters[5];
                            if (emitter != null)
                            {
                                emitter.Entity = this.m_character;
                                emitter.PlaySound(this.CharacterSounds[0], false, false, false, true, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                            }
                            if (((this.m_standingOnGrid != null) || (this.m_standingOnVoxel != null)) && this.ShouldUpdateSoundEmitters)
                            {
                                flag = true;
                            }
                            this.m_standingOnGrid = null;
                            this.m_standingOnVoxel = null;
                        }
                        goto TR_0004;

                    default:
                        if (currentMovementState == MyCharacterMovementEnum.Walking)
                        {
                            goto TR_000B;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.CrouchWalking)
                        {
                            goto TR_0004;
                        }
                        break;
                }
            }
            goto TR_0007;
        TR_0004:
            if (currentMovementState != MyCharacterMovementEnum.Flying)
            {
                this.m_jetpackSustainTimer = 0f;
                this.m_jetpackMinIdleTime = 0.5f;
            }
            if (flag)
            {
                MyEntity3DSoundEmitter.UpdateEntityEmitters(true, true, false);
            }
            return emptySoundPair;
        TR_0007:
            if (this.m_character.Breath != null)
            {
                this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
            }
            emptySoundPair = MyMaterialPropertiesHelper.Static.GetCollisionCue(MovementSoundType.CrouchWalk, orCompute, this.RayCastGround());
            this.m_isWalking = true;
            goto TR_0004;
        TR_000B:
            if (this.m_character.Breath != null)
            {
                this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
            }
            emptySoundPair = MyMaterialPropertiesHelper.Static.GetCollisionCue(MovementSoundType.Walk, orCompute, this.RayCastGround());
            this.m_isWalking = true;
            goto TR_0004;
        TR_0048:
            if (this.m_character.Breath != null)
            {
                this.m_character.Breath.CurrentState = MyCharacterBreath.State.Heated;
            }
            emptySoundPair = MyMaterialPropertiesHelper.Static.GetCollisionCue(MovementSoundType.Run, orCompute, this.RayCastGround());
            this.m_isWalking = true;
            goto TR_0004;
        }

        public void StartSecondarySound(string cueName, bool sync = false)
        {
            this.StartSecondarySound(MySoundPair.GetCueId(cueName), sync);
        }

        public void StartSecondarySound(MyCueId cueId, bool sync = false)
        {
            if (!cueId.IsNull)
            {
                bool? nullable = null;
                this.m_soundEmitters[1].PlaySoundWithDistance(cueId, false, false, false, true, false, false, nullable);
                if (sync)
                {
                    this.m_character.PlaySecondarySound(cueId);
                }
            }
        }

        public bool StopSecondarySound(bool forceStop = true)
        {
            this.m_soundEmitters[1].StopSound(forceStop, true);
            return true;
        }

        public bool StopStateSound(bool forceStop = true)
        {
            this.m_soundEmitters[0].StopSound(forceStop, true);
            return true;
        }

        public void UpdateAfterSimulation100()
        {
            this.UpdateOxygenSounds();
            this.m_soundEmitters[0].Update();
            this.m_soundEmitters[4].Update();
            if (this.m_windSystem)
            {
                this.m_inAtmosphere = (this.m_character.AtmosphereDetectorComp != null) && this.m_character.AtmosphereDetectorComp.InAtmosphere;
                this.m_windEmitter.Update();
            }
            if (this.m_oxygenEmitter != null)
            {
                this.m_oxygenEmitter.Update();
            }
        }

        private void UpdateBreath()
        {
            AdminSettingsEnum enum2;
            if ((((MySession.Static == null) || ((MySession.Static.ControlledEntity == null) || ((MySession.Static.ControlledEntity.ControllerInfo == null) || ((MySession.Static.ControlledEntity.ControllerInfo.Controller == null) || ((MySession.Static.ControlledEntity.ControllerInfo.Controller.Player == null) || ((MySession.Static.ControlledEntity.ControllerInfo.Controller.Player.Id.SerialId != 0) || !MySession.Static.RemoteAdminSettings.TryGetValue(MySession.Static.ControlledEntity.ControllerInfo.Controller.Player.Id.SteamId, out enum2))))))) || !enum2.HasFlag(AdminSettingsEnum.Invulnerable)) && ((this.m_character.OxygenComponent != null) && (this.m_character.Breath != null)))
            {
                if (!MySession.Static.Settings.EnableOxygen || MySession.Static.CreativeMode)
                {
                    this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
                    if (!this.m_character.OxygenComponent.HelmetEnabled)
                    {
                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.NoBreath;
                    }
                }
                else if ((this.m_character.Parent is MyCockpit) && (this.m_character.Parent as MyCockpit).BlockDefinition.IsPressurized)
                {
                    if (this.m_character.OxygenComponent.HelmetEnabled)
                    {
                        if (this.m_character.OxygenComponent.SuitOxygenAmount > 0f)
                        {
                            this.m_character.Breath.CurrentState = MyCharacterBreath.State.Calm;
                        }
                        else
                        {
                            this.m_character.Breath.CurrentState = MyCharacterBreath.State.Choking;
                        }
                    }
                    else if (this.m_character.EnvironmentOxygenLevel >= MyCharacterOxygenComponent.LOW_OXYGEN_RATIO)
                    {
                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.NoBreath;
                    }
                    else
                    {
                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.Choking;
                    }
                }
                else if (!this.m_character.OxygenComponent.HelmetEnabled)
                {
                    if (this.m_character.EnvironmentOxygenLevel >= MyCharacterOxygenComponent.LOW_OXYGEN_RATIO)
                    {
                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.NoBreath;
                    }
                    else if (this.m_character.EnvironmentOxygenLevel > 0f)
                    {
                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.VeryHeated;
                    }
                    else
                    {
                        this.m_character.Breath.CurrentState = MyCharacterBreath.State.Choking;
                    }
                }
                else if (this.m_character.OxygenComponent.SuitOxygenAmount <= 0f)
                {
                    this.m_character.Breath.CurrentState = MyCharacterBreath.State.Choking;
                }
            }
        }

        internal void UpdateEntityEmitters(MyCubeGrid cubeGrid)
        {
            this.m_standingOnGrid = cubeGrid;
            this.m_lastContactCounter = 10;
            MyEntity3DSoundEmitter.UpdateEntityEmitters(true, true, true);
        }

        private void UpdateOxygenSounds()
        {
            if (this.m_oxygenEmitter != null)
            {
                if ((this.m_character.IsDead || ((MySession.Static == null) || (!MySession.Static.Settings.EnableOxygen || (MySession.Static.CreativeMode || (this.m_character.OxygenComponent == null))))) || !this.m_character.OxygenComponent.HelmetEnabled)
                {
                    if (this.m_oxygenEmitter.IsPlaying)
                    {
                        this.m_oxygenEmitter.StopSound(true, true);
                    }
                }
                else
                {
                    AdminSettingsEnum enum2;
                    bool flag = false;
                    if (((MySession.Static != null) && ((MySession.Static.ControlledEntity != null) && ((MySession.Static.ControlledEntity.ControllerInfo != null) && ((MySession.Static.ControlledEntity.ControllerInfo.Controller != null) && ((MySession.Static.ControlledEntity.ControllerInfo.Controller.Player != null) && ((MySession.Static.ControlledEntity.ControllerInfo.Controller.Player.Id.SerialId == 0) && MySession.Static.RemoteAdminSettings.TryGetValue(MySession.Static.ControlledEntity.ControllerInfo.Controller.Player.Id.SteamId, out enum2))))))) && enum2.HasFlag(AdminSettingsEnum.Invulnerable))
                    {
                        flag = true;
                    }
                    MySoundPair objB = !(MySession.Static.CreativeMode | flag) ? ((this.m_character.OxygenComponent.SuitOxygenLevel <= MyCharacterOxygenComponent.LOW_OXYGEN_RATIO) ? ((this.m_character.OxygenComponent.SuitOxygenLevel <= (MyCharacterOxygenComponent.LOW_OXYGEN_RATIO / 3f)) ? ((this.m_character.OxygenComponent.SuitOxygenLevel <= 0f) ? this.CharacterSounds[15] : this.CharacterSounds[14]) : this.CharacterSounds[13]) : this.CharacterSounds[12]) : this.CharacterSounds[12];
                    if (!this.m_oxygenEmitter.IsPlaying || !ReferenceEquals(this.m_oxygenEmitter.SoundPair, objB))
                    {
                        bool? nullable = null;
                        this.m_oxygenEmitter.PlaySound(objB, true, false, false, false, false, nullable);
                    }
                }
            }
        }

        public void UpdateWindSounds()
        {
            if (this.m_windSystem && !this.m_character.IsDead)
            {
                if (!this.m_inAtmosphere)
                {
                    this.m_windTargetVolume = 0f;
                }
                else
                {
                    float num = this.m_character.Physics.LinearVelocity.Length();
                    this.m_windTargetVolume = (num >= 40f) ? ((num >= 80f) ? 1f : ((num - 40f) / 40f)) : 0f;
                }
                if (this.m_windVolume < this.m_windTargetVolume)
                {
                    this.m_windVolume = Math.Min(this.m_windVolume + 0.008333334f, this.m_windTargetVolume);
                }
                else if (this.m_windVolume > this.m_windTargetVolume)
                {
                    this.m_windVolume = Math.Max(this.m_windVolume - 0.008333334f, this.m_windTargetVolume);
                }
                if (!this.m_windEmitter.IsPlaying)
                {
                    if (this.m_windVolume > 0f)
                    {
                        bool? nullable = null;
                        this.m_windEmitter.PlaySound(this.CharacterSounds[11], true, false, true, false, false, nullable);
                        this.m_windEmitter.CustomVolume = new float?(this.m_windVolume);
                    }
                }
                else if (this.m_windVolume <= 0f)
                {
                    this.m_windEmitter.StopSound(true, true);
                }
                else
                {
                    this.m_windEmitter.CustomVolume = new float?(this.m_windVolume);
                }
                if (!(this.m_windVolume == this.m_windTargetVolume))
                {
                    MySessionComponentPlanetAmbientSounds component = MySession.Static.GetComponent<MySessionComponentPlanetAmbientSounds>();
                    if (component != null)
                    {
                        component.VolumeModifierGlobal = 1f - this.m_windVolume;
                    }
                }
            }
        }

        public MyCubeGrid StandingOnGrid =>
            this.m_standingOnGrid;

        public MyVoxelBase StandingOnVoxel =>
            this.m_standingOnVoxel;

        private bool ShouldUpdateSoundEmitters =>
            (ReferenceEquals(this.m_character, MySession.Static.LocalCharacter) && ((this.m_character.AtmosphereDetectorComp != null) && (!this.m_character.AtmosphereDetectorComp.InAtmosphere && (MyFakes.ENABLE_NEW_SOUNDS && (MySession.Static.Settings.RealisticSound && MyFakes.ENABLE_NEW_SOUNDS_QUICK_UPDATE)))));

        public override string ComponentTypeDebugString =>
            "CharacterSound";

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct MovementSoundType
        {
            public static readonly MyStringId Walk;
            public static readonly MyStringId CrouchWalk;
            public static readonly MyStringId Run;
            public static readonly MyStringId Sprint;
            public static readonly MyStringId Fall;
            static MovementSoundType()
            {
                Walk = MyStringId.GetOrCompute("Walk");
                CrouchWalk = MyStringId.GetOrCompute("CrouchWalk");
                Run = MyStringId.GetOrCompute("Run");
                Sprint = MyStringId.GetOrCompute("Sprint");
                Fall = MyStringId.GetOrCompute("Fall");
            }
        }

        private enum MySoundEmitterEnum
        {
            PrimaryState,
            SecondaryState,
            WalkState,
            Action,
            IdleJetState,
            JumpState,
            FallState
        }
    }
}

