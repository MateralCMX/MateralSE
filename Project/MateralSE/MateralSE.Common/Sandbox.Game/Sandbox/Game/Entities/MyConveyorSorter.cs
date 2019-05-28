namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;

    [MyCubeBlockType(typeof(MyObjectBuilder_ConveyorSorter)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyConveyorSorter), typeof(Sandbox.ModAPI.Ingame.IMyConveyorSorter) })]
    public class MyConveyorSorter : MyFunctionalBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyConveyorSorter, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyConveyorSorter, IMyInventoryOwner
    {
        private MyStringHash m_prevColor = MyStringHash.NullOrEmpty;
        private readonly MyInventoryConstraint m_inventoryConstraint = new MyInventoryConstraint(string.Empty, null, true);
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_drainAll;
        private MyConveyorSorterDefinition m_conveyorSorterDefinition;
        private int m_pushRequestFrameCounter;
        private static readonly StringBuilder m_helperSB = new StringBuilder();
        private static MyTerminalControlOnOffSwitch<MyConveyorSorter> drainAll;
        private static MyTerminalControlCombobox<MyConveyorSorter> blacklistWhitelist;
        private static MyTerminalControlListbox<MyConveyorSorter> currentList;
        private static MyTerminalControlButton<MyConveyorSorter> removeFromSelectionButton;
        private static MyTerminalControlListbox<MyConveyorSorter> candidates;
        private static MyTerminalControlButton<MyConveyorSorter> addToSelectionButton;
        private static readonly Dictionary<byte, Tuple<MyObjectBuilderType, StringBuilder>> CandidateTypes = new Dictionary<byte, Tuple<MyObjectBuilderType, StringBuilder>>();
        private static readonly Dictionary<MyObjectBuilderType, byte> CandidateTypesToId = new Dictionary<MyObjectBuilderType, byte>();
        private bool m_allowCurrentListUpdate = true;
        private List<MyGuiControlListbox.Item> m_selectedForDelete;
        private List<MyGuiControlListbox.Item> m_selectedForAdd;

        static MyConveyorSorter()
        {
            byte num = 0;
            CandidateTypes.Add(num = (byte) (num + 1), new Tuple<MyObjectBuilderType, StringBuilder>(typeof(MyObjectBuilder_AmmoMagazine), MyTexts.Get(MySpaceTexts.DisplayName_ConvSorterTypes_Ammo)));
            CandidateTypes.Add(num = (byte) (num + 1), new Tuple<MyObjectBuilderType, StringBuilder>(typeof(MyObjectBuilder_Component), MyTexts.Get(MySpaceTexts.DisplayName_ConvSorterTypes_Component)));
            CandidateTypes.Add(num = (byte) (num + 1), new Tuple<MyObjectBuilderType, StringBuilder>(typeof(MyObjectBuilder_PhysicalGunObject), MyTexts.Get(MySpaceTexts.DisplayName_ConvSorterTypes_HandTool)));
            CandidateTypes.Add(num = (byte) (num + 1), new Tuple<MyObjectBuilderType, StringBuilder>(typeof(MyObjectBuilder_Ingot), MyTexts.Get(MySpaceTexts.DisplayName_ConvSorterTypes_Ingot)));
            CandidateTypes.Add(num = (byte) (num + 1), new Tuple<MyObjectBuilderType, StringBuilder>(typeof(MyObjectBuilder_Ore), MyTexts.Get(MySpaceTexts.DisplayName_ConvSorterTypes_Ore)));
            foreach (KeyValuePair<byte, Tuple<MyObjectBuilderType, StringBuilder>> pair in CandidateTypes)
            {
                CandidateTypesToId.Add(pair.Value.Item1, pair.Key);
            }
        }

        public MyConveyorSorter()
        {
            this.CreateTerminalControls();
            this.m_drainAll.ValueChanged += x => this.DoChangeDrainAll();
        }

        private void AddToCurrentList()
        {
            this.ModifyCurrentList(ref this.m_selectedForAdd, true);
        }

        public bool AllowSelfPulling() => 
            false;

        public void ChangeBlWl(bool IsWl)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyConveyorSorter, bool>(this, x => new Action<bool>(x.DoChangeBlWl), IsWl, targetEndpoint);
        }

        private void ChangeListId(SerializableDefinitionId id, bool wasAdded)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyConveyorSorter, SerializableDefinitionId, bool>(this, x => new Action<SerializableDefinitionId, bool>(x.DoChangeListId), id, wasAdded, targetEndpoint);
        }

        private void ChangeListType(byte type, bool wasAdded)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyConveyorSorter, byte, bool>(this, x => new Action<byte, bool>(x.DoChangeListType), type, wasAdded, targetEndpoint);
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
            this.UpdateText();
            this.UpdateEmissivity();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyConveyorSorter>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                drainAll = new MyTerminalControlOnOffSwitch<MyConveyorSorter>("DrainAll", MySpaceTexts.Terminal_DrainAll, tooltip, on, on);
                drainAll.Getter = block => block.DrainAll;
                drainAll.Setter = (block, val) => block.DrainAll = val;
                drainAll.EnableToggleAction<MyConveyorSorter>();
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(drainAll);
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(new MyTerminalControlSeparator<MyConveyorSorter>());
                blacklistWhitelist = new MyTerminalControlCombobox<MyConveyorSorter>("blacklistWhitelist", MySpaceTexts.BlockPropertyTitle_ConveyorSorterFilterMode, MySpaceTexts.Blank);
                blacklistWhitelist.ComboBoxContent = block => FillBlWlCombo(block);
                blacklistWhitelist.Getter = block => block.IsWhitelist ? ((MyTerminalValueControl<MyConveyorSorter, long>.GetterDelegate) 1) : ((MyTerminalValueControl<MyConveyorSorter, long>.GetterDelegate) 0);
                blacklistWhitelist.Setter = (block, val) => block.ChangeBlWl(val == 1L);
                blacklistWhitelist.SetSerializerBit();
                blacklistWhitelist.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(blacklistWhitelist);
                currentList = new MyTerminalControlListbox<MyConveyorSorter>("CurrentList", MySpaceTexts.BlockPropertyTitle_ConveyorSorterFilterItemsList, MySpaceTexts.Blank, true, 8);
                currentList.ListContent = (block, list1, list2) => block.FillCurrentList(list1, list2);
                currentList.ItemSelected = (block, val) => block.SelectFromCurrentList(val);
                currentList.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(currentList);
                removeFromSelectionButton = new MyTerminalControlButton<MyConveyorSorter>("removeFromSelectionButton", MySpaceTexts.BlockPropertyTitle_ConveyorSorterRemove, MySpaceTexts.Blank, block => block.RemoveFromCurrentList());
                removeFromSelectionButton.Enabled = x => (x.m_selectedForDelete != null) && (x.m_selectedForDelete.Count > 0);
                removeFromSelectionButton.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(removeFromSelectionButton);
                candidates = new MyTerminalControlListbox<MyConveyorSorter>("candidatesList", MySpaceTexts.BlockPropertyTitle_ConveyorSorterCandidatesList, MySpaceTexts.Blank, true, 8);
                candidates.ListContent = (block, list1, list2) => block.FillCandidatesList(list1, list2);
                candidates.ItemSelected = (block, val) => block.SelectCandidate(val);
                candidates.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(candidates);
                addToSelectionButton = new MyTerminalControlButton<MyConveyorSorter>("addToSelectionButton", MySpaceTexts.BlockPropertyTitle_ConveyorSorterAdd, MySpaceTexts.Blank, x => x.AddToCurrentList());
                addToSelectionButton.SupportsMultipleBlocks = false;
                addToSelectionButton.Enabled = x => (x.m_selectedForAdd != null) && (x.m_selectedForAdd.Count > 0);
                MyTerminalControlFactory.AddControl<MyConveyorSorter>(addToSelectionButton);
            }
        }

        [Event(null, 0x156), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void DoChangeBlWl(bool IsWl)
        {
            this.IsWhitelist = IsWl;
            blacklistWhitelist.UpdateVisual();
        }

        internal void DoChangeDrainAll()
        {
            this.DrainAll = (bool) this.m_drainAll;
            drainAll.UpdateVisual();
        }

        [Event(null, 0x162), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void DoChangeListId(SerializableDefinitionId id, bool add)
        {
            if (add)
            {
                this.m_inventoryConstraint.Add(id);
            }
            else
            {
                this.m_inventoryConstraint.Remove(id);
            }
            base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
            if (this.m_allowCurrentListUpdate)
            {
                currentList.UpdateVisual();
            }
        }

        [Event(null, 0x176), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void DoChangeListType(byte type, bool add)
        {
            Tuple<MyObjectBuilderType, StringBuilder> tuple;
            if (CandidateTypes.TryGetValue(type, out tuple))
            {
                if (add)
                {
                    this.m_inventoryConstraint.AddObjectBuilderType(tuple.Item1);
                }
                else
                {
                    this.m_inventoryConstraint.RemoveObjectBuilderType(tuple.Item1);
                }
                base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
                if (this.m_allowCurrentListUpdate)
                {
                    currentList.UpdateVisual();
                }
            }
        }

        [Event(null, 0x26e), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void DoSetupFilter(MyConveyorSorterMode mode, List<MyInventoryItemFilter> items)
        {
            this.IsWhitelist = mode == MyConveyorSorterMode.Whitelist;
            this.m_inventoryConstraint.Clear();
            if (items != null)
            {
                this.m_allowCurrentListUpdate = false;
                try
                {
                    foreach (MyInventoryItemFilter filter in items)
                    {
                        if (filter.AllSubTypes)
                        {
                            this.m_inventoryConstraint.AddObjectBuilderType(filter.ItemId.TypeId);
                            continue;
                        }
                        this.m_inventoryConstraint.Add(filter.ItemId);
                    }
                }
                finally
                {
                    this.m_allowCurrentListUpdate = true;
                }
            }
            base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
            currentList.UpdateVisual();
        }

        private static void FillBlWlCombo(List<MyTerminalControlComboBoxItem> list)
        {
            MyTerminalControlComboBoxItem item = new MyTerminalControlComboBoxItem {
                Key = 0L,
                Value = MySpaceTexts.BlockPropertyTitle_ConveyorSorterFilterModeBlacklist
            };
            list.Add(item);
            item = new MyTerminalControlComboBoxItem {
                Key = 1L,
                Value = MySpaceTexts.BlockPropertyTitle_ConveyorSorterFilterModeWhitelist
            };
            list.Add(item);
        }

        private void FillCandidatesList(ICollection<MyGuiControlListbox.Item> content, ICollection<MyGuiControlListbox.Item> selectedItems)
        {
            foreach (KeyValuePair<byte, Tuple<MyObjectBuilderType, StringBuilder>> pair in CandidateTypes)
            {
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(pair.Value.Item2, null, null, pair.Key, null);
                content.Add(item);
            }
            foreach (MyDefinitionBase base2 in from x in MyDefinitionManager.Static.GetAllDefinitions()
                orderby this.sorter(x)
                select x)
            {
                if (!base2.Public)
                {
                    continue;
                }
                MyPhysicalItemDefinition definition = base2 as MyPhysicalItemDefinition;
                if ((definition != null) && (base2.Public && definition.CanSpawnFromScreen))
                {
                    m_helperSB.Clear().Append(base2.DisplayNameText);
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(m_helperSB, null, null, definition.Id, null);
                    content.Add(item);
                }
            }
        }

        private void FillCurrentList(ICollection<MyGuiControlListbox.Item> content, ICollection<MyGuiControlListbox.Item> selectedItems)
        {
            foreach (MyObjectBuilderType type in this.m_inventoryConstraint.ConstrainedTypes)
            {
                byte num;
                Tuple<MyObjectBuilderType, StringBuilder> tuple;
                if (!CandidateTypesToId.TryGetValue(type, out num))
                {
                    continue;
                }
                if (CandidateTypes.TryGetValue(num, out tuple))
                {
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(tuple.Item2, null, null, num, null);
                    content.Add(item);
                }
            }
            foreach (MyDefinitionId id in this.m_inventoryConstraint.ConstrainedIds)
            {
                MyPhysicalItemDefinition definition;
                if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition))
                {
                    m_helperSB.Clear().Append(definition.DisplayNameText);
                }
                else
                {
                    m_helperSB.Clear().Append(id.ToString());
                }
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(m_helperSB, null, null, id, null);
                content.Add(item);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ConveyorSorter objectBuilderCubeBlock = (MyObjectBuilder_ConveyorSorter) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.DrainAll = this.DrainAll;
            objectBuilderCubeBlock.IsWhiteList = this.IsWhitelist;
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            foreach (MyDefinitionId id in this.m_inventoryConstraint.ConstrainedIds)
            {
                objectBuilderCubeBlock.DefinitionIds.Add((SerializableDefinitionId) id);
            }
            foreach (MyObjectBuilderType type in this.m_inventoryConstraint.ConstrainedTypes)
            {
                byte num;
                if (CandidateTypesToId.TryGetValue(type, out num))
                {
                    objectBuilderCubeBlock.DefinitionTypes.Add(num);
                }
            }
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = this.m_inventoryConstraint;
            return information1;
        }

        public PullInformation GetPushInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = new MyInventoryConstraint("Empty constraint", null, true);
            return information1;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            this.m_conveyorSorterDefinition = (MyConveyorSorterDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(objectBuilder.GetId());
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.m_conveyorSorterDefinition.ResourceSinkGroup, this.BlockDefinition.PowerInput, new Func<float>(this.UpdatePowerInput));
            component.IsPoweredChanged += new Action(this.IsPoweredChanged);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_ConveyorSorter sorter = (MyObjectBuilder_ConveyorSorter) objectBuilder;
            this.m_drainAll.SetLocalValue(sorter.DrainAll);
            this.IsWhitelist = sorter.IsWhiteList;
            foreach (SerializableDefinitionId id in sorter.DefinitionIds)
            {
                this.m_inventoryConstraint.Add(id);
            }
            foreach (byte num in sorter.DefinitionTypes)
            {
                Tuple<MyObjectBuilderType, StringBuilder> tuple;
                if (CandidateTypes.TryGetValue(num, out tuple))
                {
                    this.m_inventoryConstraint.AddObjectBuilderType(tuple.Item1);
                }
            }
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            if (this.GetInventory(0) == null)
            {
                MyInventory inventory = new MyInventory(this.m_conveyorSorterDefinition.InventorySize.Volume, this.m_conveyorSorterDefinition.InventorySize, MyInventoryFlags.CanSend);
                base.Components.Add<MyInventoryBase>(inventory);
                inventory.Init(sorter.Inventory);
            }
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            base.ResourceSink.Update();
            this.UpdateText();
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
        }

        public bool IsAllowed(MyDefinitionId itemId) => 
            (base.Enabled && (base.IsFunctional && (base.IsWorking && (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && this.m_inventoryConstraint.Check(itemId)))));

        private void IsPoweredChanged()
        {
            base.ResourceSink.Update();
            base.UpdateIsWorking();
            this.UpdateText();
            this.UpdateEmissivity();
        }

        private void ModifyCurrentList(ref List<MyGuiControlListbox.Item> list, bool Add)
        {
            this.m_allowCurrentListUpdate = false;
            if (list != null)
            {
                foreach (MyGuiControlListbox.Item item in list)
                {
                    MyDefinitionId? userData = item.UserData as MyDefinitionId?;
                    if (userData != null)
                    {
                        this.ChangeListId(userData.Value, Add);
                        continue;
                    }
                    byte? nullable2 = item.UserData as byte?;
                    if (nullable2 != null)
                    {
                        this.ChangeListType(nullable2.Value, Add);
                    }
                }
            }
            this.m_allowCurrentListUpdate = true;
            currentList.UpdateVisual();
            addToSelectionButton.UpdateVisual();
            removeFromSelectionButton.UpdateVisual();
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.UpdateEmissivity();
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
            base.ResourceSink.Update();
            this.UpdateText();
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

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        private void RemoveFromCurrentList()
        {
            this.ModifyCurrentList(ref this.m_selectedForDelete, false);
        }

        void Sandbox.ModAPI.Ingame.IMyConveyorSorter.AddItem(MyInventoryItemFilter item)
        {
            if (!item.AllSubTypes)
            {
                this.ChangeListId((SerializableDefinitionId) item.ItemId, true);
            }
            else
            {
                byte num;
                if (CandidateTypesToId.TryGetValue(item.ItemId.TypeId, out num))
                {
                    this.ChangeListType(num, true);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyConveyorSorter.GetFilterList(List<MyInventoryItemFilter> items)
        {
            items.Clear();
            foreach (MyObjectBuilderType type in this.m_inventoryConstraint.ConstrainedTypes)
            {
                items.Add(new MyInventoryItemFilter(new MyDefinitionId(type), true));
            }
            foreach (MyDefinitionId id in this.m_inventoryConstraint.ConstrainedIds)
            {
                items.Add(new MyInventoryItemFilter(id, false));
            }
        }

        void Sandbox.ModAPI.Ingame.IMyConveyorSorter.RemoveItem(MyInventoryItemFilter item)
        {
            if (!item.AllSubTypes)
            {
                this.ChangeListId((SerializableDefinitionId) item.ItemId, false);
            }
            else
            {
                byte num;
                if (CandidateTypesToId.TryGetValue(item.ItemId.TypeId, out num))
                {
                    this.ChangeListType(num, false);
                }
            }
        }

        void Sandbox.ModAPI.Ingame.IMyConveyorSorter.SetFilter(MyConveyorSorterMode mode, List<MyInventoryItemFilter> items)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyConveyorSorter, MyConveyorSorterMode, List<MyInventoryItemFilter>>(this, x => new Action<MyConveyorSorterMode, List<MyInventoryItemFilter>>(x.DoSetupFilter), mode, items, targetEndpoint);
        }

        private void SelectCandidate(List<MyGuiControlListbox.Item> val)
        {
            this.m_selectedForAdd = val;
            addToSelectionButton.UpdateVisual();
        }

        private void SelectFromCurrentList(List<MyGuiControlListbox.Item> val)
        {
            this.m_selectedForDelete = val;
            removeFromSelectionButton.UpdateVisual();
        }

        private string sorter(MyDefinitionBase def)
        {
            MyPhysicalItemDefinition definition = def as MyPhysicalItemDefinition;
            return definition?.DisplayNameText;
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if ((Sync.IsServer && (this.DrainAll && (base.Enabled && base.IsFunctional))) && base.IsWorking)
            {
                this.m_pushRequestFrameCounter++;
                if (this.m_pushRequestFrameCounter >= 4)
                {
                    this.m_pushRequestFrameCounter = 0;
                    if (this.GetInventory(0).GetItems().Count > 0)
                    {
                        MyGridConveyorSystem.PushAnyRequest(this, this.GetInventory(0), base.OwnerId);
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (((Sync.IsServer && (this.DrainAll && (base.Enabled && base.IsFunctional))) && base.IsWorking) && !this.GetInventory(0).IsFull)
            {
                MyFixedPoint? maxAmount = null;
                MyGridConveyorSystem.PullAllRequest(this, this.GetInventory(0), base.OwnerId, this.m_inventoryConstraint, maxAmount, true);
            }
        }

        private void UpdateEmissivity()
        {
            if (!base.IsFunctional)
            {
                if (this.m_prevColor != MyCubeBlock.m_emissiveNames.Damaged)
                {
                    this.SetEmissiveStateDamaged();
                    this.m_prevColor = MyCubeBlock.m_emissiveNames.Damaged;
                }
            }
            else
            {
                bool flag = this.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId);
                if ((this.m_prevColor != MyCubeBlock.m_emissiveNames.Working) & flag)
                {
                    this.SetEmissiveStateWorking();
                    this.m_prevColor = MyCubeBlock.m_emissiveNames.Working;
                }
                else if ((this.m_prevColor != MyCubeBlock.m_emissiveNames.Disabled) && !flag)
                {
                    this.SetEmissiveStateDisabled();
                    this.m_prevColor = MyCubeBlock.m_emissiveNames.Disabled;
                }
            }
        }

        private float UpdatePowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return this.BlockDefinition.PowerInput;
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f, base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.RaisePropertiesChanged();
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public bool IsWhitelist
        {
            get => 
                this.m_inventoryConstraint.IsWhitelist;
            private set
            {
                if (this.m_inventoryConstraint.IsWhitelist != value)
                {
                    this.m_inventoryConstraint.IsWhitelist = value;
                    base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
                }
            }
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public bool DrainAll
        {
            get => 
                ((bool) this.m_drainAll);
            set => 
                (this.m_drainAll.Value = value);
        }

        public MyConveyorSorterDefinition BlockDefinition =>
            ((MyConveyorSorterDefinition) base.BlockDefinition);

        private bool UseConveyorSystem
        {
            get => 
                true;
            set
            {
            }
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
            set
            {
                throw new NotImplementedException();
            }
        }

        MyConveyorSorterMode Sandbox.ModAPI.Ingame.IMyConveyorSorter.Mode =>
            (this.m_inventoryConstraint.IsWhitelist ? MyConveyorSorterMode.Whitelist : MyConveyorSorterMode.Blacklist);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyConveyorSorter.<>c <>9 = new MyConveyorSorter.<>c();
            public static MyTerminalValueControl<MyConveyorSorter, bool>.GetterDelegate <>9__26_0;
            public static MyTerminalValueControl<MyConveyorSorter, bool>.SetterDelegate <>9__26_1;
            public static Action<List<MyTerminalControlComboBoxItem>> <>9__26_2;
            public static MyTerminalValueControl<MyConveyorSorter, long>.GetterDelegate <>9__26_3;
            public static MyTerminalValueControl<MyConveyorSorter, long>.SetterDelegate <>9__26_4;
            public static MyTerminalControlListbox<MyConveyorSorter>.ListContentDelegate <>9__26_5;
            public static MyTerminalControlListbox<MyConveyorSorter>.SelectItemDelegate <>9__26_6;
            public static Action<MyConveyorSorter> <>9__26_7;
            public static Func<MyConveyorSorter, bool> <>9__26_8;
            public static MyTerminalControlListbox<MyConveyorSorter>.ListContentDelegate <>9__26_9;
            public static MyTerminalControlListbox<MyConveyorSorter>.SelectItemDelegate <>9__26_10;
            public static Action<MyConveyorSorter> <>9__26_11;
            public static Func<MyConveyorSorter, bool> <>9__26_12;
            public static Func<MyConveyorSorter, Action<bool>> <>9__43_0;
            public static Func<MyConveyorSorter, Action<SerializableDefinitionId, bool>> <>9__45_0;
            public static Func<MyConveyorSorter, Action<byte, bool>> <>9__47_0;
            public static Func<MyConveyorSorter, Action<MyConveyorSorterMode, List<MyInventoryItemFilter>>> <>9__85_0;

            internal Action<bool> <ChangeBlWl>b__43_0(MyConveyorSorter x) => 
                new Action<bool>(x.DoChangeBlWl);

            internal Action<SerializableDefinitionId, bool> <ChangeListId>b__45_0(MyConveyorSorter x) => 
                new Action<SerializableDefinitionId, bool>(x.DoChangeListId);

            internal Action<byte, bool> <ChangeListType>b__47_0(MyConveyorSorter x) => 
                new Action<byte, bool>(x.DoChangeListType);

            internal bool <CreateTerminalControls>b__26_0(MyConveyorSorter block) => 
                block.DrainAll;

            internal void <CreateTerminalControls>b__26_1(MyConveyorSorter block, bool val)
            {
                block.DrainAll = val;
            }

            internal void <CreateTerminalControls>b__26_10(MyConveyorSorter block, List<MyGuiControlListbox.Item> val)
            {
                block.SelectCandidate(val);
            }

            internal void <CreateTerminalControls>b__26_11(MyConveyorSorter x)
            {
                x.AddToCurrentList();
            }

            internal bool <CreateTerminalControls>b__26_12(MyConveyorSorter x) => 
                ((x.m_selectedForAdd != null) && (x.m_selectedForAdd.Count > 0));

            internal void <CreateTerminalControls>b__26_2(List<MyTerminalControlComboBoxItem> block)
            {
                MyConveyorSorter.FillBlWlCombo(block);
            }

            internal long <CreateTerminalControls>b__26_3(MyConveyorSorter block) => 
                (block.IsWhitelist ? ((long) 1) : ((long) 0));

            internal void <CreateTerminalControls>b__26_4(MyConveyorSorter block, long val)
            {
                block.ChangeBlWl(val == 1L);
            }

            internal void <CreateTerminalControls>b__26_5(MyConveyorSorter block, ICollection<MyGuiControlListbox.Item> list1, ICollection<MyGuiControlListbox.Item> list2)
            {
                block.FillCurrentList(list1, list2);
            }

            internal void <CreateTerminalControls>b__26_6(MyConveyorSorter block, List<MyGuiControlListbox.Item> val)
            {
                block.SelectFromCurrentList(val);
            }

            internal void <CreateTerminalControls>b__26_7(MyConveyorSorter block)
            {
                block.RemoveFromCurrentList();
            }

            internal bool <CreateTerminalControls>b__26_8(MyConveyorSorter x) => 
                ((x.m_selectedForDelete != null) && (x.m_selectedForDelete.Count > 0));

            internal void <CreateTerminalControls>b__26_9(MyConveyorSorter block, ICollection<MyGuiControlListbox.Item> list1, ICollection<MyGuiControlListbox.Item> list2)
            {
                block.FillCandidatesList(list1, list2);
            }

            internal Action<MyConveyorSorterMode, List<MyInventoryItemFilter>> <Sandbox.ModAPI.Ingame.IMyConveyorSorter.SetFilter>b__85_0(MyConveyorSorter x) => 
                new Action<MyConveyorSorterMode, List<MyInventoryItemFilter>>(x.DoSetupFilter);
        }
    }
}

