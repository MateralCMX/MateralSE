namespace SpaceEngineers.Game.Weapons.Guns
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;

    [MyCubeBlockType(typeof(MyObjectBuilder_ConveyorTurretBase)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyLargeConveyorTurretBase), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyLargeConveyorTurretBase) })]
    public abstract class MyLargeConveyorTurretBase : MyLargeTurretBase, IMyConveyorEndpointBlock, SpaceEngineers.Game.ModAPI.IMyLargeConveyorTurretBase, Sandbox.ModAPI.IMyLargeTurretBase, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun, Sandbox.ModAPI.Ingame.IMyLargeTurretBase, IMyCameraController, SpaceEngineers.Game.ModAPI.Ingame.IMyLargeConveyorTurretBase, IMyInventoryOwner
    {
        protected readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private MyMultilineConveyorEndpoint m_endpoint;

        public MyLargeConveyorTurretBase()
        {
            this.CreateTerminalControls();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public bool AllowSelfPulling() => 
            false;

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyLargeConveyorTurretBase>())
            {
                base.CreateTerminalControls();
                MyTerminalControlFactory.AddControl<MyLargeConveyorTurretBase>(new MyTerminalControlSeparator<MyLargeConveyorTurretBase>());
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase> switch1 = new MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase> switch2 = new MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyLargeConveyorTurretBase> onOff = local5;
                onOff.EnableToggleAction<MyLargeConveyorTurretBase>();
                MyTerminalControlFactory.AddControl<MyLargeConveyorTurretBase>(onOff);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ConveyorTurretBase objectBuilderCubeBlock = (MyObjectBuilder_ConveyorTurretBase) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = information1.Inventory.Constraint;
            return information1;
        }

        public PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            this.m_useConveyorSystem.SetLocalValue(true);
            MyObjectBuilder_ConveyorTurretBase base2 = objectBuilder as MyObjectBuilder_ConveyorTurretBase;
            if (base2 != null)
            {
                this.m_useConveyorSystem.SetLocalValue(base2.UseConveyorSystem);
            }
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_endpoint = new MyMultilineConveyorEndpoint(this);
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (((this.m_useConveyorSystem != null) && (MySession.Static.SurvivalMode && (Sync.IsServer && base.IsWorking))) && (this.GetInventory(0).VolumeFillFactor < 0.5f))
            {
                MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, base.m_gunBase.CurrentAmmoMagazineId, 1, false);
            }
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_endpoint;

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyLargeConveyorTurretBase.UseConveyorSystem =>
            ((bool) this.m_useConveyorSystem);

        private bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
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
            public static readonly MyLargeConveyorTurretBase.<>c <>9 = new MyLargeConveyorTurretBase.<>c();
            public static MyTerminalValueControl<MyLargeConveyorTurretBase, bool>.GetterDelegate <>9__5_0;
            public static MyTerminalValueControl<MyLargeConveyorTurretBase, bool>.SetterDelegate <>9__5_1;

            internal bool <CreateTerminalControls>b__5_0(MyLargeConveyorTurretBase x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__5_1(MyLargeConveyorTurretBase x, bool v)
            {
                x.UseConveyorSystem = v;
            }
        }
    }
}

