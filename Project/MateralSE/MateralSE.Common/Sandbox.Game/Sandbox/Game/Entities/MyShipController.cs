namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Electricity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.ClientStates;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Groups;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyShipController), typeof(Sandbox.ModAPI.Ingame.IMyShipController) })]
    public class MyShipController : MyTerminalBlock, Sandbox.Game.Entities.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity, IMyRechargeSocketOwner, Sandbox.ModAPI.IMyShipController, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, Sandbox.ModAPI.Ingame.IMyShipController
    {
        public MyGridGyroSystem GridGyroSystem;
        public MyGridSelectionSystem GridSelectionSystem;
        public MyGridReflectorLightSystem GridReflectorLights;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_controlThrusters;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_controlWheels;
        private bool m_reactorsSwitched = true;
        private bool m_mainCockpitOverwritten;
        protected MyRechargeSocket m_rechargeSocket;
        private MyHudNotification m_notificationLeave;
        private MyHudNotification m_notificationTerminal;
        private MyHudNotification m_landingGearsNotification;
        private MyHudNotification m_noWeaponNotification;
        private MyHudNotification m_weaponSelectedNotification;
        private MyHudNotification m_outOfAmmoNotification;
        private MyHudNotification m_weaponNotWorkingNotification;
        private MyHudNotification m_noControlNotification;
        private MyHudNotification m_connectorsNotification;
        protected bool m_enableFirstPerson;
        protected bool m_enableShipControl = true;
        protected bool m_enableBuilderCockpit;
        private static readonly float RollControlMultiplier = 0.2f;
        private bool m_forcedFPS;
        private MyDefinitionId? m_selectedGunId;
        private MyToolbar m_toolbar;
        private MyToolbar m_buildToolbar;
        public bool BuildingMode;
        public bool hasPower;
        private readonly CachingList<MyGroupControlSystem> m_controlSystems = new CachingList<MyGroupControlSystem>();
        protected MyEntity3DSoundEmitter m_soundEmitter;
        protected MySoundPair m_baseIdleSound;
        protected MySoundPair GetOutOfCockpitSound = MySoundPair.Empty;
        protected MySoundPair GetInCockpitSound = MySoundPair.Empty;
        private MyCasterComponent raycaster;
        private int m_switchWeaponCounter;
        private readonly bool[] m_isShooting;
        private static bool m_shouldSetOtherToolbars;
        private bool m_syncing;
        protected MyCharacter m_lastPilot;
        private bool m_isControlled;
        private readonly MyControllerInfo m_info = new MyControllerInfo();
        protected bool m_singleWeaponMode;
        protected Vector3 m_headLocalPosition;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_isMainCockpit;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_horizonIndicatorEnabled;

        public MyShipController()
        {
            this.CreateTerminalControls();
            this.m_isShooting = new bool[((MyShootActionEnum) MyEnum<MyShootActionEnum>.Range.Max) + MyShootActionEnum.SecondaryAction];
            this.ControllerInfo.ControlAcquired += new Action<MyEntityController>(this.OnControlAcquired);
            this.ControllerInfo.ControlReleased += new Action<MyEntityController>(this.OnControlReleased);
            this.GridSelectionSystem = new MyGridSelectionSystem(this);
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            this.m_isMainCockpit.ValueChanged += x => this.MainCockpitChanged();
        }

        public void AddControlSystem(MyGroupControlSystem controlSystem)
        {
            this.m_controlSystems.Add(controlSystem);
        }

        public void BeginShoot(MyShootActionEnum action)
        {
            if (!this.IsWaitingForWeaponSwitch)
            {
                if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.CubeGrid.EntityId))
                {
                    PerFrameData data2 = new PerFrameData();
                    ShootData data3 = new ShootData {
                        Begin = true,
                        ShootAction = (byte) action
                    };
                    data2.ShootData = new ShootData?(data3);
                    PerFrameData data = data2;
                    MySessionComponentReplay.Static.ProvideEntityRecordData(base.CubeGrid.EntityId, data);
                }
                MyGunStatusEnum oK = MyGunStatusEnum.OK;
                IMyGunObject<MyDeviceBase> failedGun = null;
                this.GridSelectionSystem.CanShoot(action, out oK, out failedGun);
                if (oK != MyGunStatusEnum.OK)
                {
                    this.ShowShootNotification(oK, failedGun);
                }
                this.BeginShootSync(action);
            }
        }

        public void BeginShootSync(MyShootActionEnum action = 0)
        {
            this.StartShooting(action);
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, MyShootActionEnum>(this, x => new Action<MyShootActionEnum>(x.ShootBeginCallback), action, targetEndpoint);
            if (MyFakes.SIMULATE_QUICK_TRIGGER)
            {
                this.EndShootInternal(action);
            }
        }

        public MyShipMass CalculateShipMass()
        {
            float num;
            float num2;
            return new MyShipMass(num, base.CubeGrid.GetCurrentMass(out num, out num2), num2);
        }

        protected virtual bool CanBeMainCockpit() => 
            false;

        protected virtual bool CanHaveHorizon() => 
            true;

        public bool CanSwitchToWeapon(MyDefinitionId? weapon)
        {
            if (weapon == null)
            {
                return true;
            }
            MyObjectBuilderType typeId = weapon.Value.TypeId;
            return ((typeId == typeof(MyObjectBuilder_Drill)) || ((typeId == typeof(MyObjectBuilder_SmallMissileLauncher)) || ((typeId == typeof(MyObjectBuilder_SmallGatlingGun)) || ((typeId == typeof(MyObjectBuilder_ShipGrinder)) || ((typeId == typeof(MyObjectBuilder_ShipWelder)) || (typeId == typeof(MyObjectBuilder_SmallMissileLauncherReload)))))));
        }

        private void CheckGridCokpit(MyCubeGrid grid)
        {
            if ((!this.HasCockpit(grid) && grid.IsMainCockpit(this)) && !ReferenceEquals(base.CubeGrid, grid))
            {
                grid.SetMainCockpit(null);
            }
        }

        public void ClearMovementControl()
        {
            if ((base.CubeGrid.GridSystems.ControlSystem != null) && ReferenceEquals(base.CubeGrid.GridSystems.ControlSystem.GetShipController(), this))
            {
                if ((base.CubeGrid.GridSystems.ControlSystem != null) && ReferenceEquals(base.CubeGrid.GridSystems.ControlSystem.GetShipController(), this))
                {
                    this.MoveIndicator = Vector3.Zero;
                    this.RotationIndicator = Vector2.Zero;
                    this.RollIndicator = 0f;
                }
                if (this.m_enableShipControl)
                {
                    MyEntityThrustComponent entityThrustComponent = this.EntityThrustComponent;
                    if ((entityThrustComponent != null) && !entityThrustComponent.AutopilotEnabled)
                    {
                        entityThrustComponent.ControlThrust = Vector3.Zero;
                    }
                    if ((this.GridGyroSystem != null) && !this.GridGyroSystem.AutopilotEnabled)
                    {
                        this.GridGyroSystem.ControlTorque = Vector3.Zero;
                    }
                    if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT && (this.GridWheels != null))
                    {
                        this.GridWheels.AngularVelocity = Vector3.Zero;
                    }
                }
            }
        }

        protected override void Closing()
        {
            if (MyFakes.ENABLE_NEW_SOUNDS)
            {
                this.StopLoopSound();
            }
            this.IsMainCockpit = false;
            if (!base.CubeGrid.MarkedForClose)
            {
                base.CubeGrid.OnGridSplit -= new Action<MyCubeGrid, MyCubeGrid>(this.CubeGrid_OnGridSplit);
            }
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
                this.m_soundEmitter = null;
            }
            base.Closing();
        }

        protected virtual void ComponentStack_IsFunctionalChanged()
        {
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyShipController>())
            {
                MyStringId? nullable;
                base.CreateTerminalControls();
                if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
                {
                    nullable = null;
                    nullable = null;
                    MyTerminalControlCheckbox<MyShipController> checkbox11 = new MyTerminalControlCheckbox<MyShipController>("ControlThrusters", MySpaceTexts.TerminalControlPanel_Cockpit_ControlThrusters, MySpaceTexts.TerminalControlPanel_Cockpit_ControlThrusters, nullable, nullable);
                    MyTerminalControlCheckbox<MyShipController> checkbox12 = new MyTerminalControlCheckbox<MyShipController>("ControlThrusters", MySpaceTexts.TerminalControlPanel_Cockpit_ControlThrusters, MySpaceTexts.TerminalControlPanel_Cockpit_ControlThrusters, nullable, nullable);
                    checkbox12.Getter = x => x.ControlThrusters;
                    MyTerminalControlCheckbox<MyShipController> local69 = checkbox12;
                    MyTerminalControlCheckbox<MyShipController> local70 = checkbox12;
                    local70.Setter = (x, v) => x.ControlThrusters = v;
                    MyTerminalControlCheckbox<MyShipController> local67 = local70;
                    MyTerminalControlCheckbox<MyShipController> local68 = local70;
                    local68.Visible = x => x.m_enableShipControl;
                    MyTerminalControlCheckbox<MyShipController> local65 = local68;
                    MyTerminalControlCheckbox<MyShipController> local66 = local68;
                    local66.Enabled = x => x.IsMainCockpitFree();
                    MyTerminalControlCheckbox<MyShipController> local5 = local66;
                    MyTerminalAction<MyShipController> action = local5.EnableAction<MyShipController>(null);
                    if (action != null)
                    {
                        action.Enabled = x => x.m_enableShipControl;
                    }
                    MyTerminalControlFactory.AddControl<MyShipController>(local5);
                    nullable = null;
                    nullable = null;
                    MyTerminalControlCheckbox<MyShipController> checkbox9 = new MyTerminalControlCheckbox<MyShipController>("ControlWheels", MySpaceTexts.TerminalControlPanel_Cockpit_ControlWheels, MySpaceTexts.TerminalControlPanel_Cockpit_ControlWheels, nullable, nullable);
                    MyTerminalControlCheckbox<MyShipController> checkbox10 = new MyTerminalControlCheckbox<MyShipController>("ControlWheels", MySpaceTexts.TerminalControlPanel_Cockpit_ControlWheels, MySpaceTexts.TerminalControlPanel_Cockpit_ControlWheels, nullable, nullable);
                    checkbox10.Getter = x => x.ControlWheels;
                    MyTerminalControlCheckbox<MyShipController> local63 = checkbox10;
                    MyTerminalControlCheckbox<MyShipController> local64 = checkbox10;
                    local64.Setter = (x, v) => x.ControlWheels = v;
                    MyTerminalControlCheckbox<MyShipController> local61 = local64;
                    MyTerminalControlCheckbox<MyShipController> local62 = local64;
                    local62.Visible = x => x.m_enableShipControl;
                    MyTerminalControlCheckbox<MyShipController> local59 = local62;
                    MyTerminalControlCheckbox<MyShipController> local60 = local62;
                    local60.Enabled = x => (x.GridWheels.WheelCount > 0) && x.IsMainCockpitFree();
                    MyTerminalControlCheckbox<MyShipController> local11 = local60;
                    action = local11.EnableAction<MyShipController>(null);
                    if (action != null)
                    {
                        action.Enabled = x => x.m_enableShipControl;
                    }
                    MyTerminalControlFactory.AddControl<MyShipController>(local11);
                    nullable = null;
                    nullable = null;
                    MyTerminalControlCheckbox<MyShipController> checkbox7 = new MyTerminalControlCheckbox<MyShipController>("HandBrake", MySpaceTexts.TerminalControlPanel_Cockpit_Handbrake, MySpaceTexts.TerminalControlPanel_Cockpit_Handbrake, nullable, nullable);
                    MyTerminalControlCheckbox<MyShipController> checkbox8 = new MyTerminalControlCheckbox<MyShipController>("HandBrake", MySpaceTexts.TerminalControlPanel_Cockpit_Handbrake, MySpaceTexts.TerminalControlPanel_Cockpit_Handbrake, nullable, nullable);
                    checkbox8.Getter = x => x.CubeGrid.GridSystems.WheelSystem.HandBrake;
                    MyTerminalControlCheckbox<MyShipController> local57 = checkbox8;
                    MyTerminalControlCheckbox<MyShipController> local58 = checkbox8;
                    local58.Setter = (x, v) => x.SwitchHandbrake();
                    MyTerminalControlCheckbox<MyShipController> local55 = local58;
                    MyTerminalControlCheckbox<MyShipController> local56 = local58;
                    local56.Visible = x => x.m_enableShipControl;
                    MyTerminalControlCheckbox<MyShipController> local53 = local56;
                    MyTerminalControlCheckbox<MyShipController> local54 = local56;
                    local54.Enabled = x => (x.GridWheels.WheelCount > 0) && x.IsMainCockpitFree();
                    MyTerminalControlCheckbox<MyShipController> local17 = local54;
                    action = local17.EnableAction<MyShipController>(null);
                    if (action != null)
                    {
                        action.Enabled = x => x.m_enableShipControl;
                    }
                    MyTerminalControlFactory.AddControl<MyShipController>(local17);
                }
                if (MyFakes.ENABLE_DAMPENERS_OVERRIDE)
                {
                    nullable = null;
                    nullable = null;
                    MyTerminalControlCheckbox<MyShipController> checkbox5 = new MyTerminalControlCheckbox<MyShipController>("DampenersOverride", MySpaceTexts.ControlName_InertialDampeners, MySpaceTexts.ControlName_InertialDampeners, nullable, nullable);
                    MyTerminalControlCheckbox<MyShipController> checkbox6 = new MyTerminalControlCheckbox<MyShipController>("DampenersOverride", MySpaceTexts.ControlName_InertialDampeners, MySpaceTexts.ControlName_InertialDampeners, nullable, nullable);
                    checkbox6.Getter = x => (x.EntityThrustComponent != null) && x.EntityThrustComponent.DampenersEnabled;
                    MyTerminalControlCheckbox<MyShipController> local51 = checkbox6;
                    MyTerminalControlCheckbox<MyShipController> local52 = checkbox6;
                    local52.Setter = (x, v) => x.CubeGrid.EnableDampingInternal(v, true);
                    MyTerminalControlCheckbox<MyShipController> local49 = local52;
                    MyTerminalControlCheckbox<MyShipController> local50 = local52;
                    local50.Visible = x => x.m_enableShipControl;
                    MyTerminalControlCheckbox<MyShipController> local22 = local50;
                    MyTerminalAction<MyShipController> action2 = local22.EnableAction<MyShipController>(null);
                    if (action2 != null)
                    {
                        action2.Enabled = x => x.m_enableShipControl;
                    }
                    MyTerminalControlCheckbox<MyShipController> local47 = local22;
                    MyTerminalControlCheckbox<MyShipController> control = local22;
                    control.Enabled = x => x.IsMainCockpitFree();
                    MyTerminalControlFactory.AddControl<MyShipController>(control);
                }
                nullable = null;
                nullable = null;
                MyTerminalControlCheckbox<MyShipController> checkbox3 = new MyTerminalControlCheckbox<MyShipController>("HorizonIndicator", MySpaceTexts.TerminalControlPanel_Cockpit_HorizonIndicator, MySpaceTexts.TerminalControlPanel_Cockpit_HorizonIndicator, nullable, nullable);
                MyTerminalControlCheckbox<MyShipController> checkbox4 = new MyTerminalControlCheckbox<MyShipController>("HorizonIndicator", MySpaceTexts.TerminalControlPanel_Cockpit_HorizonIndicator, MySpaceTexts.TerminalControlPanel_Cockpit_HorizonIndicator, nullable, nullable);
                checkbox4.Getter = x => x.HorizonIndicatorEnabled;
                MyTerminalControlCheckbox<MyShipController> local45 = checkbox4;
                MyTerminalControlCheckbox<MyShipController> local46 = checkbox4;
                local46.Setter = (x, v) => x.HorizonIndicatorEnabled = v;
                MyTerminalControlCheckbox<MyShipController> local43 = local46;
                MyTerminalControlCheckbox<MyShipController> local44 = local46;
                local44.Enabled = x => true;
                MyTerminalControlCheckbox<MyShipController> local41 = local44;
                MyTerminalControlCheckbox<MyShipController> local42 = local44;
                local42.Visible = x => x.CanHaveHorizon();
                MyTerminalControlCheckbox<MyShipController> checkbox = local42;
                checkbox.EnableAction<MyShipController>(null);
                MyTerminalControlFactory.AddControl<MyShipController>(checkbox);
                nullable = null;
                nullable = null;
                MyTerminalControlCheckbox<MyShipController> checkbox1 = new MyTerminalControlCheckbox<MyShipController>("MainCockpit", MySpaceTexts.TerminalControlPanel_Cockpit_MainCockpit, MySpaceTexts.TerminalControlPanel_Cockpit_MainCockpit, nullable, nullable);
                MyTerminalControlCheckbox<MyShipController> checkbox2 = new MyTerminalControlCheckbox<MyShipController>("MainCockpit", MySpaceTexts.TerminalControlPanel_Cockpit_MainCockpit, MySpaceTexts.TerminalControlPanel_Cockpit_MainCockpit, nullable, nullable);
                checkbox2.Getter = x => x.IsMainCockpit;
                MyTerminalControlCheckbox<MyShipController> local39 = checkbox2;
                MyTerminalControlCheckbox<MyShipController> local40 = checkbox2;
                local40.Setter = (x, v) => x.IsMainCockpit = v;
                MyTerminalControlCheckbox<MyShipController> local37 = local40;
                MyTerminalControlCheckbox<MyShipController> local38 = local40;
                local38.Enabled = x => x.IsMainCockpitFree();
                MyTerminalControlCheckbox<MyShipController> local35 = local38;
                MyTerminalControlCheckbox<MyShipController> local36 = local38;
                local36.Visible = x => x.CanBeMainCockpit();
                MyTerminalControlCheckbox<MyShipController> local34 = local36;
                local34.EnableAction<MyShipController>(null);
                MyTerminalControlFactory.AddControl<MyShipController>(local34);
            }
        }

        public void Crouch()
        {
        }

        private void CubeGrid_AddedToLogicalGroup(MyGridLogicalGroupData obj)
        {
            this.SetWeaponSystem(obj.WeaponSystem);
        }

        private void CubeGrid_OnGridSplit(MyCubeGrid grid1, MyCubeGrid grid2)
        {
            this.CheckGridCokpit(grid1);
            this.CheckGridCokpit(grid2);
        }

        private void CubeGrid_RemovedFromLogicalGroup()
        {
            this.GridSelectionSystem.WeaponSystem = null;
            MyDefinitionId? gunId = null;
            this.GridSelectionSystem.SwitchTo(gunId, false);
        }

        public void Die()
        {
        }

        public void Down()
        {
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            if (camera is MySpectatorCameraController)
            {
                MyHud.Crosshair.Recenter();
            }
            else
            {
                if (this.GridSelectionSystem != null)
                {
                    this.GridSelectionSystem.DrawHud(camera, playerId);
                }
                Vector2 zero = Vector2.Zero;
                if (MyHudCrosshair.GetProjectedVector(base.PositionComp.GetPosition() + (1000.0 * base.PositionComp.WorldMatrix.Forward), ref zero))
                {
                    MyHud.Crosshair.ChangePosition(zero);
                }
                if (this.raycaster != null)
                {
                    if (this.raycaster.HitBlock != null)
                    {
                        MyHud.BlockInfo.Visible = true;
                        MyHud.BlockInfo.MissingComponentIndex = -1;
                        MySlimBlock.SetBlockComponents(MyHud.BlockInfo, this.raycaster.HitBlock, null);
                        MyHud.BlockInfo.BlockName = this.raycaster.HitBlock.BlockDefinition.DisplayNameText;
                        MyHud.BlockInfo.PCUCost = this.raycaster.HitBlock.BlockDefinition.PCU;
                        MyHud.BlockInfo.BlockIcons = this.raycaster.HitBlock.BlockDefinition.Icons;
                        MyHud.BlockInfo.BlockIntegrity = this.raycaster.HitBlock.Integrity / this.raycaster.HitBlock.MaxIntegrity;
                        MyHud.BlockInfo.CriticalIntegrity = this.raycaster.HitBlock.BlockDefinition.CriticalIntegrityRatio;
                        MyHud.BlockInfo.CriticalComponentIndex = this.raycaster.HitBlock.BlockDefinition.CriticalGroup;
                        MyHud.BlockInfo.OwnershipIntegrity = this.raycaster.HitBlock.BlockDefinition.OwnershipIntegrityRatio;
                        MyHud.BlockInfo.BlockBuiltBy = this.raycaster.HitBlock.BuiltBy;
                        MyHud.BlockInfo.GridSize = this.raycaster.HitBlock.CubeGrid.GridSizeEnum;
                        MyHud.BlockInfo.SetContextHelp(this.raycaster.HitBlock.BlockDefinition);
                    }
                    else
                    {
                        MyHud.BlockInfo.Visible = true;
                        MyHud.BlockInfo.MissingComponentIndex = -1;
                        MyHud.BlockInfo.BlockName = this.raycaster.Caster.DrillDefinition.DisplayNameText;
                        MyHud.BlockInfo.SetContextHelp(this.raycaster.Caster.DrillDefinition);
                        MyHud.BlockInfo.PCUCost = 0;
                        MyHud.BlockInfo.BlockIcons = this.raycaster.Caster.DrillDefinition.Icons;
                        MyHud.BlockInfo.BlockIntegrity = 1f;
                        MyHud.BlockInfo.CriticalIntegrity = 0f;
                        MyHud.BlockInfo.CriticalComponentIndex = 0;
                        MyHud.BlockInfo.OwnershipIntegrity = 0f;
                        MyHud.BlockInfo.BlockBuiltBy = 0L;
                        MyHud.BlockInfo.GridSize = MyCubeSize.Small;
                        MyHud.BlockInfo.Components.Clear();
                    }
                }
            }
        }

        public void EndShoot(MyShootActionEnum action)
        {
            if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.CubeGrid.EntityId))
            {
                PerFrameData data2 = new PerFrameData();
                ShootData data3 = new ShootData {
                    Begin = false,
                    ShootAction = (byte) action
                };
                data2.ShootData = new ShootData?(data3);
                PerFrameData data = data2;
                MySessionComponentReplay.Static.ProvideEntityRecordData(base.CubeGrid.EntityId, data);
            }
            if (this.BuildingMode && (this.Pilot != null))
            {
                this.Pilot.EndShoot(action);
            }
            this.EndShootSync(action);
        }

        protected void EndShootAll()
        {
            foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
            {
                if (this.IsShooting(enum2))
                {
                    this.EndShoot(enum2);
                }
            }
        }

        private void EndShootInternal(MyShootActionEnum action = 0)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, MyShootActionEnum>(this, x => new Action<MyShootActionEnum>(x.ShootEndCallback), action, targetEndpoint);
            this.StopShooting(action);
        }

        public void EndShootSync(MyShootActionEnum action = 0)
        {
            if (!MyFakes.SIMULATE_QUICK_TRIGGER)
            {
                this.EndShootInternal(action);
            }
        }

        public virtual void ForceReleaseControl()
        {
        }

        public Vector3D GetArtificialGravity() => 
            MyGravityProviderSystem.CalculateArtificialGravityInPoint(base.WorldMatrix.Translation, 1f);

        public MyEntityCameraSettings GetCameraEntitySettings() => 
            null;

        public virtual MatrixD GetHeadMatrix(bool includeY, bool includeX = true, bool forceBoneMatrix = false, bool forceHeadBone = false) => 
            base.PositionComp.WorldMatrix;

        public Vector3D GetNaturalGravity() => 
            MyGravityProviderSystem.CalculateNaturalGravityInPoint(base.WorldMatrix.Translation);

        public MyGridClientState GetNetState() => 
            new MyGridClientState { 
                Move = this.MoveIndicator,
                Rotation = this.RotationIndicator,
                Roll = this.RollIndicator
            };

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            SerializableDefinitionId? nullable1;
            MyDefinitionId? selectedGunId = this.m_selectedGunId;
            MyObjectBuilder_ShipController objectBuilderCubeBlock = (MyObjectBuilder_ShipController) base.GetObjectBuilderCubeBlock(copy);
            if (selectedGunId != null)
            {
                nullable1 = new SerializableDefinitionId?(selectedGunId.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            objectBuilderCubeBlock.SelectedGunId = nullable1;
            MyObjectBuilder_ShipController local1 = objectBuilderCubeBlock;
            local1.UseSingleWeaponMode = this.m_singleWeaponMode;
            local1.ControlThrusters = (bool) this.m_controlThrusters;
            local1.ControlWheels = (bool) this.m_controlWheels;
            local1.Toolbar = this.m_toolbar.GetObjectBuilder();
            local1.BuildToolbar = this.m_buildToolbar.GetObjectBuilder();
            local1.IsMainCockpit = (bool) this.m_isMainCockpit;
            local1.HorizonIndicatorEnabled = this.HorizonIndicatorEnabled;
            return local1;
        }

        public Vector3D GetTotalGravity() => 
            MyGravityProviderSystem.CalculateTotalGravityInPoint(base.WorldMatrix.Translation);

        public override MatrixD GetViewMatrix()
        {
            MatrixD xd2;
            MatrixD.Invert(ref this.GetHeadMatrix(!this.ForceFirstPersonCamera, !this.ForceFirstPersonCamera, false, false), out xd2);
            return xd2;
        }

        private void HandleBuldingMode()
        {
            if (MySandboxGame.Config.ExperimentalMode && ((this.BuildingMode && !MySession.Static.IsCameraControlledObject()) || ((MyInput.Static.IsNewKeyPressed(MyKeys.G) && (MyInput.Static.IsAnyCtrlKeyPressed() && (!MyInput.Static.IsAnyMousePressed() && (this.m_enableBuilderCockpit && (this.CanBeMainCockpit() && MySession.Static.IsCameraControlledObject()))))) && ReferenceEquals(MySession.Static.ControlledEntity, this))))
            {
                this.BuildingMode = !this.BuildingMode;
                MyGuiAudio.PlaySound(MyGuiSounds.HudUse);
                this.Toolbar.Unselect(true);
                if (this.BuildingMode)
                {
                    MyHud.Crosshair.ChangeDefaultSprite(MyHudTexturesEnum.Target_enemy, 0.01f);
                    MyDefinitionId? blockDefinitionId = null;
                    MyCubeBuilder.Static.Activate(blockDefinitionId);
                }
                else
                {
                    MyHud.Crosshair.ResetToDefault(true);
                    MyCubeBuilder.Static.Deactivate();
                }
            }
        }

        private bool HasCockpit(MyCubeGrid grid) => 
            grid.CubeBlocks.Contains(base.SlimBlock);

        public static bool HasPriorityOver(MyShipController first, MyShipController second)
        {
            if (first.Priority < second.Priority)
            {
                return true;
            }
            if (first.Priority > second.Priority)
            {
                return false;
            }
            if ((first.CubeGrid.Physics == null) && (second.CubeGrid.Physics == null))
            {
                return (first.CubeGrid.BlocksCount > second.CubeGrid.BlocksCount);
            }
            if (((first.CubeGrid.Physics == null) || ((second.CubeGrid.Physics == null) || (first.CubeGrid.Physics.Shape.MassProperties == null))) || (second.CubeGrid.Physics.Shape.MassProperties == null))
            {
                return ReferenceEquals(first.CubeGrid.Physics, null);
            }
            return (first.CubeGrid.Physics.Shape.MassProperties.Value.Mass > second.CubeGrid.Physics.Shape.MassProperties.Value.Mass);
        }

        public void HudNotifications()
        {
            if (this.ControllerInfo.IsLocallyHumanControlled())
            {
                if (base.CubeGrid.GridSystems.LandingSystem.HudMessage != MyStringId.NullOrEmpty)
                {
                    this.m_landingGearsNotification = new MyHudNotification(base.CubeGrid.GridSystems.LandingSystem.HudMessage, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    MyHud.Notifications.Add(this.m_landingGearsNotification);
                    base.CubeGrid.GridSystems.LandingSystem.HudMessage = MyStringId.NullOrEmpty;
                }
                if (base.CubeGrid.GridSystems.ConveyorSystem.HudMessage != MyStringId.NullOrEmpty)
                {
                    this.m_connectorsNotification = new MyHudNotification(base.CubeGrid.GridSystems.ConveyorSystem.HudMessage, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    MyHud.Notifications.Add(this.m_connectorsNotification);
                    base.CubeGrid.GridSystems.ConveyorSystem.HudMessage = MyStringId.NullOrEmpty;
                }
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyDefinitionId? nullable1;
            base.SyncFlag = true;
            base.Init(objectBuilder, cubeGrid);
            MyDefinitionManager.Static.GetCubeBlockDefinition(objectBuilder.GetId());
            this.m_enableFirstPerson = this.BlockDefinition.EnableFirstPerson || !MySession.Static.Settings.Enable3rdPersonView;
            this.m_enableShipControl = this.BlockDefinition.EnableShipControl;
            this.m_enableBuilderCockpit = this.BlockDefinition.EnableBuilderCockpit;
            this.m_rechargeSocket = new MyRechargeSocket();
            MyObjectBuilder_ShipController controller = (MyObjectBuilder_ShipController) objectBuilder;
            SerializableDefinitionId? selectedGunId = controller.SelectedGunId;
            if (selectedGunId != null)
            {
                nullable1 = new MyDefinitionId?(selectedGunId.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            this.m_selectedGunId = nullable1;
            this.m_controlThrusters.SetLocalValue(controller.ControlThrusters);
            this.m_controlWheels.SetLocalValue(controller.ControlWheels);
            if (controller.IsMainCockpit)
            {
                this.m_isMainCockpit.SetLocalValue(true);
            }
            this.m_horizonIndicatorEnabled.SetLocalValue(controller.HorizonIndicatorEnabled);
            this.m_toolbar = new MyToolbar(this.ToolbarType, 9, 9);
            this.m_toolbar.Init(controller.Toolbar, this, false);
            this.m_toolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
            this.m_buildToolbar = new MyToolbar(MyToolbarType.BuildCockpit, 9, 9);
            this.m_buildToolbar.Init(controller.BuildToolbar, this, false);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.m_baseIdleSound = this.BlockDefinition.PrimarySound;
            base.CubeGrid.OnGridSplit += new Action<MyCubeGrid, MyCubeGrid>(this.CubeGrid_OnGridSplit);
            base.Components.ComponentAdded += new Action<System.Type, MyEntityComponentBase>(this.OnComponentAdded);
            base.Components.ComponentRemoved += new Action<System.Type, MyEntityComponentBase>(this.OnComponentRemoved);
            this.UpdateShipInfo();
            if ((this.BlockDefinition.GetInSound != null) && (this.BlockDefinition.GetInSound.Length > 0))
            {
                this.GetInCockpitSound = new MySoundPair(this.BlockDefinition.GetInSound, true);
            }
            if ((this.BlockDefinition.GetOutSound != null) && (this.BlockDefinition.GetOutSound.Length > 0))
            {
                this.GetOutOfCockpitSound = new MySoundPair(this.BlockDefinition.GetOutSound, true);
            }
            this.m_controlThrusters.ValueChanged += new Action<SyncBase>(this.m_controlThrusters_ValueChanged);
        }

        protected virtual bool IsCameraController() => 
            false;

        protected bool IsControllingCockpit() => 
            (this.IsMainCockpitFree() || this.m_mainCockpitOverwritten);

        public virtual bool IsLargeShip() => 
            true;

        protected bool IsMainCockpitFree() => 
            (!base.CubeGrid.HasMainCockpit() || base.CubeGrid.IsMainCockpit(this));

        public bool IsShooting()
        {
            foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
            {
                if (this.m_isShooting[(int) enum2])
                {
                    return true;
                }
            }
            return false;
        }

        protected bool IsShooting(MyShootActionEnum action) => 
            this.m_isShooting[(int) action];

        public void Jump(Vector3 moveIndicator)
        {
        }

        private void m_controlThrusters_ValueChanged(SyncBase obj)
        {
            if ((this.EntityThrustComponent != null) && Sync.Players.HasExtendedControl(this, base.CubeGrid))
            {
                this.EntityThrustComponent.Enabled = (bool) this.m_controlThrusters;
            }
        }

        private void MainCockpitChanged()
        {
            if (this.m_isMainCockpit != null)
            {
                base.CubeGrid.SetMainCockpit(this);
            }
            else if (base.CubeGrid.IsMainCockpit(this))
            {
                base.CubeGrid.SetMainCockpit(null);
            }
        }

        public void MoveAndRotate()
        {
            if (!base.Closed)
            {
                MyGroupControlSystem controlSystem = base.CubeGrid.GridSystems.ControlSystem;
                if ((controlSystem != null) && ((controlSystem.GetShipController() == null) || ReferenceEquals(controlSystem.GetShipController(), this)))
                {
                    int num1;
                    if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.CubeGrid.EntityId))
                    {
                        PerFrameData data2 = new PerFrameData();
                        MovementData data3 = new MovementData {
                            MoveVector = this.MoveIndicator,
                            RotateVector = new SerializableVector3(this.RotationIndicator.X, this.RotationIndicator.Y, this.RollIndicator)
                        };
                        data2.MovementData = new MovementData?(data3);
                        PerFrameData data = data2;
                        MySessionComponentReplay.Static.ProvideEntityRecordData(base.CubeGrid.EntityId, data);
                    }
                    if ((!this.m_enableShipControl || (this.MoveIndicator != Vector3.Zero)) || (this.RotationIndicator != Vector2.Zero))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) (this.RollIndicator == 0f);
                    }
                    if (num1 != 0)
                    {
                        this.ClearMovementControl();
                    }
                    else if ((((this.IsMainCockpit || !base.CubeGrid.HasMainCockpit()) || this.m_mainCockpitOverwritten) && (((this.EntityThrustComponent != null) || (this.GridGyroSystem != null)) || (this.GridWheels != null))) && (this.GridResourceDistributor != null))
                    {
                        MyPlayer controllingPlayer = Sync.Players.GetControllingPlayer(base.CubeGrid);
                        if (((Sync.Players.HasExtendedControl(this, base.CubeGrid) || MySessionComponentReplay.Static.IsEntityBeingReplayed(base.CubeGrid.EntityId)) || (((this.Pilot != null) && (controllingPlayer != null)) && ReferenceEquals(controllingPlayer.Character, this.Pilot))) && this.m_enableShipControl)
                        {
                            if (!base.CubeGrid.Physics.RigidBody.IsActive)
                            {
                                base.CubeGrid.ActivatePhysics();
                            }
                            MyEntityThrustComponent entityThrustComponent = this.EntityThrustComponent;
                            if (base.CubeGrid.GridSystems.ResourceDistributor.ResourceState != MyResourceStateEnum.NoPower)
                            {
                                Matrix matrix;
                                base.Orientation.GetMatrix(out matrix);
                                if (entityThrustComponent != null)
                                {
                                    entityThrustComponent.Enabled = (bool) this.m_controlThrusters;
                                    Vector3 vector = Vector3.Transform(this.MoveIndicator, matrix);
                                    entityThrustComponent.ControlThrust += vector;
                                }
                                if (this.GridGyroSystem != null)
                                {
                                    Vector2 vector2 = Vector2.ClampToSphere(this.RotationIndicator / 20f, 1f);
                                    float num = this.RollIndicator * RollControlMultiplier;
                                    Vector3 vector = Vector3.Transform(new Vector3(-vector2.X, -vector2.Y, -num), matrix);
                                    Vector3.ClampToSphere(vector, 1f);
                                    this.GridGyroSystem.ControlTorque += vector;
                                }
                                if ((MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT && (this.GridWheels != null)) && this.ControlWheels)
                                {
                                    this.GridWheels.AngularVelocity = this.MoveIndicator;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            this.MoveIndicator = moveIndicator;
            this.RotationIndicator = rotationIndicator;
            this.RollIndicator = rollIndicator;
        }

        public void MoveAndRotateStopped()
        {
            this.ClearMovementControl();
        }

        public override void OnAddedToScene(object source)
        {
            base.Render.NearFlag = false;
            base.OnAddedToScene(source);
            MyPlayerCollection.UpdateControl(base.CubeGrid);
        }

        public void OnBeginShoot(MyShootActionEnum action)
        {
            MyGunStatusEnum oK = MyGunStatusEnum.OK;
            IMyGunObject<MyDeviceBase> failedGun = null;
            if ((this.GridSelectionSystem.CanShoot(action, out oK, out failedGun) || (oK == MyGunStatusEnum.OK)) || (oK == MyGunStatusEnum.Cooldown))
            {
                this.GridSelectionSystem.BeginShoot(action);
            }
            else
            {
                this.ShootBeginFailed(action, oK, failedGun);
            }
        }

        private void OnComponentAdded(System.Type arg1, MyEntityComponentBase arg2)
        {
            if (arg1 == typeof(MyCasterComponent))
            {
                this.raycaster = arg2 as MyCasterComponent;
                base.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.OnPositionChanged);
                this.OnPositionChanged(base.PositionComp);
            }
        }

        private void OnComponentRemoved(System.Type arg1, MyEntityComponentBase arg2)
        {
            if (arg1 == typeof(MyCasterComponent))
            {
                this.raycaster = null;
                base.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.OnPositionChanged);
            }
        }

        protected void OnControlAcquired(MyEntityController controller)
        {
            this.m_isControlled = true;
            controller.ControlledEntityChanged += new Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>(this.OnControlEntityChanged);
            if (!ReferenceEquals(MySession.Static.LocalHumanPlayer, controller.Player) && !Sync.IsServer)
            {
                this.UpdateHudMarker();
            }
            else
            {
                if ((MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT && (this.m_enableShipControl && ((this.IsMainCockpit || !base.CubeGrid.HasMainCockpit()) && (base.CubeGrid.GridSystems.ControlSystem != null)))) && (ReferenceEquals(base.CubeGrid.GridSystems.ControlSystem.GetShipController(), this) || (base.CubeGrid.GridSystems.ControlSystem.GetShipController() == null)))
                {
                    this.GridWheels.InitControl(controller.ControlledEntity as VRage.Game.Entity.MyEntity);
                }
                if (((MySession.Static.CameraController is VRage.Game.Entity.MyEntity) && (this.IsCameraController() && ReferenceEquals(MySession.Static.LocalHumanPlayer, controller.Player))) && !MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning)
                {
                    Vector3D? position = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this, position);
                }
                if (this.GridResourceDistributor != null)
                {
                    this.GridResourceDistributor.ConveyorSystem_OnPoweredChanged();
                }
                if (this.EntityThrustComponent != null)
                {
                    this.EntityThrustComponent.MarkDirty(false);
                }
                this.Static_CameraAttachedToChanged(null, null);
                if (ReferenceEquals(MySession.Static.LocalHumanPlayer, controller.Player))
                {
                    if (MySession.Static.Settings.RespawnShipDelete && base.CubeGrid.IsRespawnGrid)
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.RespawnShipWarning);
                    }
                    this.RefreshControlNotifications();
                    if (this.IsCameraController())
                    {
                        this.OnControlAcquired_UpdateCamera();
                    }
                    MyHud.HideAll();
                    MyHud.ShipInfo.Show(null);
                    MyHud.Crosshair.ResetToDefault(true);
                    MyHud.SinkGroupInfo.Visible = true;
                    MyHud.GravityIndicator.Entity = this;
                    MyHud.GravityIndicator.Show(null);
                    MyHud.OreMarkers.Visible = true;
                    MyHud.LargeTurretTargets.Visible = true;
                }
            }
            if (this.m_enableShipControl && (this.IsMainCockpit || !base.CubeGrid.HasMainCockpit()))
            {
                MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = this.ControlGroup.GetGroup(base.CubeGrid);
                if (group != null)
                {
                    group.GroupData.ControlSystem.AddControllerBlock(this);
                }
                this.GridSelectionSystem.OnControlAcquired();
            }
            if (this.BuildingMode && (MySession.Static.ControlledEntity is MyRemoteControl))
            {
                this.BuildingMode = false;
            }
            if (this.BuildingMode)
            {
                MyHud.Crosshair.ChangeDefaultSprite(MyHudTexturesEnum.Target_enemy, 0.01f);
            }
            else
            {
                MyHud.Crosshair.ResetToDefault(true);
            }
            MyEntityThrustComponent entityThrustComponent = this.EntityThrustComponent;
            if (ReferenceEquals(controller, Sync.Players.GetEntityController(base.CubeGrid)) && (entityThrustComponent != null))
            {
                entityThrustComponent.Enabled = (bool) this.m_controlThrusters;
            }
            this.UpdateShipInfo10();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            if (Sync.IsServer || ReferenceEquals(controller.Player, MySession.Static.LocalHumanPlayer))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
        }

        protected virtual void OnControlAcquired_UpdateCamera()
        {
        }

        private void OnControlEntityChanged(Sandbox.Game.Entities.IMyControllableEntity oldControl, Sandbox.Game.Entities.IMyControllableEntity newControl)
        {
            if ((this.m_enableShipControl && ((oldControl != null) && ((oldControl.Entity != null) && ((newControl != null) && (newControl.Entity != null))))) && base.CubeGrid.IsMainCockpit(oldControl.Entity as MyTerminalBlock))
            {
                VRage.Game.Entity.MyEntity entity = (newControl.Entity.Parent == null) ? newControl.Entity : newControl.Entity.Parent;
                if (((oldControl.Entity.Parent == null) ? oldControl.Entity : oldControl.Entity.Parent).EntityId == entity.EntityId)
                {
                    MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = this.ControlGroup.GetGroup(base.CubeGrid);
                    if (group != null)
                    {
                        group.GroupData.ControlSystem.AddControllerBlock(this);
                    }
                    this.GridSelectionSystem.OnControlAcquired();
                    this.m_mainCockpitOverwritten = true;
                }
            }
        }

        protected virtual void OnControlledEntity_Used()
        {
        }

        protected virtual void OnControlReleased(MyEntityController controller)
        {
            this.m_isControlled = false;
            controller.ControlledEntityChanged -= new Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>(this.OnControlEntityChanged);
            this.m_mainCockpitOverwritten = false;
            MyEntityThrustComponent entityThrustComponent = this.EntityThrustComponent;
            if (ReferenceEquals(Sync.Players.GetEntityController(this), controller) && (entityThrustComponent != null))
            {
                entityThrustComponent.Enabled = true;
            }
            if ((ReferenceEquals(MySession.Static.LocalHumanPlayer, controller.Player) || Sync.IsServer) && (entityThrustComponent != null))
            {
                this.ClearMovementControl();
            }
            if (!ReferenceEquals(MySession.Static.LocalHumanPlayer, controller.Player))
            {
                if (!MyFakes.ENABLE_RADIO_HUD)
                {
                    MyHud.LocationMarkers.UnregisterMarker(this);
                }
            }
            else
            {
                this.OnControlReleased_UpdateCamera();
                this.ForceFirstPersonCamera = false;
                if (MyGuiScreenGamePlay.Static != null)
                {
                    this.Static_CameraAttachedToChanged(null, null);
                }
                MyHud.Notifications.Remove(MyNotificationSingletons.RespawnShipWarning);
                this.RemoveControlNotifications();
                MyHud.ShipInfo.Hide();
                MyHud.GravityIndicator.Hide();
                MyHud.Crosshair.HideDefaultSprite();
                MyHud.Crosshair.Recenter();
                MyHud.LargeTurretTargets.Visible = false;
                MyHud.Notifications.Remove(this.m_noControlNotification);
                this.m_noControlNotification = null;
            }
            if (this.IsShooting())
            {
                this.EndShootAll();
            }
            if (this.m_enableShipControl && (this.IsMainCockpit || !base.CubeGrid.HasMainCockpit()))
            {
                if (this.GridSelectionSystem != null)
                {
                    this.GridSelectionSystem.OnControlReleased();
                }
                MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = this.ControlGroup.GetGroup(base.CubeGrid);
                if (group != null)
                {
                    group.GroupData.ControlSystem.RemoveControllerBlock(this);
                }
            }
            if ((MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT && this.m_enableShipControl) && this.IsControllingCockpit())
            {
                this.GridWheels.ReleaseControl(controller.ControlledEntity as VRage.Game.Entity.MyEntity);
            }
        }

        protected virtual void OnControlReleased_UpdateCamera()
        {
        }

        public void OnEndShoot(MyShootActionEnum action)
        {
            this.GridSelectionSystem.EndShoot(action);
        }

        private void OnPositionChanged(MyPositionComponentBase obj)
        {
            MatrixD worldMatrix = obj.WorldMatrix;
            worldMatrix.Translation = Vector3D.Transform(this.BlockDefinition.RaycastOffset, worldMatrix);
            if (this.raycaster != null)
            {
                this.raycaster.OnWorldPosChanged(ref worldMatrix);
            }
        }

        public override void OnRegisteredToGridSystems()
        {
            this.GridGyroSystem = base.CubeGrid.GridSystems.GyroSystem;
            this.GridReflectorLights = base.CubeGrid.GridSystems.ReflectorLightSystem;
            base.CubeGrid.AddedToLogicalGroup += new Action<MyGridLogicalGroupData>(this.CubeGrid_AddedToLogicalGroup);
            base.CubeGrid.RemovedFromLogicalGroup += new Action(this.CubeGrid_RemovedFromLogicalGroup);
            this.SetWeaponSystem(base.CubeGrid.GridSystems.WeaponSystem);
            base.OnRegisteredToGridSystems();
        }

        public override void OnRemovedFromScene(object source)
        {
            this.m_controlSystems.ApplyChanges();
            base.OnRemovedFromScene(source);
        }

        [Event(null, 0xa2e), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnSwitchAmmoMagazineRequest()
        {
            if (((Sandbox.Game.Entities.IMyControllableEntity) this).CanSwitchAmmoMagazine())
            {
                this.SwitchAmmoMagazineSuccess();
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController>(this, x => new Action(x.OnSwitchAmmoMagazineSuccess), targetEndpoint);
            }
        }

        [Event(null, 0xa3a), Reliable, Broadcast]
        private void OnSwitchAmmoMagazineSuccess()
        {
            this.SwitchAmmoMagazineSuccess();
        }

        [Event(null, 0x78c), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnSwitchHelmet()
        {
            if ((this.Pilot != null) && (this.Pilot.OxygenComponent != null))
            {
                this.Pilot.OxygenComponent.SwitchHelmet();
            }
        }

        [Event(null, 0xa15), Reliable, Client]
        private void OnSwitchToWeaponFailure(SerializableDefinitionId? weapon, [Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_EntityBase weaponObjectBuilder, long weaponEntityId)
        {
            if (!Sync.IsServer)
            {
                this.m_switchWeaponCounter--;
            }
        }

        [Event(null, 0xa1e), Reliable, Broadcast]
        private void OnSwitchToWeaponSuccess(SerializableDefinitionId? weapon, [Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_EntityBase weaponObjectBuilder, long weaponEntityId)
        {
            MyDefinitionId? nullable1;
            if (!Sync.IsServer && (this.m_switchWeaponCounter > 0))
            {
                this.m_switchWeaponCounter--;
            }
            SerializableDefinitionId? nullable = weapon;
            if (nullable != null)
            {
                nullable1 = new MyDefinitionId?(nullable.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            this.SwitchToWeaponSuccess(nullable1, weaponObjectBuilder, weaponEntityId);
        }

        public override void OnUnregisteredFromGridSystems()
        {
            if (this.EntityThrustComponent != null)
            {
                this.ClearMovementControl();
            }
            base.CubeGrid.AddedToLogicalGroup -= new Action<MyGridLogicalGroupData>(this.CubeGrid_AddedToLogicalGroup);
            base.CubeGrid.RemovedFromLogicalGroup -= new Action(this.CubeGrid_RemovedFromLogicalGroup);
            this.CubeGrid_RemovedFromLogicalGroup();
            this.GridGyroSystem = null;
            this.GridReflectorLights = null;
            base.OnUnregisteredFromGridSystems();
        }

        public void PickUp()
        {
        }

        public void PickUpContinues()
        {
        }

        public void PickUpFinished()
        {
        }

        public void PlayUseSound(bool getIn)
        {
            if (this.m_soundEmitter != null)
            {
                bool? nullable;
                this.m_soundEmitter.VolumeMultiplier = 1f;
                if (getIn)
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySound(this.GetInCockpitSound, false, false, (MySession.Static.LocalCharacter != null) && ReferenceEquals(this.Pilot, MySession.Static.LocalCharacter), false, false, nullable);
                }
                else
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySound(this.GetOutOfCockpitSound, false, false, (MySession.Static.LocalCharacter != null) && ReferenceEquals(this.m_lastPilot, MySession.Static.LocalCharacter), false, false, nullable);
                }
            }
        }

        public void RaiseControlledEntityUsed()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController>(this, x => new Action(x.sync_ControlledEntity_Used), targetEndpoint);
        }

        private void RefreshControlNotifications()
        {
            this.RemoveControlNotifications();
            if (this.m_notificationLeave == null)
            {
                string str = "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) + "]";
                this.m_notificationLeave = new MyHudNotification(this.LeaveNotificationHintText, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
                {
                    object[] arguments = new object[] { str, this.DisplayNameText };
                    this.m_notificationLeave.SetTextFormatArguments(arguments);
                }
                else
                {
                    object[] arguments = new object[] { MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_SPACESHIP, MyControlsSpace.USE), this.DisplayNameText };
                    this.m_notificationLeave.SetTextFormatArguments(arguments);
                }
                this.m_notificationLeave.Level = MyNotificationLevel.Control;
            }
            if (this.m_notificationTerminal == null)
            {
                string str2 = "[" + MyInput.Static.GetGameControl(MyControlsSpace.TERMINAL).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) + "]";
                if (MyInput.Static.IsJoystickConnected() && MyInput.Static.IsJoystickLastUsed)
                {
                    this.m_notificationTerminal = null;
                }
                else
                {
                    this.m_notificationTerminal = new MyHudNotification(MySpaceTexts.NotificationHintOpenShipControlPanel, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    object[] arguments = new object[] { str2 };
                    this.m_notificationTerminal.SetTextFormatArguments(arguments);
                    this.m_notificationTerminal.Level = MyNotificationLevel.Control;
                }
            }
            MyHud.Notifications.Add(this.m_notificationLeave);
            if (this.m_notificationTerminal != null)
            {
                MyHud.Notifications.Add(this.m_notificationTerminal);
            }
        }

        private void RemoveControlNotifications()
        {
            if (this.m_notificationLeave != null)
            {
                MyHud.Notifications.Remove(this.m_notificationLeave);
            }
            if (this.m_notificationTerminal != null)
            {
                MyHud.Notifications.Remove(this.m_notificationTerminal);
            }
        }

        public void RemoveControlSystem(MyGroupControlSystem controlSystem)
        {
            this.m_controlSystems.Remove(controlSystem, false);
        }

        protected virtual void RemoveLocal()
        {
        }

        public void RemoveUsers(bool local)
        {
            if (local)
            {
                this.RemoveLocal();
            }
            else
            {
                this.RaiseControlledEntityUsed();
            }
        }

        private void RequestSwitchToWeapon(MyDefinitionId? weapon, MyObjectBuilder_EntityBase weaponObjectBuilder, long weaponEntityId)
        {
            SerializableDefinitionId? nullable1;
            if (!Sync.IsServer)
            {
                this.m_switchWeaponCounter++;
            }
            MyDefinitionId? nullable2 = weapon;
            if (nullable2 != null)
            {
                nullable1 = new SerializableDefinitionId?(nullable2.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            SerializableDefinitionId? nullable = nullable1;
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(this, x => new Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(x.SwitchToWeaponMessage), nullable, weaponObjectBuilder, weaponEntityId, targetEndpoint);
        }

        bool Sandbox.Game.Entities.IMyControllableEntity.CanSwitchAmmoMagazine() => 
            ((this.m_selectedGunId != null) && this.GridSelectionSystem.CanSwitchAmmoMagazine());

        void Sandbox.Game.Entities.IMyControllableEntity.SwitchAmmoMagazine()
        {
            if (this.m_enableShipControl && this.GridSelectionSystem.CanSwitchAmmoMagazine())
            {
                this.SwitchAmmoMagazineInternal(true);
            }
        }

        double Sandbox.ModAPI.Ingame.IMyShipController.GetShipSpeed()
        {
            MyPhysicsComponentBase physics = base.Parent?.Physics;
            return ((physics == null) ? Vector3D.Zero : new Vector3D(physics.LinearVelocity)).Length();
        }

        MyShipVelocities Sandbox.ModAPI.Ingame.IMyShipController.GetShipVelocities()
        {
            MyPhysicsComponentBase physics = base.Parent?.Physics;
            return new MyShipVelocities((physics == null) ? Vector3D.Zero : new Vector3D(physics.LinearVelocity), (physics == null) ? Vector3D.Zero : new Vector3D(physics.AngularVelocity));
        }

        bool Sandbox.ModAPI.Ingame.IMyShipController.TryGetPlanetElevation(MyPlanetElevation detail, out double elevation)
        {
            if (!MyGravityProviderSystem.IsPositionInNaturalGravity(base.PositionComp.GetPosition(), 0.0))
            {
                elevation = double.PositiveInfinity;
                return false;
            }
            BoundingBoxD worldAABB = base.PositionComp.WorldAABB;
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
            if (closestPlanet == null)
            {
                elevation = double.PositiveInfinity;
                return false;
            }
            if (detail == MyPlanetElevation.Sealevel)
            {
                elevation = (worldAABB.Center - closestPlanet.PositionComp.GetPosition()).Length() - closestPlanet.AverageRadius;
                return true;
            }
            if (detail != MyPlanetElevation.Surface)
            {
                throw new ArgumentOutOfRangeException("detail", detail, null);
            }
            Vector3D centerOfMassWorld = base.CubeGrid.Physics.CenterOfMassWorld;
            Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref centerOfMassWorld);
            elevation = Vector3D.Distance(closestSurfacePointGlobal, centerOfMassWorld);
            return true;
        }

        bool Sandbox.ModAPI.Ingame.IMyShipController.TryGetPlanetPosition(out Vector3D position)
        {
            if (!MyGravityProviderSystem.IsPositionInNaturalGravity(base.PositionComp.GetPosition(), 0.0))
            {
                position = Vector3D.Zero;
                return false;
            }
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref base.PositionComp.WorldAABB);
            if (closestPlanet == null)
            {
                position = Vector3D.Zero;
                return false;
            }
            position = closestPlanet.PositionComp.GetPosition();
            return true;
        }

        [Event(null, 0xa94), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void SendToolbarItemChanged([DynamicObjectBuilder(false)] MyObjectBuilder_ToolbarItem sentItem, int index)
        {
            this.m_syncing = true;
            MyToolbarItem item = null;
            if (sentItem != null)
            {
                item = MyToolbarItemFactory.CreateToolbarItem(sentItem);
            }
            this.Toolbar.SetItemAtIndex(index, item);
            this.m_syncing = false;
        }

        [Event(null, 0xa8c), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void SendToolbarItemRemoved(int index)
        {
            this.m_syncing = true;
            this.Toolbar.SetItemAtIndex(index, null);
            this.m_syncing = false;
        }

        public override void SetDamageEffect(bool show)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                base.SetDamageEffect(show);
                if ((this.m_soundEmitter != null) && (this.BlockDefinition.DamagedSound != null))
                {
                    if (show)
                    {
                        bool? nullable = null;
                        this.m_soundEmitter.PlaySound(this.BlockDefinition.DamagedSound, true, false, false, false, false, nullable);
                    }
                    else if ((this.m_soundEmitter.SoundId == this.BlockDefinition.DamagedSound.Arcade) || (this.m_soundEmitter.SoundId != this.BlockDefinition.DamagedSound.Realistic))
                    {
                        this.m_soundEmitter.StopSound(false, true);
                    }
                }
            }
        }

        private void SetMainCockpit(bool value)
        {
            if ((!value || !base.CubeGrid.HasMainCockpit()) || base.CubeGrid.IsMainCockpit(this))
            {
                this.IsMainCockpit = value;
            }
            else
            {
                this.IsMainCockpit = false;
            }
        }

        public void SetNetState(MyGridClientState netState)
        {
            this.MoveAndRotate(netState.Move, netState.Rotation, netState.Roll);
        }

        public void SetWeaponSystem(MyGridWeaponSystem weaponSystem)
        {
            this.GridSelectionSystem.WeaponSystem = weaponSystem;
            this.GridSelectionSystem.SwitchTo(this.m_selectedGunId, this.m_singleWeaponMode);
        }

        public void Shoot(MyShootActionEnum action)
        {
            MyGunStatusEnum enum2;
            IMyGunObject<MyDeviceBase> obj2;
            if ((this.m_enableShipControl && !this.IsWaitingForWeaponSwitch) && this.GridSelectionSystem.CanShoot(action, out enum2, out obj2))
            {
                this.GridSelectionSystem.Shoot(action);
            }
        }

        [Event(null, 0xa4a), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        private void ShootBeginCallback(MyShootActionEnum action)
        {
            int isLocallyInvoked;
            if (Sync.IsServer)
            {
                isLocallyInvoked = (int) MyEventContext.Current.IsLocallyInvoked;
            }
            else
            {
                isLocallyInvoked = 0;
            }
            if (isLocallyInvoked == 0)
            {
                this.StartShooting(action);
            }
        }

        private void ShootBeginFailed(MyShootActionEnum action, MyGunStatusEnum status, IMyGunObject<MyDeviceBase> failedGun)
        {
            if (failedGun != null)
            {
                failedGun.BeginFailReaction(action, status);
            }
            if ((MySession.Static.ControlledEntity != null) && ReferenceEquals(base.CubeGrid, ((VRage.Game.Entity.MyEntity) MySession.Static.ControlledEntity).GetTopMostParent(null)))
            {
                failedGun.BeginFailReactionLocal(action, status);
            }
        }

        [Event(null, 0xa6e), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        private void ShootEndCallback(MyShootActionEnum action)
        {
            int isLocallyInvoked;
            if (Sync.IsServer)
            {
                isLocallyInvoked = (int) MyEventContext.Current.IsLocallyInvoked;
            }
            else
            {
                isLocallyInvoked = 0;
            }
            if (isLocallyInvoked == 0)
            {
                this.StopShooting(action);
            }
        }

        public bool ShouldEndShootingOnPause(MyShootActionEnum action) => 
            true;

        protected virtual bool ShouldSit() => 
            !this.m_enableShipControl;

        public virtual void ShowInventory()
        {
        }

        private void ShowShootNotification(MyGunStatusEnum status, IMyGunObject<MyDeviceBase> weapon)
        {
            if (this.ControllerInfo.IsLocallyHumanControlled())
            {
                switch (status)
                {
                    case MyGunStatusEnum.OutOfPower:
                    case MyGunStatusEnum.NotFunctional:
                        if (this.m_weaponNotWorkingNotification == null)
                        {
                            this.m_weaponNotWorkingNotification = new MyHudNotification(MyCommonTexts.NotificationWeaponNotWorking, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        }
                        if (weapon is MyCubeBlock)
                        {
                            object[] arguments = new object[] { (weapon as MyCubeBlock).DisplayNameText };
                            this.m_weaponNotWorkingNotification.SetTextFormatArguments(arguments);
                        }
                        MyHud.Notifications.Add(this.m_weaponNotWorkingNotification);
                        break;

                    case MyGunStatusEnum.OutOfAmmo:
                        if (this.m_outOfAmmoNotification == null)
                        {
                            this.m_outOfAmmoNotification = new MyHudNotification(MyCommonTexts.OutOfAmmo, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        }
                        if (weapon is MyCubeBlock)
                        {
                            object[] arguments = new object[] { (weapon as MyCubeBlock).DisplayNameText };
                            this.m_outOfAmmoNotification.SetTextFormatArguments(arguments);
                        }
                        MyHud.Notifications.Add(this.m_outOfAmmoNotification);
                        return;

                    case MyGunStatusEnum.Disabled:
                    case MyGunStatusEnum.Failed:
                        break;

                    case MyGunStatusEnum.NotSelected:
                        if (this.m_noWeaponNotification == null)
                        {
                            this.m_noWeaponNotification = new MyHudNotification(MyCommonTexts.NotificationNoWeaponSelected, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                            MyHud.Notifications.Add(this.m_noWeaponNotification);
                        }
                        MyHud.Notifications.Add(this.m_noWeaponNotification);
                        return;

                    default:
                        return;
                }
            }
        }

        public virtual void ShowTerminal()
        {
        }

        public void Sprint(bool enabled)
        {
        }

        protected virtual void StartLoopSound()
        {
        }

        private void StartShooting(MyShootActionEnum action)
        {
            this.m_isShooting[(int) action] = true;
            this.OnBeginShoot(action);
        }

        private void Static_CameraAttachedToChanged(IMyCameraController oldController, IMyCameraController newController)
        {
            if ((ReferenceEquals(MySession.Static.ControlledEntity, this) && !ReferenceEquals(newController, MyThirdPersonSpectator.Static)) && !ReferenceEquals(newController, this))
            {
                this.EndShootAll();
            }
            this.UpdateCameraAfterChange(true);
        }

        private void StopCurrentWeaponShooting()
        {
            foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
            {
                if (this.IsShooting(enum2))
                {
                    this.GridSelectionSystem.EndShoot(enum2);
                }
            }
        }

        public override void StopDamageEffect(bool stopSound = true)
        {
            base.StopDamageEffect(stopSound);
            if ((stopSound && ((this.m_soundEmitter != null) && (this.BlockDefinition.DamagedSound != null))) && ((this.m_soundEmitter.SoundId == this.BlockDefinition.DamagedSound.Arcade) || (this.m_soundEmitter.SoundId != this.BlockDefinition.DamagedSound.Realistic)))
            {
                this.m_soundEmitter.StopSound(true, true);
            }
        }

        protected virtual void StopLoopSound()
        {
        }

        private void StopShooting(MyShootActionEnum action)
        {
            this.m_isShooting[(int) action] = false;
            this.OnEndShoot(action);
        }

        private void SwitchAmmoMagazineInternal(bool sync)
        {
            if (sync)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController>(this, x => new Action(x.OnSwitchAmmoMagazineRequest), targetEndpoint);
            }
            else if (this.m_enableShipControl && !this.IsWaitingForWeaponSwitch)
            {
                this.GridSelectionSystem.SwitchAmmoMagazine();
            }
        }

        private void SwitchAmmoMagazineSuccess()
        {
            if (this.GridSelectionSystem.CanSwitchAmmoMagazine())
            {
                this.SwitchAmmoMagazineInternal(false);
            }
        }

        public void SwitchBroadcasting()
        {
            if (this.m_enableShipControl)
            {
                if (base.CubeGrid.GridSystems.RadioSystem.AntennasBroadcasterEnabled == MyMultipleEnabledEnum.AllDisabled)
                {
                    base.CubeGrid.GridSystems.RadioSystem.AntennasBroadcasterEnabled = MyMultipleEnabledEnum.AllEnabled;
                    MyGuiAudio.PlaySound(MyGuiSounds.HudAntennaOn);
                }
                else
                {
                    base.CubeGrid.GridSystems.RadioSystem.AntennasBroadcasterEnabled = MyMultipleEnabledEnum.AllDisabled;
                    if (base.CubeGrid.GridSystems.RadioSystem.AntennasBroadcasterEnabled != MyMultipleEnabledEnum.NoObjects)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudAntennaOff);
                    }
                }
            }
        }

        public void SwitchDamping()
        {
            if (this.m_enableShipControl && (this.EntityThrustComponent != null))
            {
                base.CubeGrid.EnableDampingInternal(!this.EntityThrustComponent.DampenersEnabled, true);
                if (!this.EntityThrustComponent.DampenersEnabled)
                {
                    this.RelativeDampeningEntity = null;
                }
            }
        }

        public void SwitchHandbrake()
        {
            if (this.m_enableShipControl && (this.IsMainCockpit || !base.CubeGrid.HasMainCockpit()))
            {
                base.CubeGrid.SetHandbrakeRequest(!base.CubeGrid.GridSystems.WheelSystem.HandBrake);
            }
        }

        public void SwitchLandingGears()
        {
            if (this.m_enableShipControl && (this.IsMainCockpit || !base.CubeGrid.HasMainCockpit()))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCubeGrid, bool>(base.CubeGrid, x => new Action<bool>(x.SetHandbrakeRequest), !base.CubeGrid.GridSystems.WheelSystem.HandBrake, targetEndpoint);
                base.CubeGrid.GridSystems.LandingSystem.Switch();
                base.CubeGrid.GridSystems.ConveyorSystem.ToggleConnectors();
                if (base.CubeGrid.GridSystems.WheelSystem.HandBrake)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudBrakeOff);
                }
                else
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudBrakeOn);
                }
            }
            this.HudNotifications();
        }

        public void SwitchLights()
        {
            if (this.m_enableShipControl)
            {
                if (this.GridReflectorLights.ReflectorsEnabled == MyMultipleEnabledEnum.AllDisabled)
                {
                    this.GridReflectorLights.ReflectorsEnabled = MyMultipleEnabledEnum.AllEnabled;
                }
                else
                {
                    this.GridReflectorLights.ReflectorsEnabled = MyMultipleEnabledEnum.AllDisabled;
                }
            }
        }

        public void SwitchReactors()
        {
            if (((base.CubeGrid.MainCockpit == null) || this.IsMainCockpit) && this.m_enableShipControl)
            {
                if (base.CubeGrid.SwitchPower())
                {
                    base.CubeGrid.ChangePowerProducerState(MyMultipleEnabledEnum.AllEnabled, MySession.Static.LocalPlayerId);
                }
                else
                {
                    base.CubeGrid.ChangePowerProducerState(MyMultipleEnabledEnum.AllDisabled, MySession.Static.LocalPlayerId);
                }
                if (!Sync.IsServer)
                {
                    base.CubeGrid.ActivatePhysics();
                }
            }
        }

        public virtual void SwitchThrusts()
        {
        }

        public void SwitchToWeapon(MyToolbarItemWeapon weapon)
        {
            if (this.m_enableShipControl)
            {
                MyDefinitionId? nullable1;
                if (weapon != null)
                {
                    nullable1 = new MyDefinitionId?(weapon.Definition.Id);
                }
                else
                {
                    nullable1 = null;
                }
                this.SwitchToWeaponInternal(nullable1, true);
            }
        }

        public void SwitchToWeapon(MyDefinitionId weapon)
        {
            if (this.m_enableShipControl)
            {
                this.SwitchToWeaponInternal(new MyDefinitionId?(weapon), true);
            }
        }

        private void SwitchToWeaponInternal(MyDefinitionId? gunId)
        {
            this.GridSelectionSystem.SwitchTo(gunId, this.m_singleWeaponMode);
            this.m_selectedGunId = gunId;
            if (this.ControllerInfo.IsLocallyHumanControlled())
            {
                if (this.m_weaponSelectedNotification == null)
                {
                    this.m_weaponSelectedNotification = new MyHudNotification(MyCommonTexts.NotificationSwitchedToWeapon, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                object[] arguments = new object[] { MyDeviceBase.GetGunNotificationName(this.m_selectedGunId.Value) };
                this.m_weaponSelectedNotification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(this.m_weaponSelectedNotification);
            }
        }

        private void SwitchToWeaponInternal(MyDefinitionId? weapon, bool updateSync)
        {
            if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.CubeGrid.EntityId))
            {
                PerFrameData data2 = new PerFrameData();
                SwitchWeaponData data3 = new SwitchWeaponData {
                    WeaponDefinition = weapon
                };
                data2.SwitchWeaponData = new SwitchWeaponData?(data3);
                PerFrameData data = data2;
                MySessionComponentReplay.Static.ProvideEntityRecordData(base.CubeGrid.EntityId, data);
            }
            if (updateSync)
            {
                this.RequestSwitchToWeapon(weapon, null, 0L);
            }
            else
            {
                this.StopCurrentWeaponShooting();
                MyAnalyticsHelper.ReportActivityEnd(this, "item_equip");
                if (weapon == null)
                {
                    this.m_selectedGunId = null;
                    MyDefinitionId? gunId = null;
                    this.GridSelectionSystem.SwitchTo(gunId, false);
                }
                else
                {
                    this.SwitchToWeaponInternal(weapon);
                    char[] separator = new char[] { '_' };
                    string[] strArray = weapon.Value.TypeId.Name.Split(separator);
                    MyAnalyticsHelper.ReportActivityStart(this, "item_equip", "character", "ship_item_usage", (strArray.Length > 1) ? strArray[1] : strArray[0], true);
                }
            }
        }

        [Event(null, 0x9fa), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void SwitchToWeaponMessage(SerializableDefinitionId? weapon, [Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_EntityBase weaponObjectBuilder, long weaponEntityId)
        {
            MyDefinitionId? nullable1;
            SerializableDefinitionId? nullable = weapon;
            if (nullable != null)
            {
                nullable1 = new MyDefinitionId?(nullable.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            if (!this.CanSwitchToWeapon(nullable1))
            {
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    this.OnSwitchToWeaponFailure(weapon, weaponObjectBuilder, weaponEntityId);
                }
                else
                {
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(this, x => new Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(x.OnSwitchToWeaponFailure), weapon, weaponObjectBuilder, weaponEntityId, MyEventContext.Current.Sender);
                }
            }
            else
            {
                if ((weaponObjectBuilder != null) && (weaponObjectBuilder.EntityId == 0))
                {
                    weaponObjectBuilder = (MyObjectBuilder_EntityBase) weaponObjectBuilder.Clone();
                    weaponObjectBuilder.EntityId = (weaponEntityId == 0) ? MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM) : weaponEntityId;
                }
                this.OnSwitchToWeaponSuccess(weapon, weaponObjectBuilder, weaponEntityId);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(this, x => new Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(x.OnSwitchToWeaponSuccess), weapon, weaponObjectBuilder, weaponEntityId, targetEndpoint);
            }
        }

        private void SwitchToWeaponSuccess(MyDefinitionId? weapon, MyObjectBuilder_Base weaponObjectBuilder, long weaponEntityId)
        {
            this.SwitchToWeaponInternal(weapon, false);
        }

        public void SwitchWalk()
        {
        }

        internal void SwitchWeaponMode()
        {
            this.SingleWeaponMode = !this.SingleWeaponMode;
        }

        [Event(null, 0x784), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        protected void sync_ControlledEntity_Used()
        {
            this.OnControlledEntity_Used();
            if (!ReferenceEquals(this.GetOutOfCockpitSound, MySoundPair.Empty))
            {
                this.PlayUseSound(false);
            }
        }

        private void Toolbar_ItemChanged(MyToolbar self, MyToolbar.IndexArgs index)
        {
            if (!this.m_syncing)
            {
                EndpointId id;
                MyToolbarItem itemAtIndex = self.GetItemAtIndex(index.ItemIndex);
                if (itemAtIndex != null)
                {
                    id = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, MyObjectBuilder_ToolbarItem, int>(this, x => new Action<MyObjectBuilder_ToolbarItem, int>(x.SendToolbarItemChanged), itemAtIndex.GetObjectBuilder(), index.ItemIndex, id);
                }
                else
                {
                    id = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController, int>(this, x => new Action<int>(x.SendToolbarItemRemoved), index.ItemIndex, id);
                }
            }
        }

        public bool TryEnableBrakes(bool enable)
        {
            if (!MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT)
            {
                return false;
            }
            if ((this.GridWheels == null) || !this.ControlWheels)
            {
                return false;
            }
            if (!this.EnableShipControl || (!this.IsMainCockpit && base.CubeGrid.HasMainCockpit()))
            {
                return false;
            }
            base.CubeGrid.GridSystems.WheelSystem.Brake = enable;
            return true;
        }

        private void TryExtendControlToGroup()
        {
            if (Sync.IsServer && (this.m_enableShipControl && (this.ControllerInfo.Controller != null)))
            {
                bool flag = false;
                bool flag2 = false;
                MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = this.ControlGroup.GetGroup(base.CubeGrid);
                MyEntityController controller = base.CubeGrid.GridSystems.ControlSystem.GetController();
                if (group != null)
                {
                    if (controller == null)
                    {
                        flag = true;
                    }
                    else
                    {
                        MyShipController controlledEntity = controller.ControlledEntity as MyShipController;
                        if (controlledEntity != null)
                        {
                            if (this.Priority < controlledEntity.Priority)
                            {
                                flag = true;
                                flag2 = true;
                            }
                            else if (((base.CubeGrid.Physics.Shape.MassProperties != null) && (controlledEntity.CubeGrid.Physics.Shape.MassProperties != null)) && (base.CubeGrid.Physics.Shape.MassProperties.Value.Mass > controlledEntity.CubeGrid.Physics.Shape.MassProperties.Value.Mass))
                            {
                                flag = true;
                            }
                        }
                    }
                }
                if (flag)
                {
                    if (controller != null)
                    {
                        MyShipController controlledEntity = controller.ControlledEntity as MyShipController;
                        foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in group.Nodes)
                        {
                            Sync.Players.TryReduceControl(controlledEntity, node.NodeData);
                        }
                        if (flag2)
                        {
                            controlledEntity.ForceReleaseControl();
                        }
                    }
                    Sync.Players.SetControlledEntity(this.ControllerInfo.Controller.Player.Id, this);
                    foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node2 in group.Nodes)
                    {
                        Sync.Players.TryExtendControl(this, node2.NodeData);
                    }
                }
                if (Sync.Players.HasExtendedControl(this, base.CubeGrid))
                {
                    MyEntityThrustComponent entityThrustComponent = this.EntityThrustComponent;
                    if (entityThrustComponent != null)
                    {
                        entityThrustComponent.Enabled = (bool) this.m_controlThrusters;
                    }
                }
            }
        }

        public void Up()
        {
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if ((base.CubeGrid.GridSystems.ControlSystem != null) && (ReferenceEquals(base.CubeGrid.GridSystems.ControlSystem.GetShipController(), this) || base.CubeGrid.ControlledFromTurret))
            {
                if ((this.EntityThrustComponent != null) && !this.EntityThrustComponent.AutopilotEnabled)
                {
                    this.EntityThrustComponent.ControlThrust = Vector3.Zero;
                }
                if ((this.GridGyroSystem != null) && !this.GridGyroSystem.AutopilotEnabled)
                {
                    this.GridGyroSystem.ControlTorque = Vector3.Zero;
                }
                if (MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT && (this.GridWheels != null))
                {
                    this.GridWheels.AngularVelocity = Vector3.Zero;
                }
            }
            this.UpdateShipInfo();
            if (((this.ControllerInfo.Controller != null) && (MySession.Static.LocalHumanPlayer != null)) && ReferenceEquals(this.ControllerInfo.Controller, MySession.Static.LocalHumanPlayer.Controller))
            {
                MyEntityController objA = base.CubeGrid.GridSystems.ControlSystem.GetController();
                if (ReferenceEquals(objA, this.ControllerInfo.Controller))
                {
                    if (this.m_noControlNotification != null)
                    {
                        MyHud.Notifications.Remove(this.m_noControlNotification);
                        this.m_noControlNotification = null;
                    }
                }
                else if ((this.m_noControlNotification == null) && this.EnableShipControl)
                {
                    if ((objA == null) && (base.CubeGrid.GridSystems.ControlSystem.GetShipController() != null))
                    {
                        this.m_noControlNotification = (base.CubeGrid.GridSystems.ControlSystem.GetShipController().Priority != ControllerPriority.AutoPilot) ? new MyHudNotification(MySpaceTexts.Notification_NoControlLowerPriority, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal) : new MyHudNotification(MySpaceTexts.Notification_NoControlAutoPilot, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    }
                    else if (base.CubeGrid.HasMainCockpit() && !base.CubeGrid.IsMainCockpit(this))
                    {
                        this.m_noControlNotification = new MyHudNotification(MySpaceTexts.Notification_NoControlNotMain, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    }
                    else if (((objA == null) || !(objA.ControlledEntity is MyCubeBlock)) || base.CubeGrid.CubeBlocks.Contains((objA.ControlledEntity as MyCubeBlock).SlimBlock))
                    {
                        this.m_noControlNotification = !base.CubeGrid.IsStatic ? new MyHudNotification(MySpaceTexts.Notification_NoControl, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal) : new MyHudNotification(MySpaceTexts.Notification_NoControlStation, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    }
                    else
                    {
                        this.m_noControlNotification = new MyHudNotification(MySpaceTexts.Notification_NoControlOtherShip, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    }
                    MyHud.Notifications.Add(this.m_noControlNotification);
                }
            }
            foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
            {
                if (this.IsShooting(enum2))
                {
                    this.Shoot(enum2);
                }
            }
            if (this.CanBeMainCockpit())
            {
                if (!base.CubeGrid.HasMainCockpit() || base.CubeGrid.IsMainCockpit(this))
                {
                    base.DetailedInfo.Clear();
                }
                else
                {
                    base.DetailedInfo.Clear();
                    base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MainCockpit));
                    base.DetailedInfo.Append(": " + base.CubeGrid.MainCockpit.CustomName);
                }
            }
            this.HandleBuldingMode();
        }

        public override void UpdateBeforeSimulation10()
        {
            this.UpdateShipInfo10();
            base.UpdateBeforeSimulation10();
        }

        public override void UpdateBeforeSimulation100()
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.Update();
                this.UpdateSoundState();
            }
            if (((this.GridResourceDistributor != null) && (this.GridGyroSystem != null)) && (this.EntityThrustComponent != null))
            {
                base.UpdateBeforeSimulation100();
            }
        }

        protected virtual void UpdateCameraAfterChange(bool resetHeadLocalAngle = true)
        {
        }

        public void UpdateControls()
        {
            this.MoveAndRotate();
        }

        private void UpdateHudMarker()
        {
            if (!MyFakes.ENABLE_RADIO_HUD)
            {
                MyHudEntityParams hudParams = new MyHudEntityParams {
                    FlagsEnum = MyHudIndicatorFlagsEnum.SHOW_TEXT,
                    Text = new StringBuilder(this.ControllerInfo.Controller.Player.DisplayName),
                    ShouldDraw = new Func<bool>(MyHud.CheckShowPlayerNamesOnHud)
                };
                MyHud.LocationMarkers.RegisterMarker(this, hudParams);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
        }

        private void UpdateShipInfo()
        {
            this.hasPower = (base.CubeGrid.GridSystems.ResourceDistributor != null) && (base.CubeGrid.GridSystems.ResourceDistributor.ResourceState != MyResourceStateEnum.NoPower);
            if ((Sandbox.Engine.Platform.Game.IsDedicated || (MySession.Static.LocalHumanPlayer == null)) || ReferenceEquals(this.ControllerInfo.Controller, MySession.Static.LocalHumanPlayer.Controller))
            {
                if (this.GridResourceDistributor != null)
                {
                    MyHud.ShipInfo.FuelRemainingTime = this.GridResourceDistributor.RemainingFuelTimeByType(MyResourceDistributorComponent.ElectricityId);
                    MyHud.ShipInfo.Reactors = this.GridResourceDistributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId);
                    MyHud.ShipInfo.ResourceState = this.GridResourceDistributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true);
                }
                if (this.GridGyroSystem != null)
                {
                    MyHud.ShipInfo.GyroCount = this.GridGyroSystem.GyroCount;
                }
                MyEntityThrustComponent entityThrustComponent = this.EntityThrustComponent;
                if (entityThrustComponent != null)
                {
                    MyHud.ShipInfo.ThrustCount = entityThrustComponent.ThrustCount;
                    MyHud.ShipInfo.DampenersEnabled = entityThrustComponent.DampenersEnabled;
                }
            }
        }

        protected void UpdateShipInfo10()
        {
            if (base.CubeGrid.GridSystems != null)
            {
                this.hasPower = (base.CubeGrid.GridSystems.ResourceDistributor != null) && (base.CubeGrid.GridSystems.ResourceDistributor.ResourceState != MyResourceStateEnum.NoPower);
                if ((this.ControllerInfo != null) && this.ControllerInfo.IsLocallyHumanControlled())
                {
                    if (this.GridResourceDistributor != null)
                    {
                        MyHud.ShipInfo.PowerUsage = (this.GridResourceDistributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId) != 0f) ? (this.GridResourceDistributor.TotalRequiredInputByType(MyResourceDistributorComponent.ElectricityId) / this.GridResourceDistributor.MaxAvailableResourceByType(MyResourceDistributorComponent.ElectricityId)) : 0f;
                        MyHud.ShipInfo.NumberOfBatteries = this.GridResourceDistributor.GetSourceCount(MyResourceDistributorComponent.ElectricityId, MyStringHash.GetOrCompute("Battery"));
                        this.GridResourceDistributor.UpdateHud(MyHud.SinkGroupInfo);
                    }
                    this.UpdateShipMass();
                    if ((base.Parent != null) && (base.Parent.Physics != null))
                    {
                        MyHud.ShipInfo.SpeedInKmH = this.HasWheels;
                        MyHud.ShipInfo.Speed = base.Parent.Physics.LinearVelocity.Length();
                    }
                    if (this.GridReflectorLights != null)
                    {
                        MyHud.ShipInfo.ReflectorLights = this.GridReflectorLights.ReflectorsEnabled;
                    }
                    if (base.CubeGrid.GridSystems.LandingSystem != null)
                    {
                        MyHud.ShipInfo.LandingGearsTotal = base.CubeGrid.GridSystems.LandingSystem.TotalGearCount;
                        MyHud.ShipInfo.LandingGearsLocked = base.CubeGrid.GridSystems.LandingSystem[LandingGearMode.Locked];
                        MyHud.ShipInfo.LandingGearsInProximity = base.CubeGrid.GridSystems.LandingSystem[LandingGearMode.ReadyToLock];
                    }
                    else
                    {
                        MyHud.ShipInfo.LandingGearsTotal = 0;
                        MyHud.ShipInfo.LandingGearsLocked = 0;
                        MyHud.ShipInfo.LandingGearsInProximity = 0;
                    }
                }
            }
        }

        private void UpdateShipMass()
        {
            MyHud.ShipInfo.Mass = 0;
            MyCubeGrid parent = base.Parent as MyCubeGrid;
            if (parent != null)
            {
                MyHud.ShipInfo.Mass = parent.GetCurrentMass();
            }
        }

        protected virtual void UpdateSoundState()
        {
        }

        public override void UpdateVisual()
        {
            if (base.Render.NearFlag)
            {
                base.Render.ColorMaskHsv = base.SlimBlock.ColorMaskHSV;
            }
            else
            {
                base.UpdateVisual();
            }
        }

        public override void UpdatingStopped()
        {
            base.UpdatingStopped();
            this.ClearMovementControl();
        }

        public void Use()
        {
            if (ReferenceEquals(this.GetOutOfCockpitSound, MySoundPair.Empty))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUse);
            }
            this.RaiseControlledEntityUsed();
        }

        public void UseContinues()
        {
        }

        public void UseFinished()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Crouch()
        {
            this.Crouch();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Die()
        {
            this.Die();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Down()
        {
            this.Down();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.DrawHud(IMyCameraController camera, long playerId)
        {
            if (camera != null)
            {
                this.DrawHud(camera, playerId);
            }
        }

        MatrixD VRage.Game.ModAPI.Interfaces.IMyControllableEntity.GetHeadMatrix(bool includeY, bool includeX, bool forceHeadAnim, bool forceHeadBone) => 
            this.GetHeadMatrix(includeY, includeX, forceHeadAnim, false);

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Jump(Vector3 moveIndicator)
        {
            this.Jump(moveIndicator);
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            this.MoveAndRotate(moveIndicator, rotationIndicator, rollIndicator);
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.MoveAndRotateStopped()
        {
            this.MoveAndRotateStopped();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PickUp()
        {
            this.PickUp();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PickUpContinues()
        {
            this.PickUpContinues();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ShowInventory()
        {
            this.ShowInventory();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ShowTerminal()
        {
            this.ShowTerminal();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchDamping()
        {
            this.SwitchDamping();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchHelmet()
        {
            if ((this.Pilot != null) && (Sync.IsServer || ReferenceEquals(MySession.Static.LocalCharacter, this.Pilot)))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipController>(this, x => new Action(x.OnSwitchHelmet), targetEndpoint);
            }
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchLandingGears()
        {
            this.SwitchLandingGears();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchLights()
        {
            this.SwitchLights();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchReactors()
        {
            this.SwitchReactors();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchThrusts()
        {
            this.SwitchThrusts();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Up()
        {
            this.Up();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Use()
        {
            this.Use();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.UseContinues()
        {
            this.UseContinues();
        }

        public void WheelJump(bool controlPressed)
        {
            if ((MyFakes.ENABLE_WHEEL_CONTROLS_IN_COCKPIT && ((this.GridWheels != null) && (this.ControlWheels && (this.m_enableShipControl && (this.m_info.Controller != null))))) && this.IsControllingCockpit())
            {
                base.CubeGrid.GridSystems.WheelSystem.UpdateJumpControlState(controlPressed, true);
            }
        }

        public void Zoom(bool newKeyPress)
        {
        }

        public MyResourceDistributorComponent GridResourceDistributor =>
            base.CubeGrid?.GridSystems.ResourceDistributor;

        public MyGridWheelSystem GridWheels =>
            base.CubeGrid?.GridSystems.WheelSystem;

        public MyEntityThrustComponent EntityThrustComponent =>
            base.CubeGrid?.Components.Get<MyEntityThrustComponent>();

        protected virtual MyStringId LeaveNotificationHintText =>
            MySpaceTexts.NotificationHintLeaveCockpit;

        public bool EnableShipControl =>
            this.m_enableShipControl;

        public bool PlayDefaultUseSound =>
            ReferenceEquals(this.GetInCockpitSound, MySoundPair.Empty);

        private Vector3 MoveIndicator { get; set; }

        private Vector2 RotationIndicator { get; set; }

        private float RollIndicator { get; set; }

        public MyToolbar Toolbar =>
            (!this.BuildingMode ? this.m_toolbar : this.m_buildToolbar);

        private bool IsWaitingForWeaponSwitch =>
            (this.m_switchWeaponCounter != 0);

        public bool HasWheels =>
            (this.ControlWheels && (this.GridWheels.WheelCount > 0));

        public MyGroups<MyCubeGrid, MyGridPhysicalGroupData> ControlGroup =>
            MyCubeGridGroups.Static.Physical;

        public virtual MyCharacter Pilot =>
            null;

        protected virtual ControllerPriority Priority =>
            ControllerPriority.Primary;

        public bool PrimaryLookaround =>
            !this.m_enableShipControl;

        public bool NeedsPerFrameUpdate =>
            ((base.CubeGrid.GridSystems.ControlSystem != null) && ReferenceEquals(base.CubeGrid.GridSystems.ControlSystem.GetShipController(), this));

        public virtual bool ForceFirstPersonCamera
        {
            get => 
                (this.m_forcedFPS && this.m_enableFirstPerson);
            set
            {
                if (this.m_forcedFPS != value)
                {
                    this.m_forcedFPS = value;
                    this.UpdateCameraAfterChange(false);
                }
            }
        }

        public virtual bool EnableFirstPersonView
        {
            get => 
                this.m_enableFirstPerson;
            set => 
                (this.m_enableFirstPerson = value);
        }

        public VRage.Game.Entity.MyEntity TopGrid =>
            base.Parent;

        public VRage.Game.Entity.MyEntity IsUsing =>
            null;

        public override Vector3D LocationForHudMarker =>
            (base.LocationForHudMarker + (((0.65 * base.CubeGrid.GridSize) * this.BlockDefinition.Size.Y) * base.PositionComp.WorldMatrix.Up));

        public MyShipControllerDefinition BlockDefinition =>
            (base.BlockDefinition as MyShipControllerDefinition);

        public bool ControlThrusters
        {
            get => 
                ((bool) this.m_controlThrusters);
            set => 
                (this.m_controlThrusters.Value = value);
        }

        public bool ControlWheels
        {
            get => 
                ((bool) this.m_controlWheels);
            set => 
                (this.m_controlWheels.Value = value);
        }

        public VRage.Game.Entity.MyEntity Entity =>
            this;

        public MyControllerInfo ControllerInfo =>
            this.m_info;

        MyRechargeSocket IMyRechargeSocketOwner.RechargeSocket =>
            this.m_rechargeSocket;

        public bool SingleWeaponMode
        {
            get => 
                this.m_singleWeaponMode;
            private set
            {
                if (this.m_singleWeaponMode != value)
                {
                    this.m_singleWeaponMode = value;
                    if (this.m_selectedGunId != null)
                    {
                        this.SwitchToWeapon(this.m_selectedGunId.Value);
                    }
                    else
                    {
                        this.SwitchToWeapon((MyToolbarItemWeapon) null);
                    }
                }
            }
        }

        public bool IsMainCockpit
        {
            get => 
                ((bool) this.m_isMainCockpit);
            set => 
                (this.m_isMainCockpit.Value = value);
        }

        public bool HorizonIndicatorEnabled
        {
            get => 
                ((bool) this.m_horizonIndicatorEnabled);
            set => 
                (this.m_horizonIndicatorEnabled.Value = value);
        }

        public virtual MyToolbarType ToolbarType =>
            (this.m_enableShipControl ? MyToolbarType.Ship : MyToolbarType.Seat);

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ForceFirstPersonCamera
        {
            get => 
                this.ForceFirstPersonCamera;
            set => 
                (this.ForceFirstPersonCamera = value);
        }

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledThrusts =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledDamping =>
            ((this.EntityThrustComponent != null) && this.EntityThrustComponent.DampenersEnabled);

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledLights =>
            (this.GridReflectorLights.ReflectorsEnabled == MyMultipleEnabledEnum.AllEnabled);

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledLeadingGears
        {
            get
            {
                MyMultipleEnabledEnum locked = base.CubeGrid.GridSystems.LandingSystem.Locked;
                return ((locked == MyMultipleEnabledEnum.Mixed) || (locked == MyMultipleEnabledEnum.AllEnabled));
            }
        }

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledReactors =>
            ((this.GridResourceDistributor != null) && (this.GridResourceDistributor.SourcesEnabled != MyMultipleEnabledEnum.AllDisabled));

        bool Sandbox.Game.Entities.IMyControllableEntity.EnabledBroadcasting =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledHelmet =>
            false;

        public virtual float HeadLocalXAngle { get; set; }

        public virtual float HeadLocalYAngle { get; set; }

        bool Sandbox.ModAPI.Ingame.IMyShipController.IsUnderControl =>
            (this.ControllerInfo.Controller != null);

        bool Sandbox.ModAPI.Ingame.IMyShipController.ControlWheels
        {
            get => 
                this.ControlWheels;
            set
            {
                if ((this.m_enableShipControl && (this.GridWheels.WheelCount > 0)) && this.IsMainCockpitFree())
                {
                    this.ControlWheels = value;
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyShipController.ControlThrusters
        {
            get => 
                this.ControlThrusters;
            set
            {
                if (this.m_enableShipControl && this.IsMainCockpitFree())
                {
                    this.ControlThrusters = value;
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyShipController.HandBrake
        {
            get => 
                base.CubeGrid.GridSystems.WheelSystem.HandBrake;
            set
            {
                if (((this.m_enableShipControl && (this.GridWheels.WheelCount > 0)) && this.IsMainCockpitFree()) && (base.CubeGrid.GridSystems.WheelSystem.HandBrake != value))
                {
                    this.SwitchHandbrake();
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyShipController.DampenersOverride
        {
            get => 
                ((this.EntityThrustComponent != null) && this.EntityThrustComponent.DampenersEnabled);
            set
            {
                if (this.m_enableShipControl)
                {
                    base.CubeGrid.EnableDampingInternal(value, true);
                }
            }
        }

        Vector3 Sandbox.ModAPI.Ingame.IMyShipController.MoveIndicator =>
            this.MoveIndicator;

        Vector2 Sandbox.ModAPI.Ingame.IMyShipController.RotationIndicator =>
            this.RotationIndicator;

        float Sandbox.ModAPI.Ingame.IMyShipController.RollIndicator =>
            this.RollIndicator;

        Vector3D Sandbox.ModAPI.Ingame.IMyShipController.CenterOfMass =>
            base.CubeGrid.Physics.CenterOfMassWorld;

        public MyStringId ControlContext =>
            MySpaceBindingCreator.CX_SPACESHIP;

        public VRage.Game.Entity.MyEntity RelativeDampeningEntity
        {
            get => 
                base.CubeGrid.GridSystems.ControlSystem.RelativeDampeningEntity;
            set => 
                (base.CubeGrid.GridSystems.ControlSystem.RelativeDampeningEntity = value);
        }

        VRage.ModAPI.IMyEntity VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Entity =>
            this.Entity;

        IMyControllerInfo VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ControllerInfo =>
            this.ControllerInfo;

        bool Sandbox.ModAPI.Ingame.IMyShipController.CanControlShip =>
            this.EnableShipControl;

        bool Sandbox.ModAPI.Ingame.IMyShipController.HasWheels =>
            (this.GridWheels.WheelCount > 0);

        bool Sandbox.ModAPI.Ingame.IMyShipController.ShowHorizonIndicator
        {
            get => 
                this.HorizonIndicatorEnabled;
            set
            {
                if (this.CanHaveHorizon())
                {
                    this.HorizonIndicatorEnabled = value;
                }
            }
        }

        Vector3 Sandbox.ModAPI.IMyShipController.MoveIndicator =>
            this.MoveIndicator;

        Vector2 Sandbox.ModAPI.IMyShipController.RotationIndicator =>
            this.RotationIndicator;

        float Sandbox.ModAPI.IMyShipController.RollIndicator =>
            this.RollIndicator;

        bool Sandbox.ModAPI.IMyShipController.HasFirstPersonCamera =>
            this.EnableFirstPersonView;

        IMyCharacter Sandbox.ModAPI.IMyShipController.Pilot =>
            this.Pilot;

        IMyCharacter Sandbox.ModAPI.IMyShipController.LastPilot =>
            this.m_lastPilot;

        bool Sandbox.ModAPI.IMyShipController.IsShooting =>
            this.IsShooting();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyShipController.<>c <>9 = new MyShipController.<>c();
            public static MyTerminalValueControl<MyShipController, bool>.GetterDelegate <>9__78_8;
            public static MyTerminalValueControl<MyShipController, bool>.SetterDelegate <>9__78_9;
            public static Func<MyShipController, bool> <>9__78_10;
            public static Func<MyShipController, bool> <>9__78_11;
            public static Func<MyShipController, bool> <>9__78_12;
            public static MyTerminalValueControl<MyShipController, bool>.GetterDelegate <>9__78_13;
            public static MyTerminalValueControl<MyShipController, bool>.SetterDelegate <>9__78_14;
            public static Func<MyShipController, bool> <>9__78_15;
            public static Func<MyShipController, bool> <>9__78_16;
            public static Func<MyShipController, bool> <>9__78_17;
            public static MyTerminalValueControl<MyShipController, bool>.GetterDelegate <>9__78_18;
            public static MyTerminalValueControl<MyShipController, bool>.SetterDelegate <>9__78_19;
            public static Func<MyShipController, bool> <>9__78_20;
            public static Func<MyShipController, bool> <>9__78_21;
            public static Func<MyShipController, bool> <>9__78_22;
            public static MyTerminalValueControl<MyShipController, bool>.GetterDelegate <>9__78_23;
            public static MyTerminalValueControl<MyShipController, bool>.SetterDelegate <>9__78_24;
            public static Func<MyShipController, bool> <>9__78_25;
            public static Func<MyShipController, bool> <>9__78_26;
            public static Func<MyShipController, bool> <>9__78_27;
            public static MyTerminalValueControl<MyShipController, bool>.GetterDelegate <>9__78_0;
            public static MyTerminalValueControl<MyShipController, bool>.SetterDelegate <>9__78_1;
            public static Func<MyShipController, bool> <>9__78_2;
            public static Func<MyShipController, bool> <>9__78_3;
            public static MyTerminalValueControl<MyShipController, bool>.GetterDelegate <>9__78_4;
            public static MyTerminalValueControl<MyShipController, bool>.SetterDelegate <>9__78_5;
            public static Func<MyShipController, bool> <>9__78_6;
            public static Func<MyShipController, bool> <>9__78_7;
            public static Func<MyShipController, Action> <>9__134_0;
            public static Func<MyCubeGrid, Action<bool>> <>9__154_0;
            public static Func<MyShipController, Action> <>9__178_0;
            public static Func<MyShipController, Action> <>9__254_0;
            public static Func<MyShipController, Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>> <>9__310_0;
            public static Func<MyShipController, Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>> <>9__311_0;
            public static Func<MyShipController, Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>> <>9__311_1;
            public static Func<MyShipController, Action> <>9__314_0;
            public static Func<MyShipController, Action<MyShootActionEnum>> <>9__316_0;
            public static Func<MyShipController, Action<MyShootActionEnum>> <>9__321_0;
            public static Func<MyShipController, Action<MyObjectBuilder_ToolbarItem, int>> <>9__323_0;
            public static Func<MyShipController, Action<int>> <>9__323_1;

            internal Action<MyShootActionEnum> <BeginShootSync>b__316_0(MyShipController x) => 
                new Action<MyShootActionEnum>(x.ShootBeginCallback);

            internal bool <CreateTerminalControls>b__78_0(MyShipController x) => 
                x.HorizonIndicatorEnabled;

            internal void <CreateTerminalControls>b__78_1(MyShipController x, bool v)
            {
                x.HorizonIndicatorEnabled = v;
            }

            internal bool <CreateTerminalControls>b__78_10(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_11(MyShipController x) => 
                x.IsMainCockpitFree();

            internal bool <CreateTerminalControls>b__78_12(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_13(MyShipController x) => 
                x.ControlWheels;

            internal void <CreateTerminalControls>b__78_14(MyShipController x, bool v)
            {
                x.ControlWheels = v;
            }

            internal bool <CreateTerminalControls>b__78_15(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_16(MyShipController x) => 
                ((x.GridWheels.WheelCount > 0) && x.IsMainCockpitFree());

            internal bool <CreateTerminalControls>b__78_17(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_18(MyShipController x) => 
                x.CubeGrid.GridSystems.WheelSystem.HandBrake;

            internal void <CreateTerminalControls>b__78_19(MyShipController x, bool v)
            {
                x.SwitchHandbrake();
            }

            internal bool <CreateTerminalControls>b__78_2(MyShipController x) => 
                true;

            internal bool <CreateTerminalControls>b__78_20(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_21(MyShipController x) => 
                ((x.GridWheels.WheelCount > 0) && x.IsMainCockpitFree());

            internal bool <CreateTerminalControls>b__78_22(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_23(MyShipController x) => 
                ((x.EntityThrustComponent != null) && x.EntityThrustComponent.DampenersEnabled);

            internal void <CreateTerminalControls>b__78_24(MyShipController x, bool v)
            {
                x.CubeGrid.EnableDampingInternal(v, true);
            }

            internal bool <CreateTerminalControls>b__78_25(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_26(MyShipController x) => 
                x.m_enableShipControl;

            internal bool <CreateTerminalControls>b__78_27(MyShipController x) => 
                x.IsMainCockpitFree();

            internal bool <CreateTerminalControls>b__78_3(MyShipController x) => 
                x.CanHaveHorizon();

            internal bool <CreateTerminalControls>b__78_4(MyShipController x) => 
                x.IsMainCockpit;

            internal void <CreateTerminalControls>b__78_5(MyShipController x, bool v)
            {
                x.IsMainCockpit = v;
            }

            internal bool <CreateTerminalControls>b__78_6(MyShipController x) => 
                x.IsMainCockpitFree();

            internal bool <CreateTerminalControls>b__78_7(MyShipController x) => 
                x.CanBeMainCockpit();

            internal bool <CreateTerminalControls>b__78_8(MyShipController x) => 
                x.ControlThrusters;

            internal void <CreateTerminalControls>b__78_9(MyShipController x, bool v)
            {
                x.ControlThrusters = v;
            }

            internal Action<MyShootActionEnum> <EndShootInternal>b__321_0(MyShipController x) => 
                new Action<MyShootActionEnum>(x.ShootEndCallback);

            internal Action <OnSwitchAmmoMagazineRequest>b__314_0(MyShipController x) => 
                new Action(x.OnSwitchAmmoMagazineSuccess);

            internal Action <RaiseControlledEntityUsed>b__134_0(MyShipController x) => 
                new Action(x.sync_ControlledEntity_Used);

            internal Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long> <RequestSwitchToWeapon>b__310_0(MyShipController x) => 
                new Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(x.SwitchToWeaponMessage);

            internal Action <SwitchAmmoMagazineInternal>b__178_0(MyShipController x) => 
                new Action(x.OnSwitchAmmoMagazineRequest);

            internal Action<bool> <SwitchLandingGears>b__154_0(MyCubeGrid x) => 
                new Action<bool>(x.SetHandbrakeRequest);

            internal Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long> <SwitchToWeaponMessage>b__311_0(MyShipController x) => 
                new Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(x.OnSwitchToWeaponFailure);

            internal Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long> <SwitchToWeaponMessage>b__311_1(MyShipController x) => 
                new Action<SerializableDefinitionId?, MyObjectBuilder_EntityBase, long>(x.OnSwitchToWeaponSuccess);

            internal Action<MyObjectBuilder_ToolbarItem, int> <Toolbar_ItemChanged>b__323_0(MyShipController x) => 
                new Action<MyObjectBuilder_ToolbarItem, int>(x.SendToolbarItemChanged);

            internal Action<int> <Toolbar_ItemChanged>b__323_1(MyShipController x) => 
                new Action<int>(x.SendToolbarItemRemoved);

            internal Action <VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchHelmet>b__254_0(MyShipController x) => 
                new Action(x.OnSwitchHelmet);
        }
    }
}

