namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyToolbar
    {
        public const int DEF_SLOT_COUNT = 9;
        public const int DEF_PAGE_COUNT = 9;
        public int SlotCount;
        public int PageCount;
        private MyToolbarItem[] m_items;
        private CachingDictionary<Type, IMyToolbarExtension> m_extensions;
        private MyToolbarType m_toolbarType;
        private MyEntity m_owner;
        private bool? m_enabledOverride;
        private int? m_selectedSlot;
        private int? m_stagedSelectedSlot;
        private bool m_activateSelectedItem;
        private int m_currentPage;
        public bool DrawNumbers = true;
        public Func<int, ColoredIcon> GetSymbol = x => new ColoredIcon();
        [CompilerGenerated]
        private Action<MyToolbar, IndexArgs> ItemChanged;
        [CompilerGenerated]
        private Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> ItemUpdated;
        [CompilerGenerated]
        private Action<MyToolbar, SlotArgs> SelectedSlotChanged;
        [CompilerGenerated]
        private Action<MyToolbar, SlotArgs, bool> SlotActivated;
        [CompilerGenerated]
        private Action<MyToolbar, SlotArgs> ItemEnabledChanged;
        [CompilerGenerated]
        private Action<MyToolbar, PageChangeArgs> CurrentPageChanged;
        [CompilerGenerated]
        private Action<MyToolbar> Unselected;

        public event Action<MyToolbar, PageChangeArgs> CurrentPageChanged
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar, PageChangeArgs> currentPageChanged = this.CurrentPageChanged;
                while (true)
                {
                    Action<MyToolbar, PageChangeArgs> a = currentPageChanged;
                    Action<MyToolbar, PageChangeArgs> action3 = (Action<MyToolbar, PageChangeArgs>) Delegate.Combine(a, value);
                    currentPageChanged = Interlocked.CompareExchange<Action<MyToolbar, PageChangeArgs>>(ref this.CurrentPageChanged, action3, a);
                    if (ReferenceEquals(currentPageChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar, PageChangeArgs> currentPageChanged = this.CurrentPageChanged;
                while (true)
                {
                    Action<MyToolbar, PageChangeArgs> source = currentPageChanged;
                    Action<MyToolbar, PageChangeArgs> action3 = (Action<MyToolbar, PageChangeArgs>) Delegate.Remove(source, value);
                    currentPageChanged = Interlocked.CompareExchange<Action<MyToolbar, PageChangeArgs>>(ref this.CurrentPageChanged, action3, source);
                    if (ReferenceEquals(currentPageChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyToolbar, IndexArgs> ItemChanged
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar, IndexArgs> itemChanged = this.ItemChanged;
                while (true)
                {
                    Action<MyToolbar, IndexArgs> a = itemChanged;
                    Action<MyToolbar, IndexArgs> action3 = (Action<MyToolbar, IndexArgs>) Delegate.Combine(a, value);
                    itemChanged = Interlocked.CompareExchange<Action<MyToolbar, IndexArgs>>(ref this.ItemChanged, action3, a);
                    if (ReferenceEquals(itemChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar, IndexArgs> itemChanged = this.ItemChanged;
                while (true)
                {
                    Action<MyToolbar, IndexArgs> source = itemChanged;
                    Action<MyToolbar, IndexArgs> action3 = (Action<MyToolbar, IndexArgs>) Delegate.Remove(source, value);
                    itemChanged = Interlocked.CompareExchange<Action<MyToolbar, IndexArgs>>(ref this.ItemChanged, action3, source);
                    if (ReferenceEquals(itemChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyToolbar, SlotArgs> ItemEnabledChanged
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar, SlotArgs> itemEnabledChanged = this.ItemEnabledChanged;
                while (true)
                {
                    Action<MyToolbar, SlotArgs> a = itemEnabledChanged;
                    Action<MyToolbar, SlotArgs> action3 = (Action<MyToolbar, SlotArgs>) Delegate.Combine(a, value);
                    itemEnabledChanged = Interlocked.CompareExchange<Action<MyToolbar, SlotArgs>>(ref this.ItemEnabledChanged, action3, a);
                    if (ReferenceEquals(itemEnabledChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar, SlotArgs> itemEnabledChanged = this.ItemEnabledChanged;
                while (true)
                {
                    Action<MyToolbar, SlotArgs> source = itemEnabledChanged;
                    Action<MyToolbar, SlotArgs> action3 = (Action<MyToolbar, SlotArgs>) Delegate.Remove(source, value);
                    itemEnabledChanged = Interlocked.CompareExchange<Action<MyToolbar, SlotArgs>>(ref this.ItemEnabledChanged, action3, source);
                    if (ReferenceEquals(itemEnabledChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> ItemUpdated
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> itemUpdated = this.ItemUpdated;
                while (true)
                {
                    Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> a = itemUpdated;
                    Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> action3 = (Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo>) Delegate.Combine(a, value);
                    itemUpdated = Interlocked.CompareExchange<Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo>>(ref this.ItemUpdated, action3, a);
                    if (ReferenceEquals(itemUpdated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> itemUpdated = this.ItemUpdated;
                while (true)
                {
                    Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> source = itemUpdated;
                    Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo> action3 = (Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo>) Delegate.Remove(source, value);
                    itemUpdated = Interlocked.CompareExchange<Action<MyToolbar, IndexArgs, MyToolbarItem.ChangeInfo>>(ref this.ItemUpdated, action3, source);
                    if (ReferenceEquals(itemUpdated, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyToolbar, SlotArgs> SelectedSlotChanged
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar, SlotArgs> selectedSlotChanged = this.SelectedSlotChanged;
                while (true)
                {
                    Action<MyToolbar, SlotArgs> a = selectedSlotChanged;
                    Action<MyToolbar, SlotArgs> action3 = (Action<MyToolbar, SlotArgs>) Delegate.Combine(a, value);
                    selectedSlotChanged = Interlocked.CompareExchange<Action<MyToolbar, SlotArgs>>(ref this.SelectedSlotChanged, action3, a);
                    if (ReferenceEquals(selectedSlotChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar, SlotArgs> selectedSlotChanged = this.SelectedSlotChanged;
                while (true)
                {
                    Action<MyToolbar, SlotArgs> source = selectedSlotChanged;
                    Action<MyToolbar, SlotArgs> action3 = (Action<MyToolbar, SlotArgs>) Delegate.Remove(source, value);
                    selectedSlotChanged = Interlocked.CompareExchange<Action<MyToolbar, SlotArgs>>(ref this.SelectedSlotChanged, action3, source);
                    if (ReferenceEquals(selectedSlotChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyToolbar, SlotArgs, bool> SlotActivated
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar, SlotArgs, bool> slotActivated = this.SlotActivated;
                while (true)
                {
                    Action<MyToolbar, SlotArgs, bool> a = slotActivated;
                    Action<MyToolbar, SlotArgs, bool> action3 = (Action<MyToolbar, SlotArgs, bool>) Delegate.Combine(a, value);
                    slotActivated = Interlocked.CompareExchange<Action<MyToolbar, SlotArgs, bool>>(ref this.SlotActivated, action3, a);
                    if (ReferenceEquals(slotActivated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar, SlotArgs, bool> slotActivated = this.SlotActivated;
                while (true)
                {
                    Action<MyToolbar, SlotArgs, bool> source = slotActivated;
                    Action<MyToolbar, SlotArgs, bool> action3 = (Action<MyToolbar, SlotArgs, bool>) Delegate.Remove(source, value);
                    slotActivated = Interlocked.CompareExchange<Action<MyToolbar, SlotArgs, bool>>(ref this.SlotActivated, action3, source);
                    if (ReferenceEquals(slotActivated, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyToolbar> Unselected
        {
            [CompilerGenerated] add
            {
                Action<MyToolbar> unselected = this.Unselected;
                while (true)
                {
                    Action<MyToolbar> a = unselected;
                    Action<MyToolbar> action3 = (Action<MyToolbar>) Delegate.Combine(a, value);
                    unselected = Interlocked.CompareExchange<Action<MyToolbar>>(ref this.Unselected, action3, a);
                    if (ReferenceEquals(unselected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyToolbar> unselected = this.Unselected;
                while (true)
                {
                    Action<MyToolbar> source = unselected;
                    Action<MyToolbar> action3 = (Action<MyToolbar>) Delegate.Remove(source, value);
                    unselected = Interlocked.CompareExchange<Action<MyToolbar>>(ref this.Unselected, action3, source);
                    if (ReferenceEquals(unselected, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyToolbar(MyToolbarType type, int slotCount = 9, int pageCount = 9)
        {
            this.SlotCount = slotCount;
            this.PageCount = pageCount;
            this.m_items = new MyToolbarItem[this.SlotCount * this.PageCount];
            this.m_toolbarType = type;
            this.Owner = null;
            this.SetDefaults(true);
        }

        public bool ActivateItemAtIndex(int index, bool checkIfWantsToBeActivated = false)
        {
            MyToolbarItem objB = this.m_items[index];
            if (this.StagedSelectedSlot != null)
            {
                int? stagedSelectedSlot = this.StagedSelectedSlot;
                if (this.SlotToIndex(stagedSelectedSlot.Value) != index)
                {
                    stagedSelectedSlot = null;
                    this.StagedSelectedSlot = stagedSelectedSlot;
                }
            }
            if ((objB == null) || !objB.Enabled)
            {
                if (objB == null)
                {
                    this.Unselect(true);
                }
                return false;
            }
            if (checkIfWantsToBeActivated && !objB.WantsToBeActivated)
            {
                return false;
            }
            if ((objB.WantsToBeActivated || MyCubeBuilder.Static.IsActivated) && !ReferenceEquals(this.SelectedItem, objB))
            {
                this.Unselect(false);
            }
            return objB.Activate();
        }

        public void ActivateItemAtSlot(int slot, bool checkIfWantsToBeActivated = false, bool playActivationSound = true, bool userActivated = true)
        {
            if (this.IsValidSlot(slot) || this.IsHolsterSlot(slot))
            {
                if (!this.IsValidSlot(slot))
                {
                    this.Unselect(true);
                }
                else if (this.ActivateItemAtIndex(this.SlotToIndex(slot), checkIfWantsToBeActivated))
                {
                    if (playActivationSound)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                    }
                    if (this.SlotActivated != null)
                    {
                        SlotArgs args = new SlotArgs {
                            SlotNumber = new int?(slot)
                        };
                        this.SlotActivated(this, args, userActivated);
                    }
                }
            }
        }

        public void ActivateStagedSelectedItem()
        {
            this.ActivateItemAtSlot(this.m_stagedSelectedSlot.Value, false, true, true);
        }

        public void AddExtension(IMyToolbarExtension newExtension)
        {
            if (this.m_extensions == null)
            {
                this.m_extensions = new CachingDictionary<Type, IMyToolbarExtension>();
            }
            this.m_extensions.Add(newExtension.GetType(), newExtension, false);
            newExtension.AddedToToolbar(this);
        }

        public void CharacterInventory_OnContentsChanged(MyInventoryBase inventory)
        {
            this.Update();
        }

        public void Clear()
        {
            for (int i = 0; i < this.m_items.Length; i++)
            {
                this.SetItemAtIndex(i, null);
            }
        }

        public long GetControllerPlayerID()
        {
            MyCockpit owner = this.Owner as MyCockpit;
            if (owner == null)
            {
                return 0L;
            }
            MyEntityController controller = owner.ControllerInfo.Controller;
            return ((controller != null) ? controller.Player.Identity.IdentityId : 0L);
        }

        public MyToolbarItem GetItemAtIndex(int index) => 
            (this.IsValidIndex(index) ? this[index] : null);

        public MyToolbarItem GetItemAtSlot(int slot)
        {
            if (this.IsValidSlot(slot) || this.IsHolsterSlot(slot))
            {
                return (!this.IsValidSlot(slot) ? null : this.m_items[this.SlotToIndex(slot)]);
            }
            return null;
        }

        public string[] GetItemIcons(int idx) => 
            (this.IsValidIndex(idx) ? this.m_items[idx]?.Icons : null);

        public int GetItemIndex(MyToolbarItem item)
        {
            for (int i = 0; i < this.m_items.Length; i++)
            {
                if (ReferenceEquals(this.m_items[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetNextValidSlot(int startSlot)
        {
            int idx = startSlot + 1;
            return (!this.IsHolsterSlot(idx) ? idx : this.SlotCount);
        }

        public int GetNextValidSlotWithItem(int startSlot)
        {
            for (int i = startSlot + 1; i != this.SlotCount; i++)
            {
                if (this.m_items[this.SlotToIndex(i)] != null)
                {
                    return i;
                }
            }
            return -1;
        }

        public MyObjectBuilder_Toolbar GetObjectBuilder()
        {
            MyObjectBuilder_Toolbar toolbar = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Toolbar>();
            if (toolbar.Slots == null)
            {
                toolbar.Slots = new List<MyObjectBuilder_Toolbar.Slot>(this.m_items.Length);
            }
            toolbar.SelectedSlot = this.SelectedSlot;
            toolbar.Slots.Clear();
            for (int i = 0; i < this.m_items.Length; i++)
            {
                if (this.m_items[i] != null)
                {
                    this.m_items[i].GetObjectBuilder();
                    MyObjectBuilder_ToolbarItem objectBuilder = this.m_items[i].GetObjectBuilder();
                    if (objectBuilder != null)
                    {
                        MyObjectBuilder_Toolbar.Slot item = new MyObjectBuilder_Toolbar.Slot {
                            Index = i,
                            Item = "",
                            Data = objectBuilder
                        };
                        toolbar.Slots.Add(item);
                    }
                }
            }
            return toolbar;
        }

        public int GetPreviousValidSlot(int startSlot)
        {
            int num = startSlot - 1;
            return ((num >= 0) ? num : this.SlotCount);
        }

        public int GetPreviousValidSlotWithItem(int startSlot)
        {
            for (int i = startSlot - 1; i >= 0; i--)
            {
                if (this.m_items[this.SlotToIndex(i)] != null)
                {
                    return i;
                }
            }
            return -1;
        }

        public MyToolbarItem GetSlotItem(int slot)
        {
            if (!this.IsValidSlot(slot))
            {
                return null;
            }
            int idx = this.SlotToIndex(slot);
            return (this.IsValidIndex(idx) ? this[idx] : null);
        }

        public int IndexToSlot(int i) => 
            (((i / this.SlotCount) == this.m_currentPage) ? MyMath.Mod(i, this.SlotCount) : -1);

        public void Init(MyObjectBuilder_Toolbar builder, MyEntity owner, bool skipAssert = false)
        {
            this.Owner = owner;
            if (builder != null)
            {
                if (builder.Slots != null)
                {
                    this.Clear();
                    foreach (MyObjectBuilder_Toolbar.Slot slot in builder.Slots)
                    {
                        this.SetItemAtSerialized(slot.Index, slot.Item, slot.Data);
                    }
                }
                this.StagedSelectedSlot = builder.SelectedSlot;
                MyCockpit cockpit = this.Owner as MyCockpit;
                if ((cockpit != null) && (cockpit.CubeGrid != null))
                {
                    cockpit.CubeGrid.OnFatBlockClosed += new Action<MyCubeBlock>(this.OnFatBlockClosed);
                }
            }
        }

        public bool IsEnabled(int idx)
        {
            if (this.EnabledOverride != null)
            {
                return this.EnabledOverride.Value;
            }
            if ((idx != this.SlotCount) || !this.ShowHolsterSlot)
            {
                return (this.IsValidIndex(idx) ? ((this.m_items[idx] == null) || this.m_items[idx].Enabled) : false);
            }
            return true;
        }

        private bool IsHolsterSlot(int idx) => 
            ((idx == this.SlotCount) && this.ShowHolsterSlot);

        public bool IsValidIndex(int idx) => 
            this.m_items.IsValidIndex<MyToolbarItem>(idx);

        public bool IsValidSlot(int slot) => 
            ((slot >= 0) && (slot < this.SlotCount));

        private void OnFatBlockClosed(MyCubeBlock block)
        {
            if ((this.Owner == null) || (this.Owner.EntityId != block.EntityId))
            {
                for (int i = 0; i < this.m_items.Length; i++)
                {
                    if (((this.m_items[i] != null) && (this.m_items[i] is IMyToolbarItemEntity)) && ((IMyToolbarItemEntity) this.m_items[i]).CompareEntityIds(block.EntityId))
                    {
                        this.m_items[i].SetEnabled(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.m_items.Length; i++)
                {
                    if (this.m_items[i] != null)
                    {
                        this.m_items[i].OnRemovedFromToolbar(this);
                        this.m_items[i] = null;
                    }
                }
            }
        }

        public void PageDown()
        {
            if (this.PageCount > 0)
            {
                this.m_currentPage = MyMath.Mod((int) (this.m_currentPage - 1), this.PageCount);
                if (this.CurrentPageChanged != null)
                {
                    PageChangeArgs args = new PageChangeArgs {
                        PageIndex = this.m_currentPage
                    };
                    this.CurrentPageChanged(this, args);
                }
            }
        }

        public void PageUp()
        {
            if (this.PageCount > 0)
            {
                this.m_currentPage = MyMath.Mod((int) (this.m_currentPage + 1), this.PageCount);
                if (this.CurrentPageChanged != null)
                {
                    PageChangeArgs args = new PageChangeArgs {
                        PageIndex = this.m_currentPage
                    };
                    this.CurrentPageChanged(this, args);
                }
            }
        }

        public void RemoveExtension(IMyToolbarExtension toRemove)
        {
            this.m_extensions.Remove(toRemove.GetType(), false);
        }

        public void SelectNextSlot()
        {
            if ((this.m_selectedSlot != null) && this.IsValidSlot(this.m_selectedSlot.Value))
            {
                if ((MyCubeBuilder.Static.CubeBuilderState.CubeSizeMode == MyCubeSize.Large) && MyCubeBuilder.Static.CubeBuilderState.HasComplementBlock())
                {
                    this.ActivateItemAtSlot(this.m_selectedSlot.Value, false, true, true);
                    return;
                }
                MyCubeBuilder.Static.CubeBuilderState.SetCubeSize(MyCubeSize.Large);
            }
            int nextValidSlotWithItem = this.GetNextValidSlotWithItem((this.m_selectedSlot != null) ? this.m_selectedSlot.Value : -1);
            if (nextValidSlotWithItem != -1)
            {
                this.ActivateItemAtSlot(nextValidSlotWithItem, false, true, true);
            }
            else
            {
                this.Unselect(true);
            }
        }

        public void SelectPreviousSlot()
        {
            if ((this.m_selectedSlot != null) && this.IsValidSlot(this.m_selectedSlot.Value))
            {
                if ((MyCubeBuilder.Static.CubeBuilderState.CubeSizeMode == MyCubeSize.Large) && MyCubeBuilder.Static.CubeBuilderState.HasComplementBlock())
                {
                    this.ActivateItemAtSlot(this.m_selectedSlot.Value, false, true, true);
                    return;
                }
                MyCubeBuilder.Static.CubeBuilderState.SetCubeSize(MyCubeSize.Large);
            }
            int previousValidSlotWithItem = this.GetPreviousValidSlotWithItem((this.m_selectedSlot != null) ? this.m_selectedSlot.Value : this.SlotCount);
            if (previousValidSlotWithItem != -1)
            {
                this.ActivateItemAtSlot(previousValidSlotWithItem, false, true, true);
            }
            else
            {
                this.Unselect(true);
            }
        }

        public void SetDefaults(bool sendEvent = true)
        {
            if (this.m_toolbarType == MyToolbarType.Character)
            {
                MyDefinitionBase base2;
                MyDefinitionBase base3;
                MyDefinitionBase base4;
                MyDefinitionBase base5;
                MyDefinitionBase base6;
                MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "LargeBlockArmorBlock");
                MyDefinitionId id2 = new MyDefinitionId(typeof(MyObjectBuilder_Cockpit), "LargeBlockCockpit");
                MyDefinitionId id3 = new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeBlockSmallGenerator");
                MyDefinitionId id4 = new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallThrust");
                MyDefinitionId id5 = new MyDefinitionId(typeof(MyObjectBuilder_Gyro), "LargeBlockGyro");
                int i = 0;
                if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(defId, out base2))
                {
                    i++;
                    this.SetItemAtIndex(i, defId, MyToolbarItemFactory.ObjectBuilderFromDefinition(base2));
                }
                if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(id2, out base3))
                {
                    i++;
                    this.SetItemAtIndex(i, defId, MyToolbarItemFactory.ObjectBuilderFromDefinition(base3));
                }
                if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(id3, out base4))
                {
                    i++;
                    this.SetItemAtIndex(i, defId, MyToolbarItemFactory.ObjectBuilderFromDefinition(base4));
                }
                if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(id4, out base5))
                {
                    i++;
                    this.SetItemAtIndex(i, defId, MyToolbarItemFactory.ObjectBuilderFromDefinition(base5));
                }
                if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(id5, out base6))
                {
                    i++;
                    this.SetItemAtIndex(i, defId, MyToolbarItemFactory.ObjectBuilderFromDefinition(base6));
                }
                for (int j = i; j < this.m_items.Length; j++)
                {
                    this.SetItemAtIndex(j, null);
                }
            }
        }

        public void SetItemAtIndex(int i, MyToolbarItem item)
        {
            this.SetItemAtIndexInternal(i, item, false);
        }

        public void SetItemAtIndex(int i, MyDefinitionId defId, MyObjectBuilder_ToolbarItem data)
        {
            MyDefinitionBase base2;
            if (this.m_items.IsValidIndex<MyToolbarItem>(i) && MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(defId, out base2))
            {
                this.SetItemAtIndex(i, MyToolbarItemFactory.CreateToolbarItem(data));
            }
        }

        private void SetItemAtIndexInternal(int i, MyToolbarItem item, bool initialization = false)
        {
            if (this.m_items.IsValidIndex<MyToolbarItem>(i))
            {
                MyToolbarItemDefinition definition = item as MyToolbarItemDefinition;
                if ((((definition == null) || ((definition.Definition == null) || definition.Definition.AvailableInSurvival)) || !MySession.Static.SurvivalMode) && ((item == null) || item.AllowedInToolbarType(this.m_toolbarType)))
                {
                    bool enabled = true;
                    bool flag2 = true;
                    if (this.m_items[i] != null)
                    {
                        enabled = this.m_items[i].Enabled;
                        this.m_items[i].OnRemovedFromToolbar(this);
                    }
                    this.m_items[i] = item;
                    if (item != null)
                    {
                        item.OnAddedToToolbar(this);
                        flag2 = true;
                        if (MyVisualScriptLogicProvider.ToolbarItemChanged != null)
                        {
                            MyObjectBuilder_ToolbarItem objectBuilder = item.GetObjectBuilder();
                            string typeId = objectBuilder.TypeId.ToString();
                            string subtypeId = objectBuilder.SubtypeId.ToString();
                            MyObjectBuilder_ToolbarItemDefinition definition2 = objectBuilder as MyObjectBuilder_ToolbarItemDefinition;
                            if (definition2 != null)
                            {
                                typeId = definition2.DefinitionId.TypeId.ToString();
                                subtypeId = definition2.DefinitionId.SubtypeId;
                            }
                            MyVisualScriptLogicProvider.ToolbarItemChanged((this.Owner != null) ? this.Owner.EntityId : 0L, typeId, subtypeId, this.m_currentPage, MyMath.Mod(i, this.SlotCount));
                        }
                    }
                    if (!initialization)
                    {
                        this.UpdateItem(i);
                        if (this.ItemChanged != null)
                        {
                            IndexArgs args = new IndexArgs {
                                ItemIndex = i
                            };
                            this.ItemChanged(this, args);
                        }
                        if (enabled != flag2)
                        {
                            int slot = this.IndexToSlot(i);
                            if (this.IsValidSlot(slot))
                            {
                                this.SlotEnabledChanged(slot);
                            }
                        }
                    }
                }
            }
        }

        private void SetItemAtSerialized(int i, string serializedItem, MyObjectBuilder_ToolbarItem data)
        {
            if (this.m_items.IsValidIndex<MyToolbarItem>(i))
            {
                if (data != null)
                {
                    this.SetItemAtIndexInternal(i, MyToolbarItemFactory.CreateToolbarItem(data), true);
                }
                else if (!string.IsNullOrEmpty(serializedItem))
                {
                    MyObjectBuilderType type;
                    char[] separator = new char[] { ':' };
                    string[] strArray = serializedItem.Split(separator);
                    if (MyObjectBuilderType.TryParse(strArray[0], out type))
                    {
                        MyDefinitionId defId = new MyDefinitionId(type, (strArray.Length == 2) ? strArray[1] : null);
                        this.SetItemAtSerializedCompat(i, defId);
                    }
                }
            }
        }

        public void SetItemAtSerializedCompat(int i, MyDefinitionId defId)
        {
            MyDefinitionBase base2;
            if (this.m_items.IsValidIndex<MyToolbarItem>(i) && MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(defId, out base2))
            {
                MyObjectBuilder_ToolbarItem data = MyToolbarItemFactory.ObjectBuilderFromDefinition(base2);
                this.SetItemAtIndexInternal(i, MyToolbarItemFactory.CreateToolbarItem(data), true);
            }
        }

        public void SetItemAtSlot(int slot, MyToolbarItem item)
        {
            this.SetItemAtIndex(this.SlotToIndex(slot), item);
        }

        private void SlotEnabledChanged(int slotIndex)
        {
            if ((this.EnabledOverride == null) && (this.ItemEnabledChanged != null))
            {
                SlotArgs args = new SlotArgs {
                    SlotNumber = new int?(slotIndex)
                };
                this.ItemEnabledChanged(this, args);
            }
        }

        public int SlotToIndex(int i) => 
            ((this.SlotCount * this.m_currentPage) + i);

        public void SwitchToPage(int page)
        {
            if (((page >= 0) && (page < this.PageCount)) && (this.m_currentPage != page))
            {
                this.m_currentPage = page;
                if (this.CurrentPageChanged != null)
                {
                    PageChangeArgs args = new PageChangeArgs {
                        PageIndex = this.m_currentPage
                    };
                    this.CurrentPageChanged(this, args);
                }
            }
        }

        private void ToolbarItem_EnabledChanged(MyToolbarItem obj)
        {
            if (this.EnabledOverride == null)
            {
                int index = Array.IndexOf<MyToolbarItem>(this.m_items, obj);
                if ((this.ItemEnabledChanged != null) && (index != -1))
                {
                    int slot = this.IndexToSlot(index);
                    if (this.IsValidSlot(slot))
                    {
                        SlotArgs args = new SlotArgs {
                            SlotNumber = new int?(slot)
                        };
                        this.ItemEnabledChanged(this, args);
                    }
                }
            }
        }

        private void ToolbarItemUpdated(int index, MyToolbarItem.ChangeInfo changed)
        {
            if (this.m_items.IsValidIndex<MyToolbarItem>(index) && (this.ItemUpdated != null))
            {
                IndexArgs args = new IndexArgs {
                    ItemIndex = index
                };
                this.ItemUpdated(this, args, changed);
            }
        }

        public bool TryGetExtension<T>(out T extension) where T: class, IMyToolbarExtension
        {
            extension = default(T);
            if (this.m_extensions == null)
            {
                return false;
            }
            IMyToolbarExtension extension2 = null;
            if (this.m_extensions.TryGetValue(typeof(T), out extension2))
            {
                extension = extension2 as T;
            }
            return (((T) extension) != null);
        }

        public void Unselect(bool unselectSound = true)
        {
            if (ReferenceEquals(MyToolbarComponent.CurrentToolbar, this))
            {
                if ((this.SelectedItem != null) & unselectSound)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                }
                if (unselectSound)
                {
                    MySession.Static.GameFocusManager.Clear();
                }
                IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
                if (controlledEntity != null)
                {
                    controlledEntity.SwitchToWeapon((MyToolbarItemWeapon) null);
                }
                if (this.Unselected != null)
                {
                    this.Unselected(this);
                }
            }
        }

        public void Update()
        {
            if (MySession.Static != null)
            {
                long controllerPlayerID = this.GetControllerPlayerID();
                for (int i = 0; i < this.m_items.Length; i++)
                {
                    if (this.m_items[i] != null)
                    {
                        MyToolbarItem.ChangeInfo changed = this.m_items[i].Update(this.Owner, controllerPlayerID);
                        if (changed != MyToolbarItem.ChangeInfo.None)
                        {
                            this.ToolbarItemUpdated(i, changed);
                        }
                    }
                }
                int? selectedSlot = this.m_selectedSlot;
                if (this.StagedSelectedSlot == null)
                {
                    this.m_selectedSlot = null;
                    for (int j = 0; j < this.SlotCount; j++)
                    {
                        if ((this.m_items[this.SlotToIndex(j)] != null) && this.m_items[this.SlotToIndex(j)].WantsToBeSelected)
                        {
                            this.m_selectedSlot = new int?(j);
                        }
                    }
                }
                else if ((this.m_selectedSlot == null) || (this.m_selectedSlot.Value != this.StagedSelectedSlot.Value))
                {
                    this.m_selectedSlot = this.StagedSelectedSlot;
                    MyToolbarItem item = this.m_items[this.SlotToIndex(this.m_selectedSlot.Value)];
                    if ((item == null) || item.ActivateOnClick)
                    {
                        this.m_activateSelectedItem = true;
                        this.Unselect(true);
                    }
                    else
                    {
                        this.ActivateItemAtSlot(this.m_selectedSlot.Value, false, true, true);
                        this.m_activateSelectedItem = false;
                    }
                }
                int? nullable2 = selectedSlot;
                int? nullable3 = this.m_selectedSlot;
                if (!((nullable2.GetValueOrDefault() == nullable3.GetValueOrDefault()) & ((nullable2 != null) == (nullable3 != null))) && (this.SelectedSlotChanged != null))
                {
                    SlotArgs args = new SlotArgs {
                        SlotNumber = this.m_selectedSlot
                    };
                    this.SelectedSlotChanged(this, args);
                }
                this.EnabledOverride = null;
                if (this.m_extensions != null)
                {
                    using (Dictionary<Type, IMyToolbarExtension>.ValueCollection.Enumerator enumerator = this.m_extensions.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Update();
                        }
                    }
                    this.m_extensions.ApplyChanges();
                }
            }
        }

        public void UpdateItem(int index)
        {
            if ((MySession.Static != null) && (this.m_items[index] != null))
            {
                this.m_items[index].Update(this.Owner, this.GetControllerPlayerID());
            }
        }

        public int ItemCount =>
            (this.SlotCount * this.PageCount);

        public MyToolbarType ToolbarType
        {
            get => 
                this.m_toolbarType;
            private set => 
                (this.m_toolbarType = value);
        }

        public MyEntity Owner
        {
            get => 
                this.m_owner;
            private set => 
                (this.m_owner = value);
        }

        public bool ShowHolsterSlot =>
            ((this.m_toolbarType == MyToolbarType.Character) || (this.m_toolbarType == MyToolbarType.BuildCockpit));

        public int? SelectedSlot
        {
            get => 
                this.m_selectedSlot;
            private set
            {
                int? selectedSlot = this.m_selectedSlot;
                int? nullable2 = value;
                if (!((selectedSlot.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((selectedSlot != null) == (nullable2 != null))))
                {
                    this.m_selectedSlot = value;
                }
            }
        }

        public int? StagedSelectedSlot
        {
            get => 
                this.m_stagedSelectedSlot;
            private set
            {
                this.m_stagedSelectedSlot = value;
                this.m_activateSelectedItem = false;
            }
        }

        public bool ShouldActivateSlot =>
            this.m_activateSelectedItem;

        public int CurrentPage =>
            this.m_currentPage;

        public MyToolbarItem SelectedItem
        {
            get
            {
                if (this.SelectedSlot == null)
                {
                    return null;
                }
                return this.GetSlotItem(this.SelectedSlot.Value);
            }
        }

        public MyToolbarItem this[int i] =>
            this.m_items[i];

        public bool? EnabledOverride
        {
            get => 
                this.m_enabledOverride;
            private set
            {
                bool? nullable = value;
                bool? enabledOverride = this.m_enabledOverride;
                if (!((nullable.GetValueOrDefault() == enabledOverride.GetValueOrDefault()) & ((nullable != null) == (enabledOverride != null))))
                {
                    this.m_enabledOverride = value;
                    if (this.ItemEnabledChanged != null)
                    {
                        SlotArgs args = new SlotArgs();
                        this.ItemEnabledChanged(this, args);
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyToolbar.<>c <>9 = new MyToolbar.<>c();
            public static Func<int, ColoredIcon> <>9__73_0;

            internal ColoredIcon <.ctor>b__73_0(int x) => 
                new ColoredIcon();
        }

        public interface IMyToolbarExtension
        {
            void AddedToToolbar(MyToolbar toolbar);
            void Update();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IndexArgs
        {
            public int ItemIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PageChangeArgs
        {
            public int PageIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SlotArgs
        {
            public int? SlotNumber;
        }
    }
}

