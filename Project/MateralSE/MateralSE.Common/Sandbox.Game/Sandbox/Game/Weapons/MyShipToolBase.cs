namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyShipToolBase), typeof(Sandbox.ModAPI.Ingame.IMyShipToolBase) })]
    public abstract class MyShipToolBase : MyFunctionalBlock, IMyGunObject<MyToolBase>, IMyInventoryOwner, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyShipToolBase, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyShipToolBase
    {
        protected float DEFAULT_REACH_DISTANCE = 4.5f;
        private MyMultilineConveyorEndpoint m_endpoint;
        private MyDefinitionId m_defId;
        private bool m_wantsToActivate;
        private bool m_isActivated;
        protected bool m_isActivatedOnSomething;
        protected int m_lastTimeActivate;
        private int m_shootHeatup;
        private int m_activateCounter;
        private HashSet<VRage.Game.Entity.MyEntity> m_entitiesInContact;
        protected BoundingSphere m_detectorSphere;
        protected bool m_checkEnvironmentSector;
        private HashSet<MySlimBlock> m_blocksToActivateOn;
        private HashSet<MySlimBlock> m_tempBlocksBuffer;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        protected MyCharacter controller;
        private bool m_effectActivated;
        private bool m_animationActivated;

        public MyShipToolBase()
        {
            this.CreateTerminalControls();
        }

        protected abstract bool Activate(HashSet<MySlimBlock> targets);
        private void ActivateCommon()
        {
            BoundingSphereD boundingSphere = new BoundingSphereD(Vector3D.Transform(this.m_detectorSphere.Center, base.CubeGrid.WorldMatrix), (double) this.m_detectorSphere.Radius);
            BoundingSphereD sphere = new BoundingSphereD(boundingSphere.Center, (double) (this.m_detectorSphere.Radius * 0.5f));
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                MyRenderProxy.DebugDrawSphere(boundingSphere.Center, (float) boundingSphere.Radius, Color.Red.ToVector3(), 1f, false, false, true, true);
                MyRenderProxy.DebugDrawSphere(sphere.Center, (float) sphere.Radius, Color.Blue.ToVector3(), 1f, false, false, true, true);
            }
            this.m_isActivatedOnSomething = false;
            List<VRage.Game.Entity.MyEntity> topMostEntitiesInSphere = Sandbox.Game.Entities.MyEntities.GetTopMostEntitiesInSphere(ref boundingSphere);
            bool flag = false;
            this.m_entitiesInContact.Clear();
            foreach (VRage.Game.Entity.MyEntity local1 in topMostEntitiesInSphere)
            {
                if (local1 is MyEnvironmentSector)
                {
                    flag = true;
                }
                VRage.Game.Entity.MyEntity topMostParent = local1.GetTopMostParent(null);
                if (this.CanInteractWith(topMostParent))
                {
                    this.m_entitiesInContact.Add(topMostParent);
                }
            }
            if (this.m_checkEnvironmentSector & flag)
            {
                MyPhysics.HitInfo? nullable = MyPhysics.CastRay(boundingSphere.Center, boundingSphere.Center + (boundingSphere.Radius * base.WorldMatrix.Forward), 0x18);
                if ((nullable != null) && (nullable != null))
                {
                    VRage.ModAPI.IMyEntity hitEntity = nullable.Value.HkHitInfo.GetHitEntity();
                    if (hitEntity is MyEnvironmentSector)
                    {
                        MyEnvironmentSector sector = hitEntity as MyEnvironmentSector;
                        int itemFromShapeKey = sector.GetItemFromShapeKey(nullable.Value.HkHitInfo.GetShapeKey(0));
                        if (sector.DataView.Items[itemFromShapeKey].ModelIndex >= 0)
                        {
                            Vector3D hitnormal = base.CubeGrid.WorldMatrix.Right + base.CubeGrid.WorldMatrix.Forward;
                            hitnormal.Normalize();
                            float mass = base.CubeGrid.Physics.Mass;
                            sector.GetModule<MyBreakableEnvironmentProxy>().BreakAt(itemFromShapeKey, nullable.Value.HkHitInfo.Position, hitnormal, (double) ((10f * 10f) * mass));
                        }
                    }
                }
            }
            topMostEntitiesInSphere.Clear();
            foreach (VRage.Game.Entity.MyEntity local2 in this.m_entitiesInContact)
            {
                MyCubeGrid grid = local2 as MyCubeGrid;
                MyCharacter character = local2 as MyCharacter;
                if (grid != null)
                {
                    this.m_tempBlocksBuffer.Clear();
                    grid.GetBlocksInsideSphere(ref boundingSphere, this.m_tempBlocksBuffer, true);
                    this.m_blocksToActivateOn.UnionWith(this.m_tempBlocksBuffer);
                }
                if ((character != null) && Sync.IsServer)
                {
                    MyStringHash drill = MyDamageType.Drill;
                    if (this is Sandbox.ModAPI.IMyShipGrinder)
                    {
                        drill = MyDamageType.Grind;
                    }
                    else if (this is Sandbox.ModAPI.IMyShipWelder)
                    {
                        drill = MyDamageType.Weld;
                    }
                    MyOrientedBoundingBoxD xd2 = new MyOrientedBoundingBoxD(character.PositionComp.LocalAABB, character.PositionComp.WorldMatrix);
                    if (xd2.Intersects(ref sphere))
                    {
                        character.DoDamage(20f, drill, true, base.EntityId);
                    }
                }
            }
            this.m_isActivatedOnSomething |= this.Activate(this.m_blocksToActivateOn);
            this.m_activateCounter++;
            this.m_lastTimeActivate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.PlayLoopSound(this.m_isActivatedOnSomething);
            this.m_blocksToActivateOn.Clear();
        }

        public bool AllowSelfPulling() => 
            false;

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public void BeginShoot(MyShootActionEnum action)
        {
        }

        private bool CanInteractWith(VRage.ModAPI.IMyEntity entity) => 
            (((entity != null) && (!ReferenceEquals(entity, base.CubeGrid) || this.CanInteractWithSelf)) && ((entity is MyCubeGrid) || (entity is MyCharacter)));

        public virtual bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            status = MyGunStatusEnum.OK;
            if (action != MyShootActionEnum.PrimaryAction)
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
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeActivate) >= 250)
            {
                return true;
            }
            status = MyGunStatusEnum.Cooldown;
            return false;
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            base.Closing();
            this.StopEffects();
            this.StopLoopSound();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        private float ComputeRequiredPower()
        {
            if (!base.IsFunctional || (!base.Enabled && !this.WantsToActivate))
            {
                return 0f;
            }
            return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyShipToolBase>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyShipToolBase> switch1 = new MyTerminalControlOnOffSwitch<MyShipToolBase>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyShipToolBase> switch2 = new MyTerminalControlOnOffSwitch<MyShipToolBase>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyShipToolBase> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyShipToolBase> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyShipToolBase> onOff = local5;
                onOff.EnableToggleAction<MyShipToolBase>();
                MyTerminalControlFactory.AddControl<MyShipToolBase>(onOff);
            }
        }

        public Vector3 DirectionToTarget(Vector3D target)
        {
            throw new NotImplementedException();
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
        }

        public void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate)
        {
            this.DrawHud(camera, playerId);
        }

        public void EndShoot(MyShootActionEnum action)
        {
            if ((action == MyShootActionEnum.PrimaryAction) && !base.Enabled)
            {
                this.StopShooting();
            }
        }

        public int GetAmmunitionAmount()
        {
            throw new NotImplementedException();
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ShipToolBase objectBuilderCubeBlock = (MyObjectBuilder_ShipToolBase) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = this.UseConveyorSystem;
            objectBuilderCubeBlock.CheckEnvironmentSector = this.m_checkEnvironmentSector;
            return objectBuilderCubeBlock;
        }

        public virtual PullInformation GetPullInformation() => 
            null;

        public virtual PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(MyStringHash.GetOrCompute("Defense"), 0.002f, new Func<float>(this.ComputeRequiredPower));
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            this.m_entitiesInContact = new HashSet<VRage.Game.Entity.MyEntity>();
            this.m_blocksToActivateOn = new HashSet<MySlimBlock>();
            this.m_tempBlocksBuffer = new HashSet<MySlimBlock>();
            this.m_isActivated = false;
            this.m_isActivatedOnSomething = false;
            this.m_wantsToActivate = false;
            this.m_shootHeatup = 0;
            this.m_activateCounter = 0;
            this.m_defId = objectBuilder.GetId();
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(this.m_defId);
            MyObjectBuilder_ShipToolBase base2 = objectBuilder as MyObjectBuilder_ShipToolBase;
            float maxVolume = (((((cubeBlockDefinition.Size.X * cubeGrid.GridSize) * cubeBlockDefinition.Size.Y) * cubeGrid.GridSize) * cubeBlockDefinition.Size.Z) * cubeGrid.GridSize) * 0.5f;
            Vector3 size = new Vector3((float) cubeBlockDefinition.Size.X, (float) cubeBlockDefinition.Size.Y, cubeBlockDefinition.Size.Z * 0.5f);
            if (this.GetInventory(0) == null)
            {
                MyInventory inventory = new MyInventory(maxVolume, size, MyInventoryFlags.CanSend);
                base.Components.Add<MyInventoryBase>(inventory);
                inventory.Init(base2.Inventory);
            }
            base.Enabled = base2.Enabled;
            this.UseConveyorSystem = base2.UseConveyorSystem;
            this.m_checkEnvironmentSector = base2.CheckEnvironmentSector;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            this.LoadDummies();
            this.UpdateActivationState();
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyShipToolBase_IsWorkingChanged);
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_endpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_endpoint));
        }

        private void LoadDummies()
        {
            MyShipToolDefinition blockDefinition = (MyShipToolDefinition) base.BlockDefinition;
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("detector_shiptool"))
                {
                    Matrix matrix = pair.Value.Matrix;
                    matrix.Scale.AbsMin();
                    Matrix matrix3 = matrix * base.PositionComp.LocalMatrix;
                    Vector3 translation = matrix3.Translation;
                    this.m_detectorSphere = new BoundingSphere(translation + (matrix3.Forward * blockDefinition.SensorOffset), blockDefinition.SensorRadius);
                    break;
                }
            }
        }

        private void MyShipToolBase_IsWorkingChanged(MyCubeBlock obj)
        {
            this.UpdateActivationState();
        }

        public override void OnAddedToScene(object source)
        {
            this.LoadDummies();
            base.OnAddedToScene(source);
            this.UpdateActivationState();
        }

        public virtual void OnControlAcquired(MyCharacter owner)
        {
        }

        public virtual void OnControlReleased()
        {
            if (!base.Enabled && !base.Closed)
            {
                this.StopShooting();
            }
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            this.WantsToActivate = base.Enabled;
            base.OnEnabledChanged();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.UpdateActivationState();
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            this.StopEffects();
            this.StopLoopSound();
        }

        protected abstract void PlayLoopSound(bool activated);
        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            this.UpdateActivationState();
        }

        protected void SetBuildingMusic(int amount)
        {
            if (((MySession.Static != null) && ReferenceEquals(this.controller, MySession.Static.LocalCharacter)) && (MyMusicController.Static != null))
            {
                MyMusicController.Static.Building(amount);
            }
        }

        public void SetInventory(MyInventory inventory, int index)
        {
            base.Components.Add<MyInventoryBase>(inventory);
        }

        public void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (action == MyShootActionEnum.PrimaryAction)
            {
                if (this.m_shootHeatup < this.HeatUpFrames)
                {
                    this.m_shootHeatup++;
                }
                else
                {
                    this.WantsToActivate = true;
                    base.ResourceSink.Update();
                }
            }
        }

        public void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        protected virtual void StartAnimation()
        {
        }

        protected abstract void StartEffects();
        protected virtual void StartShooting()
        {
            this.m_isActivated = true;
        }

        protected virtual void StopAnimation()
        {
        }

        protected abstract void StopEffects();
        protected abstract void StopLoopSound();
        protected virtual void StopShooting()
        {
            this.m_wantsToActivate = false;
            this.m_isActivated = false;
            this.m_isActivatedOnSomething = false;
            if (base.Physics != null)
            {
                base.Physics.Enabled = false;
            }
            if (base.ResourceSink != null)
            {
                base.ResourceSink.Update();
            }
            this.m_shootHeatup = 0;
            this.StopEffects();
            this.StopLoopSound();
        }

        public bool SupressShootAnimation() => 
            false;

        private void UpdateActivationState()
        {
            if (base.ResourceSink != null)
            {
                base.ResourceSink.Update();
            }
            if (((!base.Enabled && !this.WantsToActivate) || !base.IsFunctional) || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.StopShooting();
            }
            else
            {
                this.StartShooting();
            }
        }

        public override void UpdateAfterSimulation10()
        {
            MyGunStatusEnum enum2;
            base.UpdateAfterSimulation10();
            if (!this.m_isActivated || !this.CanShoot(MyShootActionEnum.PrimaryAction, base.OwnerId, out enum2))
            {
                if (this.m_animationActivated)
                {
                    this.m_animationActivated = false;
                    this.StopAnimation();
                }
            }
            else
            {
                if (!this.m_animationActivated)
                {
                    this.m_animationActivated = true;
                    this.StartAnimation();
                }
                this.ActivateCommon();
            }
            if (this.m_isActivatedOnSomething || this.m_effectActivated)
            {
                bool flag = Vector3D.DistanceSquared(MySector.MainCamera.Position, base.PositionComp.GetPosition()) < 10000.0;
                if (!this.m_isActivatedOnSomething)
                {
                    goto TR_0002;
                }
                else if (flag)
                {
                    if (this.m_isActivatedOnSomething)
                    {
                        if (!this.m_effectActivated)
                        {
                            this.StartEffects();
                        }
                        this.m_effectActivated = true;
                    }
                }
                else
                {
                    goto TR_0002;
                }
            }
            return;
        TR_0002:
            if (this.m_effectActivated)
            {
                this.StopEffects();
            }
            this.m_effectActivated = false;
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

        protected bool WantsToActivate
        {
            get => 
                this.m_wantsToActivate;
            set
            {
                this.m_wantsToActivate = value;
                this.UpdateActivationState();
            }
        }

        public bool IsHeatingUp =>
            (this.m_shootHeatup > 0);

        public int HeatUpFrames { get; protected set; }

        public bool IsSkinnable =>
            false;

        protected virtual bool CanInteractWithSelf =>
            false;

        public float BackkickForcePerSecond =>
            0f;

        public float ShakeAmount { get; protected set; }

        public MyDefinitionId DefinitionId =>
            this.m_defId;

        public bool EnabledInWorldRules =>
            true;

        public bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_endpoint;

        public bool IsShooting =>
            this.m_isActivated;

        public int ShootDirectionUpdateTime =>
            0;

        public MyToolBase GunBase =>
            null;

        bool Sandbox.ModAPI.Ingame.IMyShipToolBase.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set => 
                (this.UseConveyorSystem = value);
        }

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        long IMyInventoryOwner.EntityId =>
            base.EntityId;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        bool IMyInventoryOwner.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set => 
                (this.UseConveyorSystem = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyShipToolBase.<>c <>9 = new MyShipToolBase.<>c();
            public static MyTerminalValueControl<MyShipToolBase, bool>.GetterDelegate <>9__29_0;
            public static MyTerminalValueControl<MyShipToolBase, bool>.SetterDelegate <>9__29_1;

            internal bool <CreateTerminalControls>b__29_0(MyShipToolBase x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__29_1(MyShipToolBase x, bool v)
            {
                x.UseConveyorSystem = v;
            }
        }
    }
}

