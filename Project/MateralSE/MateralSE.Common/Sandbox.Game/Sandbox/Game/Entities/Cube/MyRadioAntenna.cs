namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Groups;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_RadioAntenna)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyRadioAntenna), typeof(Sandbox.ModAPI.Ingame.IMyRadioAntenna) })]
    public class MyRadioAntenna : MyFunctionalBlock, IMyGizmoDrawableObject, Sandbox.ModAPI.IMyRadioAntenna, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyRadioAntenna
    {
        protected Color m_gizmoColor;
        protected const float m_maxGizmoDrawDistance = 10000f;
        private VRage.Sync.Sync<long, SyncDirection.BothWays> m_attachedPB;
        [CompilerGenerated]
        private static Action<MyRadioAntenna, string, MyTransmitTarget> m_messageRequest;
        private MyTuple<string, MyTransmitTarget>? m_nextBroadcast;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_ignoreOtherBroadcast;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_ignoreAlliedBroadcast;
        private const int TRANSMISSION_LIMIT = 0x186a0;
        private static MyTerminalControlCheckbox<MyRadioAntenna> m_ignoreOtherCheckbox;
        private static MyTerminalControlCheckbox<MyRadioAntenna> m_ignoreAllyCheckbox;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_radius;
        private bool onceUpdated;
        public readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> EnableBroadcasting;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_showShipName;

        private static  event Action<MyRadioAntenna, string, MyTransmitTarget> m_messageRequest
        {
            [CompilerGenerated] add
            {
                Action<MyRadioAntenna, string, MyTransmitTarget> messageRequest = m_messageRequest;
                while (true)
                {
                    Action<MyRadioAntenna, string, MyTransmitTarget> a = messageRequest;
                    Action<MyRadioAntenna, string, MyTransmitTarget> action3 = (Action<MyRadioAntenna, string, MyTransmitTarget>) Delegate.Combine(a, value);
                    messageRequest = Interlocked.CompareExchange<Action<MyRadioAntenna, string, MyTransmitTarget>>(ref m_messageRequest, action3, a);
                    if (ReferenceEquals(messageRequest, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyRadioAntenna, string, MyTransmitTarget> messageRequest = m_messageRequest;
                while (true)
                {
                    Action<MyRadioAntenna, string, MyTransmitTarget> source = messageRequest;
                    Action<MyRadioAntenna, string, MyTransmitTarget> action3 = (Action<MyRadioAntenna, string, MyTransmitTarget>) Delegate.Remove(source, value);
                    messageRequest = Interlocked.CompareExchange<Action<MyRadioAntenna, string, MyTransmitTarget>>(ref m_messageRequest, action3, source);
                    if (ReferenceEquals(messageRequest, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyRadioAntenna()
        {
            this.CreateTerminalControls();
            this.HudText = new StringBuilder();
            base.NeedsWorldMatrix = true;
        }

        public bool CanBeDrawn() => 
            (MyCubeGrid.ShowAntennaGizmos && (base.IsWorking && (base.HasLocalPlayerAccess() && ((base.GetDistanceBetweenCameraAndBoundingSphere() <= 10000.0) && IsRecievedByPlayer(this)))));

        private void ChangeEnableBroadcast()
        {
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.Enabled = (this.EnableBroadcasting != null) && base.IsWorking;
                this.RadioBroadcaster.WantsToBeEnabled = (bool) this.EnableBroadcasting;
                base.ResourceSink.Update();
                base.RaisePropertiesChanged();
                this.UpdateText();
            }
        }

        private void ChangeRadius()
        {
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.BroadcastRadius = (float) this.m_radius;
                this.RadioBroadcaster.RaiseBroadcastRadiusChanged();
            }
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            if (Sync.IsServer)
            {
                this.UpdatePirateAntenna(true);
            }
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
            this.UpdateText();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyRadioAntenna>())
            {
                base.CreateTerminalControls();
                MyTerminalControlFactory.RemoveBaseClass<MyRadioAntenna, MyTerminalBlock>();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyRadioAntenna> switch3 = new MyTerminalControlOnOffSwitch<MyRadioAntenna>("ShowInTerminal", MySpaceTexts.Terminal_ShowInTerminal, MySpaceTexts.Terminal_ShowInTerminalToolTip, on, on);
                MyTerminalControlOnOffSwitch<MyRadioAntenna> switch4 = new MyTerminalControlOnOffSwitch<MyRadioAntenna>("ShowInTerminal", MySpaceTexts.Terminal_ShowInTerminal, MySpaceTexts.Terminal_ShowInTerminalToolTip, on, on);
                switch4.Getter = x => x.ShowInTerminal;
                MyTerminalControlOnOffSwitch<MyRadioAntenna> local57 = switch4;
                MyTerminalControlOnOffSwitch<MyRadioAntenna> control = switch4;
                control.Setter = (x, v) => x.ShowInTerminal = v;
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(control);
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyRadioAntenna> switch1 = new MyTerminalControlOnOffSwitch<MyRadioAntenna>("ShowInToolbarConfig", MySpaceTexts.Terminal_ShowInToolbarConfig, MySpaceTexts.Terminal_ShowInToolbarConfigToolTip, on, on);
                MyTerminalControlOnOffSwitch<MyRadioAntenna> switch2 = new MyTerminalControlOnOffSwitch<MyRadioAntenna>("ShowInToolbarConfig", MySpaceTexts.Terminal_ShowInToolbarConfig, MySpaceTexts.Terminal_ShowInToolbarConfigToolTip, on, on);
                switch2.Getter = x => x.ShowInToolbarConfig;
                MyTerminalControlOnOffSwitch<MyRadioAntenna> local55 = switch2;
                MyTerminalControlOnOffSwitch<MyRadioAntenna> local56 = switch2;
                local56.Setter = (x, v) => x.ShowInToolbarConfig = v;
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(local56);
                MyTerminalControlButton<MyRadioAntenna> button1 = new MyTerminalControlButton<MyRadioAntenna>("CustomData", MySpaceTexts.Terminal_CustomData, MySpaceTexts.Terminal_CustomDataTooltip, new Action<MyRadioAntenna>(this.CustomDataClicked));
                MyTerminalControlButton<MyRadioAntenna> button2 = new MyTerminalControlButton<MyRadioAntenna>("CustomData", MySpaceTexts.Terminal_CustomData, MySpaceTexts.Terminal_CustomDataTooltip, new Action<MyRadioAntenna>(this.CustomDataClicked));
                button2.Enabled = x => !x.m_textboxOpen;
                MyTerminalControlButton<MyRadioAntenna> local6 = button2;
                local6.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(local6);
                MyTerminalControlTextbox<MyRadioAntenna> textbox3 = new MyTerminalControlTextbox<MyRadioAntenna>("CustomName", MyCommonTexts.Name, MySpaceTexts.Blank);
                MyTerminalControlTextbox<MyRadioAntenna> textbox4 = new MyTerminalControlTextbox<MyRadioAntenna>("CustomName", MyCommonTexts.Name, MySpaceTexts.Blank);
                textbox4.Getter = x => x.CustomName;
                MyTerminalControlTextbox<MyRadioAntenna> local53 = textbox4;
                MyTerminalControlTextbox<MyRadioAntenna> local54 = textbox4;
                local54.Setter = (x, v) => x.SetCustomName(v);
                MyTerminalControlTextbox<MyRadioAntenna> local9 = local54;
                local9.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(local9);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(new MyTerminalControlSeparator<MyRadioAntenna>());
                MyTerminalControlTextbox<MyRadioAntenna> textbox1 = new MyTerminalControlTextbox<MyRadioAntenna>("HudText", MySpaceTexts.BlockPropertiesTitle_HudText, MySpaceTexts.BlockPropertiesTitle_HudText_Tooltip);
                MyTerminalControlTextbox<MyRadioAntenna> textbox2 = new MyTerminalControlTextbox<MyRadioAntenna>("HudText", MySpaceTexts.BlockPropertiesTitle_HudText, MySpaceTexts.BlockPropertiesTitle_HudText_Tooltip);
                textbox2.Getter = x => x.HudText;
                MyTerminalControlTextbox<MyRadioAntenna> local51 = textbox2;
                MyTerminalControlTextbox<MyRadioAntenna> local52 = textbox2;
                local52.Setter = (x, v) => x.SetHudText(v);
                MyTerminalControlTextbox<MyRadioAntenna> local12 = local52;
                local12.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(local12);
                MyTerminalControlCombobox<MyRadioAntenna> combobox1 = new MyTerminalControlCombobox<MyRadioAntenna>("PBList", MySpaceTexts.BlockPropertyTitle_AssignedPB, MySpaceTexts.Blank);
                MyTerminalControlCombobox<MyRadioAntenna> combobox2 = new MyTerminalControlCombobox<MyRadioAntenna>("PBList", MySpaceTexts.BlockPropertyTitle_AssignedPB, MySpaceTexts.Blank);
                combobox2.ComboBoxContentWithBlock = (x, list) => x.FillPBComboBoxContent(list);
                MyTerminalControlCombobox<MyRadioAntenna> local49 = combobox2;
                MyTerminalControlCombobox<MyRadioAntenna> local50 = combobox2;
                local50.Getter = x => (long) x.m_attachedPB;
                MyTerminalControlCombobox<MyRadioAntenna> local47 = local50;
                MyTerminalControlCombobox<MyRadioAntenna> local48 = local50;
                local48.Setter = delegate (MyRadioAntenna x, long y) {
                    x.m_attachedPB.Value = y;
                    m_ignoreAllyCheckbox.UpdateVisual();
                    m_ignoreOtherCheckbox.UpdateVisual();
                };
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(local48);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(new MyTerminalControlSeparator<MyRadioAntenna>());
                MyTerminalControlSlider<MyRadioAntenna> slider1 = new MyTerminalControlSlider<MyRadioAntenna>("Radius", MySpaceTexts.BlockPropertyTitle_BroadcastRadius, MySpaceTexts.BlockPropertyDescription_BroadcastRadius);
                MyTerminalControlSlider<MyRadioAntenna> slider2 = new MyTerminalControlSlider<MyRadioAntenna>("Radius", MySpaceTexts.BlockPropertyTitle_BroadcastRadius, MySpaceTexts.BlockPropertyDescription_BroadcastRadius);
                slider2.SetLogLimits(block => 1f, block => (block.BlockDefinition as MyRadioAntennaDefinition).MaxBroadcastRadius);
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local45 = (MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate) slider2;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local46 = (MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate) slider2;
                local46.DefaultValueGetter = x => (x.BlockDefinition as MyRadioAntennaDefinition).MaxBroadcastRadius / 10f;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local43 = local46;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local44 = local46;
                local44.Getter = x => x.RadioBroadcaster.BroadcastRadius;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local41 = local44;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local42 = local44;
                local42.Setter = (x, v) => x.m_radius.Value = v;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local39 = local42;
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local40 = local42;
                local40.Writer = (x, result) => result.Append(new StringBuilder().AppendDecimal(x.RadioBroadcaster.BroadcastRadius, 0).Append(" m"));
                MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate local22 = local40;
                ((MyTerminalControlSlider<MyRadioAntenna>) local22).EnableActions<MyRadioAntenna>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>((MyTerminalControl<MyRadioAntenna>) local22);
                on = null;
                on = null;
                MyTerminalControlCheckbox<MyRadioAntenna> checkbox3 = new MyTerminalControlCheckbox<MyRadioAntenna>("EnableBroadCast", MySpaceTexts.Antenna_EnableBroadcast, MySpaceTexts.Antenna_EnableBroadcast, on, on);
                MyTerminalControlCheckbox<MyRadioAntenna> checkbox4 = new MyTerminalControlCheckbox<MyRadioAntenna>("EnableBroadCast", MySpaceTexts.Antenna_EnableBroadcast, MySpaceTexts.Antenna_EnableBroadcast, on, on);
                checkbox4.Getter = x => x.EnableBroadcasting.Value;
                MyTerminalControlCheckbox<MyRadioAntenna> local37 = checkbox4;
                MyTerminalControlCheckbox<MyRadioAntenna> local38 = checkbox4;
                local38.Setter = (x, v) => x.EnableBroadcasting.Value = v;
                MyTerminalControlCheckbox<MyRadioAntenna> checkbox = local38;
                checkbox.EnableAction<MyRadioAntenna>(null);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(checkbox);
                on = null;
                on = null;
                MyTerminalControlCheckbox<MyRadioAntenna> checkbox1 = new MyTerminalControlCheckbox<MyRadioAntenna>("ShowShipName", MySpaceTexts.BlockPropertyTitle_ShowShipName, MySpaceTexts.BlockPropertyDescription_ShowShipName, on, on);
                MyTerminalControlCheckbox<MyRadioAntenna> checkbox2 = new MyTerminalControlCheckbox<MyRadioAntenna>("ShowShipName", MySpaceTexts.BlockPropertyTitle_ShowShipName, MySpaceTexts.BlockPropertyDescription_ShowShipName, on, on);
                checkbox2.Getter = x => x.ShowShipName;
                MyTerminalControlCheckbox<MyRadioAntenna> local35 = checkbox2;
                MyTerminalControlCheckbox<MyRadioAntenna> local36 = checkbox2;
                local36.Setter = (x, v) => x.ShowShipName = v;
                MyTerminalControlCheckbox<MyRadioAntenna> local28 = local36;
                local28.EnableAction<MyRadioAntenna>(null);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(local28);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(new MyTerminalControlSeparator<MyRadioAntenna>());
                on = null;
                on = null;
                m_ignoreAllyCheckbox = new MyTerminalControlCheckbox<MyRadioAntenna>("IgnoreAlliedBroadcast", MySpaceTexts.Antenna_IgnoreAlliedBroadcast, MySpaceTexts.Antenna_IgnoreAlliedBroadcastTooltip, on, on);
                m_ignoreAllyCheckbox.Enabled = x => x.m_attachedPB.Value != 0L;
                m_ignoreAllyCheckbox.Getter = x => (bool) x.m_ignoreAlliedBroadcast;
                m_ignoreAllyCheckbox.Setter = (x, y) => x.m_ignoreAlliedBroadcast.Value = y;
                m_ignoreAllyCheckbox.EnableAction<MyRadioAntenna>(null);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(m_ignoreAllyCheckbox);
                on = null;
                on = null;
                m_ignoreOtherCheckbox = new MyTerminalControlCheckbox<MyRadioAntenna>("IgnoreOtherBroadcast", MySpaceTexts.Antenna_IgnoreOtherBroadcast, MySpaceTexts.Antenna_IgnoreOtherBroadcastTooltip, on, on);
                m_ignoreOtherCheckbox.Enabled = x => x.m_attachedPB.Value != 0L;
                m_ignoreOtherCheckbox.Getter = x => (bool) x.m_ignoreOtherBroadcast;
                m_ignoreOtherCheckbox.Setter = (x, y) => x.m_ignoreOtherBroadcast.Value = y;
                m_ignoreOtherCheckbox.EnableAction<MyRadioAntenna>(null);
                MyTerminalControlFactory.AddControl<MyRadioAntenna>(m_ignoreOtherCheckbox);
            }
        }

        public bool EnableLongDrawDistance() => 
            true;

        private void FillPBComboBoxContent(ICollection<MyTerminalControlComboBoxItem> items)
        {
            MyTerminalControlComboBoxItem item = new MyTerminalControlComboBoxItem {
                Key = 0L,
                Value = MyCommonTexts.ScreenGraphicsOptions_AntiAliasing_None
            };
            items.Add(item);
            bool flag = false;
            MyFatBlockReader<MyProgrammableBlock> fatBlocks = base.CubeGrid.GetFatBlocks<MyProgrammableBlock>();
            foreach (MyProgrammableBlock block in fatBlocks)
            {
                item = new MyTerminalControlComboBoxItem {
                    Key = block.EntityId,
                    Value = MyStringId.GetOrCompute(block.CustomName.ToString())
                };
                items.Add(item);
                if (block.EntityId == this.m_attachedPB)
                {
                    flag = true;
                }
            }
            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(base.CubeGrid);
            if (group != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in group.Nodes)
                {
                    if (node.NodeData != base.CubeGrid)
                    {
                        foreach (MyProgrammableBlock block2 in node.NodeData.GetFatBlocks<MyProgrammableBlock>())
                        {
                            if (!block2.ShowInToolbarConfig)
                            {
                                continue;
                            }
                            item = new MyTerminalControlComboBoxItem {
                                Key = block2.EntityId,
                                Value = MyStringId.GetOrCompute(block2.CustomName.ToString())
                            };
                            items.Add(item);
                            if (block2.EntityId == this.m_attachedPB)
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                this.m_attachedPB.Value = 0L;
            }
        }

        public BoundingBox? GetBoundingBox() => 
            null;

        public Color GetGizmoColor() => 
            this.m_gizmoColor;

        public override List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            base.m_hudParams.Clear();
            if ((((base.CubeGrid != null) && !base.CubeGrid.MarkedForClose) && !base.CubeGrid.Closed) && base.IsWorking)
            {
                List<MyHudEntityParams> hudParams = this.GetHudParams(allowBlink && base.HasLocalPlayerAccess());
                StringBuilder hudText = this.HudText;
                if (this.ShowShipName || (hudText.Length > 0))
                {
                    StringBuilder text = hudParams[0].Text;
                    text.Clear();
                    if (!string.IsNullOrEmpty(base.GetOwnerFactionTag()))
                    {
                        text.Append(base.GetOwnerFactionTag());
                        text.Append(".");
                    }
                    if (this.ShowShipName)
                    {
                        text.Append(base.CubeGrid.DisplayName);
                        text.Append(" - ");
                    }
                    text.Append(hudText);
                }
                base.m_hudParams.AddList<MyHudEntityParams>(hudParams);
                if (base.HasLocalPlayerAccess() && (base.SlimBlock.CubeGrid.GridSystems.TerminalSystem != null))
                {
                    base.SlimBlock.CubeGrid.GridSystems.TerminalSystem.NeedsHudUpdate = true;
                    foreach (MyTerminalBlock block in base.SlimBlock.CubeGrid.GridSystems.TerminalSystem.HudBlocks)
                    {
                        if (!ReferenceEquals(block, this))
                        {
                            base.m_hudParams.AddList<MyHudEntityParams>(block.GetHudParams(true));
                        }
                    }
                }
                MyEntityController entityController = MySession.Static.Players.GetEntityController(base.CubeGrid);
                if (entityController != null)
                {
                    MyCockpit controlledEntity = entityController.ControlledEntity as MyCockpit;
                    if ((controlledEntity != null) && (controlledEntity.Pilot != null))
                    {
                        base.m_hudParams.AddList<MyHudEntityParams>(controlledEntity.GetHudParams(true));
                    }
                }
            }
            return base.m_hudParams;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_RadioAntenna objectBuilderCubeBlock = (MyObjectBuilder_RadioAntenna) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.BroadcastRadius = this.RadioBroadcaster.BroadcastRadius;
            objectBuilderCubeBlock.ShowShipName = this.ShowShipName;
            objectBuilderCubeBlock.EnableBroadcasting = this.RadioBroadcaster.WantsToBeEnabled;
            objectBuilderCubeBlock.HudText = this.HudText.ToString();
            objectBuilderCubeBlock.AttachedPB = this.m_attachedPB.Value;
            objectBuilderCubeBlock.IgnoreAllied = this.m_ignoreAlliedBroadcast.Value;
            objectBuilderCubeBlock.IgnoreOther = this.m_ignoreOtherBroadcast.Value;
            return objectBuilderCubeBlock;
        }

        public Vector3 GetPositionInGrid() => 
            ((Vector3) base.Position);

        public float GetRadius() => 
            this.RadioBroadcaster.BroadcastRadius;

        public MatrixD GetWorldMatrix() => 
            base.PositionComp.WorldMatrix;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            this.RadioBroadcaster = new MyRadioBroadcaster(100f);
            this.RadioReceiver = new MyRadioReceiver();
            MyRadioAntennaDefinition blockDefinition = base.BlockDefinition as MyRadioAntennaDefinition;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(blockDefinition.ResourceSinkGroup, 0.002f, new Func<float>(this.UpdatePowerInput));
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_RadioAntenna antenna = (MyObjectBuilder_RadioAntenna) objectBuilder;
            this.RadioBroadcaster.BroadcastRadius = (antenna.BroadcastRadius <= 0f) ? (blockDefinition.MaxBroadcastRadius / 10f) : antenna.BroadcastRadius;
            this.HudText.Clear();
            if (antenna.HudText != null)
            {
                this.HudText.Append(antenna.HudText);
            }
            this.RadioBroadcaster.BroadcastRadius = MathHelper.Clamp(this.RadioBroadcaster.BroadcastRadius, 1f, blockDefinition.MaxBroadcastRadius);
            base.ResourceSink.Update();
            this.RadioBroadcaster.WantsToBeEnabled = antenna.EnableBroadcasting;
            this.m_showShipName.SetLocalValue(antenna.ShowShipName);
            if (Sync.IsServer)
            {
                this.m_attachedPB.Value = antenna.AttachedPB;
                this.m_attachedPB.ValueChanged += delegate (SyncBase x) {
                    m_messageRequest -= new Action<MyRadioAntenna, string, MyTransmitTarget>(this.OnMessageRecieved);
                    if (this.m_attachedPB.Value != 0)
                    {
                        m_messageRequest += new Action<MyRadioAntenna, string, MyTransmitTarget>(this.OnMessageRecieved);
                    }
                };
            }
            this.m_ignoreOtherBroadcast.SetLocalValue(antenna.IgnoreOther);
            this.m_ignoreAlliedBroadcast.SetLocalValue(antenna.IgnoreAllied);
            base.ShowOnHUD = false;
            this.m_gizmoColor = new VRageMath.Vector4(0.2f, 0.2f, 0f, 0.5f);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.m_radius.ValueChanged += obj => this.ChangeRadius();
            this.EnableBroadcasting.ValueChanged += obj => this.ChangeEnableBroadcast();
            this.m_showShipName.ValueChanged += obj => this.OnShowShipNameChanged();
        }

        private bool IsBroadcasting() => 
            ((this.RadioBroadcaster != null) ? this.RadioBroadcaster.WantsToBeEnabled : false);

        public static bool IsRecievedByPlayer(MyCubeBlock cubeBlock)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter == null)
            {
                return false;
            }
            MyRadioReceiver radioReceiver = localCharacter.RadioReceiver;
            return MyAntennaSystem.Static.CheckConnection((MyDataReceiver) radioReceiver, (VRage.Game.Entity.MyEntity) cubeBlock, localCharacter.GetPlayerIdentityId(), false);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (Sync.IsServer)
            {
                this.EnableBroadcasting.Value = this.RadioBroadcaster.WantsToBeEnabled;
            }
            MyRadioBroadcaster radioBroadcaster = this.RadioBroadcaster;
            radioBroadcaster.OnBroadcastRadiusChanged = (Action) Delegate.Combine(radioBroadcaster.OnBroadcastRadiusChanged, new Action(this.OnBroadcastRadiusChanged));
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            if ((this.m_attachedPB.Value != 0) && Sync.IsServer)
            {
                m_messageRequest += new Action<MyRadioAntenna, string, MyTransmitTarget>(this.OnMessageRecieved);
            }
            if (this.m_showShipName != null)
            {
                base.CubeGrid.OnNameChanged += new Action<MyCubeGrid>(this.OnShipNameChanged);
            }
        }

        private void OnBroadcastRadiusChanged()
        {
            base.ResourceSink.Update();
            base.RaisePropertiesChanged();
            this.UpdateText();
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            if (this.m_showShipName != null)
            {
                oldGrid.OnNameChanged -= new Action<MyCubeGrid>(this.OnShipNameChanged);
                base.CubeGrid.OnNameChanged += new Action<MyCubeGrid>(this.OnShipNameChanged);
                this.RadioBroadcaster.RaiseAntennaNameChanged(this);
            }
            base.OnCubeGridChanged(oldGrid);
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            base.ResourceSink.Update();
            if (this.onceUpdated)
            {
                this.RadioReceiver.Enabled = base.IsWorking;
                this.RadioBroadcaster.Enabled = (this.EnableBroadcasting != null) && base.IsWorking;
                this.RadioBroadcaster.WantsToBeEnabled = (bool) this.EnableBroadcasting;
                this.RadioReceiver.UpdateBroadcastersInRange();
            }
            this.UpdateText();
        }

        private void OnMessageRecieved(MyRadioAntenna requester, string arg, MyTransmitTarget target)
        {
            if (!ReferenceEquals(requester.CubeGrid, base.CubeGrid))
            {
                MyProgrammableBlock block2;
                switch (MyIDModule.GetRelation(base.OwnerId, requester.OwnerId, MyOwnershipShareModeEnum.Faction, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare))
                {
                    case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                    case MyRelationsBetweenPlayerAndBlock.Neutral:
                        if (target.HasFlag(MyTransmitTarget.Neutral))
                        {
                            break;
                        }
                        return;

                    case MyRelationsBetweenPlayerAndBlock.Owner:
                        if (target.HasFlag(MyTransmitTarget.Owned))
                        {
                            break;
                        }
                        return;

                    case MyRelationsBetweenPlayerAndBlock.FactionShare:
                        if (!this.m_ignoreAlliedBroadcast.Value && target.HasFlag(MyTransmitTarget.Ally))
                        {
                            break;
                        }
                        return;

                    case MyRelationsBetweenPlayerAndBlock.Enemies:
                        if (!this.m_ignoreOtherBroadcast.Value && target.HasFlag(MyTransmitTarget.Enemy))
                        {
                            break;
                        }
                        return;

                    default:
                        return;
                }
                Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyProgrammableBlock>(this.m_attachedPB.Value, out block2, false);
                if (((this.RadioReceiver != null) && ((block2 != null) && MyCubeGridGroups.Static.Logical.GetGroup(block2.CubeGrid).Nodes.Any<MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node>(n => (n.NodeData == base.CubeGrid)))) && MyAntennaSystem.Static.GetAllRelayedBroadcasters(this.RadioReceiver, base.OwnerId, false, null).Contains(requester.RadioBroadcaster))
                {
                    block2.Run(arg, UpdateType.Antenna);
                }
            }
        }

        protected override void OnOwnershipChanged()
        {
            base.OnOwnershipChanged();
            this.RadioBroadcaster.RaiseOwnerChanged();
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            m_messageRequest -= new Action<MyRadioAntenna, string, MyTransmitTarget>(this.OnMessageRecieved);
            MyRadioBroadcaster radioBroadcaster = this.RadioBroadcaster;
            radioBroadcaster.OnBroadcastRadiusChanged = (Action) Delegate.Remove(radioBroadcaster.OnBroadcastRadiusChanged, new Action(this.OnBroadcastRadiusChanged));
            base.SlimBlock.ComponentStack.IsFunctionalChanged -= new Action(this.ComponentStack_IsFunctionalChanged);
            if (this.m_showShipName != null)
            {
                base.CubeGrid.OnNameChanged -= new Action<MyCubeGrid>(this.OnShipNameChanged);
            }
        }

        private void OnShipNameChanged(MyCubeGrid grid)
        {
            this.RadioBroadcaster.RaiseAntennaNameChanged(this);
        }

        private void OnShowShipNameChanged()
        {
            this.RadioBroadcaster.RaiseAntennaNameChanged(this);
            if (this.m_showShipName != null)
            {
                base.CubeGrid.OnNameChanged += new Action<MyCubeGrid>(this.OnShipNameChanged);
            }
            else
            {
                base.CubeGrid.OnNameChanged -= new Action<MyCubeGrid>(this.OnShipNameChanged);
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.Enabled = base.IsWorking && ((bool) this.EnableBroadcasting);
            }
            if (this.RadioReceiver != null)
            {
                this.RadioReceiver.Enabled = base.IsWorking;
            }
            this.UpdateText();
        }

        bool Sandbox.ModAPI.Ingame.IMyRadioAntenna.TransmitMessage(string message, MyTransmitTarget target)
        {
            if (((this.m_nextBroadcast != null) || ((this.EnableBroadcasting == null) || !base.Enabled)) || !base.IsFunctional)
            {
                return false;
            }
            if (target == MyTransmitTarget.None)
            {
                return false;
            }
            if (message.Length > 0x186a0)
            {
                return false;
            }
            this.m_nextBroadcast = new MyTuple<string, MyTransmitTarget>(message, target);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            return true;
        }

        private void SetHudText(string text)
        {
            if (this.HudText.CompareUpdate(text))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyRadioAntenna, string>(this, x => new Action<string>(x.SetHudTextEvent), text, targetEndpoint);
            }
        }

        private void SetHudText(StringBuilder text)
        {
            this.SetHudText(text.ToString());
        }

        [Event(null, 0x65), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        protected void SetHudTextEvent(string text)
        {
            this.HudText.CompareUpdate(text);
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.RadioReceiver.UpdateBroadcastersInRange();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (Sync.IsServer && !this.onceUpdated)
            {
                base.IsWorkingChanged += new Action<MyCubeBlock>(this.UpdatePirateAntenna);
                base.CustomNameChanged += new Action<MyTerminalBlock>(this.UpdatePirateAntenna);
                base.OwnershipChanged += new Action<MyTerminalBlock>(this.UpdatePirateAntenna);
                this.UpdatePirateAntenna(this);
            }
            this.onceUpdated = true;
            if (this.m_nextBroadcast != null)
            {
                Action<MyRadioAntenna, string, MyTransmitTarget> messageRequest = m_messageRequest;
                if (messageRequest != null)
                {
                    messageRequest(this, this.m_nextBroadcast.Value.Item1, this.m_nextBroadcast.Value.Item2);
                }
                this.m_nextBroadcast = null;
            }
        }

        private void UpdatePirateAntenna(MyCubeBlock obj)
        {
            this.UpdatePirateAntenna(false);
        }

        public void UpdatePirateAntenna(bool remove = false)
        {
            bool activeState = base.IsWorking && Sync.Players.GetNPCIdentities().Contains(base.OwnerId);
            MyPirateAntennas.UpdatePirateAntenna(base.EntityId, remove, activeState, (this.HudText.Length > 0) ? this.HudText : base.CustomName);
        }

        private float UpdatePowerInput()
        {
            float num = ((this.EnableBroadcasting != null) ? this.RadioBroadcaster.BroadcastRadius : 1f) / 500f;
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return (num * 0.002f);
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(base.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f, base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.MoveBroadcaster();
            }
        }

        private MyRadioBroadcaster RadioBroadcaster
        {
            get => 
                ((MyRadioBroadcaster) base.Components.Get<MyDataBroadcaster>());
            set => 
                base.Components.Add<MyDataBroadcaster>(value);
        }

        private MyRadioReceiver RadioReceiver
        {
            get => 
                ((MyRadioReceiver) base.Components.Get<MyDataReceiver>());
            set => 
                base.Components.Add<MyDataReceiver>(value);
        }

        public bool ShowShipName
        {
            get => 
                ((bool) this.m_showShipName);
            set => 
                (this.m_showShipName.Value = value);
        }

        public StringBuilder HudText { get; private set; }

        float Sandbox.ModAPI.Ingame.IMyRadioAntenna.Radius
        {
            get => 
                this.GetRadius();
            set => 
                (this.m_radius.Value = MathHelper.Clamp(value, 0f, ((MyRadioAntennaDefinition) base.BlockDefinition).MaxBroadcastRadius));
        }

        string Sandbox.ModAPI.Ingame.IMyRadioAntenna.HudText
        {
            get => 
                this.HudText.ToString();
            set => 
                this.SetHudText(value);
        }

        bool Sandbox.ModAPI.Ingame.IMyRadioAntenna.IsBroadcasting =>
            this.IsBroadcasting();

        bool Sandbox.ModAPI.Ingame.IMyRadioAntenna.EnableBroadcasting
        {
            get => 
                this.EnableBroadcasting.Value;
            set => 
                (this.EnableBroadcasting.Value = value);
        }

        long Sandbox.ModAPI.Ingame.IMyRadioAntenna.AttachedProgrammableBlock
        {
            get => 
                this.m_attachedPB.Value;
            set => 
                (this.m_attachedPB.Value = value);
        }

        bool Sandbox.ModAPI.Ingame.IMyRadioAntenna.IgnoreOtherBroadcast
        {
            get => 
                this.m_ignoreOtherBroadcast.Value;
            set => 
                (this.m_ignoreOtherBroadcast.Value = value);
        }

        bool Sandbox.ModAPI.Ingame.IMyRadioAntenna.IgnoreAlliedBroadcast
        {
            get => 
                this.m_ignoreAlliedBroadcast.Value;
            set => 
                (this.m_ignoreAlliedBroadcast.Value = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRadioAntenna.<>c <>9 = new MyRadioAntenna.<>c();
            public static Func<MyRadioAntenna, Action<string>> <>9__30_0;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.GetterDelegate <>9__41_0;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.SetterDelegate <>9__41_1;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.GetterDelegate <>9__41_2;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.SetterDelegate <>9__41_3;
            public static Func<MyRadioAntenna, bool> <>9__41_4;
            public static MyTerminalControlTextbox<MyRadioAntenna>.GetterDelegate <>9__41_5;
            public static MyTerminalControlTextbox<MyRadioAntenna>.SetterDelegate <>9__41_6;
            public static MyTerminalControlTextbox<MyRadioAntenna>.GetterDelegate <>9__41_7;
            public static MyTerminalControlTextbox<MyRadioAntenna>.SetterDelegate <>9__41_8;
            public static MyTerminalControlCombobox<MyRadioAntenna>.ComboBoxContentDelegate <>9__41_9;
            public static MyTerminalValueControl<MyRadioAntenna, long>.GetterDelegate <>9__41_10;
            public static MyTerminalValueControl<MyRadioAntenna, long>.SetterDelegate <>9__41_11;
            public static MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate <>9__41_12;
            public static MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate <>9__41_13;
            public static MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate <>9__41_14;
            public static MyTerminalValueControl<MyRadioAntenna, float>.GetterDelegate <>9__41_15;
            public static MyTerminalValueControl<MyRadioAntenna, float>.SetterDelegate <>9__41_16;
            public static MyTerminalControl<MyRadioAntenna>.WriterDelegate <>9__41_17;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.GetterDelegate <>9__41_18;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.SetterDelegate <>9__41_19;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.GetterDelegate <>9__41_20;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.SetterDelegate <>9__41_21;
            public static Func<MyRadioAntenna, bool> <>9__41_22;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.GetterDelegate <>9__41_23;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.SetterDelegate <>9__41_24;
            public static Func<MyRadioAntenna, bool> <>9__41_25;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.GetterDelegate <>9__41_26;
            public static MyTerminalValueControl<MyRadioAntenna, bool>.SetterDelegate <>9__41_27;

            internal bool <CreateTerminalControls>b__41_0(MyRadioAntenna x) => 
                x.ShowInTerminal;

            internal void <CreateTerminalControls>b__41_1(MyRadioAntenna x, bool v)
            {
                x.ShowInTerminal = v;
            }

            internal long <CreateTerminalControls>b__41_10(MyRadioAntenna x) => 
                ((long) x.m_attachedPB);

            internal void <CreateTerminalControls>b__41_11(MyRadioAntenna x, long y)
            {
                x.m_attachedPB.Value = y;
                MyRadioAntenna.m_ignoreAllyCheckbox.UpdateVisual();
                MyRadioAntenna.m_ignoreOtherCheckbox.UpdateVisual();
            }

            internal float <CreateTerminalControls>b__41_12(MyRadioAntenna block) => 
                1f;

            internal float <CreateTerminalControls>b__41_13(MyRadioAntenna block) => 
                (block.BlockDefinition as MyRadioAntennaDefinition).MaxBroadcastRadius;

            internal float <CreateTerminalControls>b__41_14(MyRadioAntenna x) => 
                ((x.BlockDefinition as MyRadioAntennaDefinition).MaxBroadcastRadius / 10f);

            internal float <CreateTerminalControls>b__41_15(MyRadioAntenna x) => 
                x.RadioBroadcaster.BroadcastRadius;

            internal void <CreateTerminalControls>b__41_16(MyRadioAntenna x, float v)
            {
                x.m_radius.Value = v;
            }

            internal void <CreateTerminalControls>b__41_17(MyRadioAntenna x, StringBuilder result)
            {
                result.Append(new StringBuilder().AppendDecimal(x.RadioBroadcaster.BroadcastRadius, 0).Append(" m"));
            }

            internal bool <CreateTerminalControls>b__41_18(MyRadioAntenna x) => 
                x.EnableBroadcasting.Value;

            internal void <CreateTerminalControls>b__41_19(MyRadioAntenna x, bool v)
            {
                x.EnableBroadcasting.Value = v;
            }

            internal bool <CreateTerminalControls>b__41_2(MyRadioAntenna x) => 
                x.ShowInToolbarConfig;

            internal bool <CreateTerminalControls>b__41_20(MyRadioAntenna x) => 
                x.ShowShipName;

            internal void <CreateTerminalControls>b__41_21(MyRadioAntenna x, bool v)
            {
                x.ShowShipName = v;
            }

            internal bool <CreateTerminalControls>b__41_22(MyRadioAntenna x) => 
                (x.m_attachedPB.Value != 0L);

            internal bool <CreateTerminalControls>b__41_23(MyRadioAntenna x) => 
                ((bool) x.m_ignoreAlliedBroadcast);

            internal void <CreateTerminalControls>b__41_24(MyRadioAntenna x, bool y)
            {
                x.m_ignoreAlliedBroadcast.Value = y;
            }

            internal bool <CreateTerminalControls>b__41_25(MyRadioAntenna x) => 
                (x.m_attachedPB.Value != 0L);

            internal bool <CreateTerminalControls>b__41_26(MyRadioAntenna x) => 
                ((bool) x.m_ignoreOtherBroadcast);

            internal void <CreateTerminalControls>b__41_27(MyRadioAntenna x, bool y)
            {
                x.m_ignoreOtherBroadcast.Value = y;
            }

            internal void <CreateTerminalControls>b__41_3(MyRadioAntenna x, bool v)
            {
                x.ShowInToolbarConfig = v;
            }

            internal bool <CreateTerminalControls>b__41_4(MyRadioAntenna x) => 
                !x.m_textboxOpen;

            internal StringBuilder <CreateTerminalControls>b__41_5(MyRadioAntenna x) => 
                x.CustomName;

            internal void <CreateTerminalControls>b__41_6(MyRadioAntenna x, StringBuilder v)
            {
                x.SetCustomName(v);
            }

            internal StringBuilder <CreateTerminalControls>b__41_7(MyRadioAntenna x) => 
                x.HudText;

            internal void <CreateTerminalControls>b__41_8(MyRadioAntenna x, StringBuilder v)
            {
                x.SetHudText(v);
            }

            internal void <CreateTerminalControls>b__41_9(MyRadioAntenna x, ICollection<MyTerminalControlComboBoxItem> list)
            {
                x.FillPBComboBoxContent(list);
            }

            internal Action<string> <SetHudText>b__30_0(MyRadioAntenna x) => 
                new Action<string>(x.SetHudTextEvent);
        }
    }
}

