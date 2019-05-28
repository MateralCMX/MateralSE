namespace Sandbox.Game.Entities.Cube
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;

    [MyCubeBlockType(typeof(MyObjectBuilder_TerminalBlock)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyTerminalBlock), typeof(Sandbox.ModAPI.Ingame.IMyTerminalBlock) })]
    public class MyTerminalBlock : MySyncedBlock, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock
    {
        private static readonly Guid m_storageGuid = new Guid("74DE02B3-27F9-4960-B1C4-27351F2B06D1");
        private const int DATA_CHARACTER_LIMIT = 0xfa00;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_showOnHUD;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_showInTerminal;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_showInToolbarConfig;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_showInInventory;
        private bool m_isBeingHackedPrevValue;
        private MyGuiScreenTextPanel m_textBox;
        protected bool m_textboxOpen;
        private ulong m_currentUser;
        public int? HackAttemptTime;
        public bool IsAccessibleForProgrammableBlock = true;
        [CompilerGenerated]
        private Action<MyTerminalBlock> CustomDataChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> CustomNameChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> PropertiesChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> OwnershipChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> VisibilityChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> ShowOnHUDChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> ShowInTerminalChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> ShowInIventoryChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> ShowInToolbarConfigChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock> IsBeingHackedChanged;
        [CompilerGenerated]
        private Action<MyTerminalBlock, StringBuilder> AppendingCustomInfo;
        private static FastResourceLock m_createControlsLock = new FastResourceLock();

        public event Action<MyTerminalBlock, StringBuilder> AppendingCustomInfo
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock, StringBuilder> appendingCustomInfo = this.AppendingCustomInfo;
                while (true)
                {
                    Action<MyTerminalBlock, StringBuilder> a = appendingCustomInfo;
                    Action<MyTerminalBlock, StringBuilder> action3 = (Action<MyTerminalBlock, StringBuilder>) Delegate.Combine(a, value);
                    appendingCustomInfo = Interlocked.CompareExchange<Action<MyTerminalBlock, StringBuilder>>(ref this.AppendingCustomInfo, action3, a);
                    if (ReferenceEquals(appendingCustomInfo, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock, StringBuilder> appendingCustomInfo = this.AppendingCustomInfo;
                while (true)
                {
                    Action<MyTerminalBlock, StringBuilder> source = appendingCustomInfo;
                    Action<MyTerminalBlock, StringBuilder> action3 = (Action<MyTerminalBlock, StringBuilder>) Delegate.Remove(source, value);
                    appendingCustomInfo = Interlocked.CompareExchange<Action<MyTerminalBlock, StringBuilder>>(ref this.AppendingCustomInfo, action3, source);
                    if (ReferenceEquals(appendingCustomInfo, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> CustomDataChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> customDataChanged = this.CustomDataChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = customDataChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    customDataChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.CustomDataChanged, action3, a);
                    if (ReferenceEquals(customDataChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> customDataChanged = this.CustomDataChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = customDataChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    customDataChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.CustomDataChanged, action3, source);
                    if (ReferenceEquals(customDataChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> CustomNameChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> customNameChanged = this.CustomNameChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = customNameChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    customNameChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.CustomNameChanged, action3, a);
                    if (ReferenceEquals(customNameChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> customNameChanged = this.CustomNameChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = customNameChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    customNameChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.CustomNameChanged, action3, source);
                    if (ReferenceEquals(customNameChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> IsBeingHackedChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> isBeingHackedChanged = this.IsBeingHackedChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = isBeingHackedChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    isBeingHackedChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.IsBeingHackedChanged, action3, a);
                    if (ReferenceEquals(isBeingHackedChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> isBeingHackedChanged = this.IsBeingHackedChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = isBeingHackedChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    isBeingHackedChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.IsBeingHackedChanged, action3, source);
                    if (ReferenceEquals(isBeingHackedChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> OwnershipChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> ownershipChanged = this.OwnershipChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = ownershipChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    ownershipChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.OwnershipChanged, action3, a);
                    if (ReferenceEquals(ownershipChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> ownershipChanged = this.OwnershipChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = ownershipChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    ownershipChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.OwnershipChanged, action3, source);
                    if (ReferenceEquals(ownershipChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> PropertiesChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> propertiesChanged = this.PropertiesChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = propertiesChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    propertiesChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.PropertiesChanged, action3, a);
                    if (ReferenceEquals(propertiesChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> propertiesChanged = this.PropertiesChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = propertiesChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    propertiesChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.PropertiesChanged, action3, source);
                    if (ReferenceEquals(propertiesChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock, StringBuilder> Sandbox.ModAPI.IMyTerminalBlock.AppendingCustomInfo
        {
            add
            {
                this.AppendingCustomInfo += this.GetDelegate(value);
            }
            remove
            {
                this.AppendingCustomInfo -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyTerminalBlock.CustomDataChanged
        {
            add
            {
                this.CustomDataChanged += this.GetDelegate(value);
            }
            remove
            {
                this.CustomDataChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyTerminalBlock.CustomNameChanged
        {
            add
            {
                this.CustomNameChanged += this.GetDelegate(value);
            }
            remove
            {
                this.CustomNameChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyTerminalBlock.OwnershipChanged
        {
            add
            {
                this.OwnershipChanged += this.GetDelegate(value);
            }
            remove
            {
                this.OwnershipChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyTerminalBlock.PropertiesChanged
        {
            add
            {
                this.PropertiesChanged += this.GetDelegate(value);
            }
            remove
            {
                this.PropertiesChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyTerminalBlock.ShowOnHUDChanged
        {
            add
            {
                this.ShowOnHUDChanged += this.GetDelegate(value);
            }
            remove
            {
                this.ShowOnHUDChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyTerminalBlock.VisibilityChanged
        {
            add
            {
                this.VisibilityChanged += this.GetDelegate(value);
            }
            remove
            {
                this.VisibilityChanged -= this.GetDelegate(value);
            }
        }

        public event Action<MyTerminalBlock> ShowInIventoryChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> showInIventoryChanged = this.ShowInIventoryChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = showInIventoryChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    showInIventoryChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowInIventoryChanged, action3, a);
                    if (ReferenceEquals(showInIventoryChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> showInIventoryChanged = this.ShowInIventoryChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = showInIventoryChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    showInIventoryChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowInIventoryChanged, action3, source);
                    if (ReferenceEquals(showInIventoryChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> ShowInTerminalChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> showInTerminalChanged = this.ShowInTerminalChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = showInTerminalChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    showInTerminalChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowInTerminalChanged, action3, a);
                    if (ReferenceEquals(showInTerminalChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> showInTerminalChanged = this.ShowInTerminalChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = showInTerminalChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    showInTerminalChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowInTerminalChanged, action3, source);
                    if (ReferenceEquals(showInTerminalChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> ShowInToolbarConfigChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> showInToolbarConfigChanged = this.ShowInToolbarConfigChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = showInToolbarConfigChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    showInToolbarConfigChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowInToolbarConfigChanged, action3, a);
                    if (ReferenceEquals(showInToolbarConfigChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> showInToolbarConfigChanged = this.ShowInToolbarConfigChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = showInToolbarConfigChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    showInToolbarConfigChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowInToolbarConfigChanged, action3, source);
                    if (ReferenceEquals(showInToolbarConfigChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> ShowOnHUDChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> showOnHUDChanged = this.ShowOnHUDChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = showOnHUDChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    showOnHUDChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowOnHUDChanged, action3, a);
                    if (ReferenceEquals(showOnHUDChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> showOnHUDChanged = this.ShowOnHUDChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = showOnHUDChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    showOnHUDChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.ShowOnHUDChanged, action3, source);
                    if (ReferenceEquals(showOnHUDChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyTerminalBlock> VisibilityChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> visibilityChanged = this.VisibilityChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = visibilityChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    visibilityChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.VisibilityChanged, action3, a);
                    if (ReferenceEquals(visibilityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> visibilityChanged = this.VisibilityChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = visibilityChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    visibilityChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.VisibilityChanged, action3, source);
                    if (ReferenceEquals(visibilityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyTerminalBlock()
        {
            using (m_createControlsLock.AcquireExclusiveUsing())
            {
                this.CreateTerminalControls();
            }
            this.DetailedInfo = new StringBuilder();
            this.CustomInfo = new StringBuilder();
            this.CustomNameWithFaction = new StringBuilder();
            this.CustomName = new StringBuilder();
            base.SyncType.PropertyChanged += sync => this.RaisePropertiesChanged();
        }

        private void ClientRemoved(ulong steamId)
        {
            if (steamId == this.m_currentUser)
            {
                this.SendChangeOpenMessage(false, false, 0UL);
            }
        }

        private void CloseWindow()
        {
            MyGuiScreenGamePlay.ActiveGameplayScreen = MyGuiScreenGamePlay.TmpGameplayScreenHolder;
            MyGuiScreenGamePlay.TmpGameplayScreenHolder = null;
            foreach (MySlimBlock block in base.CubeGrid.CubeBlocks)
            {
                if ((block.FatBlock != null) && (block.FatBlock.EntityId == base.EntityId))
                {
                    this.CustomData = this.m_textBox.Description.Text.ToString();
                    this.SendChangeOpenMessage(false, false, 0UL);
                    break;
                }
            }
        }

        protected override void Closing()
        {
            base.Closing();
            if (Sync.IsServer && (Sync.Clients != null))
            {
                MyClientCollection clients = Sync.Clients;
                clients.ClientRemoved = (Action<ulong>) Delegate.Remove(clients.ClientRemoved, new Action<ulong>(this.ClientRemoved));
            }
        }

        protected virtual void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyTerminalBlock>())
            {
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch7 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowInTerminal", MySpaceTexts.Terminal_ShowInTerminal, MySpaceTexts.Terminal_ShowInTerminalToolTip, on, on);
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch8 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowInTerminal", MySpaceTexts.Terminal_ShowInTerminal, MySpaceTexts.Terminal_ShowInTerminalToolTip, on, on);
                switch8.Getter = x => (bool) x.m_showInTerminal;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local26 = switch8;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> control = switch8;
                control.Setter = (x, v) => x.ShowInTerminal = v;
                MyTerminalControlFactory.AddControl<MyTerminalBlock>(control);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch5 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowInInventory", MySpaceTexts.Terminal_ShowInInventory, MySpaceTexts.Terminal_ShowInInventoryToolTip, on, on);
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch6 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowInInventory", MySpaceTexts.Terminal_ShowInInventory, MySpaceTexts.Terminal_ShowInInventoryToolTip, on, on);
                switch6.Getter = x => (bool) x.m_showInInventory;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local24 = switch6;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local25 = switch6;
                local25.Setter = (x, v) => x.ShowInInventory = v;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local22 = local25;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local23 = local25;
                local23.Visible = x => x.HasInventory;
                MyTerminalControlFactory.AddControl<MyTerminalBlock>(local23);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch3 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowInToolbarConfig", MySpaceTexts.Terminal_ShowInToolbarConfig, MySpaceTexts.Terminal_ShowInToolbarConfigToolTip, on, on);
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch4 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowInToolbarConfig", MySpaceTexts.Terminal_ShowInToolbarConfig, MySpaceTexts.Terminal_ShowInToolbarConfigToolTip, on, on);
                switch4.Getter = x => (bool) x.m_showInToolbarConfig;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local20 = switch4;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local21 = switch4;
                local21.Setter = (x, v) => x.ShowInToolbarConfig = v;
                MyTerminalControlFactory.AddControl<MyTerminalBlock>(local21);
                MyTerminalControlTextbox<MyTerminalBlock> textbox1 = new MyTerminalControlTextbox<MyTerminalBlock>("Name", MyCommonTexts.Name, MySpaceTexts.Blank);
                MyTerminalControlTextbox<MyTerminalBlock> textbox2 = new MyTerminalControlTextbox<MyTerminalBlock>("Name", MyCommonTexts.Name, MySpaceTexts.Blank);
                textbox2.Getter = x => x.CustomName;
                MyTerminalControlTextbox<MyTerminalBlock> local18 = textbox2;
                MyTerminalControlTextbox<MyTerminalBlock> local19 = textbox2;
                local19.Setter = (x, v) => x.SetCustomName(v);
                MyTerminalControlTextbox<MyTerminalBlock> local10 = local19;
                local10.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyTerminalBlock>(local10);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch1 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowOnHUD", MySpaceTexts.Terminal_ShowOnHUD, MySpaceTexts.Terminal_ShowOnHUDToolTip, on, on);
                MyTerminalControlOnOffSwitch<MyTerminalBlock> switch2 = new MyTerminalControlOnOffSwitch<MyTerminalBlock>("ShowOnHUD", MySpaceTexts.Terminal_ShowOnHUD, MySpaceTexts.Terminal_ShowOnHUDToolTip, on, on);
                switch2.Getter = x => x.ShowOnHUD;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local16 = switch2;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> local17 = switch2;
                local17.Setter = (x, v) => x.ShowOnHUD = v;
                MyTerminalControlOnOffSwitch<MyTerminalBlock> onOff = local17;
                onOff.EnableToggleAction<MyTerminalBlock>();
                onOff.EnableOnOffActions<MyTerminalBlock>();
                MyTerminalControlFactory.AddControl<MyTerminalBlock>(onOff);
                MyTerminalControlButton<MyTerminalBlock> button1 = new MyTerminalControlButton<MyTerminalBlock>("CustomData", MySpaceTexts.Terminal_CustomData, MySpaceTexts.Terminal_CustomDataTooltip, new Action<MyTerminalBlock>(this.CustomDataClicked));
                MyTerminalControlButton<MyTerminalBlock> button2 = new MyTerminalControlButton<MyTerminalBlock>("CustomData", MySpaceTexts.Terminal_CustomData, MySpaceTexts.Terminal_CustomDataTooltip, new Action<MyTerminalBlock>(this.CustomDataClicked));
                button2.Enabled = x => !x.m_textboxOpen;
                MyTerminalControlButton<MyTerminalBlock> local15 = button2;
                local15.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyTerminalBlock>(local15);
            }
        }

        private void CreateTextBox(bool isEditable, string description)
        {
            bool editable = isEditable;
            this.m_textBox = new MyGuiScreenTextPanel(this.CustomName.ToString(), "", MyTexts.GetString(MySpaceTexts.Terminal_CustomData), description, new Action<VRage.Game.ModAPI.ResultEnum>(this.OnClosedTextBox), null, null, editable, null);
        }

        protected void CustomDataClicked(MyTerminalBlock myTerminalBlock)
        {
            myTerminalBlock.OpenWindow(true, true);
        }

        protected void FixSingleInventory()
        {
            MyInventoryBase base2;
            if (base.Components.TryGet<MyInventoryBase>(out base2))
            {
                MyInventoryAggregate aggregate = base2 as MyInventoryAggregate;
                MyInventory component = null;
                if (aggregate != null)
                {
                    foreach (MyInventory inventory2 in aggregate.ChildList.Reader)
                    {
                        if (inventory2 == null)
                        {
                            continue;
                        }
                        if (component == null)
                        {
                            component = inventory2;
                            continue;
                        }
                        if (component.GetItemsCount() < inventory2.GetItemsCount())
                        {
                            component = inventory2;
                        }
                    }
                }
                if (component != null)
                {
                    base.Components.Remove<MyInventoryBase>();
                    base.Components.Add<MyInventoryBase>(component);
                }
            }
        }

        private Action<MyTerminalBlock> GetDelegate(Action<Sandbox.ModAPI.IMyTerminalBlock> value) => 
            ((Action<MyTerminalBlock>) Delegate.CreateDelegate(typeof(Action<MyTerminalBlock>), value.Target, value.Method));

        private Action<MyTerminalBlock, StringBuilder> GetDelegate(Action<Sandbox.ModAPI.IMyTerminalBlock, StringBuilder> value) => 
            ((Action<MyTerminalBlock, StringBuilder>) Delegate.CreateDelegate(typeof(Action<MyTerminalBlock, StringBuilder>), value.Target, value.Method));

        public override unsafe List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            MyHudEntityParams* paramsPtr1;
            MyHudEntityParams* paramsPtr2;
            MyHudEntityParams* paramsPtr3;
            int num1;
            this.CustomNameWithFaction.Clear();
            if (!string.IsNullOrEmpty(base.GetOwnerFactionTag()))
            {
                this.CustomNameWithFaction.Append(base.GetOwnerFactionTag());
                this.CustomNameWithFaction.Append(".");
            }
            this.CustomNameWithFaction.AppendStringBuilder(this.CustomName);
            base.m_hudParams.Clear();
            MyHudEntityParams item = new MyHudEntityParams {
                FlagsEnum = ~MyHudIndicatorFlagsEnum.NONE,
                Text = this.CustomNameWithFaction
            };
            paramsPtr1.Owner = (base.IDModule != null) ? base.IDModule.Owner : 0L;
            paramsPtr1 = (MyHudEntityParams*) ref item;
            paramsPtr2.Share = (base.IDModule != null) ? base.IDModule.ShareMode : MyOwnershipShareModeEnum.None;
            paramsPtr2 = (MyHudEntityParams*) ref item;
            item.Entity = this;
            if (!allowBlink || !this.IsBeingHacked)
            {
                num1 = 0;
            }
            else
            {
                num1 = 10;
            }
            paramsPtr3.BlinkingTime = num1;
            paramsPtr3 = (MyHudEntityParams*) ref item;
            base.m_hudParams.Add(item);
            return base.m_hudParams;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_TerminalBlock objectBuilderCubeBlock = (MyObjectBuilder_TerminalBlock) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.CustomName = this.DisplayNameText.ToString();
            objectBuilderCubeBlock.ShowOnHUD = this.ShowOnHUD;
            objectBuilderCubeBlock.ShowInTerminal = this.ShowInTerminal;
            objectBuilderCubeBlock.ShowInInventory = this.ShowInInventory;
            objectBuilderCubeBlock.ShowInToolbarConfig = this.ShowInToolbarConfig;
            return objectBuilderCubeBlock;
        }

        public void GetProperties(List<ITerminalProperty> resultList, Func<ITerminalProperty, bool> collect = null)
        {
            MyTerminalControlFactoryHelper.Static.GetProperties(base.GetType(), resultList, collect);
        }

        public ITerminalProperty GetProperty(string id) => 
            MyTerminalControlFactoryHelper.Static.GetProperty(id, base.GetType());

        public virtual void GetTerminalName(StringBuilder result)
        {
            result.AppendStringBuilder(this.CustomName);
        }

        public bool HasLocalPlayerAccess() => 
            this.HasPlayerAccess(MySession.Static.LocalPlayerId);

        public bool HasPlayerAccess(long playerId) => 
            (this.HasPlayerAccessReason(playerId) == AccessRightsResult.Granted);

        public AccessRightsResult HasPlayerAccessReason(long playerId)
        {
            if (!MyFakes.SHOW_FACTIONS_GUI)
            {
                return AccessRightsResult.Other;
            }
            ulong steamId = MySession.Static.Players.TryGetSteamId(playerId);
            if (!MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(base.BlockDefinition, steamId))
            {
                return AccessRightsResult.MissingDLC;
            }
            if (Sync.IsServer)
            {
                AdminSettingsEnum enum2;
                if (MySession.Static.RemoteAdminSettings.TryGetValue(steamId, out enum2) && enum2.HasFlag(AdminSettingsEnum.UseTerminals))
                {
                    return AccessRightsResult.Granted;
                }
            }
            else if (playerId == MySession.Static.LocalPlayerId)
            {
                if (MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals))
                {
                    return AccessRightsResult.Granted;
                }
            }
            else
            {
                AdminSettingsEnum enum3;
                if (MySession.Static.RemoteAdminSettings.TryGetValue(steamId, out enum3) && enum3.HasFlag(AdminSettingsEnum.UseTerminals))
                {
                    return AccessRightsResult.Granted;
                }
            }
            return (base.GetUserRelationToOwner(playerId).IsFriendly() ? AccessRightsResult.Granted : AccessRightsResult.Enemies);
        }

        protected virtual bool HasUnsafeSettingsCollector() => 
            false;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_TerminalBlock block = (MyObjectBuilder_TerminalBlock) objectBuilder;
            if (block.CustomName == null)
            {
                this.CustomName.Append(this.DisplayNameText);
            }
            else
            {
                this.CustomName.Clear().Append(block.CustomName);
                this.DisplayNameText = block.CustomName;
            }
            if (Sync.IsServer && (Sync.Clients != null))
            {
                MyClientCollection clients = Sync.Clients;
                clients.ClientRemoved = (Action<ulong>) Delegate.Combine(clients.ClientRemoved, new Action<ulong>(this.ClientRemoved));
            }
            this.m_showOnHUD.ValueChanged += new Action<SyncBase>(this.m_showOnHUD_ValueChanged);
            this.m_showOnHUD.SetLocalValue(block.ShowOnHUD);
            this.m_showInTerminal.SetLocalValue(block.ShowInTerminal);
            this.m_showInInventory.SetLocalValue(block.ShowInInventory);
            this.m_showInToolbarConfig.SetLocalValue(block.ShowInToolbarConfig);
            base.AddDebugRenderComponent(new MyDebugRenderComponentTerminal(this));
        }

        private void m_showOnHUD_ValueChanged(SyncBase obj)
        {
            if (base.CubeGrid != null)
            {
                base.CubeGrid.MarkForUpdate();
            }
        }

        public void NotifyTerminalValueChanged(ITerminalControl control)
        {
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (this is IMyControllableEntity)
            {
                MyPlayerCollection.UpdateControl(this);
            }
        }

        private void OnChangeOpen(bool isOpen, bool editable, ulong user)
        {
            this.m_textboxOpen = isOpen;
            this.m_currentUser = user;
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && (user == Sync.MyId)) & isOpen)
            {
                this.OpenWindow(editable, false);
            }
        }

        [Event(null, 0x28e), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnChangeOpenRequest(bool isOpen, bool editable, ulong user)
        {
            if (!((Sync.IsServer && this.m_textboxOpen) & isOpen))
            {
                this.OnChangeOpen(isOpen, editable, user);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTerminalBlock, bool, bool, ulong>(this, x => new Action<bool, bool, ulong>(x.OnChangeOpenSuccess), isOpen, editable, user, targetEndpoint);
            }
        }

        [Event(null, 0x299), Reliable, Broadcast]
        private void OnChangeOpenSuccess(bool isOpen, bool editable, ulong user)
        {
            this.OnChangeOpen(isOpen, editable, user);
        }

        public void OnClosedMessageBox(VRage.Game.ModAPI.ResultEnum result)
        {
            if (result == VRage.Game.ModAPI.ResultEnum.OK)
            {
                this.CloseWindow();
            }
            else
            {
                this.CreateTextBox(true, this.m_textBox.Description.Text.ToString());
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        public void OnClosedTextBox(VRage.Game.ModAPI.ResultEnum result)
        {
            if (this.m_textBox != null)
            {
                this.CloseWindow();
            }
        }

        [Event(null, 0x283), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        private void OnCustomDataChanged(string data)
        {
            this.SetCustomData_Internal(data, false);
        }

        protected override void OnOwnershipChanged()
        {
            base.OnOwnershipChanged();
            this.RaiseOwnershipChanged();
            this.RaiseShowOnHUDChanged();
            this.RaisePropertiesChanged();
        }

        public override void OnRemovedFromScene(object source)
        {
            if (this.HasUnsafeValues)
            {
                base.CubeGrid.UnregisterUnsafeBlock(this);
            }
            base.OnRemovedFromScene(source);
        }

        protected void OnUnsafeSettingsChanged()
        {
            MySandboxGame.Static.Invoke("", this, x => OnUnsafeSettingsChangedInternal(x));
        }

        private static void OnUnsafeSettingsChangedInternal(object o)
        {
            MyTerminalBlock block = (MyTerminalBlock) o;
            if (!block.MarkedForClose)
            {
                bool flag = block.HasUnsafeSettingsCollector();
                if (block.HasUnsafeValues != flag)
                {
                    block.HasUnsafeValues = flag;
                    if (flag)
                    {
                        block.CubeGrid.RegisterUnsafeBlock(block);
                    }
                    else
                    {
                        block.CubeGrid.UnregisterUnsafeBlock(block);
                    }
                }
            }
        }

        public void OpenWindow(bool isEditable, bool sync)
        {
            if (sync)
            {
                this.SendChangeOpenMessage(true, isEditable, Sync.MyId);
            }
            else
            {
                this.CreateTextBox(isEditable, this.CustomData);
                MyGuiScreenGamePlay.TmpGameplayScreenHolder = MyGuiScreenGamePlay.ActiveGameplayScreen;
                MyGuiScreenGamePlay.ActiveGameplayScreen = this.m_textBox;
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        protected void PrintUpgradeModuleInfo()
        {
            if (base.GetComponent().ConnectionPositions.Count != 0)
            {
                int num = 0;
                if (base.CurrentAttachedUpgradeModules != null)
                {
                    foreach (MyCubeBlock.AttachedUpgradeModule module in base.CurrentAttachedUpgradeModules.Values)
                    {
                        num += module.SlotCount;
                    }
                }
                string[] textArray1 = new string[] { MyTexts.Get(MyCommonTexts.Module_UsedSlots).ToString(), num.ToString(), " / ", base.GetComponent().ConnectionPositions.Count.ToString(), "\n" };
                this.DetailedInfo.Append(string.Concat(textArray1));
                if (base.CurrentAttachedUpgradeModules != null)
                {
                    int num3 = 0;
                    foreach (MyCubeBlock.AttachedUpgradeModule module2 in base.CurrentAttachedUpgradeModules.Values)
                    {
                        int num1;
                        if ((module2.Block == null) || !module2.Block.IsWorking)
                        {
                            num1 = 0;
                        }
                        else
                        {
                            num1 = 1;
                        }
                        num3 += num1;
                    }
                    this.DetailedInfo.Append(MyTexts.Get(MyCommonTexts.Module_Attached).ToString() + base.CurrentAttachedUpgradeModules.Count.ToString());
                    if (num3 != base.CurrentAttachedUpgradeModules.Count)
                    {
                        this.DetailedInfo.Append(" (" + num3.ToString() + MyTexts.Get(MyCommonTexts.Module_Functioning).ToString());
                    }
                    this.DetailedInfo.Append("\n");
                    foreach (MyCubeBlock.AttachedUpgradeModule module3 in base.CurrentAttachedUpgradeModules.Values)
                    {
                        if (module3.Block != null)
                        {
                            this.DetailedInfo.Append(" - " + module3.Block.DisplayNameText + (module3.Block.IsFunctional ? (module3.Compatible ? (module3.Block.Enabled ? "" : MyTexts.Get(MyCommonTexts.Module_Off).ToString()) : MyTexts.Get(MyCommonTexts.Module_Incompatible).ToString()) : MyTexts.Get(MyCommonTexts.Module_Damaged).ToString()));
                        }
                        else
                        {
                            this.DetailedInfo.Append(MyTexts.Get(MyCommonTexts.Module_Unknown).ToString());
                        }
                        this.DetailedInfo.Append("\n");
                    }
                }
                this.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            }
        }

        private void RaiseCustomDataChanged()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTerminalBlock, string>(this, x => new Action<string>(x.OnCustomDataChanged), this.CustomData, targetEndpoint);
        }

        private void RaiseCustomNameChanged()
        {
            Action<MyTerminalBlock> customNameChanged = this.CustomNameChanged;
            if (customNameChanged != null)
            {
                customNameChanged(this);
            }
        }

        protected void RaiseIsBeingHackedChanged()
        {
            Action<MyTerminalBlock> isBeingHackedChanged = this.IsBeingHackedChanged;
            if (isBeingHackedChanged != null)
            {
                isBeingHackedChanged(this);
            }
        }

        private void RaiseOwnershipChanged()
        {
            if (this.OwnershipChanged != null)
            {
                this.OwnershipChanged(this);
            }
        }

        public void RaisePropertiesChanged()
        {
            Action<MyTerminalBlock> propertiesChanged = this.PropertiesChanged;
            if (propertiesChanged != null)
            {
                propertiesChanged(this);
            }
        }

        protected void RaiseShowInInventoryChanged()
        {
            Action<MyTerminalBlock> showInIventoryChanged = this.ShowInIventoryChanged;
            if (showInIventoryChanged != null)
            {
                showInIventoryChanged(this);
            }
        }

        protected void RaiseShowInTerminalChanged()
        {
            Action<MyTerminalBlock> showInTerminalChanged = this.ShowInTerminalChanged;
            if (showInTerminalChanged != null)
            {
                showInTerminalChanged(this);
            }
        }

        protected void RaiseShowInToolbarConfigChanged()
        {
            Action<MyTerminalBlock> showInToolbarConfigChanged = this.ShowInToolbarConfigChanged;
            if (showInToolbarConfigChanged != null)
            {
                showInToolbarConfigChanged(this);
            }
        }

        protected void RaiseShowOnHUDChanged()
        {
            Action<MyTerminalBlock> showOnHUDChanged = this.ShowOnHUDChanged;
            if (showOnHUDChanged != null)
            {
                showOnHUDChanged(this);
            }
        }

        protected void RaiseVisibilityChanged()
        {
            Action<MyTerminalBlock> visibilityChanged = this.VisibilityChanged;
            if (visibilityChanged != null)
            {
                visibilityChanged(this);
            }
        }

        public void RefreshCustomInfo()
        {
            this.CustomInfo.Clear();
            Action<MyTerminalBlock, StringBuilder> appendingCustomInfo = this.AppendingCustomInfo;
            if (appendingCustomInfo != null)
            {
                appendingCustomInfo(this, this.CustomInfo);
            }
        }

        bool Sandbox.ModAPI.IMyTerminalBlock.IsInSameLogicalGroupAs(Sandbox.ModAPI.IMyTerminalBlock other) => 
            base.CubeGrid.IsInSameLogicalGroupAs(other.CubeGrid);

        bool Sandbox.ModAPI.IMyTerminalBlock.IsSameConstructAs(Sandbox.ModAPI.IMyTerminalBlock other) => 
            base.CubeGrid.IsSameConstructAs(other.CubeGrid);

        void Sandbox.ModAPI.Ingame.IMyTerminalBlock.GetActions(List<Sandbox.ModAPI.Interfaces.ITerminalAction> resultList, Func<Sandbox.ModAPI.Interfaces.ITerminalAction, bool> collect)
        {
            ((IMyTerminalActionsHelper) MyTerminalControlFactoryHelper.Static).GetActions(base.GetType(), resultList, collect);
        }

        Sandbox.ModAPI.Interfaces.ITerminalAction Sandbox.ModAPI.Ingame.IMyTerminalBlock.GetActionWithName(string name) => 
            ((IMyTerminalActionsHelper) MyTerminalControlFactoryHelper.Static).GetActionWithName(name, base.GetType());

        bool Sandbox.ModAPI.Ingame.IMyTerminalBlock.IsSameConstructAs(Sandbox.ModAPI.Ingame.IMyTerminalBlock other) => 
            ((VRage.Game.ModAPI.Ingame.IMyCubeGrid) base.CubeGrid).IsSameConstructAs(other.CubeGrid);

        void Sandbox.ModAPI.Ingame.IMyTerminalBlock.SearchActionsOfName(string name, List<Sandbox.ModAPI.Interfaces.ITerminalAction> resultList, Func<Sandbox.ModAPI.Interfaces.ITerminalAction, bool> collect = null)
        {
            ((IMyTerminalActionsHelper) MyTerminalControlFactoryHelper.Static).SearchActionsOfName(name, base.GetType(), resultList, collect);
        }

        private void SendChangeOpenMessage(bool isOpen, bool editable = false, ulong user = 0UL)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTerminalBlock, bool, bool, ulong>(this, x => new Action<bool, bool, ulong>(x.OnChangeOpenRequest), isOpen, editable, user, targetEndpoint);
        }

        private void SetCustomData_Internal(string value, bool sync)
        {
            if (base.Storage == null)
            {
                base.Storage = new MyModStorageComponent();
                base.Components.Add<MyModStorageComponentBase>(base.Storage);
            }
            base.Storage[m_storageGuid] = (value.Length > 0xfa00) ? value.Substring(0, 0xfa00) : value;
            if (sync)
            {
                this.RaiseCustomDataChanged();
            }
            else
            {
                Action<MyTerminalBlock> customDataChanged = this.CustomDataChanged;
                if (customDataChanged != null)
                {
                    customDataChanged(this);
                }
            }
        }

        public void SetCustomName(string text)
        {
            this.UpdateCustomName(text);
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTerminalBlock, string>(this, x => new Action<string>(x.SetCustomNameEvent), text, targetEndpoint);
        }

        public void SetCustomName(StringBuilder text)
        {
            this.UpdateCustomName(text);
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTerminalBlock, string>(this, x => new Action<string>(x.SetCustomNameEvent), text.ToString(), targetEndpoint);
        }

        [Event(null, 0x157), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        public void SetCustomNameEvent(string name)
        {
            this.UpdateCustomName(name);
        }

        public override string ToString() => 
            (base.ToString() + " " + this.CustomName);

        public void UpdateCustomName(string text)
        {
            if (this.CustomName.CompareUpdate(text))
            {
                this.RaiseCustomNameChanged();
                this.RaiseShowOnHUDChanged();
                this.DisplayNameText = text;
            }
        }

        public void UpdateCustomName(StringBuilder text)
        {
            if (this.CustomName.CompareUpdate(text))
            {
                this.DisplayNameText = text.ToString();
                this.RaiseCustomNameChanged();
                this.RaiseShowOnHUDChanged();
            }
        }

        public StringBuilder CustomName { get; private set; }

        public StringBuilder CustomNameWithFaction { get; private set; }

        public string CustomData
        {
            get
            {
                string str;
                if ((base.Storage == null) || !base.Storage.TryGetValue(m_storageGuid, out str))
                {
                    return string.Empty;
                }
                return str;
            }
            set => 
                this.SetCustomData_Internal(value, true);
        }

        public bool ShowOnHUD
        {
            get => 
                ((bool) this.m_showOnHUD);
            set
            {
                if (this.m_showOnHUD != value)
                {
                    this.m_showOnHUD.Value = value;
                    this.RaiseShowOnHUDChanged();
                }
            }
        }

        public bool ShowInTerminal
        {
            get => 
                ((bool) this.m_showInTerminal);
            set
            {
                if (this.m_showInTerminal != value)
                {
                    this.m_showInTerminal.Value = value;
                    this.RaiseShowInTerminalChanged();
                }
            }
        }

        public bool ShowInInventory
        {
            get => 
                ((bool) this.m_showInInventory);
            set
            {
                if (this.m_showInInventory != value)
                {
                    this.m_showInInventory.Value = value;
                    this.RaiseShowInInventoryChanged();
                }
            }
        }

        public bool ShowInToolbarConfig
        {
            get => 
                ((bool) this.m_showInToolbarConfig);
            set
            {
                if (this.m_showInToolbarConfig != value)
                {
                    this.m_showInToolbarConfig.Value = value;
                    this.RaiseShowInToolbarConfigChanged();
                }
            }
        }

        public bool IsBeingHacked
        {
            get
            {
                if (this.HackAttemptTime == null)
                {
                    return false;
                }
                bool flag = (MySandboxGame.TotalSimulationTimeInMilliseconds - this.HackAttemptTime.Value) < 0x3e8;
                if (flag != this.m_isBeingHackedPrevValue)
                {
                    this.m_isBeingHackedPrevValue = flag;
                    this.RaiseIsBeingHackedChanged();
                }
                return flag;
            }
        }

        public StringBuilder DetailedInfo { get; private set; }

        public StringBuilder CustomInfo { get; private set; }

        public bool HasUnsafeValues { get; private set; }

        public bool IsOpenedInTerminal { get; set; }

        string Sandbox.ModAPI.Ingame.IMyTerminalBlock.CustomName
        {
            get => 
                this.CustomName.ToString();
            set => 
                this.SetCustomName(value);
        }

        string Sandbox.ModAPI.Ingame.IMyTerminalBlock.CustomNameWithFaction =>
            this.CustomNameWithFaction.ToString();

        string Sandbox.ModAPI.Ingame.IMyTerminalBlock.DetailedInfo =>
            this.DetailedInfo.ToString();

        string Sandbox.ModAPI.Ingame.IMyTerminalBlock.CustomInfo =>
            this.CustomInfo.ToString();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalBlock.<>c <>9 = new MyTerminalBlock.<>c();
            public static Func<MyTerminalBlock, Action<string>> <>9__98_0;
            public static Func<MyTerminalBlock, Action<string>> <>9__100_0;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.GetterDelegate <>9__120_0;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.SetterDelegate <>9__120_1;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.GetterDelegate <>9__120_2;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.SetterDelegate <>9__120_3;
            public static Func<MyTerminalBlock, bool> <>9__120_4;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.GetterDelegate <>9__120_5;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.SetterDelegate <>9__120_6;
            public static MyTerminalControlTextbox<MyTerminalBlock>.GetterDelegate <>9__120_7;
            public static MyTerminalControlTextbox<MyTerminalBlock>.SetterDelegate <>9__120_8;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.GetterDelegate <>9__120_9;
            public static MyTerminalValueControl<MyTerminalBlock, bool>.SetterDelegate <>9__120_10;
            public static Func<MyTerminalBlock, bool> <>9__120_11;
            public static Func<MyTerminalBlock, Action<string>> <>9__122_0;
            public static Func<MyTerminalBlock, Action<bool, bool, ulong>> <>9__124_0;
            public static Func<MyTerminalBlock, Action<bool, bool, ulong>> <>9__125_0;
            public static Action<object> <>9__134_0;

            internal bool <CreateTerminalControls>b__120_0(MyTerminalBlock x) => 
                ((bool) x.m_showInTerminal);

            internal void <CreateTerminalControls>b__120_1(MyTerminalBlock x, bool v)
            {
                x.ShowInTerminal = v;
            }

            internal void <CreateTerminalControls>b__120_10(MyTerminalBlock x, bool v)
            {
                x.ShowOnHUD = v;
            }

            internal bool <CreateTerminalControls>b__120_11(MyTerminalBlock x) => 
                !x.m_textboxOpen;

            internal bool <CreateTerminalControls>b__120_2(MyTerminalBlock x) => 
                ((bool) x.m_showInInventory);

            internal void <CreateTerminalControls>b__120_3(MyTerminalBlock x, bool v)
            {
                x.ShowInInventory = v;
            }

            internal bool <CreateTerminalControls>b__120_4(MyTerminalBlock x) => 
                x.HasInventory;

            internal bool <CreateTerminalControls>b__120_5(MyTerminalBlock x) => 
                ((bool) x.m_showInToolbarConfig);

            internal void <CreateTerminalControls>b__120_6(MyTerminalBlock x, bool v)
            {
                x.ShowInToolbarConfig = v;
            }

            internal StringBuilder <CreateTerminalControls>b__120_7(MyTerminalBlock x) => 
                x.CustomName;

            internal void <CreateTerminalControls>b__120_8(MyTerminalBlock x, StringBuilder v)
            {
                x.SetCustomName(v);
            }

            internal bool <CreateTerminalControls>b__120_9(MyTerminalBlock x) => 
                x.ShowOnHUD;

            internal Action<bool, bool, ulong> <OnChangeOpenRequest>b__125_0(MyTerminalBlock x) => 
                new Action<bool, bool, ulong>(x.OnChangeOpenSuccess);

            internal void <OnUnsafeSettingsChanged>b__134_0(object x)
            {
                MyTerminalBlock.OnUnsafeSettingsChangedInternal(x);
            }

            internal Action<string> <RaiseCustomDataChanged>b__122_0(MyTerminalBlock x) => 
                new Action<string>(x.OnCustomDataChanged);

            internal Action<bool, bool, ulong> <SendChangeOpenMessage>b__124_0(MyTerminalBlock x) => 
                new Action<bool, bool, ulong>(x.OnChangeOpenRequest);

            internal Action<string> <SetCustomName>b__100_0(MyTerminalBlock x) => 
                new Action<string>(x.SetCustomNameEvent);

            internal Action<string> <SetCustomName>b__98_0(MyTerminalBlock x) => 
                new Action<string>(x.SetCustomNameEvent);
        }

        public enum AccessRightsResult
        {
            Granted,
            Enemies,
            MissingDLC,
            Other
        }
    }
}

