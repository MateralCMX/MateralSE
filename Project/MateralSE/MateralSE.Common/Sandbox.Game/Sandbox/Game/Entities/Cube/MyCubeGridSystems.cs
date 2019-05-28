namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.GameSystems.Electricity;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyCubeGridSystems
    {
        private readonly MyCubeGrid m_cubeGrid;
        private Action<MyBlockGroup> m_terminalSystem_GroupAdded;
        private Action<MyBlockGroup> m_terminalSystem_GroupRemoved;
        private bool m_blocksRegistered;
        private readonly HashSet<MyResourceSinkComponent> m_tmpSinks = new HashSet<MyResourceSinkComponent>();

        public MyCubeGridSystems(MyCubeGrid grid)
        {
            this.m_cubeGrid = grid;
            this.m_terminalSystem_GroupAdded = new Action<MyBlockGroup>(this.TerminalSystem_GroupAdded);
            this.m_terminalSystem_GroupRemoved = new Action<MyBlockGroup>(this.TerminalSystem_GroupRemoved);
            this.GyroSystem = new MyGridGyroSystem(this.m_cubeGrid);
            this.WeaponSystem = new MyGridWeaponSystem();
            this.ReflectorLightSystem = new MyGridReflectorLightSystem(this.m_cubeGrid);
            this.RadioSystem = new MyGridRadioSystem(this.m_cubeGrid);
            if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
            {
                this.WheelSystem = new MyGridWheelSystem(this.m_cubeGrid);
            }
            this.ConveyorSystem = new MyGridConveyorSystem(this.m_cubeGrid);
            this.LandingSystem = new MyGridLandingSystem();
            this.ControlSystem = new MyGroupControlSystem();
            this.CameraSystem = new MyGridCameraSystem(this.m_cubeGrid);
            this.OreDetectorSystem = new MyGridOreDetectorSystem(this.m_cubeGrid);
            if ((Sync.IsServer && MySession.Static.Settings.EnableOxygen) && MySession.Static.Settings.EnableOxygenPressurization)
            {
                this.GasSystem = new MyGridGasSystem(this.m_cubeGrid);
            }
            if (MyPerGameSettings.EnableJumpDrive)
            {
                this.JumpSystem = new MyGridJumpDriveSystem(this.m_cubeGrid);
            }
            if ((MyPerGameSettings.EnableShipSoundSystem && (MyFakes.ENABLE_NEW_SMALL_SHIP_SOUNDS || MyFakes.ENABLE_NEW_LARGE_SHIP_SOUNDS)) && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.ShipSoundComponent = new MyShipSoundComponent();
            }
            this.m_blocksRegistered = true;
        }

        public virtual void AddGroup(MyBlockGroup group)
        {
            if (this.TerminalSystem != null)
            {
                this.TerminalSystem.AddUpdateGroup(group, false, false);
            }
        }

        public virtual void AfterBlockDeserialization()
        {
            this.ConveyorSystem.AfterBlockDeserialization();
            this.ConveyorSystem.ResourceSink.Update();
        }

        public virtual void AfterGridClose()
        {
            this.ConveyorSystem.AfterGridClose();
            if (MyPerGameSettings.EnableJumpDrive)
            {
                this.JumpSystem.AfterGridClose();
            }
            this.m_blocksRegistered = false;
            this.GasSystem = null;
        }

        public virtual void BeforeBlockDeserialization(MyObjectBuilder_CubeGrid builder)
        {
            this.ConveyorSystem.BeforeBlockDeserialization(builder.ConveyorLines);
        }

        public virtual void BeforeGridClose()
        {
            this.ConveyorSystem.IsClosing = true;
            this.ReflectorLightSystem.IsClosing = true;
            this.RadioSystem.IsClosing = true;
            if (this.ShipSoundComponent != null)
            {
                this.ShipSoundComponent.DestroyComponent();
                this.ShipSoundComponent = null;
            }
            if (this.GasSystem != null)
            {
                this.GasSystem.OnGridClosing();
            }
        }

        public virtual void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_TERMINAL_SYSTEMS)
            {
                MyRenderProxy.DebugDrawText3D(this.m_cubeGrid.WorldMatrix.Translation, this.TerminalSystem.GetHashCode().ToString(), Color.NavajoWhite, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_CONVEYORS)
            {
                this.ConveyorSystem.DebugDraw(this.m_cubeGrid);
                this.ConveyorSystem.DebugDrawLinePackets();
            }
            if ((this.GyroSystem != null) && MyDebugDrawSettings.DEBUG_DRAW_GYROS)
            {
                this.GyroSystem.DebugDraw();
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS && (this.ResourceDistributor != null))
            {
                this.ResourceDistributor.DebugDraw(this.m_cubeGrid);
            }
            if (((MySession.Static != null) && ((this.GasSystem != null) && (MySession.Static.Settings.EnableOxygen && MySession.Static.Settings.EnableOxygenPressurization))) && MyDebugDrawSettings.DEBUG_DRAW_OXYGEN)
            {
                this.GasSystem.DebugDraw();
            }
        }

        public virtual void GetObjectBuilder(MyObjectBuilder_CubeGrid ob)
        {
            MyEntityThrustComponent component = this.CubeGrid.Components.Get<MyEntityThrustComponent>();
            ob.DampenersEnabled = (component == null) || component.DampenersEnabled;
            this.ConveyorSystem.SerializeLines(ob.ConveyorLines);
            if (ob.ConveyorLines.Count == 0)
            {
                ob.ConveyorLines = null;
            }
            if (this.WheelSystem != null)
            {
                ob.Handbrake = this.WheelSystem.HandBrake;
            }
            if (((this.GasSystem != null) && MySession.Static.Settings.EnableOxygen) && MySession.Static.Settings.EnableOxygenPressurization)
            {
                ob.OxygenRooms = this.GasSystem.GetOxygenAmount();
            }
            if (MyPerGameSettings.EnableJumpDrive)
            {
                ob.JumpDriveDirection = this.JumpSystem.GetJumpDriveDirection();
                ob.JumpRemainingTime = this.JumpSystem.GetRemainingJumpTime();
            }
        }

        public virtual void Init(MyObjectBuilder_CubeGrid builder)
        {
            MyEntityThrustComponent component = this.CubeGrid.Components.Get<MyEntityThrustComponent>();
            if (component != null)
            {
                component.DampenersEnabled = builder.DampenersEnabled;
            }
            if (this.WheelSystem != null)
            {
                this.m_cubeGrid.SetHandbrakeRequest(builder.Handbrake);
            }
            if (((this.GasSystem != null) && MySession.Static.Settings.EnableOxygen) && MySession.Static.Settings.EnableOxygenPressurization)
            {
                this.GasSystem.Init(builder.OxygenRooms);
            }
            if ((this.ShipSoundComponent != null) && !this.ShipSoundComponent.InitComponent(this.m_cubeGrid))
            {
                this.ShipSoundComponent.DestroyComponent();
                this.ShipSoundComponent = null;
            }
            if (MyPerGameSettings.EnableJumpDrive)
            {
                this.JumpSystem.Init(builder.JumpDriveDirection, builder.JumpRemainingTime);
            }
            MyEntityThrustComponent component2 = this.CubeGrid.Components.Get<MyEntityThrustComponent>();
            if (component2 != null)
            {
                component2.MergeAllGroupsDirty();
            }
        }

        public virtual bool IsTrash() => 
            ((this.ResourceDistributor.ResourceState == MyResourceStateEnum.NoPower) ? !this.ControlSystem.IsControlled : false);

        public virtual void OnAddedToGroup(MyGridLogicalGroupData group)
        {
            this.TerminalSystem = group.TerminalSystem;
            this.ResourceDistributor = group.ResourceDistributor;
            this.WeaponSystem = group.WeaponSystem;
            if (string.IsNullOrEmpty(this.ResourceDistributor.DebugName))
            {
                this.ResourceDistributor.DebugName = this.m_cubeGrid.ToString();
            }
            this.m_cubeGrid.OnFatBlockAdded += new Action<MyCubeBlock>(this.ResourceDistributor.CubeGrid_OnFatBlockAddedOrRemoved);
            this.m_cubeGrid.OnFatBlockRemoved += new Action<MyCubeBlock>(this.ResourceDistributor.CubeGrid_OnFatBlockAddedOrRemoved);
            this.ResourceDistributor.AddSink(this.GyroSystem.ResourceSink);
            this.ResourceDistributor.AddSink(this.ConveyorSystem.ResourceSink);
            this.ConveyorSystem.ResourceSink.IsPoweredChanged += new Action(this.ResourceDistributor.ConveyorSystem_OnPoweredChanged);
            foreach (MyBlockGroup group2 in this.m_cubeGrid.BlockGroups)
            {
                this.TerminalSystem.AddUpdateGroup(group2, false, false);
            }
            this.TerminalSystem.GroupAdded += this.m_terminalSystem_GroupAdded;
            this.TerminalSystem.GroupRemoved += this.m_terminalSystem_GroupRemoved;
            foreach (MyCubeBlock block in this.m_cubeGrid.GetFatBlocks())
            {
                if (!block.MarkedForClose)
                {
                    MyTerminalBlock block2 = block as MyTerminalBlock;
                    if (block2 != null)
                    {
                        this.TerminalSystem.Add(block2);
                    }
                    MyResourceSourceComponent source = block.Components.Get<MyResourceSourceComponent>();
                    if (source != null)
                    {
                        this.ResourceDistributor.AddSource(source);
                    }
                    MyResourceSinkComponent sink = block.Components.Get<MyResourceSinkComponent>();
                    if (sink != null)
                    {
                        this.ResourceDistributor.AddSink(sink);
                    }
                    IMyRechargeSocketOwner owner = block as IMyRechargeSocketOwner;
                    if (owner != null)
                    {
                        owner.RechargeSocket.ResourceDistributor = group.ResourceDistributor;
                    }
                    IMyGunObject<MyDeviceBase> gun = block as IMyGunObject<MyDeviceBase>;
                    if (gun != null)
                    {
                        this.WeaponSystem.Register(gun);
                    }
                }
            }
            MyResourceDistributorComponent resourceDistributor = this.ResourceDistributor;
            resourceDistributor.OnPowerGenerationChanged = (Action<bool>) Delegate.Combine(resourceDistributor.OnPowerGenerationChanged, new Action<bool>(this.OnPowerGenerationChanged));
            this.TerminalSystem.BlockManipulationFinishedFunction();
            this.ResourceDistributor.UpdateBeforeSimulation();
        }

        public void OnAddedToGroup(MyGridPhysicalGroupData group)
        {
            this.ControlSystem = group.ControlSystem;
            foreach (MyShipController controller in this.m_cubeGrid.GetFatBlocks<MyShipController>())
            {
                if (controller != null)
                {
                    if (controller.ControllerInfo.Controller == null)
                    {
                        if (controller.Pilot == null)
                        {
                            continue;
                        }
                        if (!MySessionComponentReplay.Static.HasEntityReplayData(this.CubeGrid.EntityId))
                        {
                            continue;
                        }
                    }
                    if (controller.EnableShipControl && ((!(controller is MyCockpit) || controller.IsMainCockpit) || !controller.CubeGrid.HasMainCockpit()))
                    {
                        this.ControlSystem.AddControllerBlock(controller);
                    }
                }
            }
            this.ControlSystem.AddGrid(this.CubeGrid);
        }

        public virtual void OnBlockAdded(MySlimBlock block)
        {
            if ((this.ShipSoundComponent != null) && (block.FatBlock is MyThrust))
            {
                this.ShipSoundComponent.ShipHasChanged = true;
            }
            if (this.ConveyorSystem != null)
            {
                this.ConveyorSystem.UpdateLines();
            }
        }

        public virtual void OnBlockIntegrityChanged(MySlimBlock block)
        {
        }

        public virtual void OnBlockOwnershipChanged(MyCubeGrid cubeGrid)
        {
            this.ConveyorSystem.FlagForRecomputation();
        }

        public virtual void OnBlockRemoved(MySlimBlock block)
        {
            if ((this.ShipSoundComponent != null) && (block.FatBlock is MyThrust))
            {
                this.ShipSoundComponent.ShipHasChanged = true;
            }
            if (this.ConveyorSystem != null)
            {
                this.ConveyorSystem.UpdateLines();
            }
        }

        private void OnPowerGenerationChanged(bool powerIsGenerated)
        {
            if (MyVisualScriptLogicProvider.GridPowerGenerationStateChanged != null)
            {
                MyVisualScriptLogicProvider.GridPowerGenerationStateChanged(this.CubeGrid.EntityId, this.CubeGrid.Name, powerIsGenerated);
            }
        }

        public virtual void OnRemovedFromGroup(MyGridLogicalGroupData group)
        {
            if (this.m_blocksRegistered)
            {
                this.TerminalSystem.GroupAdded -= this.m_terminalSystem_GroupAdded;
                this.TerminalSystem.GroupRemoved -= this.m_terminalSystem_GroupRemoved;
                foreach (MyBlockGroup group2 in this.m_cubeGrid.BlockGroups)
                {
                    this.TerminalSystem.RemoveGroup(group2, false);
                }
                foreach (MyCubeBlock block in this.m_cubeGrid.GetFatBlocks())
                {
                    MyTerminalBlock block2 = block as MyTerminalBlock;
                    if (block2 != null)
                    {
                        this.TerminalSystem.Remove(block2);
                    }
                    MyResourceSourceComponent source = block.Components.Get<MyResourceSourceComponent>();
                    if (source != null)
                    {
                        this.ResourceDistributor.RemoveSource(source);
                    }
                    MyResourceSinkComponent sink = block.Components.Get<MyResourceSinkComponent>();
                    if (sink != null)
                    {
                        this.ResourceDistributor.RemoveSink(sink, false, block.MarkedForClose);
                    }
                    IMyRechargeSocketOwner owner = block as IMyRechargeSocketOwner;
                    if (owner != null)
                    {
                        owner.RechargeSocket.ResourceDistributor = null;
                    }
                    IMyGunObject<MyDeviceBase> gun = block as IMyGunObject<MyDeviceBase>;
                    if (gun != null)
                    {
                        this.WeaponSystem.Unregister(gun);
                    }
                }
                this.TerminalSystem.BlockManipulationFinishedFunction();
            }
            this.ConveyorSystem.ResourceSink.IsPoweredChanged -= new Action(this.ResourceDistributor.ConveyorSystem_OnPoweredChanged);
            group.ResourceDistributor.RemoveSink(this.ConveyorSystem.ResourceSink, false, false);
            group.ResourceDistributor.RemoveSink(this.GyroSystem.ResourceSink, false, false);
            group.ResourceDistributor.UpdateBeforeSimulation();
            this.m_cubeGrid.OnFatBlockAdded -= new Action<MyCubeBlock>(this.ResourceDistributor.CubeGrid_OnFatBlockAddedOrRemoved);
            this.m_cubeGrid.OnFatBlockRemoved -= new Action<MyCubeBlock>(this.ResourceDistributor.CubeGrid_OnFatBlockAddedOrRemoved);
            MyResourceDistributorComponent resourceDistributor = this.ResourceDistributor;
            resourceDistributor.OnPowerGenerationChanged = (Action<bool>) Delegate.Remove(resourceDistributor.OnPowerGenerationChanged, new Action<bool>(this.OnPowerGenerationChanged));
            this.ResourceDistributor = null;
            this.TerminalSystem = null;
            this.WeaponSystem = null;
        }

        public void OnRemovedFromGroup(MyGridPhysicalGroupData group)
        {
            this.ControlSystem.RemoveGrid(this.CubeGrid);
            if (this.m_blocksRegistered)
            {
                foreach (MyShipController controller in this.m_cubeGrid.GetFatBlocks<MyShipController>())
                {
                    if (controller == null)
                    {
                        continue;
                    }
                    if ((controller.ControllerInfo.Controller != null) && (controller.EnableShipControl && ((!(controller is MyCockpit) || controller.IsMainCockpit) || !controller.CubeGrid.HasMainCockpit())))
                    {
                        this.ControlSystem.RemoveControllerBlock(controller);
                    }
                }
            }
            this.ControlSystem = null;
        }

        public virtual void PrepareForDraw()
        {
            this.CameraSystem.PrepareForDraw();
        }

        public virtual void RegisterInSystems(MyCubeBlock block)
        {
            if (block.GetType() != typeof(MyCubeBlock))
            {
                MyCubeBlock block1;
                if (this.ResourceDistributor != null)
                {
                    MyResourceSourceComponent source = block.Components.Get<MyResourceSourceComponent>();
                    if (source != null)
                    {
                        this.ResourceDistributor.AddSource(source);
                    }
                    MyResourceSinkComponent sink = block.Components.Get<MyResourceSinkComponent>();
                    if (!(block is MyThrust) && (sink != null))
                    {
                        this.ResourceDistributor.AddSink(sink);
                    }
                    IMyRechargeSocketOwner owner = block as IMyRechargeSocketOwner;
                    if (owner != null)
                    {
                        owner.RechargeSocket.ResourceDistributor = this.ResourceDistributor;
                    }
                }
                if (this.WeaponSystem != null)
                {
                    IMyGunObject<MyDeviceBase> gun = block as IMyGunObject<MyDeviceBase>;
                    if (gun != null)
                    {
                        this.WeaponSystem.Register(gun);
                    }
                }
                if (this.TerminalSystem != null)
                {
                    MyTerminalBlock block6 = block as MyTerminalBlock;
                    if (block6 != null)
                    {
                        this.TerminalSystem.Add(block6);
                    }
                }
                if ((block == null) || !block.HasInventory)
                {
                    block1 = null;
                }
                else
                {
                    block1 = block;
                }
                MyCubeBlock block2 = block1;
                if (block2 != null)
                {
                    this.ConveyorSystem.Add(block2);
                }
                IMyConveyorEndpointBlock endpointBlock = block as IMyConveyorEndpointBlock;
                if (endpointBlock != null)
                {
                    endpointBlock.InitializeConveyorEndpoint();
                    this.ConveyorSystem.AddConveyorBlock(endpointBlock);
                }
                IMyConveyorSegmentBlock segmentBlock = block as IMyConveyorSegmentBlock;
                if (segmentBlock != null)
                {
                    segmentBlock.InitializeConveyorSegment();
                    this.ConveyorSystem.AddSegmentBlock(segmentBlock);
                }
                MyReflectorLight reflector = block as MyReflectorLight;
                if (reflector != null)
                {
                    this.ReflectorLightSystem.Register(reflector);
                }
                if (block.Components.Contains(typeof(MyDataBroadcaster)))
                {
                    MyDataBroadcaster broadcaster = block.Components.Get<MyDataBroadcaster>();
                    this.RadioSystem.Register(broadcaster);
                }
                if (block.Components.Contains(typeof(MyDataReceiver)))
                {
                    MyDataReceiver reciever = block.Components.Get<MyDataReceiver>();
                    this.RadioSystem.Register(reciever);
                }
                if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
                {
                    MyMotorSuspension motor = block as MyMotorSuspension;
                    if (motor != null)
                    {
                        this.WheelSystem.Register(motor);
                    }
                }
                IMyLandingGear gear = block as IMyLandingGear;
                if (gear != null)
                {
                    this.LandingSystem.Register(gear);
                }
                MyGyro gyro = block as MyGyro;
                if (gyro != null)
                {
                    this.GyroSystem.Register(gyro);
                }
                MyCameraBlock camera = block as MyCameraBlock;
                if (camera != null)
                {
                    this.CameraSystem.Register(camera);
                }
            }
            block.OnRegisteredToGridSystems();
        }

        public virtual void RemoveGroup(MyBlockGroup group)
        {
            if (this.TerminalSystem != null)
            {
                this.TerminalSystem.RemoveGroup(group, false);
            }
        }

        public void SyncObject_PowerProducerStateChanged(MyMultipleEnabledEnum enabledState, long playerId)
        {
            if (Sync.IsServer)
            {
                foreach (MyCubeBlock block in this.CubeGrid.GetFatBlocks())
                {
                    IMyPowerProducer producer = block as IMyPowerProducer;
                    if ((producer != null) && (((playerId >= 0L) && ((MyFunctionalBlock) block).HasPlayerAccess(playerId)) || (playerId == -1L)))
                    {
                        producer.Enabled = enabledState == MyMultipleEnabledEnum.AllEnabled;
                    }
                }
            }
            if (this.ResourceDistributor != null)
            {
                this.ResourceDistributor.ChangeSourcesState(MyResourceDistributorComponent.ElectricityId, enabledState, playerId);
            }
            this.CubeGrid.ActivatePhysics();
        }

        private void TerminalSystem_GroupAdded(MyBlockGroup group)
        {
            MyBlockGroup item = this.m_cubeGrid.BlockGroups.Find(x => x.Name.CompareTo(group.Name) == 0);
            if (group.Blocks.FirstOrDefault<MyTerminalBlock>(x => (this.m_cubeGrid.GetFatBlocks().IndexOf(x) != -1)) != null)
            {
                if (item == null)
                {
                    item = new MyBlockGroup();
                    item.Name.AppendStringBuilder(group.Name);
                    this.m_cubeGrid.BlockGroups.Add(item);
                }
                item.Blocks.Clear();
                foreach (MyTerminalBlock block in group.Blocks)
                {
                    if (ReferenceEquals(block.CubeGrid, this.m_cubeGrid))
                    {
                        item.Blocks.Add(block);
                    }
                }
                this.m_cubeGrid.ModifyGroup(item);
            }
        }

        private void TerminalSystem_GroupRemoved(MyBlockGroup group)
        {
            MyBlockGroup item = this.m_cubeGrid.BlockGroups.Find(x => x.Name.CompareTo(group.Name) == 0);
            if (item != null)
            {
                item.Blocks.Clear();
                this.m_cubeGrid.BlockGroups.Remove(item);
                this.m_cubeGrid.ModifyGroup(item);
            }
        }

        public virtual void UnregisterFromSystems(MyCubeBlock block)
        {
            if (block.GetType() != typeof(MyCubeBlock))
            {
                if (this.ResourceDistributor != null)
                {
                    MyResourceSourceComponent source = block.Components.Get<MyResourceSourceComponent>();
                    if (source != null)
                    {
                        this.ResourceDistributor.RemoveSource(source);
                    }
                    MyResourceSinkComponent sink = block.Components.Get<MyResourceSinkComponent>();
                    if (sink != null)
                    {
                        this.ResourceDistributor.RemoveSink(sink, true, false);
                    }
                    IMyRechargeSocketOwner owner = block as IMyRechargeSocketOwner;
                    if (owner != null)
                    {
                        owner.RechargeSocket.ResourceDistributor = null;
                    }
                }
                if (this.WeaponSystem != null)
                {
                    IMyGunObject<MyDeviceBase> gun = block as IMyGunObject<MyDeviceBase>;
                    if (gun != null)
                    {
                        this.WeaponSystem.Unregister(gun);
                    }
                }
                if (this.TerminalSystem != null)
                {
                    MyTerminalBlock block5 = block as MyTerminalBlock;
                    if (block5 != null)
                    {
                        this.TerminalSystem.Remove(block5);
                    }
                }
                if (block.HasInventory)
                {
                    this.ConveyorSystem.Remove(block);
                }
                IMyConveyorEndpointBlock block2 = block as IMyConveyorEndpointBlock;
                if (block2 != null)
                {
                    this.ConveyorSystem.RemoveConveyorBlock(block2);
                }
                IMyConveyorSegmentBlock segmentBlock = block as IMyConveyorSegmentBlock;
                if (segmentBlock != null)
                {
                    this.ConveyorSystem.RemoveSegmentBlock(segmentBlock);
                }
                MyReflectorLight reflector = block as MyReflectorLight;
                if (reflector != null)
                {
                    this.ReflectorLightSystem.Unregister(reflector);
                }
                MyDataBroadcaster broadcaster = block.Components.Get<MyDataBroadcaster>();
                if (broadcaster != null)
                {
                    this.RadioSystem.Unregister(broadcaster);
                }
                MyDataReceiver reciever = block.Components.Get<MyDataReceiver>();
                if (reciever != null)
                {
                    this.RadioSystem.Unregister(reciever);
                }
                if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
                {
                    MyMotorSuspension motor = block as MyMotorSuspension;
                    if (motor != null)
                    {
                        this.WheelSystem.Unregister(motor);
                    }
                }
                IMyLandingGear gear = block as IMyLandingGear;
                if (gear != null)
                {
                    this.LandingSystem.Unregister(gear);
                }
                MyGyro gyro = block as MyGyro;
                if (gyro != null)
                {
                    this.GyroSystem.Unregister(gyro);
                }
                MyCameraBlock camera = block as MyCameraBlock;
                if (camera != null)
                {
                    this.CameraSystem.Unregister(camera);
                }
            }
            block.OnUnregisteredFromGridSystems();
        }

        public virtual void UpdateAfterSimulation()
        {
            this.ConveyorSystem.UpdateAfterSimulation();
        }

        public virtual void UpdateAfterSimulation100()
        {
            this.ConveyorSystem.UpdateAfterSimulation100();
        }

        public void UpdateBeforeSimulation()
        {
            MyEntityThrustComponent component;
            this.ControlSystem.UpdateBeforeSimulation();
            if (MyPerGameSettings.EnableJumpDrive)
            {
                this.JumpSystem.UpdateBeforeSimulation();
            }
            if (this.CubeGrid.Components.TryGet<MyEntityThrustComponent>(out component))
            {
                if (((this.CubeGrid.Physics != null) && (!Sync.IsServer && (this.CubeGrid.Physics.LinearVelocity.LengthSquared() < 1E-05f))) && (this.CubeGrid.Physics.LastLinearVelocity.LengthSquared() >= 1E-05f))
                {
                    component.MarkDirty(false);
                }
                component.UpdateBeforeSimulation(true, this.ControlSystem.RelativeDampeningEntity);
            }
            if (this.GyroSystem.IsDirty || this.GyroSystem.NeedsPerFrameUpdate)
            {
                this.GyroSystem.UpdateBeforeSimulation();
            }
            if (this.TerminalSystem.NeedsHudUpdate)
            {
                this.TerminalSystem.UpdateHud();
            }
            else
            {
                this.TerminalSystem.IncrementHudLastUpdated();
            }
            if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
            {
                this.WheelSystem.UpdateBeforeSimulation();
            }
            this.CameraSystem.UpdateBeforeSimulation();
            if (((this.GasSystem != null) && MySession.Static.Settings.EnableOxygen) && MySession.Static.Settings.EnableOxygenPressurization)
            {
                this.GasSystem.UpdateBeforeSimulation();
            }
            if (this.ShipSoundComponent != null)
            {
                this.ShipSoundComponent.Update();
            }
            this.UpdatePower();
        }

        public virtual void UpdateBeforeSimulation10()
        {
            this.CameraSystem.UpdateBeforeSimulation10();
            this.ConveyorSystem.UpdateBeforeSimulation10();
        }

        public virtual void UpdateBeforeSimulation100()
        {
            if (this.ControlSystem != null)
            {
                this.ControlSystem.UpdateBeforeSimulation100();
            }
            if (((this.GasSystem != null) && MySession.Static.Settings.EnableOxygen) && MySession.Static.Settings.EnableOxygenPressurization)
            {
                this.GasSystem.UpdateBeforeSimulation100();
            }
            if (this.ShipSoundComponent != null)
            {
                this.ShipSoundComponent.Update100();
            }
            if (this.ResourceDistributor != null)
            {
                this.ResourceDistributor.UpdateBeforeSimulation100();
            }
        }

        public virtual void UpdateOnceBeforeFrame()
        {
        }

        public void UpdatePower()
        {
            if (this.ResourceDistributor != null)
            {
                this.ResourceDistributor.UpdateBeforeSimulation();
            }
        }

        public MyResourceDistributorComponent ResourceDistributor { get; private set; }

        public MyGridTerminalSystem TerminalSystem { get; private set; }

        public MyGridConveyorSystem ConveyorSystem { get; private set; }

        public MyGridGyroSystem GyroSystem { get; private set; }

        public MyGridWeaponSystem WeaponSystem { get; private set; }

        public MyGridReflectorLightSystem ReflectorLightSystem { get; private set; }

        public MyGridRadioSystem RadioSystem { get; private set; }

        public MyGridWheelSystem WheelSystem { get; private set; }

        public MyGridLandingSystem LandingSystem { get; private set; }

        public MyGroupControlSystem ControlSystem { get; private set; }

        public MyGridCameraSystem CameraSystem { get; private set; }

        public MyShipSoundComponent ShipSoundComponent { get; private set; }

        public MyGridOreDetectorSystem OreDetectorSystem { get; private set; }

        public MyGridGasSystem GasSystem { get; private set; }

        public MyGridJumpDriveSystem JumpSystem { get; private set; }

        protected MyCubeGrid CubeGrid =>
            this.m_cubeGrid;

        public bool NeedsPerFrameUpdate
        {
            get
            {
                if ((!this.TerminalSystem.NeedsHudUpdate && (!this.ConveyorSystem.NeedsUpdateLines && (!this.GyroSystem.IsDirty && (!this.CameraSystem.NeedsPerFrameUpdate && (!this.ControlSystem.NeedsPerFrameUpdate && !(MyPerGameSettings.EnableJumpDrive ? this.JumpSystem.NeedsPerFrameUpdate : false)))))) && !(MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT ? this.WheelSystem.NeedsPerFrameUpdate : false))
                {
                    MyEntityThrustComponent component;
                    bool needsPerFrameUpdate;
                    if (((this.GasSystem == null) || !MySession.Static.Settings.EnableOxygen) || !MySession.Static.Settings.EnableOxygenPressurization)
                    {
                        needsPerFrameUpdate = false;
                    }
                    else
                    {
                        needsPerFrameUpdate = this.GasSystem.NeedsPerFrameUpdate;
                    }
                    if ((!needsPerFrameUpdate && (!((this.ShipSoundComponent != null) ? this.ShipSoundComponent.NeedsPerFrameUpdate : false) && !this.GyroSystem.NeedsPerFrameUpdate)) && ((!this.CubeGrid.Components.TryGet<MyEntityThrustComponent>(out component) || (component.ThrustCount <= 0)) || !component.HasPower))
                    {
                        return ((this.ResourceDistributor != null) ? this.ResourceDistributor.NeedsPerFrameUpdate : false);
                    }
                }
                return true;
            }
        }

        public bool NeedsPerFrameDraw =>
            this.CameraSystem.NeedsPerFrameUpdate;
    }
}

