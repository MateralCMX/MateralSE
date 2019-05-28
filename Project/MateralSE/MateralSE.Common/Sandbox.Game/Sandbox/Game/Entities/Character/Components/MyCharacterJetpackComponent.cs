namespace Sandbox.Game.Entities.Character.Components
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRageMath;

    public class MyCharacterJetpackComponent : MyCharacterComponent
    {
        public const float FuelLowThresholdPlayer = 0.1f;
        public const float FuelCriticalThresholdPlayer = 0.05f;
        public const float ROTATION_FACTOR = 0.02f;
        private const float AUTO_ENABLE_JETPACK_INTERVAL = 1f;
        private bool m_isOnPlanetSurface;
        private int m_planetSurfaceRaycastCounter;

        public MyCharacterJetpackComponent()
        {
            this.CurrentAutoEnableDelay = 0f;
            this.TurnedOn = false;
        }

        public void ClearMovement()
        {
            this.ThrustComp.ControlThrust = Vector3.Zero;
        }

        public void EnableDampeners(bool enable)
        {
            if (this.DampenersTurnedOn != enable)
            {
                this.ThrustComp.DampenersEnabled = enable;
            }
        }

        public virtual void GetObjectBuilder(MyObjectBuilder_Character characterBuilder)
        {
            characterBuilder.DampenersEnabled = this.DampenersTurnedOn;
            bool turnedOn = this.TurnedOn;
            if (MySession.Static.ControlledEntity is MyCockpit)
            {
                turnedOn = (MySession.Static.ControlledEntity as MyCockpit).PilotJetpackEnabledBackup;
            }
            characterBuilder.JetpackEnabled = turnedOn;
            characterBuilder.AutoenableJetpackDelay = this.CurrentAutoEnableDelay;
        }

        public virtual void Init(MyObjectBuilder_Character characterBuilder)
        {
            if (characterBuilder != null)
            {
                MyFuelConverterInfo fuelConverter;
                this.CurrentAutoEnableDelay = characterBuilder.AutoenableJetpackDelay;
                if (this.ThrustComp != null)
                {
                    base.Character.Components.Remove<MyJetpackThrustComponent>();
                }
                MyObjectBuilder_ThrustDefinition thrustProperties = base.Character.Definition.Jetpack.ThrustProperties;
                this.FuelConverterDefinition = null;
                if (MyFakes.ENABLE_HYDROGEN_FUEL)
                {
                    fuelConverter = base.Character.Definition.Jetpack.ThrustProperties.FuelConverter;
                }
                else
                {
                    MyFuelConverterInfo info1 = new MyFuelConverterInfo();
                    info1.Efficiency = 1f;
                    fuelConverter = info1;
                }
                this.FuelConverterDefinition = fuelConverter;
                MyDefinitionId defId = new MyDefinitionId();
                if (!this.FuelConverterDefinition.FuelId.IsNull())
                {
                    defId = thrustProperties.FuelConverter.FuelId;
                }
                MyGasProperties definition = null;
                if (MyFakes.ENABLE_HYDROGEN_FUEL)
                {
                    MyDefinitionManager.Static.TryGetDefinition<MyGasProperties>(defId, out definition);
                }
                MyGasProperties properties2 = definition;
                if (definition == null)
                {
                    MyGasProperties local1 = definition;
                    MyGasProperties properties1 = new MyGasProperties();
                    properties1.Id = MyResourceDistributorComponent.ElectricityId;
                    properties1.EnergyDensity = 1f;
                    properties2 = properties1;
                }
                this.FuelDefinition = properties2;
                this.ForceMagnitude = thrustProperties.ForceMagnitude;
                this.MinPowerConsumption = thrustProperties.MinPowerConsumption;
                this.MaxPowerConsumption = thrustProperties.MaxPowerConsumption;
                this.MinPlanetaryInfluence = thrustProperties.MinPlanetaryInfluence;
                this.MaxPlanetaryInfluence = thrustProperties.MaxPlanetaryInfluence;
                this.EffectivenessAtMinInfluence = thrustProperties.EffectivenessAtMinInfluence;
                this.EffectivenessAtMaxInfluence = thrustProperties.EffectivenessAtMaxInfluence;
                this.NeedsAtmosphereForInfluence = thrustProperties.NeedsAtmosphereForInfluence;
                this.ConsumptionFactorPerG = thrustProperties.ConsumptionFactorPerG;
                MyEntityThrustComponent component = new MyJetpackThrustComponent();
                component.Init();
                base.Character.Components.Add<MyEntityThrustComponent>(component);
                this.ThrustComp.DampenersEnabled = characterBuilder.DampenersEnabled;
                foreach (Vector3I vectori in Base6Directions.IntDirections)
                {
                    this.ThrustComp.Register(base.Character, vectori, null);
                }
                component.ResourceSink(base.Character).TemporaryConnectedEntity = base.Character;
                base.Character.SuitRechargeDistributor.AddSink(component.ResourceSink(base.Character));
                this.TurnOnJetpack(characterBuilder.JetpackEnabled, true, true);
            }
        }

        public unsafe void MoveAndRotate(ref Vector3 moveIndicator, ref Vector2 rotationIndicator, float roll, bool canRotate)
        {
            MyCharacterProxy characterProxy = base.Character.Physics.CharacterProxy;
            this.ThrustComp.ControlThrust = Vector3.Zero;
            base.Character.SwitchAnimation(MyCharacterMovementEnum.Flying, true);
            base.Character.SetCurrentMovementState(MyCharacterMovementEnum.Flying);
            MyCharacterMovementFlags movementFlags = base.Character.MovementFlags;
            MyCharacterMovementFlags flags2 = base.Character.MovementFlags;
            this.IsFlying = !(moveIndicator.LengthSquared() == 0f);
            HkCharacterStateType type = (characterProxy != null) ? characterProxy.GetState() : HkCharacterStateType.HK_CHARACTER_ON_GROUND;
            if ((type == HkCharacterStateType.HK_CHARACTER_IN_AIR) || (type == ((HkCharacterStateType) 5)))
            {
                base.Character.PlayCharacterAnimation("Jetpack", MyBlendOption.Immediate, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                base.Character.CanJump = true;
            }
            MatrixD worldMatrix = base.Character.WorldMatrix;
            if (canRotate)
            {
                MatrixD identity = MatrixD.Identity;
                MatrixD xd3 = MatrixD.Identity;
                MatrixD xd4 = MatrixD.Identity;
                if (Math.Abs(rotationIndicator.X) > float.Epsilon)
                {
                    if (base.Character.Definition.VerticalPositionFlyingOnly)
                    {
                        base.Character.SetHeadLocalXAngle(base.Character.HeadLocalXAngle - (rotationIndicator.X * base.Character.RotationSpeed));
                    }
                    else
                    {
                        identity = MatrixD.CreateFromAxisAngle(worldMatrix.Right, (double) ((-rotationIndicator.X * base.Character.RotationSpeed) * 0.02f));
                    }
                }
                if (Math.Abs(rotationIndicator.Y) > float.Epsilon)
                {
                    xd3 = MatrixD.CreateFromAxisAngle(worldMatrix.Up, (double) ((-rotationIndicator.Y * base.Character.RotationSpeed) * 0.02f));
                }
                if (!base.Character.Definition.VerticalPositionFlyingOnly && (Math.Abs(roll) > float.Epsilon))
                {
                    xd4 = MatrixD.CreateFromAxisAngle(worldMatrix.Forward, (double) (roll * 0.02f));
                }
                float y = base.Character.ModelCollision.BoundingBoxSizeHalf.Y;
                MatrixD xd7 = worldMatrix.GetOrientation() * ((identity * xd3) * xd4);
                MatrixD* xdPtr1 = (MatrixD*) ref xd7;
                xdPtr1.Translation = (base.Character.Physics.GetWorldMatrix().Translation + (worldMatrix.Up * y)) - (xd7.Up * y);
                base.Character.WorldMatrix = xd7;
                base.Character.ClearShapeContactPoints();
            }
            Vector3 position = moveIndicator;
            if (base.Character.Definition.VerticalPositionFlyingOnly)
            {
                float num2 = Math.Sign(base.Character.HeadLocalXAngle);
                double x = Math.Abs(MathHelper.ToRadians(base.Character.HeadLocalXAngle));
                double y = 1.95;
                double num5 = Math.Pow(x, y) * (x / Math.Pow((double) MathHelper.ToRadians((float) 89f), y));
                position = (Vector3) Vector3D.Transform(position, MatrixD.CreateFromAxisAngle(Vector3D.Right, num2 * num5));
            }
            if (!Vector3.IsZero(position))
            {
                position.Normalize();
            }
            MyJetpackThrustComponent thrustComp = this.ThrustComp;
            thrustComp.ControlThrust += position * this.ForceMagnitude;
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            base.NeedsUpdateSimulation = true;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            if (!base.Entity.MarkedForClose)
            {
                base.Character.SuitRechargeDistributor.RemoveSink(this.ThrustComp.ResourceSink(base.Character), true, base.Entity.MarkedForClose);
                base.OnBeforeRemovedFromContainer();
            }
        }

        public override void OnCharacterDead()
        {
            base.OnCharacterDead();
            this.TurnOnJetpack(false, false, false);
        }

        public override void Simulate()
        {
            this.ThrustComp.UpdateBeforeSimulation(Sync.IsServer || ReferenceEquals(base.Character, MySession.Static.LocalCharacter), base.Character.RelativeDampeningEntity);
        }

        public void SwitchDamping()
        {
            if (base.Character.GetCurrentMovementState() != MyCharacterMovementEnum.Died)
            {
                this.EnableDampeners(!this.DampenersTurnedOn);
            }
        }

        public void SwitchThrusts()
        {
            if ((base.Character.GetCurrentMovementState() != MyCharacterMovementEnum.Died) && (((MyPerGameSettings.Game != GameEnum.ME_GAME) || (!MySession.Static.SurvivalMode || MySession.Static.CreativeToolsEnabled(base.Character.ControllerInfo.Controller.Player.Id.SteamId))) || MySession.Static.CreativeToolsEnabled(base.Character.ControlSteamId)))
            {
                this.TurnOnJetpack(!this.TurnedOn, false, false);
            }
        }

        public void TurnOnJetpack(bool newState, bool fromInit = false, bool fromLoad = false)
        {
            int num1;
            int num2;
            MyEntityController controller = base.Character.ControllerInfo.Controller;
            newState = newState && MySession.Static.Settings.EnableJetpack;
            newState = newState && (base.Character.Definition.Jetpack != null);
            if (!newState)
            {
                num1 = 0;
            }
            else if ((!MySession.Static.SurvivalMode || MyFakes.ENABLE_JETPACK_IN_SURVIVAL) || (controller == null))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) MySession.Static.CreativeToolsEnabled(controller.Player.Id.SteamId);
            }
            newState = (bool) num2;
            bool flag = this.TurnedOn != newState;
            this.TurnedOn = newState;
            this.ThrustComp.Enabled = newState;
            this.ThrustComp.ControlThrust = Vector3.Zero;
            this.ThrustComp.MarkDirty(false);
            this.ThrustComp.UpdateBeforeSimulation(true, base.Character.RelativeDampeningEntity);
            if (!this.ThrustComp.Enabled)
            {
                this.ThrustComp.SetRequiredFuelInput(ref this.FuelDefinition.Id, 0f, null);
            }
            this.ThrustComp.ResourceSink(base.Character).Update();
            if ((base.Character.ControllerInfo.IsLocallyControlled() || fromInit) || Sync.IsServer)
            {
                MyCharacterMovementEnum currentMovementState = base.Character.GetCurrentMovementState();
                if (currentMovementState != MyCharacterMovementEnum.Sitting)
                {
                    if (this.TurnedOn)
                    {
                        base.Character.StopFalling();
                    }
                    bool flag2 = false;
                    bool flag3 = newState;
                    if ((!this.IsPowered & flag3) && (((base.Character.ControllerInfo.Controller != null) && !MySession.Static.CreativeToolsEnabled(base.Character.ControllerInfo.Controller.Player.Id.SteamId)) || (!ReferenceEquals(MySession.Static.LocalCharacter, base.Character) && !Sync.IsServer)))
                    {
                        flag3 = false;
                        flag2 = true;
                    }
                    if (flag3)
                    {
                        if (base.Character.IsOnLadder)
                        {
                            base.Character.GetOffLadder();
                        }
                        base.Character.IsUsing = null;
                    }
                    if (flag && !base.Character.IsDead)
                    {
                        base.Character.UpdateCharacterPhysics(false);
                    }
                    if ((ReferenceEquals(MySession.Static.ControlledEntity, base.Character) & flag) && !fromLoad)
                    {
                        if (flag3)
                        {
                            MyAnalyticsHelper.ReportActivityStart(base.Character, "jetpack", "character", string.Empty, string.Empty, true);
                        }
                        else
                        {
                            MyAnalyticsHelper.ReportActivityEnd(base.Character, "jetpack");
                        }
                        if (flag2)
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                            this.TurnedOn = false;
                            this.ThrustComp.Enabled = false;
                            this.ThrustComp.ControlThrust = Vector3.Zero;
                            this.ThrustComp.MarkDirty(false);
                            this.ThrustComp.UpdateBeforeSimulation(true, base.Character.RelativeDampeningEntity);
                            this.ThrustComp.SetRequiredFuelInput(ref this.FuelDefinition.Id, 0f, null);
                            this.ThrustComp.ResourceSink(base.Character).Update();
                        }
                    }
                    MyCharacterProxy characterProxy = base.Character.Physics.CharacterProxy;
                    if (characterProxy == null)
                    {
                        if (this.Running && (currentMovementState != MyCharacterMovementEnum.Died))
                        {
                            base.Character.PlayCharacterAnimation("Jetpack", MyBlendOption.Immediate, MyFrameOption.Loop, 0f, 1f, false, null, false);
                            base.Character.SetLocalHeadAnimation(0f, 0f, 0.3f);
                        }
                    }
                    else
                    {
                        MatrixD worldMatrix = base.Character.WorldMatrix;
                        characterProxy.SetForwardAndUp((Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
                        characterProxy.EnableFlyingState(this.Running);
                        if ((currentMovementState != MyCharacterMovementEnum.Died) && !base.Character.IsOnLadder)
                        {
                            if (!this.Running && ((characterProxy.GetState() == HkCharacterStateType.HK_CHARACTER_IN_AIR) || (characterProxy.GetState() == ((HkCharacterStateType) 5))))
                            {
                                base.Character.StartFalling();
                            }
                            else if ((currentMovementState != MyCharacterMovementEnum.Standing) && !newState)
                            {
                                base.Character.PlayCharacterAnimation("Idle", MyBlendOption.Immediate, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                                base.Character.SetCurrentMovementState(MyCharacterMovementEnum.Standing);
                                currentMovementState = base.Character.GetCurrentMovementState();
                            }
                        }
                        if (this.Running && (currentMovementState != MyCharacterMovementEnum.Died))
                        {
                            base.Character.PlayCharacterAnimation("Jetpack", MyBlendOption.Immediate, MyFrameOption.Loop, 0f, 1f, false, null, false);
                            base.Character.SetCurrentMovementState(MyCharacterMovementEnum.Flying);
                            base.Character.SetLocalHeadAnimation(0f, 0f, 0.3f);
                            characterProxy.PosX = 0f;
                            characterProxy.PosY = 0f;
                        }
                        if ((!fromLoad && !newState) && (base.Character.Physics.Gravity.LengthSquared() <= 0.1f))
                        {
                            this.CurrentAutoEnableDelay = -1f;
                        }
                    }
                }
            }
        }

        public void UpdateFall()
        {
            if (this.CurrentAutoEnableDelay >= 1f)
            {
                this.ThrustComp.DampenersEnabled = true;
                this.TurnOnJetpack(true, false, false);
                this.CurrentAutoEnableDelay = -1f;
            }
        }

        public bool UpdatePhysicalMovement()
        {
            if (!this.Running)
            {
                return false;
            }
            MyPhysicsBody physics = base.Character.Physics;
            MyCharacterProxy characterProxy = physics.CharacterProxy;
            if ((characterProxy != null) && (characterProxy.LinearVelocity.Length() < 0.001f))
            {
                characterProxy.LinearVelocity = Vector3.Zero;
            }
            float num = 1f;
            HkRigidBody rigidBody = physics.RigidBody;
            if (rigidBody != null)
            {
                rigidBody.Gravity = Vector3.Zero;
                if (MySession.Static.SurvivalMode || MyFakes.ENABLE_PLANETS_JETPACK_LIMIT_IN_CREATIVE)
                {
                    Vector3 vector2 = (Vector3) (num * MyGravityProviderSystem.CalculateNaturalGravityInPoint(base.Character.PositionComp.WorldAABB.Center));
                    if (vector2 != Vector3.Zero)
                    {
                        rigidBody.Gravity = vector2 * MyPerGameSettings.CharacterGravityMultiplier;
                    }
                }
                return true;
            }
            if (characterProxy == null)
            {
                return false;
            }
            characterProxy.Gravity = Vector3.Zero;
            if (MySession.Static.SurvivalMode || MyFakes.ENABLE_PLANETS_JETPACK_LIMIT_IN_CREATIVE)
            {
                Vector3 vector3 = (Vector3) (num * MyGravityProviderSystem.CalculateNaturalGravityInPoint(base.Character.PositionComp.WorldAABB.Center));
                if (vector3 != Vector3.Zero)
                {
                    characterProxy.Gravity = vector3 * MyPerGameSettings.CharacterGravityMultiplier;
                }
            }
            return true;
        }

        private MyJetpackThrustComponent ThrustComp =>
            (base.Character.Components.Get<MyEntityThrustComponent>() as MyJetpackThrustComponent);

        public float CurrentAutoEnableDelay { get; set; }

        public float ForceMagnitude { get; private set; }

        public float MinPowerConsumption { get; private set; }

        public float MaxPowerConsumption { get; private set; }

        public Vector3 FinalThrust =>
            this.ThrustComp.FinalThrust;

        public bool CanDrawThrusts =>
            (base.Character.ActualUpdateFrame >= 2L);

        public bool DampenersTurnedOn =>
            this.ThrustComp.DampenersEnabled;

        public MyGasProperties FuelDefinition { get; private set; }

        public MyFuelConverterInfo FuelConverterDefinition { get; private set; }

        public bool IsPowered
        {
            get
            {
                if (((!ReferenceEquals(MySession.Static.LocalCharacter, base.Character) && !Sync.IsServer) || (base.Character.ControllerInfo.Controller == null)) || !MySession.Static.CreativeToolsEnabled(base.Character.ControllerInfo.Controller.Player.Id.SteamId))
                {
                    return (MySession.Static.CreativeToolsEnabled(base.Character.ControlSteamId) || ((this.ThrustComp != null) && this.ThrustComp.IsThrustPoweredByType(base.Character, ref this.FuelDefinition.Id)));
                }
                return true;
            }
        }

        public bool DampenersEnabled =>
            ((this.ThrustComp != null) && this.ThrustComp.DampenersEnabled);

        public bool Running =>
            (this.TurnedOn && (this.IsPowered && !base.Character.IsDead));

        public bool TurnedOn { get; private set; }

        public float MinPlanetaryInfluence { get; private set; }

        public float MaxPlanetaryInfluence { get; private set; }

        public float EffectivenessAtMaxInfluence { get; private set; }

        public float EffectivenessAtMinInfluence { get; private set; }

        public bool NeedsAtmosphereForInfluence { get; private set; }

        public float ConsumptionFactorPerG { get; private set; }

        public bool IsFlying { get; private set; }

        public override string ComponentTypeDebugString =>
            "Jetpack Component";
    }
}

