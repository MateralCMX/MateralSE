namespace Sandbox.Game.Entities.Blocks
{
    using Havok;
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_SensorBlock)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMySensorBlock), typeof(Sandbox.ModAPI.Ingame.IMySensorBlock) })]
    public class MySensorBlock : MyFunctionalBlock, Sandbox.ModAPI.IMySensorBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMySensorBlock, IMyGizmoDrawableObject
    {
        private Color m_gizmoColor;
        private const float m_maxGizmoDrawDistance = 400f;
        private BoundingBox m_gizmoBoundingBox;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_playProximitySound;
        private bool m_enablePlaySoundEvent;
        private readonly MyConcurrentHashSet<MyDetectedEntityInfo> m_detectedEntities = new MyConcurrentHashSet<MyDetectedEntityInfo>();
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_active;
        private List<ToolbarItem> m_items;
        private static readonly List<VRage.Game.Entity.MyEntity> m_potentialPenetrations = new List<VRage.Game.Entity.MyEntity>();
        private static readonly List<MyVoxelBase> m_potentialVoxelPenetrations = new List<MyVoxelBase>();
        protected HkShape m_fieldShape;
        private bool m_recreateField;
        private readonly VRage.Sync.Sync<Vector3, SyncDirection.BothWays> m_fieldMin;
        private readonly VRage.Sync.Sync<Vector3, SyncDirection.BothWays> m_fieldMax;
        private readonly VRage.Sync.Sync<MySensorFilterFlags, SyncDirection.BothWays> m_flags;
        private static List<MyToolbar> m_openedToolbars;
        private static bool m_shouldSetOtherToolbars;
        private bool m_syncing;
        [CompilerGenerated]
        private Action<bool> StateChanged;

        event Action<bool> Sandbox.ModAPI.IMySensorBlock.StateChanged
        {
            add
            {
                this.StateChanged += value;
            }
            remove
            {
                this.StateChanged -= value;
            }
        }

        private event Action<bool> StateChanged
        {
            [CompilerGenerated] add
            {
                Action<bool> stateChanged = this.StateChanged;
                while (true)
                {
                    Action<bool> a = stateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    stateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.StateChanged, action3, a);
                    if (ReferenceEquals(stateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> stateChanged = this.StateChanged;
                while (true)
                {
                    Action<bool> source = stateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    stateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.StateChanged, action3, source);
                    if (ReferenceEquals(stateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MySensorBlock()
        {
            this.CreateTerminalControls();
            this.m_active.ValueChanged += x => this.IsActiveChanged();
            this.m_fieldMax.ValueChanged += x => this.UpdateField();
            this.m_fieldMin.ValueChanged += x => this.UpdateField();
        }

        protected float CalculateRequiredPowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return (0.0003f * ((float) Math.Pow((double) (this.m_fieldMax.Value - this.m_fieldMin.Value).Volume, 0.3333333432674408)));
        }

        public bool CanBeDrawn() => 
            (MyCubeGrid.ShowSenzorGizmos && (base.ShowOnHUD && (base.IsWorking && (base.HasLocalPlayerAccess() && (base.GetDistanceBetweenPlayerPositionAndBoundingSphere() <= 400.0)))));

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
            this.UpdateEmissive();
        }

        protected void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySensorBlock>())
            {
                base.CreateTerminalControls();
                m_openedToolbars = new List<MyToolbar>();
                MyTerminalControlFactory.AddControl<MySensorBlock>(new MyTerminalControlButton<MySensorBlock>("Open Toolbar", MySpaceTexts.BlockPropertyTitle_SensorToolbarOpen, MySpaceTexts.BlockPropertyDescription_SensorToolbarOpen, delegate (MySensorBlock self) {
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
                MyTerminalControlSlider<MySensorBlock> slider11 = new MyTerminalControlSlider<MySensorBlock>("Left", MySpaceTexts.BlockPropertyTitle_SensorFieldWidthMin, MySpaceTexts.BlockPropertyDescription_SensorFieldLeft);
                MyTerminalControlSlider<MySensorBlock> slider12 = new MyTerminalControlSlider<MySensorBlock>("Left", MySpaceTexts.BlockPropertyTitle_SensorFieldWidthMin, MySpaceTexts.BlockPropertyDescription_SensorFieldLeft);
                slider12.SetLimits(block => 1f, block => block.MaxRange);
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local4 = (MyTerminalValueControl<MySensorBlock, float>.GetterDelegate) slider12;
                local4.DefaultValue = new float?((float) 5);
                local4.Getter = x => x.LeftExtend;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local125 = local4;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local126 = local4;
                local126.Setter = (x, v) => x.LeftExtend = v;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local123 = local126;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local124 = local126;
                local124.Writer = (x, result) => result.AppendInt32(((int) x.LeftExtend)).Append(" m");
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local8 = local124;
                ((MyTerminalControlSlider<MySensorBlock>) local8).EnableActions<MySensorBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySensorBlock>((MyTerminalControl<MySensorBlock>) local8);
                MyTerminalControlSlider<MySensorBlock> slider9 = new MyTerminalControlSlider<MySensorBlock>("Right", MySpaceTexts.BlockPropertyTitle_SensorFieldWidthMax, MySpaceTexts.BlockPropertyDescription_SensorFieldRight);
                MyTerminalControlSlider<MySensorBlock> slider10 = new MyTerminalControlSlider<MySensorBlock>("Right", MySpaceTexts.BlockPropertyTitle_SensorFieldWidthMax, MySpaceTexts.BlockPropertyDescription_SensorFieldRight);
                slider10.SetLimits(block => 1f, block => block.MaxRange);
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local11 = (MyTerminalValueControl<MySensorBlock, float>.GetterDelegate) slider10;
                local11.DefaultValue = new float?((float) 5);
                local11.Getter = x => x.RightExtend;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local121 = local11;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local122 = local11;
                local122.Setter = (x, v) => x.RightExtend = v;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local119 = local122;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local120 = local122;
                local120.Writer = (x, result) => result.AppendInt32(((int) x.RightExtend)).Append(" m");
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local15 = local120;
                ((MyTerminalControlSlider<MySensorBlock>) local15).EnableActions<MySensorBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySensorBlock>((MyTerminalControl<MySensorBlock>) local15);
                MyTerminalControlSlider<MySensorBlock> slider7 = new MyTerminalControlSlider<MySensorBlock>("Bottom", MySpaceTexts.BlockPropertyTitle_SensorFieldHeightMin, MySpaceTexts.BlockPropertyDescription_SensorFieldBottom);
                MyTerminalControlSlider<MySensorBlock> slider8 = new MyTerminalControlSlider<MySensorBlock>("Bottom", MySpaceTexts.BlockPropertyTitle_SensorFieldHeightMin, MySpaceTexts.BlockPropertyDescription_SensorFieldBottom);
                slider8.SetLimits(block => 1f, block => block.MaxRange);
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local18 = (MyTerminalValueControl<MySensorBlock, float>.GetterDelegate) slider8;
                local18.DefaultValue = new float?((float) 5);
                local18.Getter = x => x.BottomExtend;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local117 = local18;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local118 = local18;
                local118.Setter = (x, v) => x.BottomExtend = v;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local115 = local118;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local116 = local118;
                local116.Writer = (x, result) => result.AppendInt32(((int) x.BottomExtend)).Append(" m");
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local22 = local116;
                ((MyTerminalControlSlider<MySensorBlock>) local22).EnableActions<MySensorBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySensorBlock>((MyTerminalControl<MySensorBlock>) local22);
                MyTerminalControlSlider<MySensorBlock> slider5 = new MyTerminalControlSlider<MySensorBlock>("Top", MySpaceTexts.BlockPropertyTitle_SensorFieldHeightMax, MySpaceTexts.BlockPropertyDescription_SensorFieldTop);
                MyTerminalControlSlider<MySensorBlock> slider6 = new MyTerminalControlSlider<MySensorBlock>("Top", MySpaceTexts.BlockPropertyTitle_SensorFieldHeightMax, MySpaceTexts.BlockPropertyDescription_SensorFieldTop);
                slider6.SetLimits(block => 1f, block => block.MaxRange);
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local25 = (MyTerminalValueControl<MySensorBlock, float>.GetterDelegate) slider6;
                local25.DefaultValue = new float?((float) 5);
                local25.Getter = x => x.TopExtend;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local113 = local25;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local114 = local25;
                local114.Setter = (x, v) => x.TopExtend = v;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local111 = local114;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local112 = local114;
                local112.Writer = (x, result) => result.AppendInt32(((int) x.TopExtend)).Append(" m");
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local29 = local112;
                ((MyTerminalControlSlider<MySensorBlock>) local29).EnableActions<MySensorBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySensorBlock>((MyTerminalControl<MySensorBlock>) local29);
                MyTerminalControlSlider<MySensorBlock> slider3 = new MyTerminalControlSlider<MySensorBlock>("Back", MySpaceTexts.BlockPropertyTitle_SensorFieldDepthMax, MySpaceTexts.BlockPropertyDescription_SensorFieldBack);
                MyTerminalControlSlider<MySensorBlock> slider4 = new MyTerminalControlSlider<MySensorBlock>("Back", MySpaceTexts.BlockPropertyTitle_SensorFieldDepthMax, MySpaceTexts.BlockPropertyDescription_SensorFieldBack);
                slider4.SetLimits(block => 1f, block => block.MaxRange);
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local32 = (MyTerminalValueControl<MySensorBlock, float>.GetterDelegate) slider4;
                local32.DefaultValue = new float?((float) 5);
                local32.Getter = x => x.BackExtend;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local109 = local32;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local110 = local32;
                local110.Setter = (x, v) => x.BackExtend = v;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local107 = local110;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local108 = local110;
                local108.Writer = (x, result) => result.AppendInt32(((int) x.BackExtend)).Append(" m");
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local36 = local108;
                ((MyTerminalControlSlider<MySensorBlock>) local36).EnableActions<MySensorBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySensorBlock>((MyTerminalControl<MySensorBlock>) local36);
                MyTerminalControlSlider<MySensorBlock> slider1 = new MyTerminalControlSlider<MySensorBlock>("Front", MySpaceTexts.BlockPropertyTitle_SensorFieldDepthMin, MySpaceTexts.BlockPropertyDescription_SensorFieldFront);
                MyTerminalControlSlider<MySensorBlock> slider2 = new MyTerminalControlSlider<MySensorBlock>("Front", MySpaceTexts.BlockPropertyTitle_SensorFieldDepthMin, MySpaceTexts.BlockPropertyDescription_SensorFieldFront);
                slider2.SetLimits(block => 1f, block => block.MaxRange);
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local39 = (MyTerminalValueControl<MySensorBlock, float>.GetterDelegate) slider2;
                local39.DefaultValue = new float?((float) 5);
                local39.Getter = x => x.FrontExtend;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local105 = local39;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local106 = local39;
                local106.Setter = (x, v) => x.FrontExtend = v;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local103 = local106;
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local104 = local106;
                local104.Writer = (x, result) => result.AppendInt32(((int) x.FrontExtend)).Append(" m");
                MyTerminalValueControl<MySensorBlock, float>.GetterDelegate local43 = local104;
                ((MyTerminalControlSlider<MySensorBlock>) local43).EnableActions<MySensorBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySensorBlock>((MyTerminalControl<MySensorBlock>) local43);
                MyTerminalControlFactory.AddControl<MySensorBlock>(new MyTerminalControlSeparator<MySensorBlock>());
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch23 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Audible Proximity Alert", MySpaceTexts.BlockPropertyTitle_SensorPlaySound, MySpaceTexts.BlockPropertyTitle_SensorPlaySound, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch24 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Audible Proximity Alert", MySpaceTexts.BlockPropertyTitle_SensorPlaySound, MySpaceTexts.BlockPropertyTitle_SensorPlaySound, on, on);
                switch24.Getter = x => x.PlayProximitySound;
                MyTerminalControlOnOffSwitch<MySensorBlock> local101 = switch24;
                MyTerminalControlOnOffSwitch<MySensorBlock> control = switch24;
                control.Setter = (x, v) => x.PlayProximitySound = v;
                MyTerminalControlFactory.AddControl<MySensorBlock>(control);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch21 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Players", MySpaceTexts.BlockPropertyTitle_SensorDetectPlayers, MySpaceTexts.BlockPropertyTitle_SensorDetectPlayers, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch22 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Players", MySpaceTexts.BlockPropertyTitle_SensorDetectPlayers, MySpaceTexts.BlockPropertyTitle_SensorDetectPlayers, on, on);
                switch22.Getter = x => x.DetectPlayers;
                MyTerminalControlOnOffSwitch<MySensorBlock> local99 = switch22;
                MyTerminalControlOnOffSwitch<MySensorBlock> local100 = switch22;
                local100.Setter = (x, v) => x.DetectPlayers = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> onOff = local100;
                onOff.EnableToggleAction<MySensorBlock>(MyTerminalActionIcons.CHARACTER_TOGGLE);
                onOff.EnableOnOffActions<MySensorBlock>(MyTerminalActionIcons.CHARACTER_ON, MyTerminalActionIcons.CHARACTER_OFF);
                MyTerminalControlFactory.AddControl<MySensorBlock>(onOff);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch19 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Floating Objects", MySpaceTexts.BlockPropertyTitle_SensorDetectFloatingObjects, MySpaceTexts.BlockPropertyTitle_SensorDetectFloatingObjects, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch20 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Floating Objects", MySpaceTexts.BlockPropertyTitle_SensorDetectFloatingObjects, MySpaceTexts.BlockPropertyTitle_SensorDetectFloatingObjects, on, on);
                switch20.Getter = x => x.DetectFloatingObjects;
                MyTerminalControlOnOffSwitch<MySensorBlock> local97 = switch20;
                MyTerminalControlOnOffSwitch<MySensorBlock> local98 = switch20;
                local98.Setter = (x, v) => x.DetectFloatingObjects = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local51 = local98;
                local51.EnableToggleAction<MySensorBlock>(MyTerminalActionIcons.MOVING_OBJECT_TOGGLE);
                local51.EnableOnOffActions<MySensorBlock>(MyTerminalActionIcons.MOVING_OBJECT_ON, MyTerminalActionIcons.MOVING_OBJECT_OFF);
                MyTerminalControlFactory.AddControl<MySensorBlock>(local51);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch17 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Small Ships", MySpaceTexts.BlockPropertyTitle_SensorDetectSmallShips, MySpaceTexts.BlockPropertyTitle_SensorDetectSmallShips, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch18 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Small Ships", MySpaceTexts.BlockPropertyTitle_SensorDetectSmallShips, MySpaceTexts.BlockPropertyTitle_SensorDetectSmallShips, on, on);
                switch18.Getter = x => x.DetectSmallShips;
                MyTerminalControlOnOffSwitch<MySensorBlock> local95 = switch18;
                MyTerminalControlOnOffSwitch<MySensorBlock> local96 = switch18;
                local96.Setter = (x, v) => x.DetectSmallShips = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local54 = local96;
                local54.EnableToggleAction<MySensorBlock>(MyTerminalActionIcons.SMALLSHIP_TOGGLE);
                local54.EnableOnOffActions<MySensorBlock>(MyTerminalActionIcons.SMALLSHIP_ON, MyTerminalActionIcons.SMALLSHIP_OFF);
                MyTerminalControlFactory.AddControl<MySensorBlock>(local54);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch15 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Large Ships", MySpaceTexts.BlockPropertyTitle_SensorDetectLargeShips, MySpaceTexts.BlockPropertyTitle_SensorDetectLargeShips, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch16 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Large Ships", MySpaceTexts.BlockPropertyTitle_SensorDetectLargeShips, MySpaceTexts.BlockPropertyTitle_SensorDetectLargeShips, on, on);
                switch16.Getter = x => x.DetectLargeShips;
                MyTerminalControlOnOffSwitch<MySensorBlock> local93 = switch16;
                MyTerminalControlOnOffSwitch<MySensorBlock> local94 = switch16;
                local94.Setter = (x, v) => x.DetectLargeShips = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local57 = local94;
                local57.EnableToggleAction<MySensorBlock>(MyTerminalActionIcons.LARGESHIP_TOGGLE);
                local57.EnableOnOffActions<MySensorBlock>(MyTerminalActionIcons.LARGESHIP_ON, MyTerminalActionIcons.LARGESHIP_OFF);
                MyTerminalControlFactory.AddControl<MySensorBlock>(local57);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch13 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Stations", MySpaceTexts.BlockPropertyTitle_SensorDetectStations, MySpaceTexts.BlockPropertyTitle_SensorDetectStations, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch14 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Stations", MySpaceTexts.BlockPropertyTitle_SensorDetectStations, MySpaceTexts.BlockPropertyTitle_SensorDetectStations, on, on);
                switch14.Getter = x => x.DetectStations;
                MyTerminalControlOnOffSwitch<MySensorBlock> local91 = switch14;
                MyTerminalControlOnOffSwitch<MySensorBlock> local92 = switch14;
                local92.Setter = (x, v) => x.DetectStations = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local60 = local92;
                local60.EnableToggleAction<MySensorBlock>(MyTerminalActionIcons.STATION_TOGGLE);
                local60.EnableOnOffActions<MySensorBlock>(MyTerminalActionIcons.STATION_ON, MyTerminalActionIcons.STATION_OFF);
                MyTerminalControlFactory.AddControl<MySensorBlock>(local60);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch11 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Subgrids", MySpaceTexts.BlockPropertyTitle_SensorDetectSubgrids, MySpaceTexts.BlockPropertyTitle_SensorDetectSubgrids, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch12 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Subgrids", MySpaceTexts.BlockPropertyTitle_SensorDetectSubgrids, MySpaceTexts.BlockPropertyTitle_SensorDetectSubgrids, on, on);
                switch12.Getter = x => x.DetectSubgrids;
                MyTerminalControlOnOffSwitch<MySensorBlock> local89 = switch12;
                MyTerminalControlOnOffSwitch<MySensorBlock> local90 = switch12;
                local90.Setter = (x, v) => x.DetectSubgrids = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local63 = local90;
                local63.EnableToggleAction<MySensorBlock>(MyTerminalActionIcons.SUBGRID_TOGGLE);
                local63.EnableOnOffActions<MySensorBlock>(MyTerminalActionIcons.SUBGRID_ON, MyTerminalActionIcons.SUBGRID_OFF);
                MyTerminalControlFactory.AddControl<MySensorBlock>(local63);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch9 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Asteroids", MySpaceTexts.BlockPropertyTitle_SensorDetectAsteroids, MySpaceTexts.BlockPropertyTitle_SensorDetectAsteroids, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch10 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Asteroids", MySpaceTexts.BlockPropertyTitle_SensorDetectAsteroids, MySpaceTexts.BlockPropertyTitle_SensorDetectAsteroids, on, on);
                switch10.Getter = x => x.DetectAsteroids;
                MyTerminalControlOnOffSwitch<MySensorBlock> local87 = switch10;
                MyTerminalControlOnOffSwitch<MySensorBlock> local88 = switch10;
                local88.Setter = (x, v) => x.DetectAsteroids = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local66 = local88;
                local66.EnableToggleAction<MySensorBlock>();
                local66.EnableOnOffActions<MySensorBlock>();
                MyTerminalControlFactory.AddControl<MySensorBlock>(local66);
                MyTerminalControlFactory.AddControl<MySensorBlock>(new MyTerminalControlSeparator<MySensorBlock>());
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch7 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Owner", MySpaceTexts.BlockPropertyTitle_SensorDetectOwner, MySpaceTexts.BlockPropertyTitle_SensorDetectOwner, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch8 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Owner", MySpaceTexts.BlockPropertyTitle_SensorDetectOwner, MySpaceTexts.BlockPropertyTitle_SensorDetectOwner, on, on);
                switch8.Getter = x => x.DetectOwner;
                MyTerminalControlOnOffSwitch<MySensorBlock> local85 = switch8;
                MyTerminalControlOnOffSwitch<MySensorBlock> local86 = switch8;
                local86.Setter = (x, v) => x.DetectOwner = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local69 = local86;
                local69.EnableToggleAction<MySensorBlock>();
                local69.EnableOnOffActions<MySensorBlock>();
                MyTerminalControlFactory.AddControl<MySensorBlock>(local69);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch5 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Friendly", MySpaceTexts.BlockPropertyTitle_SensorDetectFriendly, MySpaceTexts.BlockPropertyTitle_SensorDetectFriendly, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch6 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Friendly", MySpaceTexts.BlockPropertyTitle_SensorDetectFriendly, MySpaceTexts.BlockPropertyTitle_SensorDetectFriendly, on, on);
                switch6.Getter = x => x.DetectFriendly;
                MyTerminalControlOnOffSwitch<MySensorBlock> local83 = switch6;
                MyTerminalControlOnOffSwitch<MySensorBlock> local84 = switch6;
                local84.Setter = (x, v) => x.DetectFriendly = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local72 = local84;
                local72.EnableToggleAction<MySensorBlock>();
                local72.EnableOnOffActions<MySensorBlock>();
                MyTerminalControlFactory.AddControl<MySensorBlock>(local72);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch3 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Neutral", MySpaceTexts.BlockPropertyTitle_SensorDetectNeutral, MySpaceTexts.BlockPropertyTitle_SensorDetectNeutral, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch4 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Neutral", MySpaceTexts.BlockPropertyTitle_SensorDetectNeutral, MySpaceTexts.BlockPropertyTitle_SensorDetectNeutral, on, on);
                switch4.Getter = x => x.DetectNeutral;
                MyTerminalControlOnOffSwitch<MySensorBlock> local81 = switch4;
                MyTerminalControlOnOffSwitch<MySensorBlock> local82 = switch4;
                local82.Setter = (x, v) => x.DetectNeutral = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local75 = local82;
                local75.EnableToggleAction<MySensorBlock>();
                local75.EnableOnOffActions<MySensorBlock>();
                MyTerminalControlFactory.AddControl<MySensorBlock>(local75);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySensorBlock> switch1 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Enemy", MySpaceTexts.BlockPropertyTitle_SensorDetectEnemy, MySpaceTexts.BlockPropertyTitle_SensorDetectEnemy, on, on);
                MyTerminalControlOnOffSwitch<MySensorBlock> switch2 = new MyTerminalControlOnOffSwitch<MySensorBlock>("Detect Enemy", MySpaceTexts.BlockPropertyTitle_SensorDetectEnemy, MySpaceTexts.BlockPropertyTitle_SensorDetectEnemy, on, on);
                switch2.Getter = x => x.DetectEnemy;
                MyTerminalControlOnOffSwitch<MySensorBlock> local79 = switch2;
                MyTerminalControlOnOffSwitch<MySensorBlock> local80 = switch2;
                local80.Setter = (x, v) => x.DetectEnemy = v;
                MyTerminalControlOnOffSwitch<MySensorBlock> local78 = local80;
                local78.EnableToggleAction<MySensorBlock>();
                local78.EnableOnOffActions<MySensorBlock>();
                MyTerminalControlFactory.AddControl<MySensorBlock>(local78);
            }
        }

        public bool EnableLongDrawDistance() => 
            false;

        public BoundingBox? GetBoundingBox()
        {
            this.m_gizmoBoundingBox.Min = base.PositionComp.LocalVolume.Center + this.FieldMin;
            this.m_gizmoBoundingBox.Max = base.PositionComp.LocalVolume.Center + this.FieldMax;
            return new BoundingBox?(this.m_gizmoBoundingBox);
        }

        public Color GetGizmoColor() => 
            this.m_gizmoColor;

        protected HkShape GetHkShape() => 
            ((HkShape) new HkBoxShape((this.m_fieldMax.Value - this.m_fieldMin.Value) * 0.5f));

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_SensorBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_SensorBlock;
            objectBuilderCubeBlock.FieldMin = this.FieldMin;
            objectBuilderCubeBlock.FieldMax = this.FieldMax;
            objectBuilderCubeBlock.PlaySound = this.PlayProximitySound;
            objectBuilderCubeBlock.DetectPlayers = this.DetectPlayers;
            objectBuilderCubeBlock.DetectFloatingObjects = this.DetectFloatingObjects;
            objectBuilderCubeBlock.DetectSmallShips = this.DetectSmallShips;
            objectBuilderCubeBlock.DetectLargeShips = this.DetectLargeShips;
            objectBuilderCubeBlock.DetectStations = this.DetectStations;
            objectBuilderCubeBlock.DetectSubgrids = this.DetectSubgrids;
            objectBuilderCubeBlock.DetectAsteroids = this.DetectAsteroids;
            objectBuilderCubeBlock.DetectOwner = this.DetectOwner;
            objectBuilderCubeBlock.DetectFriendly = this.DetectFriendly;
            objectBuilderCubeBlock.DetectNeutral = this.DetectNeutral;
            objectBuilderCubeBlock.DetectEnemy = this.DetectEnemy;
            objectBuilderCubeBlock.IsActive = this.IsActive;
            objectBuilderCubeBlock.Toolbar = this.Toolbar.GetObjectBuilder();
            return objectBuilderCubeBlock;
        }

        public Vector3 GetPositionInGrid() => 
            ((Vector3) base.Position);

        private bool GetPropertiesFromEntity(VRage.Game.Entity.MyEntity entity, ref Vector3D position1, out Quaternion rotation2, out Vector3 posDiff, out HkShape? shape2)
        {
            rotation2 = new Quaternion();
            posDiff = Vector3.Zero;
            shape2 = 0;
            if ((entity.Physics == null) || !entity.Physics.Enabled)
            {
                return false;
            }
            if (entity.Physics.RigidBody == null)
            {
                if (entity.GetPhysicsBody().CharacterProxy == null)
                {
                    return false;
                }
                shape2 = new HkShape?(entity.GetPhysicsBody().CharacterProxy.GetShape());
                MatrixD worldMatrix = entity.WorldMatrix;
                rotation2 = Quaternion.CreateFromForwardUp((Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
                posDiff = (Vector3) (entity.PositionComp.WorldAABB.Center - position1);
            }
            else
            {
                shape2 = new HkShape?(entity.Physics.RigidBody.GetShape());
                MatrixD worldMatrix = entity.WorldMatrix;
                rotation2 = Quaternion.CreateFromForwardUp((Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
                posDiff = (Vector3) (entity.PositionComp.GetPosition() - position1);
                if (entity is MyVoxelBase)
                {
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    posDiff -= base2.Size / 2;
                }
            }
            return true;
        }

        public float GetRadius() => 
            -1f;

        public MatrixD GetWorldMatrix() => 
            base.WorldMatrix;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, new Func<float>(this.CalculateRequiredPowerInput));
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            this.m_items = new List<ToolbarItem>(2);
            for (int i = 0; i < 2; i++)
            {
                ToolbarItem item = new ToolbarItem {
                    EntityID = 0L
                };
                this.m_items.Add(item);
            }
            this.Toolbar = new MyToolbar(MyToolbarType.ButtonPanel, 2, 1);
            this.Toolbar.DrawNumbers = false;
            MyObjectBuilder_SensorBlock block = (MyObjectBuilder_SensorBlock) objectBuilder;
            this.m_fieldMin.SetLocalValue(Vector3.Clamp((Vector3) block.FieldMin, new Vector3(-this.MaxRange), -Vector3.One));
            this.m_fieldMax.SetLocalValue(Vector3.Clamp((Vector3) block.FieldMax, Vector3.One, new Vector3(this.MaxRange)));
            this.m_playProximitySound.SetLocalValue(block.PlaySound);
            if (Sync.IsServer)
            {
                this.DetectPlayers = block.DetectPlayers;
                this.DetectFloatingObjects = block.DetectFloatingObjects;
                this.DetectSmallShips = block.DetectSmallShips;
                this.DetectLargeShips = block.DetectLargeShips;
                this.DetectStations = block.DetectStations;
                this.DetectSubgrids = block.DetectSubgrids;
                this.DetectAsteroids = block.DetectAsteroids;
                this.DetectOwner = block.DetectOwner;
                this.DetectFriendly = block.DetectFriendly;
                this.DetectNeutral = block.DetectNeutral;
                this.DetectEnemy = block.DetectEnemy;
            }
            this.m_active.SetLocalValue(block.IsActive);
            this.Toolbar.Init(block.Toolbar, this, false);
            for (int j = 0; j < 2; j++)
            {
                MyToolbarItem itemAtIndex = this.Toolbar.GetItemAtIndex(j);
                if (itemAtIndex != null)
                {
                    this.m_items.RemoveAt(j);
                    this.m_items.Insert(j, ToolbarItem.FromItem(itemAtIndex));
                }
            }
            this.Toolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink.RequiredInputChanged += new MyRequiredResourceChangeDelegate(this.Receiver_RequiredInputChanged);
            base.ResourceSink.Update();
            this.m_fieldShape = this.GetHkShape();
            base.OnClose += self => this.m_fieldShape.RemoveReference();
            this.m_gizmoColor = new VRageMath.Vector4(0.35f, 0f, 0f, 0.5f);
        }

        private void IsActiveChanged()
        {
            if (this.m_active != null)
            {
                this.OnFirstEnter();
            }
            else
            {
                this.OnLastLeave();
            }
            this.m_gizmoColor = (this.m_active == null) ? new VRageMath.Vector4(0.35f, 0f, 0f, 0.5f) : new VRageMath.Vector4(0f, 0.35f, 0f, 0.5f);
            Action<bool> stateChanged = this.StateChanged;
            if (stateChanged != null)
            {
                stateChanged((bool) this.m_active);
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.ResourceSink.Update();
            this.UpdateEmissive();
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.ResourceSink.Update();
            base.OnBuildSuccess(builtBy, instantBuild);
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            this.UpdateEmissive();
            base.OnEnabledChanged();
        }

        private void OnFirstEnter()
        {
            this.UpdateEmissive();
            this.Toolbar.UpdateItem(0);
            if (Sync.IsServer)
            {
                this.Toolbar.ActivateItemAtSlot(0, false, false, true);
                if (this.PlayProximitySound)
                {
                    this.PlayActionSound();
                    if (this.m_enablePlaySoundEvent)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySensorBlock>(this, x => new Action(x.PlayActionSound), targetEndpoint);
                    }
                }
            }
        }

        private void OnLastLeave()
        {
            this.UpdateEmissive();
            this.Toolbar.UpdateItem(1);
            if (Sync.IsServer)
            {
                this.Toolbar.ActivateItemAtSlot(1, false, false, true);
            }
        }

        [Event(null, 0x4a8), Reliable, Broadcast]
        private void PlayActionSound()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.PlaySound(base.m_actionSound, false, false, false, false, false, true);
            }
        }

        protected void Receiver_IsPoweredChanged()
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!base.Closed)
                {
                    base.UpdateIsWorking();
                    base.ResourceSink.Update();
                    this.UpdateEmissive();
                    this.UpdateText();
                }
            }, "MySensorBlock::Receiver_IsPoweredChanged");
        }

        protected void Receiver_RequiredInputChanged(MyDefinitionId resourceTypeId, MyResourceSinkComponent receiver, float oldRequirement, float newRequirement)
        {
            this.UpdateText();
        }

        void Sandbox.ModAPI.Ingame.IMySensorBlock.DetectedEntities(List<MyDetectedEntityInfo> result)
        {
            result.Clear();
            result.AddRange(this.m_detectedEntities);
        }

        [Event(null, 0x49b), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
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

        private bool ShouldDetect(VRage.Game.Entity.MyEntity entity)
        {
            if (entity == null)
            {
                return false;
            }
            if (ReferenceEquals(entity, base.CubeGrid))
            {
                return false;
            }
            if (this.DetectPlayers)
            {
                if (entity is MyCharacter)
                {
                    return this.ShouldDetectRelation((entity as MyCharacter).GetRelationTo(base.OwnerId));
                }
                if (entity is MyGhostCharacter)
                {
                    return this.ShouldDetectRelation((entity as IMyControllableEntity).ControllerInfo.Controller.Player.GetRelationTo(base.OwnerId));
                }
            }
            if (this.DetectFloatingObjects && (entity is MyFloatingObject))
            {
                return true;
            }
            MyCubeGrid nodeA = entity as MyCubeGrid;
            if ((!this.DetectSubgrids || (nodeA == null)) || !MyCubeGridGroups.Static.Logical.HasSameGroup(nodeA, base.CubeGrid))
            {
                if ((nodeA != null) && MyCubeGridGroups.Static.Logical.HasSameGroup(nodeA, base.CubeGrid))
                {
                    return false;
                }
                if ((this.DetectSmallShips && (nodeA != null)) && (nodeA.GridSizeEnum == MyCubeSize.Small))
                {
                    return this.ShouldDetectGrid(nodeA);
                }
                if ((this.DetectLargeShips && ((nodeA != null) && (nodeA.GridSizeEnum == MyCubeSize.Large))) && !nodeA.IsStatic)
                {
                    return this.ShouldDetectGrid(nodeA);
                }
                if ((!this.DetectStations || ((nodeA == null) || (nodeA.GridSizeEnum != MyCubeSize.Large))) || !nodeA.IsStatic)
                {
                    return (this.DetectAsteroids && (entity is MyVoxelBase));
                }
            }
            return this.ShouldDetectGrid(nodeA);
        }

        public bool ShouldDetectGrid(MyCubeGrid grid)
        {
            bool flag = true;
            using (List<long>.Enumerator enumerator = grid.BigOwners.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    long current = enumerator.Current;
                    MyRelationsBetweenPlayerAndBlock relationBetweenPlayers = MyPlayer.GetRelationBetweenPlayers(base.OwnerId, current);
                    if (!this.ShouldDetectRelation(relationBetweenPlayers))
                    {
                        flag = false;
                        continue;
                    }
                    return true;
                }
            }
            return (flag && this.ShouldDetectRelation(MyRelationsBetweenPlayerAndBlock.Enemies));
        }

        public bool ShouldDetectRelation(MyRelationsBetweenPlayerAndBlock relation)
        {
            switch (relation)
            {
                case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                case MyRelationsBetweenPlayerAndBlock.FactionShare:
                    return this.DetectFriendly;

                case MyRelationsBetweenPlayerAndBlock.Owner:
                    return this.DetectOwner;

                case MyRelationsBetweenPlayerAndBlock.Neutral:
                    return this.DetectNeutral;

                case MyRelationsBetweenPlayerAndBlock.Enemies:
                    return this.DetectEnemy;
            }
            throw new InvalidBranchException();
        }

        private void Toolbar_ItemChanged(MyToolbar self, MyToolbar.IndexArgs index)
        {
            if (!this.m_syncing)
            {
                ToolbarItem item = ToolbarItem.FromItem(self.GetItemAtIndex(index.ItemIndex));
                ToolbarItem item2 = this.m_items[index.ItemIndex];
                if (((item.EntityID != 0) || (item2.EntityID != 0)) && (((item.EntityID == 0) || (item2.EntityID == 0)) || !item.Equals(item2)))
                {
                    this.m_items.RemoveAt(index.ItemIndex);
                    this.m_items.Insert(index.ItemIndex, item);
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySensorBlock, ToolbarItem, int>(this, x => new Action<ToolbarItem, int>(x.SendToolbarItemChanged), item, index.ItemIndex, targetEndpoint);
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
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.m_enablePlaySoundEvent = true;
            if (Sync.IsServer && base.IsWorking)
            {
                if (!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
                {
                    if (!base.ResourceSink.IsPowerAvailable(MyResourceDistributorComponent.ElectricityId, this.BlockDefinition.RequiredPowerInput))
                    {
                        return;
                    }
                    float newRequiredInput = base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId);
                    base.ResourceSink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, 0f);
                    base.ResourceSink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, newRequiredInput);
                }
                Quaternion rotation1 = Quaternion.CreateFromForwardUp((Vector3) base.WorldMatrix.Forward, (Vector3) base.WorldMatrix.Up);
                Vector3D position1 = base.PositionComp.GetPosition() + Vector3D.Transform(base.PositionComp.LocalVolume.Center + ((this.m_fieldMax.Value + this.m_fieldMin.Value) * 0.5f), rotation1);
                if (this.m_recreateField)
                {
                    this.m_recreateField = false;
                    this.m_fieldShape.RemoveReference();
                    this.m_fieldShape = this.GetHkShape();
                    base.ResourceSink.Update();
                }
                BoundingBoxD box = new BoundingBoxD(this.m_fieldMin.Value, this.m_fieldMax.Value).Translate(base.PositionComp.LocalVolume.Center).TransformFast(base.WorldMatrix.GetOrientation()).Translate(base.PositionComp.GetPosition());
                m_potentialPenetrations.Clear();
                MyGamePruningStructure.GetTopMostEntitiesInBox(ref box, m_potentialPenetrations, MyEntityQueryType.Both);
                m_potentialVoxelPenetrations.Clear();
                MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, m_potentialVoxelPenetrations);
                this.LastDetectedEntity = null;
                this.m_detectedEntities.Clear();
                WorkOptions? options = null;
                Parallel.ForEach<VRage.Game.Entity.MyEntity>(m_potentialPenetrations, delegate (VRage.Game.Entity.MyEntity entity) {
                    Quaternion quaternion;
                    Vector3 vector;
                    HkShape? nullable;
                    if (!(entity is MyVoxelBase) && ((this.ShouldDetect(entity) && this.GetPropertiesFromEntity(entity, ref position1, out quaternion, out vector, out nullable)) && entity.GetPhysicsBody().HavokWorld.IsPenetratingShapeShape(this.m_fieldShape, ref Vector3.Zero, ref rotation1, nullable.Value, ref vector, ref quaternion)))
                    {
                        this.LastDetectedEntity = entity;
                        Vector3D? hitPosition = null;
                        this.m_detectedEntities.Add(MyDetectedEntityInfoHelper.Create(entity, this.OwnerId, hitPosition));
                    }
                }, WorkPriority.Normal, options, false);
                if (this.DetectAsteroids)
                {
                    foreach (MyVoxelBase base2 in m_potentialVoxelPenetrations)
                    {
                        Vector3 vector;
                        Vector3 vector2;
                        Vector3D? nullable2;
                        MyVoxelPhysics physics = base2 as MyVoxelPhysics;
                        if (physics == null)
                        {
                            Quaternion quaternion;
                            Vector3 vector3;
                            HkShape? nullable3;
                            if (!this.GetPropertiesFromEntity(base2, ref position1, out quaternion, out vector3, out nullable3))
                            {
                                continue;
                            }
                            if (!base2.GetPhysicsBody().HavokWorld.IsPenetratingShapeShape(this.m_fieldShape, ref Vector3.Zero, ref rotation1, nullable3.Value, ref vector3, ref quaternion))
                            {
                                continue;
                            }
                            this.LastDetectedEntity = base2;
                            nullable2 = null;
                            this.m_detectedEntities.Add(MyDetectedEntityInfoHelper.Create(base2, base.OwnerId, nullable2));
                            continue;
                        }
                        MyVoxelCoordSystems.WorldPositionToLocalPosition(box.Min, physics.PositionComp.WorldMatrix, physics.PositionComp.WorldMatrixInvScaled, physics.SizeInMetresHalf, out vector);
                        MyVoxelCoordSystems.WorldPositionToLocalPosition(box.Max, physics.PositionComp.WorldMatrix, physics.PositionComp.WorldMatrixInvScaled, physics.SizeInMetresHalf, out vector2);
                        BoundingBoxI xi = new BoundingBoxI(new Vector3I(vector), new Vector3I(vector2));
                        xi.Translate(physics.StorageMin);
                        if (physics.Storage.Intersect(ref xi, 1, false) != ContainmentType.Disjoint)
                        {
                            this.LastDetectedEntity = base2;
                            nullable2 = null;
                            this.m_detectedEntities.Add(MyDetectedEntityInfoHelper.Create(base2, base.OwnerId, nullable2));
                        }
                    }
                }
                this.IsActive = this.m_detectedEntities.Count > 0;
                m_potentialPenetrations.Clear();
                m_potentialVoxelPenetrations.Clear();
            }
        }

        public bool UpdateEmissive()
        {
            if (!base.IsWorking || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                return this.SetEmissiveState(base.IsFunctional ? MyCubeBlock.m_emissiveNames.Disabled : MyCubeBlock.m_emissiveNames.Damaged, base.Render.RenderObjectIDs[0], null);
            }
            return this.SetEmissiveState(this.IsActive ? MyCubeBlock.m_emissiveNames.Alternative : MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null);
        }

        protected void UpdateField()
        {
            this.m_recreateField = true;
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f, base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        private MySensorBlockDefinition BlockDefinition =>
            ((MySensorBlockDefinition) base.BlockDefinition);

        public bool IsActive
        {
            get => 
                ((bool) this.m_active);
            set => 
                (this.m_active.Value = value);
        }

        public VRage.Game.Entity.MyEntity LastDetectedEntity { get; private set; }

        public MyToolbar Toolbar { get; set; }

        public Vector3 FieldMin
        {
            get => 
                ((Vector3) this.m_fieldMin);
            set => 
                (this.m_fieldMin.Value = value);
        }

        public Vector3 FieldMax
        {
            get => 
                ((Vector3) this.m_fieldMax);
            set => 
                (this.m_fieldMax.Value = value);
        }

        public float MaxRange =>
            this.BlockDefinition.MaxRange;

        public MySensorFilterFlags Filters
        {
            get => 
                ((MySensorFilterFlags) this.m_flags);
            set => 
                (this.m_flags.Value = value);
        }

        public bool PlayProximitySound
        {
            get => 
                ((bool) this.m_playProximitySound);
            set => 
                (this.m_playProximitySound.Value = value);
        }

        public bool DetectPlayers
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Players)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Players;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfffe);
                }
            }
        }

        public bool DetectFloatingObjects
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.FloatingObjects)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.FloatingObjects;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfffd);
                }
            }
        }

        public bool DetectSmallShips
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.SmallShips)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.SmallShips;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfffb);
                }
            }
        }

        public bool DetectLargeShips
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.LargeShips)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.LargeShips;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfff7);
                }
            }
        }

        public bool DetectStations
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Stations)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Stations;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xffef);
                }
            }
        }

        public bool DetectSubgrids
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Subgrids)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Subgrids;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xffbf);
                }
            }
        }

        public bool DetectAsteroids
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Asteroids)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Asteroids;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xffdf);
                }
            }
        }

        public bool DetectOwner
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Owner)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Owner;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfeff);
                }
            }
        }

        public bool DetectFriendly
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Friendly)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Friendly;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfdff);
                }
            }
        }

        public bool DetectNeutral
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Neutral)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Neutral;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xfbff);
                }
            }
        }

        public bool DetectEnemy
        {
            get => 
                (((int) (this.Filters & MySensorFilterFlags.Enemy)) != 0);
            set
            {
                if (value)
                {
                    this.Filters |= MySensorFilterFlags.Enemy;
                }
                else
                {
                    this.Filters = ((MySensorFilterFlags) ((int) this.Filters)) & ((MySensorFilterFlags) 0xf7ff);
                }
            }
        }

        public float LeftExtend
        {
            get => 
                -this.m_fieldMin.Value.X;
            set
            {
                Vector3 fieldMin = this.FieldMin;
                if (fieldMin.X != -value)
                {
                    fieldMin.X = -value;
                    this.FieldMin = fieldMin;
                }
            }
        }

        public float RightExtend
        {
            get => 
                this.m_fieldMax.Value.X;
            set
            {
                Vector3 fieldMax = this.FieldMax;
                if (fieldMax.X != value)
                {
                    fieldMax.X = value;
                    this.FieldMax = fieldMax;
                }
            }
        }

        public float BottomExtend
        {
            get => 
                -this.m_fieldMin.Value.Y;
            set
            {
                Vector3 fieldMin = this.FieldMin;
                if (fieldMin.Y != -value)
                {
                    fieldMin.Y = -value;
                    this.FieldMin = fieldMin;
                }
            }
        }

        public float TopExtend
        {
            get => 
                this.m_fieldMax.Value.Y;
            set
            {
                Vector3 fieldMax = this.FieldMax;
                if (fieldMax.Y != value)
                {
                    fieldMax.Y = value;
                    this.FieldMax = fieldMax;
                }
            }
        }

        public float FrontExtend
        {
            get => 
                -this.m_fieldMin.Value.Z;
            set
            {
                Vector3 fieldMin = this.FieldMin;
                if (fieldMin.Z != -value)
                {
                    fieldMin.Z = -value;
                    this.FieldMin = fieldMin;
                }
            }
        }

        public float BackExtend
        {
            get => 
                this.m_fieldMax.Value.Z;
            set
            {
                Vector3 fieldMax = this.FieldMax;
                if (fieldMax.Z != value)
                {
                    fieldMax.Z = value;
                    this.FieldMax = fieldMax;
                }
            }
        }

        float Sandbox.ModAPI.Ingame.IMySensorBlock.LeftExtend
        {
            get => 
                this.LeftExtend;
            set => 
                (this.LeftExtend = MathHelper.Clamp(value, 1f, this.BlockDefinition.MaxRange));
        }

        float Sandbox.ModAPI.Ingame.IMySensorBlock.RightExtend
        {
            get => 
                this.RightExtend;
            set => 
                (this.RightExtend = MathHelper.Clamp(value, 1f, this.BlockDefinition.MaxRange));
        }

        float Sandbox.ModAPI.Ingame.IMySensorBlock.TopExtend
        {
            get => 
                this.TopExtend;
            set => 
                (this.TopExtend = MathHelper.Clamp(value, 1f, this.BlockDefinition.MaxRange));
        }

        float Sandbox.ModAPI.Ingame.IMySensorBlock.BottomExtend
        {
            get => 
                this.BottomExtend;
            set => 
                (this.BottomExtend = MathHelper.Clamp(value, 1f, this.BlockDefinition.MaxRange));
        }

        float Sandbox.ModAPI.Ingame.IMySensorBlock.FrontExtend
        {
            get => 
                this.FrontExtend;
            set => 
                (this.FrontExtend = MathHelper.Clamp(value, 1f, this.BlockDefinition.MaxRange));
        }

        float Sandbox.ModAPI.Ingame.IMySensorBlock.BackExtend
        {
            get => 
                this.BackExtend;
            set => 
                (this.BackExtend = MathHelper.Clamp(value, 1f, this.BlockDefinition.MaxRange));
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.PlayProximitySound
        {
            get => 
                this.PlayProximitySound;
            set => 
                (this.PlayProximitySound = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectPlayers
        {
            get => 
                this.DetectPlayers;
            set => 
                (this.DetectPlayers = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectFloatingObjects
        {
            get => 
                this.DetectFloatingObjects;
            set => 
                (this.DetectFloatingObjects = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectSmallShips
        {
            get => 
                this.DetectSmallShips;
            set => 
                (this.DetectSmallShips = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectLargeShips
        {
            get => 
                this.DetectLargeShips;
            set => 
                (this.DetectLargeShips = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectStations
        {
            get => 
                this.DetectStations;
            set => 
                (this.DetectStations = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectAsteroids
        {
            get => 
                this.DetectAsteroids;
            set => 
                (this.DetectAsteroids = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectOwner
        {
            get => 
                this.DetectOwner;
            set => 
                (this.DetectOwner = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectFriendly
        {
            get => 
                this.DetectFriendly;
            set => 
                (this.DetectFriendly = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectNeutral
        {
            get => 
                this.DetectNeutral;
            set => 
                (this.DetectNeutral = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.DetectEnemy
        {
            get => 
                this.DetectEnemy;
            set => 
                (this.DetectEnemy = value);
        }

        Vector3 Sandbox.ModAPI.IMySensorBlock.FieldMin
        {
            get => 
                this.FieldMin;
            set => 
                (this.FieldMin = value);
        }

        Vector3 Sandbox.ModAPI.IMySensorBlock.FieldMax
        {
            get => 
                this.FieldMax;
            set => 
                (this.FieldMax = value);
        }

        bool Sandbox.ModAPI.Ingame.IMySensorBlock.IsActive =>
            this.IsActive;

        MyDetectedEntityInfo Sandbox.ModAPI.Ingame.IMySensorBlock.LastDetectedEntity
        {
            get
            {
                Vector3D? hitPosition = null;
                return MyDetectedEntityInfoHelper.Create(this.LastDetectedEntity, base.OwnerId, hitPosition);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySensorBlock.<>c <>9 = new MySensorBlock.<>c();
            public static MyGuiScreenBase.ScreenHandler <>9__97_55;
            public static Action<MySensorBlock> <>9__97_0;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_1;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_2;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_3;
            public static MyTerminalValueControl<MySensorBlock, float>.SetterDelegate <>9__97_4;
            public static MyTerminalControl<MySensorBlock>.WriterDelegate <>9__97_5;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_6;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_7;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_8;
            public static MyTerminalValueControl<MySensorBlock, float>.SetterDelegate <>9__97_9;
            public static MyTerminalControl<MySensorBlock>.WriterDelegate <>9__97_10;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_11;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_12;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_13;
            public static MyTerminalValueControl<MySensorBlock, float>.SetterDelegate <>9__97_14;
            public static MyTerminalControl<MySensorBlock>.WriterDelegate <>9__97_15;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_16;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_17;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_18;
            public static MyTerminalValueControl<MySensorBlock, float>.SetterDelegate <>9__97_19;
            public static MyTerminalControl<MySensorBlock>.WriterDelegate <>9__97_20;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_21;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_22;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_23;
            public static MyTerminalValueControl<MySensorBlock, float>.SetterDelegate <>9__97_24;
            public static MyTerminalControl<MySensorBlock>.WriterDelegate <>9__97_25;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_26;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_27;
            public static MyTerminalValueControl<MySensorBlock, float>.GetterDelegate <>9__97_28;
            public static MyTerminalValueControl<MySensorBlock, float>.SetterDelegate <>9__97_29;
            public static MyTerminalControl<MySensorBlock>.WriterDelegate <>9__97_30;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_31;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_32;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_33;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_34;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_35;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_36;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_37;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_38;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_39;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_40;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_41;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_42;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_43;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_44;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_45;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_46;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_47;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_48;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_49;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_50;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_51;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_52;
            public static MyTerminalValueControl<MySensorBlock, bool>.GetterDelegate <>9__97_53;
            public static MyTerminalValueControl<MySensorBlock, bool>.SetterDelegate <>9__97_54;
            public static Func<MySensorBlock, Action<ToolbarItem, int>> <>9__111_0;
            public static Func<MySensorBlock, Action> <>9__112_0;

            internal void <CreateTerminalControls>b__97_0(MySensorBlock self)
            {
                MySensorBlock.m_openedToolbars.Add(self.Toolbar);
                if (MyGuiScreenToolbarConfigBase.Static == null)
                {
                    MySensorBlock.m_shouldSetOtherToolbars = true;
                    MyToolbarComponent.CurrentToolbar = self.Toolbar;
                    object[] args = new object[] { 0, self };
                    MyToolbarComponent.AutoUpdate = false;
                    MyGuiScreenBase base1 = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    screen.Closed += delegate (MyGuiScreenBase source) {
                        MyToolbarComponent.AutoUpdate = true;
                        MySensorBlock.m_openedToolbars.Clear();
                    };
                    MyGuiSandbox.AddScreen(screen);
                }
            }

            internal float <CreateTerminalControls>b__97_1(MySensorBlock block) => 
                1f;

            internal void <CreateTerminalControls>b__97_10(MySensorBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.RightExtend)).Append(" m");
            }

            internal float <CreateTerminalControls>b__97_11(MySensorBlock block) => 
                1f;

            internal float <CreateTerminalControls>b__97_12(MySensorBlock block) => 
                block.MaxRange;

            internal float <CreateTerminalControls>b__97_13(MySensorBlock x) => 
                x.BottomExtend;

            internal void <CreateTerminalControls>b__97_14(MySensorBlock x, float v)
            {
                x.BottomExtend = v;
            }

            internal void <CreateTerminalControls>b__97_15(MySensorBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.BottomExtend)).Append(" m");
            }

            internal float <CreateTerminalControls>b__97_16(MySensorBlock block) => 
                1f;

            internal float <CreateTerminalControls>b__97_17(MySensorBlock block) => 
                block.MaxRange;

            internal float <CreateTerminalControls>b__97_18(MySensorBlock x) => 
                x.TopExtend;

            internal void <CreateTerminalControls>b__97_19(MySensorBlock x, float v)
            {
                x.TopExtend = v;
            }

            internal float <CreateTerminalControls>b__97_2(MySensorBlock block) => 
                block.MaxRange;

            internal void <CreateTerminalControls>b__97_20(MySensorBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.TopExtend)).Append(" m");
            }

            internal float <CreateTerminalControls>b__97_21(MySensorBlock block) => 
                1f;

            internal float <CreateTerminalControls>b__97_22(MySensorBlock block) => 
                block.MaxRange;

            internal float <CreateTerminalControls>b__97_23(MySensorBlock x) => 
                x.BackExtend;

            internal void <CreateTerminalControls>b__97_24(MySensorBlock x, float v)
            {
                x.BackExtend = v;
            }

            internal void <CreateTerminalControls>b__97_25(MySensorBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.BackExtend)).Append(" m");
            }

            internal float <CreateTerminalControls>b__97_26(MySensorBlock block) => 
                1f;

            internal float <CreateTerminalControls>b__97_27(MySensorBlock block) => 
                block.MaxRange;

            internal float <CreateTerminalControls>b__97_28(MySensorBlock x) => 
                x.FrontExtend;

            internal void <CreateTerminalControls>b__97_29(MySensorBlock x, float v)
            {
                x.FrontExtend = v;
            }

            internal float <CreateTerminalControls>b__97_3(MySensorBlock x) => 
                x.LeftExtend;

            internal void <CreateTerminalControls>b__97_30(MySensorBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.FrontExtend)).Append(" m");
            }

            internal bool <CreateTerminalControls>b__97_31(MySensorBlock x) => 
                x.PlayProximitySound;

            internal void <CreateTerminalControls>b__97_32(MySensorBlock x, bool v)
            {
                x.PlayProximitySound = v;
            }

            internal bool <CreateTerminalControls>b__97_33(MySensorBlock x) => 
                x.DetectPlayers;

            internal void <CreateTerminalControls>b__97_34(MySensorBlock x, bool v)
            {
                x.DetectPlayers = v;
            }

            internal bool <CreateTerminalControls>b__97_35(MySensorBlock x) => 
                x.DetectFloatingObjects;

            internal void <CreateTerminalControls>b__97_36(MySensorBlock x, bool v)
            {
                x.DetectFloatingObjects = v;
            }

            internal bool <CreateTerminalControls>b__97_37(MySensorBlock x) => 
                x.DetectSmallShips;

            internal void <CreateTerminalControls>b__97_38(MySensorBlock x, bool v)
            {
                x.DetectSmallShips = v;
            }

            internal bool <CreateTerminalControls>b__97_39(MySensorBlock x) => 
                x.DetectLargeShips;

            internal void <CreateTerminalControls>b__97_4(MySensorBlock x, float v)
            {
                x.LeftExtend = v;
            }

            internal void <CreateTerminalControls>b__97_40(MySensorBlock x, bool v)
            {
                x.DetectLargeShips = v;
            }

            internal bool <CreateTerminalControls>b__97_41(MySensorBlock x) => 
                x.DetectStations;

            internal void <CreateTerminalControls>b__97_42(MySensorBlock x, bool v)
            {
                x.DetectStations = v;
            }

            internal bool <CreateTerminalControls>b__97_43(MySensorBlock x) => 
                x.DetectSubgrids;

            internal void <CreateTerminalControls>b__97_44(MySensorBlock x, bool v)
            {
                x.DetectSubgrids = v;
            }

            internal bool <CreateTerminalControls>b__97_45(MySensorBlock x) => 
                x.DetectAsteroids;

            internal void <CreateTerminalControls>b__97_46(MySensorBlock x, bool v)
            {
                x.DetectAsteroids = v;
            }

            internal bool <CreateTerminalControls>b__97_47(MySensorBlock x) => 
                x.DetectOwner;

            internal void <CreateTerminalControls>b__97_48(MySensorBlock x, bool v)
            {
                x.DetectOwner = v;
            }

            internal bool <CreateTerminalControls>b__97_49(MySensorBlock x) => 
                x.DetectFriendly;

            internal void <CreateTerminalControls>b__97_5(MySensorBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.LeftExtend)).Append(" m");
            }

            internal void <CreateTerminalControls>b__97_50(MySensorBlock x, bool v)
            {
                x.DetectFriendly = v;
            }

            internal bool <CreateTerminalControls>b__97_51(MySensorBlock x) => 
                x.DetectNeutral;

            internal void <CreateTerminalControls>b__97_52(MySensorBlock x, bool v)
            {
                x.DetectNeutral = v;
            }

            internal bool <CreateTerminalControls>b__97_53(MySensorBlock x) => 
                x.DetectEnemy;

            internal void <CreateTerminalControls>b__97_54(MySensorBlock x, bool v)
            {
                x.DetectEnemy = v;
            }

            internal void <CreateTerminalControls>b__97_55(MyGuiScreenBase source)
            {
                MyToolbarComponent.AutoUpdate = true;
                MySensorBlock.m_openedToolbars.Clear();
            }

            internal float <CreateTerminalControls>b__97_6(MySensorBlock block) => 
                1f;

            internal float <CreateTerminalControls>b__97_7(MySensorBlock block) => 
                block.MaxRange;

            internal float <CreateTerminalControls>b__97_8(MySensorBlock x) => 
                x.RightExtend;

            internal void <CreateTerminalControls>b__97_9(MySensorBlock x, float v)
            {
                x.RightExtend = v;
            }

            internal Action <OnFirstEnter>b__112_0(MySensorBlock x) => 
                new Action(x.PlayActionSound);

            internal Action<ToolbarItem, int> <Toolbar_ItemChanged>b__111_0(MySensorBlock x) => 
                new Action<ToolbarItem, int>(x.SendToolbarItemChanged);
        }
    }
}

