namespace Sandbox.Game.Weapons
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Debugging;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_Drill)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyShipDrill), typeof(Sandbox.ModAPI.Ingame.IMyShipDrill) })]
    public class MyShipDrill : MyFunctionalBlock, IMyGunObject<MyToolBase>, IMyInventoryOwner, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyShipDrill, Sandbox.ModAPI.Ingame.IMyShipDrill, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity
    {
        public const float HEAD_MAX_ROTATION_SPEED = 12.56637f;
        public const float HEAD_SLOWDOWN_TIME_IN_SECONDS = 0.5f;
        public const float DRILL_RANGE_SQ = 0.9604f;
        private static int m_countdownDistributor;
        private int m_blockLength;
        private float m_cubeSideLength;
        private MyDefinitionId m_defId;
        private int m_headLastUpdateTime;
        private bool m_isControlled;
        private MyDrillBase m_drillBase;
        private int m_drillFrameCountdown = 90;
        private bool m_wantsToDrill;
        private bool m_wantsToCollect;
        private MyDrillHead m_drillHeadEntity;
        private MyCharacter m_owner;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private IMyConveyorEndpoint m_multilineConveyorEndpoint;
        private float m_drillMultiplier = 1f;
        private float m_powerConsumptionMultiplier = 1f;

        public MyShipDrill()
        {
            base.Render = new MyShipDrillRenderComponent();
            this.CreateTerminalControls();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            this.SetupDrillFrameCountdown();
            base.NeedsWorldMatrix = true;
        }

        public bool AllowSelfPulling() => 
            false;

        private void ApplyShakeForce(float standbyRotationRatio = 1f)
        {
            MyGridPhysics physics = base.CubeGrid.Physics;
            MyPositionComponentBase positionComp = base.PositionComp;
            if ((physics != null) && (positionComp != null))
            {
                float num = (base.CubeGrid.GridSizeEnum == MyCubeSize.Small) ? 1f : 5f;
                MatrixD worldMatrix = base.WorldMatrix;
                float num2 = (float) MyPerformanceCounter.TicksToMs(MyPerformanceCounter.ElapsedTicks);
                float num3 = this.GetHashCode() + num2;
                Vector3? torque = null;
                float? maxSpeed = null;
                physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?(((Vector3.Zero + (worldMatrix.Up * Math.Sin((double) ((num3 * 13.35f) / 5f)))) + (worldMatrix.Right * Math.Sin((double) ((num3 * 18.154f) / 5f)))) * ((((standbyRotationRatio * num) * 240f) * this.m_drillBase.AnimationMaxSpeedRatio) * this.m_drillBase.AnimationMaxSpeedRatio)), new Vector3D?(positionComp.GetPosition()), torque, maxSpeed, true, false);
            }
        }

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            status = MyGunStatusEnum.OK;
            if ((action != MyShootActionEnum.PrimaryAction) && (action != MyShootActionEnum.SecondaryAction))
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (!base.IsFunctional)
            {
                status = MyGunStatusEnum.NotFunctional;
                return false;
            }
            if (!base.HasPlayerAccess(shooter))
            {
                status = MyGunStatusEnum.AccessDenied;
                return false;
            }
            if (MySessionComponentSafeZones.IsActionAllowed(base.CubeGrid, MySafeZoneAction.Drilling, shooter))
            {
                return true;
            }
            status = MyGunStatusEnum.Failed;
            return false;
        }

        private void CheckDustEffect()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((this.m_drillBase.DustParticles != null) && !this.m_drillBase.DustParticles.IsEmittingStopped))
            {
                float maxValue = float.MaxValue;
                Vector3D zero = Vector3D.Zero;
                Vector3D center = this.m_drillBase.Sensor.Center;
                foreach (KeyValuePair<long, MyDrillSensorBase.DetectionInfo> pair in this.m_drillBase.Sensor.CachedEntitiesInRange)
                {
                    MyDrillSensorBase.DetectionInfo info = pair.Value;
                    if (info.Entity is MyVoxelBase)
                    {
                        float num2 = Vector3.DistanceSquared((Vector3) info.DetectionPoint, (Vector3) center);
                        if (num2 < maxValue)
                        {
                            maxValue = num2;
                            zero = info.DetectionPoint;
                        }
                    }
                }
                if (maxValue != float.MaxValue)
                {
                    MatrixD worldMatrix = base.PositionComp.WorldMatrix;
                    Vector3D vectord1 = (zero + center) / 2.0;
                }
            }
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private bool CheckPlayerControl()
        {
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if ((localHumanPlayer == null) || (localHumanPlayer.Controller == null))
            {
                return false;
            }
            MyCubeBlock controlledEntity = localHumanPlayer.Controller.ControlledEntity as MyCubeBlock;
            return ((controlledEntity != null) && (!(controlledEntity is MyRemoteControl) && ReferenceEquals(controlledEntity.CubeGrid, base.CubeGrid)));
        }

        protected override void Closing()
        {
            base.Closing();
            this.m_drillBase.Close();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        private Vector3 ComputeDrillSensorCenter() => 
            ((Vector3) (((base.WorldMatrix.Forward * (this.m_blockLength - 2)) * this.m_cubeSideLength) + base.WorldMatrix.Translation));

        private float ComputeMaxRequiredPower() => 
            (0.002f * this.m_powerConsumptionMultiplier);

        private float ComputeRequiredPower()
        {
            MyGunStatusEnum enum2;
            if ((!base.IsFunctional || !this.CanShoot(MyShootActionEnum.PrimaryAction, base.OwnerId, out enum2)) || (!base.Enabled && !this.WantsToDrill))
            {
                return 0f;
            }
            return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyShipDrill>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyShipDrill> switch1 = new MyTerminalControlOnOffSwitch<MyShipDrill>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyShipDrill> switch2 = new MyTerminalControlOnOffSwitch<MyShipDrill>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyShipDrill> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyShipDrill> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyShipDrill> onOff = local5;
                onOff.EnableToggleAction<MyShipDrill>();
                MyTerminalControlFactory.AddControl<MyShipDrill>(onOff);
            }
        }

        public Vector3 DirectionToTarget(Vector3D target)
        {
            throw new NotImplementedException();
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyHud.BlockInfo.Visible = true;
            MyHud.BlockInfo.MissingComponentIndex = -1;
            MyHud.BlockInfo.BlockName = base.BlockDefinition.DisplayNameText;
            MyHud.BlockInfo.SetContextHelp(base.BlockDefinition);
            MyHud.BlockInfo.PCUCost = 0;
            MyHud.BlockInfo.BlockIcons = base.BlockDefinition.Icons;
            MyHud.BlockInfo.BlockIntegrity = 1f;
            MyHud.BlockInfo.CriticalIntegrity = 0f;
            MyHud.BlockInfo.CriticalComponentIndex = 0;
            MyHud.BlockInfo.OwnershipIntegrity = 0f;
            MyHud.BlockInfo.BlockBuiltBy = 0L;
            MyHud.BlockInfo.GridSize = MyCubeSize.Small;
            MyHud.BlockInfo.Components.Clear();
        }

        public void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate)
        {
            this.DrawHud(camera, playerId);
        }

        public void EndShoot(MyShootActionEnum action)
        {
            this.WantsToDrill = false;
            base.ResourceSink.Update();
        }

        public int GetAmmunitionAmount() => 
            0;

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Drill objectBuilderCubeBlock = (MyObjectBuilder_Drill) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = information1.Inventory.Constraint;
            return information1;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            this.m_defId = builder.GetId();
            MyShipDrillDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(this.m_defId) as MyShipDrillDefinition;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(cubeBlockDefinition.ResourceSinkGroup, this.ComputeMaxRequiredPower(), new Func<float>(this.ComputeRequiredPower));
            base.ResourceSink = component;
            this.m_drillBase = new MyDrillBase(this, "Smoke_DrillDust", "Smoke_DrillDust", "Smoke_DrillDust_Metal", new MyDrillSensorSphere(cubeBlockDefinition.SensorRadius, cubeBlockDefinition.SensorOffset, base.BlockDefinition), new MyDrillCutOut(cubeBlockDefinition.CutOutOffset, cubeBlockDefinition.CutOutRadius), 0.5f, -0.4f, 0.4f, 1f, delegate (float amount, string typeId, string subtypeId) {
                if (MyVisualScriptLogicProvider.ShipDrillCollected != null)
                {
                    MyVisualScriptLogicProvider.ShipDrillCollected(base.Name, base.EntityId, base.CubeGrid.Name, base.CubeGrid.EntityId, typeId, subtypeId, amount);
                }
            });
            base.Init(builder, cubeGrid);
            this.m_blockLength = cubeBlockDefinition.Size.Z;
            this.m_cubeSideLength = MyDefinitionManager.Static.GetCubeSize(cubeBlockDefinition.CubeSize);
            float maxVolume = (((((cubeBlockDefinition.Size.X * cubeBlockDefinition.Size.Y) * cubeBlockDefinition.Size.Z) * this.m_cubeSideLength) * this.m_cubeSideLength) * this.m_cubeSideLength) * 0.5f;
            Vector3 size = new Vector3((float) cubeBlockDefinition.Size.X, (float) cubeBlockDefinition.Size.Y, cubeBlockDefinition.Size.Z * 0.5f);
            if (this.GetInventory(0) == null)
            {
                MyInventory inventory = new MyInventory(maxVolume, size, MyInventoryFlags.CanSend);
                base.Components.Add<MyInventoryBase>(inventory);
            }
            this.GetInventory(0).Constraint = new MyInventoryConstraint(MySpaceTexts.ToolTipItemFilter_AnyOre, null, true).AddObjectBuilderType(typeof(MyObjectBuilder_Ore));
            this.m_drillBase.OutputInventory = this.GetInventory(0);
            this.m_drillBase.IgnoredEntities.Add(this);
            this.m_drillBase.IgnoredEntities.Add(cubeGrid);
            this.m_drillBase.UpdatePosition(base.WorldMatrix);
            this.m_wantsToCollect = false;
            base.AddDebugRenderComponent(new MyDebugRenderCompomentDrawDrillBase(this.m_drillBase));
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink.Update();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerReciever(base.ResourceSink, this));
            MyObjectBuilder_Drill drill = (MyObjectBuilder_Drill) builder;
            this.GetInventory(0).Init(drill.Inventory);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            this.m_useConveyorSystem.SetLocalValue(drill.UseConveyorSystem);
            this.UpdateDetailedInfo();
            this.m_wantsToDrill = drill.Enabled;
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.OnIsWorkingChanged);
            this.m_drillBase.m_drillMaterial = MyStringHash.GetOrCompute("ShipDrill");
            base.m_baseIdleSound = cubeBlockDefinition.PrimarySound;
            this.m_drillBase.m_idleSoundLoop = base.m_baseIdleSound;
            this.m_drillBase.ParticleOffset = cubeBlockDefinition.ParticleOffset;
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_multilineConveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_multilineConveyorEndpoint));
        }

        protected override MyEntitySubpart InstantiateSubpart(MyModelDummy subpartDummy, ref MyEntitySubpart.Data data)
        {
            this.m_drillHeadEntity = new MyDrillHead(this);
            this.m_drillHeadEntity.OnClosing += delegate (VRage.Game.Entity.MyEntity x) {
                MyDrillHead head = x as MyDrillHead;
                if ((head != null) && (head.DrillParent != null))
                {
                    head.DrillParent.UnregisterHead(head);
                }
            };
            return this.m_drillHeadEntity;
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (this.m_drillHeadEntity != null)
            {
                this.m_drillHeadEntity.Render.UpdateSpeed(0f);
            }
        }

        public void OnControlAcquired(MyCharacter owner)
        {
            this.m_owner = owner;
            this.m_isControlled = true;
        }

        public void OnControlReleased()
        {
            this.m_owner = null;
            this.m_isControlled = false;
            if (!base.Enabled)
            {
                this.m_drillBase.StopDrill();
            }
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            base.OnCubeGridChanged(oldGrid);
            this.m_drillBase.IgnoredEntities.Remove(oldGrid);
            this.m_drillBase.IgnoredEntities.Add(base.CubeGrid);
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            this.WantsToDrill = base.Enabled;
            base.OnEnabledChanged();
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
        }

        private void OnIsWorkingChanged(MyCubeBlock obj)
        {
            this.WantstoDrillChanged();
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            this.WantstoDrillChanged();
        }

        private void SetupDrillFrameCountdown()
        {
            m_countdownDistributor += 10;
            if (m_countdownDistributor > 10)
            {
                m_countdownDistributor = -10;
            }
            this.m_drillFrameCountdown = 90 + m_countdownDistributor;
        }

        public void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if ((action == MyShootActionEnum.PrimaryAction) || (action == MyShootActionEnum.SecondaryAction))
            {
                this.ShakeAmount = 0.5f;
                this.m_wantsToCollect = action == MyShootActionEnum.PrimaryAction;
                this.WantsToDrill = true;
            }
        }

        public void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public bool SupressShootAnimation() => 
            false;

        public void UnregisterHead(MyDrillHead head)
        {
            if (ReferenceEquals(this.m_drillHeadEntity, head))
            {
                this.m_drillHeadEntity = null;
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (!base.CubeGrid.IsPreview)
            {
                MyGunStatusEnum enum2;
                this.m_drillBase.UpdateAfterSimulation();
                if ((!this.WantsToDrill || !this.CanShoot(MyShootActionEnum.PrimaryAction, base.OwnerId, out enum2)) || (this.m_drillBase.AnimationMaxSpeedRatio <= 0f))
                {
                    if (this.m_drillHeadEntity != null)
                    {
                        this.m_drillHeadEntity.Render.UpdateSpeed(0f);
                    }
                    if (!base.HasDamageEffect)
                    {
                        base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                    }
                }
                else
                {
                    if ((this.CheckPlayerControl() && (MySession.Static.CameraController != null)) && (MySession.Static.CameraController.IsInFirstPersonView || MySession.Static.CameraController.ForceFirstPersonCamera))
                    {
                        this.m_drillBase.PerformCameraShake(this.ShakeAmount);
                    }
                    if ((MySession.Static.EnableToolShake && MyFakes.ENABLE_TOOL_SHAKE) && !base.CubeGrid.Physics.IsStatic)
                    {
                        this.ApplyShakeForce(1f);
                    }
                    if (this.WantsToDrill)
                    {
                        this.CheckDustEffect();
                    }
                    if (this.m_drillHeadEntity != null)
                    {
                        this.m_drillHeadEntity.Render.UpdateSpeed(12.56637f);
                    }
                }
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.ResourceSink.Update();
            base.UpdateAfterSimulation100();
            this.m_drillBase.UpdateSoundEmitter(Vector3.Zero);
            if ((Sync.IsServer && (base.IsFunctional && (this.m_useConveyorSystem != null))) && (this.GetInventory(0).GetItems().Count > 0))
            {
                MyGridConveyorSystem.PushAnyRequest(this, this.GetInventory(0), base.OwnerId);
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            this.Receiver_IsPoweredChanged();
            base.UpdateBeforeSimulation10();
            if ((base.Parent != null) && (base.Parent.Physics != null))
            {
                this.m_drillFrameCountdown -= 10;
                if (this.m_drillFrameCountdown <= 0)
                {
                    MyGunStatusEnum enum2;
                    this.m_drillFrameCountdown += 90;
                    if (!this.CanShoot(MyShootActionEnum.PrimaryAction, base.OwnerId, out enum2))
                    {
                        this.ShakeAmount = 0f;
                    }
                    else if (this.m_drillBase.Drill(base.Enabled || this.m_wantsToCollect, true, false, 0.1f))
                    {
                        this.ShakeAmount = 1f;
                    }
                    else
                    {
                        this.ShakeAmount = 0.5f;
                    }
                }
            }
        }

        private void UpdateDetailedInfo()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(base.BlockDefinition.DisplayNameText);
            base.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            base.RaisePropertiesChanged();
        }

        public void UpdateSoundEmitter()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.Update();
            }
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        private void WantstoDrillChanged()
        {
            base.ResourceSink.Update();
            if (((!base.Enabled && !this.WantsToDrill) || (!base.IsFunctional || (base.ResourceSink == null))) || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
                this.SetupDrillFrameCountdown();
                this.m_drillBase.StopDrill();
            }
            else
            {
                MyGunStatusEnum enum2;
                if (!this.CanShoot(MyShootActionEnum.PrimaryAction, base.OwnerId, out enum2))
                {
                    this.m_drillBase.StopDrill();
                }
                else if (!this.m_drillBase.IsDrilling)
                {
                    this.m_drillBase.Drill(base.Enabled || this.m_wantsToCollect, false, false, 0.1f);
                }
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            if (this.m_drillBase != null)
            {
                this.m_drillBase.UpdatePosition(base.WorldMatrix);
            }
        }

        private bool WantsToDrill
        {
            get => 
                this.m_wantsToDrill;
            set
            {
                this.m_wantsToDrill = value;
                this.WantstoDrillChanged();
            }
        }

        public MyDrillHead DrillHeadEntity =>
            this.m_drillHeadEntity;

        public bool IsDeconstructor =>
            false;

        public MyCharacter Owner =>
            this.m_owner;

        public float BackkickForcePerSecond =>
            0f;

        public float ShakeAmount { get; protected set; }

        public bool EnabledInWorldRules =>
            true;

        public MyDefinitionId DefinitionId =>
            this.m_defId;

        public bool IsSkinnable =>
            false;

        public bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_multilineConveyorEndpoint;

        public bool IsShooting =>
            this.m_drillBase.IsDrilling;

        int IMyGunObject<MyToolBase>.ShootDirectionUpdateTime =>
            0;

        public MyToolBase GunBase =>
            null;

        bool Sandbox.ModAPI.Ingame.IMyShipDrill.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set => 
                (this.UseConveyorSystem = value);
        }

        float Sandbox.ModAPI.IMyShipDrill.DrillHarvestMultiplier
        {
            get => 
                this.m_drillMultiplier;
            set
            {
                this.m_drillMultiplier = value;
                if (this.m_drillBase != null)
                {
                    this.m_drillBase.VoxelHarvestRatio = 0.009f * this.m_drillMultiplier;
                    this.m_drillBase.VoxelHarvestRatio = MathHelper.Clamp(this.m_drillBase.VoxelHarvestRatio, 0f, 1f);
                }
            }
        }

        float Sandbox.ModAPI.IMyShipDrill.PowerConsumptionMultiplier
        {
            get => 
                this.m_powerConsumptionMultiplier;
            set
            {
                this.m_powerConsumptionMultiplier = value;
                if (this.m_powerConsumptionMultiplier < 0.01f)
                {
                    this.m_powerConsumptionMultiplier = 0.01f;
                }
                if (base.ResourceSink != null)
                {
                    base.ResourceSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, this.ComputeMaxRequiredPower() * this.m_powerConsumptionMultiplier);
                    base.ResourceSink.Update();
                    this.UpdateDetailedInfo();
                }
            }
        }

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyShipDrill.<>c <>9 = new MyShipDrill.<>c();
            public static MyTerminalValueControl<MyShipDrill, bool>.GetterDelegate <>9__39_0;
            public static MyTerminalValueControl<MyShipDrill, bool>.SetterDelegate <>9__39_1;
            public static Action<VRage.Game.Entity.MyEntity> <>9__104_0;

            internal bool <CreateTerminalControls>b__39_0(MyShipDrill x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__39_1(MyShipDrill x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal void <InstantiateSubpart>b__104_0(VRage.Game.Entity.MyEntity x)
            {
                MyShipDrill.MyDrillHead head = x as MyShipDrill.MyDrillHead;
                if ((head != null) && (head.DrillParent != null))
                {
                    head.DrillParent.UnregisterHead(head);
                }
            }
        }

        public class MyDrillHead : MyEntitySubpart
        {
            public MyShipDrill DrillParent;

            public MyDrillHead(MyShipDrill parent)
            {
                this.DrillParent = parent;
                base.Render = new MyShipDrillRenderComponent.MyDrillHeadRenderComponent();
                base.InvalidateOnMove = false;
                base.NeedsWorldMatrix = false;
            }

            public MyShipDrillRenderComponent.MyDrillHeadRenderComponent Render =>
                ((MyShipDrillRenderComponent.MyDrillHeadRenderComponent) base.Render);
        }
    }
}

