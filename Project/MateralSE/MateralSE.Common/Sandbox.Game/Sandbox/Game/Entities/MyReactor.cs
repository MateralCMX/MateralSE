namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;

    [MyCubeBlockType(typeof(MyObjectBuilder_Reactor)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyReactor), typeof(Sandbox.ModAPI.Ingame.IMyReactor) })]
    public class MyReactor : MyFueledPowerProducer, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyReactor, Sandbox.ModAPI.Ingame.IMyReactor, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyPowerProducer, IMyInventoryOwner
    {
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private float m_powerOutputMultiplier = 1f;

        public MyReactor()
        {
            this.CreateTerminalControls();
        }

        public override void CheckEmissiveState(bool force = false)
        {
            if (base.IsWorking)
            {
                this.SetEmissiveStateWorking();
            }
            else if (!base.IsFunctional)
            {
                this.SetEmissiveStateDamaged();
            }
            else if (base.Enabled)
            {
                base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Warning, base.Render.RenderObjectIDs[0], "Emissive");
            }
            else
            {
                this.SetEmissiveStateDisabled();
            }
        }

        private void ConsumeFuel(int timeDeltaMilliseconds)
        {
            if (base.SourceComp.HasCapacityRemaining)
            {
                float currentOutput = base.SourceComp.CurrentOutput;
                if (currentOutput != 0f)
                {
                    MyInventory inventory = this.GetInventory(0);
                    float num2 = currentOutput / this.BlockDefinition.MaxPowerOutput;
                    foreach (MyReactorDefinition.FuelInfo info in this.BlockDefinition.FuelInfos)
                    {
                        float num4 = (num2 * info.ConsumptionPerSecond_Items) / 1000f;
                        MyFixedPoint b = (MyFixedPoint) (timeDeltaMilliseconds * num4);
                        if (b == 0)
                        {
                            b = MyFixedPoint.SmallestPossibleValue;
                        }
                        inventory.RemoveItemsOfType(MyFixedPoint.Min(inventory.GetItemAmount(info.FuelId, MyItemFlags.None, false), b), info.FuelId, MyItemFlags.None, false);
                        if (MyFakes.ENABLE_INFINITE_REACTOR_FUEL && !inventory.ContainItems(new MyFixedPoint?(b), info.FuelId, MyItemFlags.None))
                        {
                            inventory.AddItems((MyFixedPoint) (50 * b), info.FuelItem);
                        }
                    }
                }
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyReactor>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyReactor> switch1 = new MyTerminalControlOnOffSwitch<MyReactor>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyReactor> switch2 = new MyTerminalControlOnOffSwitch<MyReactor>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyReactor> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyReactor> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyReactor> onOff = local5;
                onOff.EnableToggleAction<MyReactor>();
                MyTerminalControlFactory.AddControl<MyReactor>(onOff);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Reactor objectBuilderCubeBlock = (MyObjectBuilder_Reactor) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            return objectBuilderCubeBlock;
        }

        public override PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.OwnerID = base.OwnerId;
            information1.Inventory = this.GetInventory(0);
            information1.Constraint = this.BlockDefinition.InventoryConstraint;
            return information1;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_Reactor reactor = (MyObjectBuilder_Reactor) objectBuilder;
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            MyInventory component = this.GetInventory(0);
            if (component == null)
            {
                component = new MyInventory(this.BlockDefinition.InventoryMaxVolume, this.BlockDefinition.InventorySize, MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(component);
                component.Init(reactor.Inventory);
            }
            component.Constraint = this.BlockDefinition.InventoryConstraint;
            if (Sync.IsServer)
            {
                this.RefreshRemainingCapacity();
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.m_useConveyorSystem.SetLocalValue(reactor.UseConveyorSystem);
        }

        protected override void OnCurrentOrMaxOutputChanged(MyDefinitionId resourceTypeId, float oldOutput, MyResourceSourceComponent source)
        {
            base.OnCurrentOrMaxOutputChanged(resourceTypeId, oldOutput, source);
            if (((base.SoundEmitter != null) && (base.SoundEmitter.Sound != null)) && base.SoundEmitter.Sound.IsPlaying)
            {
                if (base.SourceComp.MaxOutput != 0f)
                {
                    float semitones = (4f * (base.SourceComp.CurrentOutput - (0.5f * base.SourceComp.MaxOutput))) / base.SourceComp.MaxOutput;
                    base.SoundEmitter.Sound.FrequencyRatio = MyAudio.Static.SemitonesToFrequencyRatio(semitones);
                }
                else
                {
                    base.SoundEmitter.Sound.FrequencyRatio = 1f;
                }
            }
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.CheckEmissiveState(true);
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            if (Sync.IsServer && (this.GetInventory(0) != null))
            {
                this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.OnInventoryContentChanged);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
            MyInventory inventory2 = inventory as MyInventory;
            if (Sync.IsServer && (inventory2 != null))
            {
                inventory2.ContentsChanged -= new Action<MyInventoryBase>(this.OnInventoryContentChanged);
            }
        }

        private void OnInventoryContentChanged(MyInventoryBase obj)
        {
            this.RefreshRemainingCapacity();
            bool isWorking = base.IsWorking;
            if (base.IsWorking != isWorking)
            {
                if (isWorking)
                {
                    this.OnStartWorking();
                }
                else
                {
                    this.OnStopWorking();
                }
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        private void RefreshRemainingCapacity()
        {
            MyInventory inventory = this.GetInventory(0);
            if ((inventory != null) && Sync.IsServer)
            {
                float maxValue = float.MaxValue;
                MyReactorDefinition.FuelInfo[] fuelInfos = this.BlockDefinition.FuelInfos;
                int index = 0;
                while (true)
                {
                    if (index >= fuelInfos.Length)
                    {
                        if ((maxValue == 0f) && MySession.Static.CreativeMode)
                        {
                            MyReactorDefinition.FuelInfo info2 = this.BlockDefinition.FuelInfos[0];
                            maxValue = info2.FuelDefinition.Mass / info2.Ratio;
                        }
                        base.Capacity = maxValue;
                        break;
                    }
                    MyReactorDefinition.FuelInfo info = fuelInfos[index];
                    float num3 = ((float) inventory.GetItemAmount(info.FuelId, MyItemFlags.None, false)) / info.Ratio;
                    maxValue = Math.Min(maxValue, num3);
                    index++;
                }
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if ((base.Enabled && (!MySession.Static.CreativeMode && Sync.IsServer)) && base.IsWorking)
            {
                int timeDeltaMilliseconds = 0x640;
                this.ConsumeFuel(timeDeltaMilliseconds);
            }
            if ((Sync.IsServer && (base.IsFunctional && (this.m_useConveyorSystem != null))) && (this.GetInventory(0).VolumeFillFactor < 0.6f))
            {
                foreach (MyReactorDefinition.FuelInfo info in this.BlockDefinition.FuelInfos)
                {
                    float num3 = info.ConsumptionPerSecond_Items * 60f;
                    MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, info.FuelId, new MyFixedPoint?((MyFixedPoint) num3), false);
                }
            }
        }

        IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public MyReactorDefinition BlockDefinition =>
            ((MyReactorDefinition) base.BlockDefinition);

        public override float MaxOutput =>
            (base.MaxOutput * this.m_powerOutputMultiplier);

        public bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        bool Sandbox.ModAPI.Ingame.IMyReactor.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set => 
                (this.UseConveyorSystem = value);
        }

        float Sandbox.ModAPI.IMyReactor.PowerOutputMultiplier
        {
            get => 
                this.m_powerOutputMultiplier;
            set
            {
                this.m_powerOutputMultiplier = value;
                if (this.m_powerOutputMultiplier < 0.01f)
                {
                    this.m_powerOutputMultiplier = 0.01f;
                }
                this.OnProductionChanged();
            }
        }

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyReactor.<>c <>9 = new MyReactor.<>c();
            public static MyTerminalValueControl<MyReactor, bool>.GetterDelegate <>9__5_0;
            public static MyTerminalValueControl<MyReactor, bool>.SetterDelegate <>9__5_1;

            internal bool <CreateTerminalControls>b__5_0(MyReactor x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__5_1(MyReactor x, bool v)
            {
                x.UseConveyorSystem = v;
            }
        }
    }
}

