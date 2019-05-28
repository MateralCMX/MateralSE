namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
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
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_TimerBlock)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyTimerBlock), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock) })]
    public class MyTimerBlock : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyTimerBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock, IMyTriggerableBlock
    {
        private int m_countdownMsCurrent;
        private int m_countdownMsStart;
        private MySoundPair m_beepStart = MySoundPair.Empty;
        private MySoundPair m_beepMid = MySoundPair.Empty;
        private MySoundPair m_beepEnd = MySoundPair.Empty;
        private MyEntity3DSoundEmitter m_beepEmitter;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_silent;
        private static List<MyToolbar> m_openedToolbars;
        private static bool m_shouldSetOtherToolbars;
        private bool m_syncing;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_timerSync;

        public MyTimerBlock()
        {
            this.CreateTerminalControls();
            m_openedToolbars = new List<MyToolbar>();
            this.m_timerSync.ValueChanged += x => this.TimerChanged();
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private void ClearMemory()
        {
            this.m_countdownMsCurrent = 0;
            base.DetailedInfo.Clear();
            base.RaisePropertiesChanged();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyTimerBlock>())
            {
                base.CreateTerminalControls();
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MyTimerBlock> checkbox1 = new MyTerminalControlCheckbox<MyTimerBlock>("Silent", MySpaceTexts.BlockPropertyTitle_Silent, MySpaceTexts.ToolTipTimerBlock_Silent, on, on);
                MyTerminalControlCheckbox<MyTimerBlock> checkbox2 = new MyTerminalControlCheckbox<MyTimerBlock>("Silent", MySpaceTexts.BlockPropertyTitle_Silent, MySpaceTexts.ToolTipTimerBlock_Silent, on, on);
                checkbox2.Getter = x => x.Silent;
                MyTerminalControlCheckbox<MyTimerBlock> local19 = checkbox2;
                MyTerminalControlCheckbox<MyTimerBlock> local20 = checkbox2;
                local20.Setter = (x, v) => x.Silent = v;
                MyTerminalControlCheckbox<MyTimerBlock> checkbox = local20;
                checkbox.EnableAction<MyTimerBlock>(null);
                MyTerminalControlFactory.AddControl<MyTimerBlock>(checkbox);
                MyTerminalControlSlider<MyTimerBlock> slider1 = new MyTerminalControlSlider<MyTimerBlock>("TriggerDelay", MySpaceTexts.TerminalControlPanel_TimerDelay, MySpaceTexts.TerminalControlPanel_TimerDelay);
                slider1.SetLogLimits((float) 1f, (float) 3600f);
                slider1.DefaultValue = new float?((float) 10);
                slider1.Enabled = x => !x.IsCountingDown;
                MyTerminalControlSlider<MyTimerBlock> local17 = slider1;
                MyTerminalControlSlider<MyTimerBlock> local18 = slider1;
                local18.Getter = x => x.TriggerDelay;
                MyTerminalControlSlider<MyTimerBlock> local15 = local18;
                MyTerminalControlSlider<MyTimerBlock> local16 = local18;
                local16.Setter = (x, v) => x.TriggerDelay = v;
                MyTerminalControlSlider<MyTimerBlock> local13 = local16;
                MyTerminalControlSlider<MyTimerBlock> local14 = local16;
                local14.Writer = (x, sb) => MyValueFormatter.AppendTimeExact(Math.Max((int) x.TriggerDelay, 1), sb);
                MyTerminalControlSlider<MyTimerBlock> slider = local14;
                slider.EnableActions<MyTimerBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyTimerBlock>(slider);
                MyTerminalControlFactory.AddControl<MyTimerBlock>(new MyTerminalControlButton<MyTimerBlock>("OpenToolbar", MySpaceTexts.BlockPropertyTitle_TimerToolbarOpen, MySpaceTexts.BlockPropertyTitle_TimerToolbarOpen, delegate (MyTimerBlock self) {
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
                on = null;
                MyTerminalControlButton<MyTimerBlock> button = new MyTerminalControlButton<MyTimerBlock>("TriggerNow", MySpaceTexts.BlockPropertyTitle_TimerTrigger, MySpaceTexts.BlockPropertyTitle_TimerTrigger, x => x.OnTrigger());
                button.EnableAction<MyTimerBlock>(null, on, null);
                MyTerminalControlFactory.AddControl<MyTimerBlock>(button);
                on = null;
                MyTerminalControlButton<MyTimerBlock> button2 = new MyTerminalControlButton<MyTimerBlock>("Start", MySpaceTexts.BlockPropertyTitle_TimerStart, MySpaceTexts.BlockPropertyTitle_TimerStart, x => x.StartBtn());
                button2.EnableAction<MyTimerBlock>(null, on, null);
                MyTerminalControlFactory.AddControl<MyTimerBlock>(button2);
                on = null;
                MyTerminalControlButton<MyTimerBlock> button3 = new MyTerminalControlButton<MyTimerBlock>("Stop", MySpaceTexts.BlockPropertyTitle_TimerStop, MySpaceTexts.BlockPropertyTitle_TimerStop, x => x.StopBtn());
                button3.EnableAction<MyTimerBlock>(null, on, null);
                MyTerminalControlFactory.AddControl<MyTimerBlock>(button3);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_TimerBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_TimerBlock;
            objectBuilderCubeBlock.Toolbar = this.Toolbar.GetObjectBuilder();
            objectBuilderCubeBlock.JustTriggered = base.NeedsUpdate.HasFlag(MyEntityUpdateEnum.BEFORE_NEXT_FRAME);
            objectBuilderCubeBlock.Delay = this.m_countdownMsStart;
            objectBuilderCubeBlock.CurrentTime = this.m_countdownMsCurrent;
            objectBuilderCubeBlock.IsCountingDown = this.IsCountingDown;
            objectBuilderCubeBlock.Silent = this.Silent;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MyTimerBlockDefinition blockDefinition = base.BlockDefinition as MyTimerBlockDefinition;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(blockDefinition.ResourceSinkGroup, 1E-07f, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            base.ResourceSink = component;
            if (blockDefinition.EmissiveColorPreset == MyStringHash.NullOrEmpty)
            {
                blockDefinition.EmissiveColorPreset = MyStringHash.GetOrCompute("Timer");
            }
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_TimerBlock block = objectBuilder as MyObjectBuilder_TimerBlock;
            this.Toolbar = new MyToolbar(MyToolbarType.ButtonPanel, 9, 10);
            this.Toolbar.Init(block.Toolbar, this, false);
            this.Toolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
            if (block.JustTriggered)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            this.IsCountingDown = block.IsCountingDown;
            if (Sync.IsServer)
            {
                this.Silent = block.Silent;
                this.TriggerDelay = MathHelper.Clamp(block.Delay, blockDefinition.MinDelay, blockDefinition.MaxDelay) / 0x3e8;
            }
            this.m_countdownMsStart = MathHelper.Clamp(block.Delay, blockDefinition.MinDelay, blockDefinition.MaxDelay);
            this.m_countdownMsCurrent = MathHelper.Clamp(block.CurrentTime, 0, blockDefinition.MaxDelay);
            if (this.IsCountingDown)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            this.m_beepStart = new MySoundPair(blockDefinition.TimerSoundStart, true);
            this.m_beepMid = new MySoundPair(blockDefinition.TimerSoundMid, true);
            this.m_beepEnd = new MySoundPair(blockDefinition.TimerSoundEnd, true);
            this.m_beepEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            if (this.m_countdownMsCurrent != 0)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        protected void OnTrigger()
        {
            if (base.IsWorking)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTimerBlock>(this, x => new Action(x.Trigger), targetEndpoint);
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        void IMyTriggerableBlock.Trigger()
        {
            this.OnTrigger();
        }

        [Event(null, 0x1da), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void SendToolbarItemChanged(ToolbarItem sentItem, int index)
        {
            this.m_syncing = true;
            MyToolbarItem item = null;
            if (sentItem.EntityID != 0)
            {
                item = ToolbarItem.ToItem(sentItem);
            }
            this.Toolbar.SetItemAtIndex(index, item);
            this.m_syncing = false;
        }

        public override bool SetEmissiveStateWorking() => 
            this.UpdateEmissivity();

        public void SetTimer(int p)
        {
            this.m_countdownMsStart = p;
            base.RaisePropertiesChanged();
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock.StartCountdown()
        {
            this.StartBtn();
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock.StopCountdown()
        {
            this.StopBtn();
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock.Trigger()
        {
            this.OnTrigger();
        }

        [Event(null, 0xab), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void Start()
        {
            this.IsCountingDown = true;
            this.m_countdownMsCurrent = this.m_countdownMsStart;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            if ((this.m_beepEmitter != null) && !this.Silent)
            {
                bool? nullable = null;
                this.m_beepEmitter.PlaySound(this.m_beepStart, false, false, false, false, false, nullable);
            }
            this.UpdateEmissivity();
        }

        private void StartBtn()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTimerBlock>(this, x => new Action(x.Start), targetEndpoint);
        }

        [Event(null, 0x9b), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void Stop()
        {
            this.IsCountingDown = false;
            base.NeedsUpdate &= ~(MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME);
            this.ClearMemory();
            this.UpdateEmissivity();
        }

        private void StopBtn()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTimerBlock>(this, x => new Action(x.Stop), targetEndpoint);
        }

        public void StopCountdown()
        {
            base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.IsCountingDown = false;
            this.ClearMemory();
        }

        private void TimerChanged()
        {
            this.SetTimer(((int) this.TriggerDelay) * 0x3e8);
        }

        private void Toolbar_ItemChanged(MyToolbar self, MyToolbar.IndexArgs index)
        {
            if (!this.m_syncing)
            {
                ToolbarItem item = ToolbarItem.FromItem(self.GetItemAtIndex(index.ItemIndex));
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTimerBlock, ToolbarItem, int>(this, x => new Action<ToolbarItem, int>(x.SendToolbarItemChanged), item, index.ItemIndex, targetEndpoint);
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
            }
        }

        [Event(null, 0x179), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        protected void Trigger()
        {
            if (base.IsWorking)
            {
                this.StopCountdown();
                this.UpdateEmissivity();
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (base.IsWorking)
            {
                int num = this.m_countdownMsCurrent % 0x3e8;
                if (this.m_countdownMsCurrent > 0)
                {
                    this.m_countdownMsCurrent -= 0xa6;
                }
                int num2 = this.m_countdownMsCurrent % 0x3e8;
                if (((num > 800) && (num2 <= 800)) || ((num <= 800) && (num2 > 800)))
                {
                    this.UpdateEmissivity();
                }
                if (this.m_countdownMsCurrent <= 0)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
                    this.m_countdownMsCurrent = 0;
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                    if ((this.m_beepEmitter != null) && !this.Silent)
                    {
                        bool? nullable = null;
                        this.m_beepEmitter.PlaySound(this.m_beepEnd, true, false, false, false, false, nullable);
                    }
                }
                base.DetailedInfo.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyTitle_TimerToTrigger));
                MyValueFormatter.AppendTimeExact(this.m_countdownMsCurrent / 0x3e8, base.DetailedInfo);
                base.RaisePropertiesChanged();
            }
        }

        private bool UpdateEmissivity()
        {
            if (!base.InScene)
            {
                return false;
            }
            if (!base.IsWorking)
            {
                return false;
            }
            if (!this.IsCountingDown)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null);
            }
            if ((this.m_countdownMsCurrent % 0x3e8) <= 800)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Warning, base.Render.RenderObjectIDs[0], null);
            }
            if ((this.m_beepEmitter != null) && !this.Silent)
            {
                bool? nullable = null;
                this.m_beepEmitter.PlaySound(this.m_beepMid, false, false, false, false, false, nullable);
            }
            return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Alternative, base.Render.RenderObjectIDs[0], null);
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.IsCountingDown = false;
            if (Sync.IsServer)
            {
                int index = 0;
                while (true)
                {
                    if (index >= this.Toolbar.ItemCount)
                    {
                        if ((base.CubeGrid.Physics != null) && (MyVisualScriptLogicProvider.TimerBlockTriggered != null))
                        {
                            MyVisualScriptLogicProvider.TimerBlockTriggered(base.CustomName.ToString());
                        }
                        if (((base.CubeGrid.Physics != null) && !string.IsNullOrEmpty(base.Name)) && (MyVisualScriptLogicProvider.TimerBlockTriggeredEntityName != null))
                        {
                            MyVisualScriptLogicProvider.TimerBlockTriggeredEntityName(base.Name);
                        }
                        break;
                    }
                    this.Toolbar.UpdateItem(index);
                    this.Toolbar.ActivateItemAtIndex(index, false);
                    index++;
                }
            }
            this.UpdateEmissivity();
            base.DetailedInfo.Clear();
            base.RaisePropertiesChanged();
        }

        public override void UpdateSoundEmitters()
        {
            base.UpdateSoundEmitters();
            if (this.m_beepEmitter != null)
            {
                this.m_beepEmitter.Update();
            }
        }

        public MyToolbar Toolbar { get; set; }

        public bool IsCountingDown { get; set; }

        public bool Silent
        {
            get => 
                ((bool) this.m_silent);
            private set => 
                (this.m_silent.Value = value);
        }

        public float TriggerDelay
        {
            get => 
                (this.m_timerSync.Value / 1000f);
            set => 
                (this.m_timerSync.Value = value * 1000f);
        }

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock.IsCountingDown =>
            this.IsCountingDown;

        float SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock.TriggerDelay
        {
            get => 
                this.TriggerDelay;
            set => 
                (this.TriggerDelay = value);
        }

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyTimerBlock.Silent
        {
            get => 
                this.Silent;
            set => 
                (this.Silent = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTimerBlock.<>c <>9 = new MyTimerBlock.<>c();
            public static MyTerminalValueControl<MyTimerBlock, bool>.GetterDelegate <>9__26_0;
            public static MyTerminalValueControl<MyTimerBlock, bool>.SetterDelegate <>9__26_1;
            public static Func<MyTimerBlock, bool> <>9__26_2;
            public static MyTerminalValueControl<MyTimerBlock, float>.GetterDelegate <>9__26_3;
            public static MyTerminalValueControl<MyTimerBlock, float>.SetterDelegate <>9__26_4;
            public static MyTerminalControl<MyTimerBlock>.WriterDelegate <>9__26_5;
            public static MyGuiScreenBase.ScreenHandler <>9__26_10;
            public static Action<MyTimerBlock> <>9__26_6;
            public static Action<MyTimerBlock> <>9__26_7;
            public static Action<MyTimerBlock> <>9__26_8;
            public static Action<MyTimerBlock> <>9__26_9;
            public static Func<MyTimerBlock, Action> <>9__28_0;
            public static Func<MyTimerBlock, Action> <>9__29_0;
            public static Func<MyTimerBlock, Action<ToolbarItem, int>> <>9__33_0;
            public static Func<MyTimerBlock, Action> <>9__43_0;

            internal bool <CreateTerminalControls>b__26_0(MyTimerBlock x) => 
                x.Silent;

            internal void <CreateTerminalControls>b__26_1(MyTimerBlock x, bool v)
            {
                x.Silent = v;
            }

            internal void <CreateTerminalControls>b__26_10(MyGuiScreenBase source)
            {
                MyToolbarComponent.AutoUpdate = true;
                MyTimerBlock.m_openedToolbars.Clear();
            }

            internal bool <CreateTerminalControls>b__26_2(MyTimerBlock x) => 
                !x.IsCountingDown;

            internal float <CreateTerminalControls>b__26_3(MyTimerBlock x) => 
                x.TriggerDelay;

            internal void <CreateTerminalControls>b__26_4(MyTimerBlock x, float v)
            {
                x.TriggerDelay = v;
            }

            internal void <CreateTerminalControls>b__26_5(MyTimerBlock x, StringBuilder sb)
            {
                MyValueFormatter.AppendTimeExact(Math.Max((int) x.TriggerDelay, 1), sb);
            }

            internal void <CreateTerminalControls>b__26_6(MyTimerBlock self)
            {
                MyTimerBlock.m_openedToolbars.Add(self.Toolbar);
                if (MyGuiScreenToolbarConfigBase.Static == null)
                {
                    MyTimerBlock.m_shouldSetOtherToolbars = true;
                    MyToolbarComponent.CurrentToolbar = self.Toolbar;
                    object[] args = new object[] { 0, self };
                    MyToolbarComponent.AutoUpdate = false;
                    MyGuiScreenBase base1 = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    screen.Closed += delegate (MyGuiScreenBase source) {
                        MyToolbarComponent.AutoUpdate = true;
                        MyTimerBlock.m_openedToolbars.Clear();
                    };
                    MyGuiSandbox.AddScreen(screen);
                }
            }

            internal void <CreateTerminalControls>b__26_7(MyTimerBlock x)
            {
                x.OnTrigger();
            }

            internal void <CreateTerminalControls>b__26_8(MyTimerBlock x)
            {
                x.StartBtn();
            }

            internal void <CreateTerminalControls>b__26_9(MyTimerBlock x)
            {
                x.StopBtn();
            }

            internal Action <OnTrigger>b__43_0(MyTimerBlock x) => 
                new Action(x.Trigger);

            internal Action <StartBtn>b__29_0(MyTimerBlock x) => 
                new Action(x.Start);

            internal Action <StopBtn>b__28_0(MyTimerBlock x) => 
                new Action(x.Stop);

            internal Action<ToolbarItem, int> <Toolbar_ItemChanged>b__33_0(MyTimerBlock x) => 
                new Action<ToolbarItem, int>(x.SendToolbarItemChanged);
        }
    }
}

