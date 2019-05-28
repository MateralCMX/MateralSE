namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    [MyCubeBlockType(typeof(MyObjectBuilder_CargoContainer)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyCargoContainer), typeof(Sandbox.ModAPI.Ingame.IMyCargoContainer) })]
    public class MyCargoContainer : MyTerminalBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyCargoContainer, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, Sandbox.ModAPI.Ingame.IMyCargoContainer, IMyInventoryOwner
    {
        private MyCargoContainerDefinition m_cargoDefinition;
        private bool m_useConveyorSystem = true;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private string m_containerType;

        public bool AllowSelfPulling() => 
            false;

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_CargoContainer objectBuilderCubeBlock = (MyObjectBuilder_CargoContainer) base.GetObjectBuilderCubeBlock(copy);
            if (this.m_containerType != null)
            {
                objectBuilderCubeBlock.ContainerType = this.m_containerType;
            }
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            this.m_cargoDefinition = (MyCargoContainerDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(objectBuilder.GetId());
            MyObjectBuilder_CargoContainer container = (MyObjectBuilder_CargoContainer) objectBuilder;
            this.m_containerType = container.ContainerType;
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            if (this.GetInventory(0) == null)
            {
                MyInventory component = new MyInventory(this.m_cargoDefinition.InventorySize.Volume, this.m_cargoDefinition.InventorySize, MyInventoryFlags.CanSend | MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(component);
                if (((this.m_containerType != null) && MyFakes.RANDOM_CARGO_PLACEMENT) && ((container.Inventory == null) || (container.Inventory.Items.Count == 0)))
                {
                    this.SpawnRandomCargo();
                }
            }
            if ((container.Inventory != null) && (container.Inventory.Items.Count > 0))
            {
                this.GetInventory(0).Init(container.Inventory);
            }
            this.GetInventory(0).SetFlags(MyInventoryFlags.CanSend | MyInventoryFlags.CanReceive);
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
            base.UpdateIsWorking();
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        public void SpawnRandomCargo()
        {
            if (this.m_containerType != null)
            {
                MyContainerTypeDefinition containerTypeDefinition = MyDefinitionManager.Static.GetContainerTypeDefinition(this.m_containerType);
                if ((containerTypeDefinition != null) && (containerTypeDefinition.Items.Length != 0))
                {
                    this.GetInventory(0).GenerateContent(containerTypeDefinition);
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            MyContainerDropComponent component;
            base.UpdateBeforeSimulation100();
            if (base.Components.TryGet<MyContainerDropComponent>(out component))
            {
                component.UpdateSound();
            }
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public string ContainerType
        {
            get => 
                this.m_containerType;
            set => 
                (this.m_containerType = value);
        }

        private bool UseConveyorSystem
        {
            get => 
                this.m_useConveyorSystem;
            set => 
                (this.m_useConveyorSystem = value);
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
    }
}

