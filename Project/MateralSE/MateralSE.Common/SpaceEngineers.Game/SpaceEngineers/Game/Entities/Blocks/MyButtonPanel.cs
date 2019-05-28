namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Serialization;
    using VRage.Sync;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_ButtonPanel)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyButtonPanel), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel) })]
    public class MyButtonPanel : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyButtonPanel, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel
    {
        private const string DETECTOR_NAME = "panel";
        private List<string> m_emissiveNames;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_anyoneCanUse;
        private int m_selectedButton = -1;
        private MyHudNotification m_activationFailedNotification = new MyHudNotification(MySpaceTexts.Notification_ActivationFailed, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        private static List<MyToolbar> m_openedToolbars;
        private static bool m_shouldSetOtherToolbars;
        private SerializableDictionary<int, string> m_customButtonNames = new SerializableDictionary<int, string>();
        private List<MyUseObjectPanelButton> m_buttonsUseObjects = new List<MyUseObjectPanelButton>();
        private StringBuilder m_emptyName = new StringBuilder("");
        private bool m_syncing;
        [CompilerGenerated]
        private Action<int> ButtonPressed;
        private static StringBuilder m_helperSB = new StringBuilder();

        private event Action<int> ButtonPressed
        {
            [CompilerGenerated] add
            {
                Action<int> buttonPressed = this.ButtonPressed;
                while (true)
                {
                    Action<int> a = buttonPressed;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    buttonPressed = Interlocked.CompareExchange<Action<int>>(ref this.ButtonPressed, action3, a);
                    if (ReferenceEquals(buttonPressed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> buttonPressed = this.ButtonPressed;
                while (true)
                {
                    Action<int> source = buttonPressed;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    buttonPressed = Interlocked.CompareExchange<Action<int>>(ref this.ButtonPressed, action3, source);
                    if (ReferenceEquals(buttonPressed, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<int> SpaceEngineers.Game.ModAPI.IMyButtonPanel.ButtonPressed
        {
            add
            {
                this.ButtonPressed += value;
            }
            remove
            {
                this.ButtonPressed -= value;
            }
        }

        public MyButtonPanel()
        {
            this.CreateTerminalControls();
            m_openedToolbars = new List<MyToolbar>();
        }

        [Event(null, 0x142), Reliable, Server(ValidationType.Access)]
        public void ActivateButton(int index)
        {
            this.Toolbar.UpdateItem(index);
            this.PressButton(index);
            if (!this.Toolbar.ActivateItemAtIndex(index, false))
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyButtonPanel>(this, x => new Action(x.NotifyActivationFailed), MyEventContext.Current.Sender);
            }
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId));

        protected override void Closing()
        {
            base.Closing();
            using (List<MyUseObjectPanelButton>.Enumerator enumerator = this.m_buttonsUseObjects.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.RemoveButtonMarker();
                }
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
            this.UpdateEmissivity();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyButtonPanel>())
            {
                base.CreateTerminalControls();
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MyButtonPanel> checkbox1 = new MyTerminalControlCheckbox<MyButtonPanel>("AnyoneCanUse", MySpaceTexts.BlockPropertyText_AnyoneCanUse, MySpaceTexts.BlockPropertyDescription_AnyoneCanUse, on, on);
                MyTerminalControlCheckbox<MyButtonPanel> checkbox2 = new MyTerminalControlCheckbox<MyButtonPanel>("AnyoneCanUse", MySpaceTexts.BlockPropertyText_AnyoneCanUse, MySpaceTexts.BlockPropertyDescription_AnyoneCanUse, on, on);
                checkbox2.Getter = x => x.AnyoneCanUse;
                MyTerminalControlCheckbox<MyButtonPanel> local14 = checkbox2;
                MyTerminalControlCheckbox<MyButtonPanel> local15 = checkbox2;
                local15.Setter = (x, v) => x.AnyoneCanUse = v;
                MyTerminalControlCheckbox<MyButtonPanel> checkbox = local15;
                checkbox.EnableAction<MyButtonPanel>(null);
                MyTerminalControlFactory.AddControl<MyButtonPanel>(checkbox);
                MyTerminalControlFactory.AddControl<MyButtonPanel>(new MyTerminalControlButton<MyButtonPanel>("Open Toolbar", MySpaceTexts.BlockPropertyTitle_SensorToolbarOpen, MySpaceTexts.BlockPropertyDescription_SensorToolbarOpen, delegate (MyButtonPanel self) {
                    m_openedToolbars.Add(self.Toolbar);
                    if (MyGuiScreenToolbarConfigBase.Static == null)
                    {
                        m_shouldSetOtherToolbars = true;
                        MyToolbarComponent.CurrentToolbar = self.Toolbar;
                        object[] args = new object[] { 0, self };
                        MyToolbarComponent.AutoUpdate = false;
                        MyGuiScreenBase base1 = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                        MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                        screen.Closed += delegate (MyGuiScreenBase source) {
                            MyToolbarComponent.AutoUpdate = true;
                            m_openedToolbars.Clear();
                        };
                        MyGuiSandbox.AddScreen(screen);
                    }
                }));
                MyTerminalControlListbox<MyButtonPanel> listbox1 = new MyTerminalControlListbox<MyButtonPanel>("ButtonText", MySpaceTexts.BlockPropertyText_ButtonList, MySpaceTexts.Blank, false, 8);
                MyTerminalControlListbox<MyButtonPanel> listbox2 = new MyTerminalControlListbox<MyButtonPanel>("ButtonText", MySpaceTexts.BlockPropertyText_ButtonList, MySpaceTexts.Blank, false, 8);
                listbox2.ListContent = (x, list1, list2) => x.FillListContent(list1, list2);
                MyTerminalControlListbox<MyButtonPanel> local12 = listbox2;
                MyTerminalControlListbox<MyButtonPanel> control = listbox2;
                control.ItemSelected = (x, y) => x.SelectButtonToName(y);
                MyTerminalControlFactory.AddControl<MyButtonPanel>(control);
                MyTerminalControlTextbox<MyButtonPanel> textbox1 = new MyTerminalControlTextbox<MyButtonPanel>("ButtonName", MySpaceTexts.BlockPropertyText_ButtonName, MySpaceTexts.Blank);
                MyTerminalControlTextbox<MyButtonPanel> textbox2 = new MyTerminalControlTextbox<MyButtonPanel>("ButtonName", MySpaceTexts.BlockPropertyText_ButtonName, MySpaceTexts.Blank);
                textbox2.Getter = x => x.GetButtonName();
                MyTerminalControlTextbox<MyButtonPanel> local10 = textbox2;
                MyTerminalControlTextbox<MyButtonPanel> local11 = textbox2;
                local11.Setter = (x, v) => x.SetCustomButtonName(v);
                MyTerminalControlTextbox<MyButtonPanel> local9 = local11;
                local9.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyButtonPanel>(local9);
            }
        }

        public void FillListContent(ICollection<MyGuiControlListbox.Item> listBoxContent, ICollection<MyGuiControlListbox.Item> listBoxSelectedItems)
        {
            string str = MyTexts.GetString(MySpaceTexts.BlockPropertyText_Button);
            for (int i = 0; i < this.m_buttonsUseObjects.Count; i++)
            {
                int num2 = i + 1;
                m_helperSB.Clear().Append(str + " " + num2.ToString());
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(m_helperSB, null, null, i, null);
                listBoxContent.Add(item);
                if (i == this.m_selectedButton)
                {
                    listBoxSelectedItems.Add(item);
                }
            }
        }

        public StringBuilder GetButtonName()
        {
            if (this.m_selectedButton == -1)
            {
                return this.m_emptyName;
            }
            string str = null;
            if (this.m_customButtonNames.Dictionary.TryGetValue(this.m_selectedButton, out str))
            {
                return new StringBuilder(str);
            }
            MyToolbarItem itemAtIndex = this.Toolbar.GetItemAtIndex(this.m_selectedButton);
            return ((itemAtIndex == null) ? this.m_emptyName : itemAtIndex.DisplayName);
        }

        public string GetCustomButtonName(int pos)
        {
            string str = null;
            if (this.m_customButtonNames.Dictionary.TryGetValue(pos, out str))
            {
                return str;
            }
            MyToolbarItem itemAtIndex = this.Toolbar.GetItemAtIndex(pos);
            return ((itemAtIndex == null) ? MyTexts.GetString(MySpaceTexts.NotificationHintNoAction) : itemAtIndex.DisplayName.ToString());
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ButtonPanel objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_ButtonPanel;
            objectBuilderCubeBlock.Toolbar = this.Toolbar.GetObjectBuilder();
            objectBuilderCubeBlock.AnyoneCanUse = this.AnyoneCanUse;
            objectBuilderCubeBlock.CustomButtonNames = this.m_customButtonNames;
            return objectBuilderCubeBlock;
        }

        public bool HasCustomButtonName(int pos) => 
            this.m_customButtonNames.Dictionary.ContainsKey(pos);

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, 0.0001f, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return 0.0001f;
            });
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            component.IsPoweredChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink = component;
            base.Init(builder, cubeGrid);
            this.m_emissiveNames = new List<string>(this.BlockDefinition.ButtonCount);
            for (int i = 1; i <= this.BlockDefinition.ButtonCount; i++)
            {
                this.m_emissiveNames.Add($"Emissive{i}");
            }
            MyObjectBuilder_ButtonPanel panel = builder as MyObjectBuilder_ButtonPanel;
            this.Toolbar = new MyToolbar(MyToolbarType.ButtonPanel, Math.Min(this.BlockDefinition.ButtonCount, 9), (this.BlockDefinition.ButtonCount / 9) + 1);
            this.Toolbar.DrawNumbers = false;
            this.Toolbar.GetSymbol = delegate (int slot) {
                ColoredIcon icon = new ColoredIcon();
                if (this.Toolbar.SlotToIndex(slot) < this.BlockDefinition.ButtonCount)
                {
                    icon.Icon = this.BlockDefinition.ButtonSymbols[this.Toolbar.SlotToIndex(slot) % this.BlockDefinition.ButtonSymbols.Length];
                    Vector4 vector = this.BlockDefinition.ButtonColors[this.Toolbar.SlotToIndex(slot) % this.BlockDefinition.ButtonColors.Length];
                    vector.W = 1f;
                    icon.Color = vector;
                }
                return icon;
            };
            this.Toolbar.Init(panel.Toolbar, this, false);
            this.Toolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
            this.m_anyoneCanUse.SetLocalValue(panel.AnyoneCanUse);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink.Update();
            if (panel.CustomButtonNames != null)
            {
                foreach (int num2 in panel.CustomButtonNames.Dictionary.Keys)
                {
                    this.m_customButtonNames.Dictionary.Add(num2, MyStatControlText.SubstituteTexts(panel.CustomButtonNames[num2], null));
                }
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            base.UseObjectsComponent.GetInteractiveObjects<MyUseObjectPanelButton>(this.m_buttonsUseObjects);
        }

        public bool IsButtonAssigned(int pos) => 
            (this.Toolbar.GetItemAtIndex(pos) != null);

        [Event(null, 0x14c), Reliable, Client]
        private void NotifyActivationFailed()
        {
            MyHud.Notifications.Add(this.m_activationFailedNotification);
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            base.ResourceSink.Update();
            this.UpdateEmissivity();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            if (base.InScene)
            {
                this.UpdateEmissivity();
            }
        }

        public override void OnRegisteredToGridSystems()
        {
            base.OnRegisteredToGridSystems();
            this.UpdateEmissivity();
        }

        public void PressButton(int i)
        {
            if (this.ButtonPressed != null)
            {
                this.ButtonPressed(i);
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        public void SelectButtonToName(List<MyGuiControlListbox.Item> imageIds)
        {
            this.m_selectedButton = (int) imageIds[0].UserData;
            base.RaisePropertiesChanged();
        }

        [Event(null, 0x1c1), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void SendToolbarItemChanged(ToolbarItem sentItem, int index)
        {
            this.m_syncing = true;
            MyToolbarItem item = null;
            if (sentItem.EntityID != 0)
            {
                item = ToolbarItem.ToItem(sentItem);
            }
            this.Toolbar.SetItemAtIndex(index, item);
            this.UpdateButtonEmissivity(index);
            this.m_syncing = false;
        }

        [Event(null, 0x188), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void SetButtonName(string name, int position)
        {
            string str = null;
            if (name == null)
            {
                this.m_customButtonNames.Dictionary.Remove(position);
            }
            else if (this.m_customButtonNames.Dictionary.TryGetValue(position, out str))
            {
                this.m_customButtonNames.Dictionary[position] = name.ToString();
            }
            else
            {
                this.m_customButtonNames.Dictionary.Add(position, name.ToString());
            }
        }

        public void SetCustomButtonName(StringBuilder name)
        {
            if (this.m_selectedButton != -1)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyButtonPanel, string, int>(this, x => new Action<string, int>(x.SetButtonName), name.ToString(), this.m_selectedButton, targetEndpoint);
            }
        }

        public void SetCustomButtonName(string name, int pos)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyButtonPanel, string, int>(this, x => new Action<string, int>(x.SetButtonName), name, pos, targetEndpoint);
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel.ClearCustomButtonName(int index)
        {
            this.SetCustomButtonName(null, index);
        }

        string SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel.GetButtonName(int index) => 
            this.GetCustomButtonName(index);

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel.HasCustomButtonName(int index) => 
            this.HasCustomButtonName(index);

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel.IsButtonAssigned(int index) => 
            this.IsButtonAssigned(index);

        void SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel.SetCustomButtonName(int index, string name)
        {
            this.SetCustomButtonName(name, index);
        }

        private void Toolbar_ItemChanged(MyToolbar self, MyToolbar.IndexArgs index)
        {
            if (!this.m_syncing)
            {
                ToolbarItem item = ToolbarItem.FromItem(self.GetItemAtIndex(index.ItemIndex));
                this.UpdateButtonEmissivity(index.ItemIndex);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyButtonPanel, ToolbarItem, int>(this, x => new Action<ToolbarItem, int>(x.SendToolbarItemChanged), item, index.ItemIndex, targetEndpoint);
                if (m_shouldSetOtherToolbars)
                {
                    m_shouldSetOtherToolbars = false;
                    foreach (MyToolbar toolbar in m_openedToolbars)
                    {
                        if (!ReferenceEquals(toolbar, self))
                        {
                            toolbar.SetItemAtIndex(index.ItemIndex, self.GetItemAtIndex(index.ItemIndex));
                        }
                    }
                    m_shouldSetOtherToolbars = true;
                }
                MyToolbarItem itemAtIndex = this.Toolbar.GetItemAtIndex(index.ItemIndex);
                if (itemAtIndex == null)
                {
                    targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyButtonPanel, string, int>(this, x => new Action<string, int>(x.SetButtonName), MyTexts.GetString(MySpaceTexts.NotificationHintNoAction), index.ItemIndex, targetEndpoint);
                }
                else
                {
                    string str = itemAtIndex.DisplayName.ToString();
                    targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyButtonPanel, string, int>(this, x => new Action<string, int>(x.SetButtonName), str, index.ItemIndex, targetEndpoint);
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

        private void UpdateButtonEmissivity(int index)
        {
            if (base.InScene)
            {
                Vector4 unassignedButtonColor = this.BlockDefinition.ButtonColors[index % this.BlockDefinition.ButtonColors.Length];
                if (this.Toolbar.GetItemAtIndex(index) == null)
                {
                    unassignedButtonColor = this.BlockDefinition.UnassignedButtonColor;
                }
                float w = unassignedButtonColor.W;
                if (!base.IsWorking)
                {
                    if (base.IsFunctional)
                    {
                        unassignedButtonColor = Color.Red.ToVector4();
                        w = 0f;
                    }
                    else
                    {
                        unassignedButtonColor = Color.Black.ToVector4();
                        w = 0f;
                    }
                }
                UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], this.m_emissiveNames[index], new Color(unassignedButtonColor.X, unassignedButtonColor.Y, unassignedButtonColor.Z), w);
            }
        }

        private void UpdateEmissivity()
        {
            for (int i = 0; i < this.BlockDefinition.ButtonCount; i++)
            {
                this.UpdateButtonEmissivity(i);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.UpdateEmissivity();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity();
            this.m_buttonsUseObjects.Clear();
            base.UseObjectsComponent.GetInteractiveObjects<MyUseObjectPanelButton>(this.m_buttonsUseObjects);
        }

        public MyToolbar Toolbar { get; set; }

        public MyButtonPanelDefinition BlockDefinition =>
            (base.BlockDefinition as MyButtonPanelDefinition);

        public bool AnyoneCanUse
        {
            get => 
                ((bool) this.m_anyoneCanUse);
            set => 
                (this.m_anyoneCanUse.Value = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyButtonPanel.<>c <>9 = new MyButtonPanel.<>c();
            public static MyTerminalValueControl<MyButtonPanel, bool>.GetterDelegate <>9__21_0;
            public static MyTerminalValueControl<MyButtonPanel, bool>.SetterDelegate <>9__21_1;
            public static MyGuiScreenBase.ScreenHandler <>9__21_7;
            public static Action<MyButtonPanel> <>9__21_2;
            public static MyTerminalControlListbox<MyButtonPanel>.ListContentDelegate <>9__21_3;
            public static MyTerminalControlListbox<MyButtonPanel>.SelectItemDelegate <>9__21_4;
            public static MyTerminalControlTextbox<MyButtonPanel>.GetterDelegate <>9__21_5;
            public static MyTerminalControlTextbox<MyButtonPanel>.SetterDelegate <>9__21_6;
            public static Func<MyButtonPanel, Action<ToolbarItem, int>> <>9__28_0;
            public static Func<MyButtonPanel, Action<string, int>> <>9__28_2;
            public static Func<MyButtonPanel, Action<string, int>> <>9__28_1;
            public static Func<MyButtonPanel, Action> <>9__40_0;
            public static Func<MyButtonPanel, Action<string, int>> <>9__50_0;
            public static Func<MyButtonPanel, Action<string, int>> <>9__51_0;

            internal Action <ActivateButton>b__40_0(MyButtonPanel x) => 
                new Action(x.NotifyActivationFailed);

            internal bool <CreateTerminalControls>b__21_0(MyButtonPanel x) => 
                x.AnyoneCanUse;

            internal void <CreateTerminalControls>b__21_1(MyButtonPanel x, bool v)
            {
                x.AnyoneCanUse = v;
            }

            internal void <CreateTerminalControls>b__21_2(MyButtonPanel self)
            {
                MyButtonPanel.m_openedToolbars.Add(self.Toolbar);
                if (MyGuiScreenToolbarConfigBase.Static == null)
                {
                    MyButtonPanel.m_shouldSetOtherToolbars = true;
                    MyToolbarComponent.CurrentToolbar = self.Toolbar;
                    object[] args = new object[] { 0, self };
                    MyToolbarComponent.AutoUpdate = false;
                    MyGuiScreenBase base1 = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    screen.Closed += delegate (MyGuiScreenBase source) {
                        MyToolbarComponent.AutoUpdate = true;
                        MyButtonPanel.m_openedToolbars.Clear();
                    };
                    MyGuiSandbox.AddScreen(screen);
                }
            }

            internal void <CreateTerminalControls>b__21_3(MyButtonPanel x, ICollection<MyGuiControlListbox.Item> list1, ICollection<MyGuiControlListbox.Item> list2)
            {
                x.FillListContent(list1, list2);
            }

            internal void <CreateTerminalControls>b__21_4(MyButtonPanel x, List<MyGuiControlListbox.Item> y)
            {
                x.SelectButtonToName(y);
            }

            internal StringBuilder <CreateTerminalControls>b__21_5(MyButtonPanel x) => 
                x.GetButtonName();

            internal void <CreateTerminalControls>b__21_6(MyButtonPanel x, StringBuilder v)
            {
                x.SetCustomButtonName(v);
            }

            internal void <CreateTerminalControls>b__21_7(MyGuiScreenBase source)
            {
                MyToolbarComponent.AutoUpdate = true;
                MyButtonPanel.m_openedToolbars.Clear();
            }

            internal Action<string, int> <SetCustomButtonName>b__50_0(MyButtonPanel x) => 
                new Action<string, int>(x.SetButtonName);

            internal Action<string, int> <SetCustomButtonName>b__51_0(MyButtonPanel x) => 
                new Action<string, int>(x.SetButtonName);

            internal Action<ToolbarItem, int> <Toolbar_ItemChanged>b__28_0(MyButtonPanel x) => 
                new Action<ToolbarItem, int>(x.SendToolbarItemChanged);

            internal Action<string, int> <Toolbar_ItemChanged>b__28_1(MyButtonPanel x) => 
                new Action<string, int>(x.SetButtonName);

            internal Action<string, int> <Toolbar_ItemChanged>b__28_2(MyButtonPanel x) => 
                new Action<string, int>(x.SetButtonName);
        }
    }
}

