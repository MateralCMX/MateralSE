namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
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
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_AirVent)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyAirVent), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent) })]
    public class MyAirVent : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyAirVent, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent, IMyGasBlock, IMyConveyorEndpointBlock
    {
        private static readonly string[] m_emissiveTextureNames = new string[] { "Emissive0", "Emissive1", "Emissive2", "Emissive3" };
        private MyStringHash m_prevColor = MyStringHash.NullOrEmpty;
        private int m_prevFillCount = -1;
        private bool m_isProducing;
        private bool m_producedSinceLastUpdate;
        private bool m_isPlayingVentEffect;
        private MyParticleEffect m_effect;
        private MyToolbarItem m_onFullAction;
        private MyToolbarItem m_onEmptyAction;
        private MyToolbar m_actionToolbar;
        private bool? m_wasRoomFull;
        private bool? m_wasRoomEmpty;
        private readonly MyDefinitionId m_oxygenGasId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_isDepressurizing;
        private readonly VRage.Sync.Sync<MyAirVentBlockRoomInfo, SyncDirection.FromServer> m_blockRoomInfo;
        private MyResourceSourceComponent m_sourceComp;
        private MyResourceSinkInfo OxygenSinkInfo;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private bool m_syncing;

        public MyAirVent()
        {
            this.CreateTerminalControls();
            base.ResourceSink = new MyResourceSinkComponent(2);
            this.SourceComp = new MyResourceSourceComponent(1);
            this.m_isDepressurizing.ValueChanged += x => this.SetDepressurizing();
        }

        public bool AllowSelfPulling() => 
            false;

        private void CheckForVentEffect(float amount)
        {
            if (amount > 0f)
            {
                this.m_producedSinceLastUpdate = true;
                if ((amount > 1f) && !this.m_isPlayingVentEffect)
                {
                    this.CreateEffect();
                }
            }
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId));

        protected override void Closing()
        {
            base.Closing();
            this.StopVentEffect();
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            if (((base.CubeGrid != null) && ((this.SourceComp != null) && ((base.CubeGrid.GridSystems != null) && (base.CubeGrid.GridSystems.ResourceDistributor != null)))) && !base.CubeGrid.Closed)
            {
                this.SourceComp.Enabled = base.IsWorking;
                base.ResourceSink.Update();
                base.CubeGrid.GridSystems.ResourceDistributor.ConveyorSystem_OnPoweredChanged();
                this.UpdateEmissivity();
                this.UpdateStatus();
            }
        }

        private float ComputeRequiredPower()
        {
            if ((!MySession.Static.Settings.EnableOxygen || (!base.Enabled || !base.IsFunctional)) || !MySession.Static.Settings.EnableOxygenPressurization)
            {
                return 0f;
            }
            return (this.m_isProducing ? this.BlockDefinition.OperationalPowerConsumption : this.BlockDefinition.StandbyPowerConsumption);
        }

        private void CreateEffect()
        {
            if (Sync.IsServer)
            {
                this.m_isPlayingVentEffect = true;
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAirVent>(this, x => new Action(x.CreateVentEffectImplementation), targetEndpoint);
                if (Sync.IsServer && !Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    this.CreateVentEffectImplementation();
                }
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyAirVent>())
            {
                base.CreateTerminalControls();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyAirVent> switch1 = new MyTerminalControlOnOffSwitch<MyAirVent>("Depressurize", MySpaceTexts.BlockPropertyTitle_Depressurize, MySpaceTexts.BlockPropertyDescription_Depressurize, on, on);
                MyTerminalControlOnOffSwitch<MyAirVent> switch2 = new MyTerminalControlOnOffSwitch<MyAirVent>("Depressurize", MySpaceTexts.BlockPropertyTitle_Depressurize, MySpaceTexts.BlockPropertyDescription_Depressurize, on, on);
                switch2.Getter = x => x.IsDepressurizing;
                MyTerminalControlOnOffSwitch<MyAirVent> local5 = switch2;
                MyTerminalControlOnOffSwitch<MyAirVent> local6 = switch2;
                local6.Setter = delegate (MyAirVent x, bool v) {
                    x.IsDepressurizing = v;
                    x.UpdateEmissivity();
                };
                MyTerminalControlOnOffSwitch<MyAirVent> onOff = local6;
                onOff.EnableToggleAction<MyAirVent>();
                onOff.EnableOnOffActions<MyAirVent>();
                MyTerminalControlFactory.AddControl<MyAirVent>(onOff);
                MyTerminalControlButton<MyAirVent> control = new MyTerminalControlButton<MyAirVent>("Open Toolbar", MySpaceTexts.BlockPropertyTitle_SensorToolbarOpen, MySpaceTexts.BlockPropertyDescription_SensorToolbarOpen, delegate (MyAirVent self) {
                    if (self.m_onFullAction != null)
                    {
                        self.m_actionToolbar.SetItemAtIndex(0, self.m_onFullAction);
                    }
                    if (self.m_onEmptyAction != null)
                    {
                        self.m_actionToolbar.SetItemAtIndex(1, self.m_onEmptyAction);
                    }
                    self.m_actionToolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(self.Toolbar_ItemChanged);
                    if (MyGuiScreenToolbarConfigBase.Static == null)
                    {
                        MyToolbarComponent.CurrentToolbar = self.m_actionToolbar;
                        object[] args = new object[] { 0, self };
                        MyToolbarComponent.AutoUpdate = false;
                        MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                        screen.Closed += delegate (MyGuiScreenBase source) {
                            MyToolbarComponent.AutoUpdate = true;
                            self.m_actionToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(self.Toolbar_ItemChanged);
                            self.m_actionToolbar.Clear();
                        };
                        MyGuiSandbox.AddScreen(screen);
                    }
                });
                control.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyAirVent>(control);
            }
        }

        [Event(null, 0x1b3), Reliable, Broadcast]
        private unsafe void CreateVentEffectImplementation()
        {
            this.StopVentEffectImplementation();
            MatrixD localMatrix = base.PositionComp.LocalMatrix;
            if (this.IsDepressurizing)
            {
                MatrixD* xdPtr1 = (MatrixD*) ref localMatrix;
                xdPtr1.Left = localMatrix.Right;
                MatrixD* xdPtr2 = (MatrixD*) ref localMatrix;
                xdPtr2.Forward = localMatrix.Backward;
            }
            MatrixD* xdPtr3 = (MatrixD*) ref localMatrix;
            xdPtr3.Translation += base.PositionComp.LocalMatrix.Forward * ((this.BlockDefinition.CubeSize == MyCubeSize.Large) ? 1f : 0.1f);
            Vector3D zero = Vector3D.Zero;
            if (MyParticlesManager.TryCreateParticleEffect("OxyVent", ref localMatrix, ref zero, base.Render.ParentIDs[0], out this.m_effect))
            {
                this.m_effect.UserScale = (this.BlockDefinition.CubeSize != MyCubeSize.Large) ? 0.5f : 3f;
            }
            this.m_isPlayingVentEffect = true;
        }

        private void DrainFromRoom(float amount)
        {
            if ((amount != 0f) && this.IsDepressurizing)
            {
                MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
                if ((oxygenBlock != null) && (oxygenBlock.Room != null))
                {
                    float oxygenAmount = oxygenBlock.Room.OxygenAmount;
                    if (!oxygenBlock.Room.IsAirtight)
                    {
                        float newRemainingCapacity = (MyOxygenProviderSystem.GetOxygenInPoint(base.WorldMatrix.Translation) != 0f) ? (this.BlockDefinition.VentilationCapacityPerSecond * 100f) : 0f;
                        this.SourceComp.SetRemainingCapacityByType(this.m_oxygenGasId, newRemainingCapacity);
                        this.m_producedSinceLastUpdate = true;
                    }
                    else
                    {
                        MyOxygenRoom room = oxygenBlock.Room;
                        room.OxygenAmount -= amount;
                        if (oxygenBlock.Room.OxygenAmount < 0f)
                        {
                            oxygenBlock.Room.OxygenAmount = 0f;
                        }
                        this.SourceComp.SetRemainingCapacityByType(this.m_oxygenGasId, oxygenBlock.Room.OxygenAmount);
                    }
                    base.ResourceSink.Update();
                    this.CheckForVentEffect(amount);
                }
            }
        }

        private void ExecuteAction(MyToolbarItem action)
        {
            this.m_actionToolbar.SetItemAtIndex(0, action);
            this.m_actionToolbar.UpdateItem(0);
            this.m_actionToolbar.ActivateItemAtSlot(0, false, true, true);
            this.m_actionToolbar.Clear();
        }

        private void ExecuteGasTransfer()
        {
            float transferAmount = this.GasInputPerUpdate - this.GasOutputPerUpdate;
            if (transferAmount != 0f)
            {
                this.Transfer(transferAmount);
            }
            else
            {
                if (!base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
                this.StopVentEffect();
            }
            base.ResourceSink.Update();
            this.SourceComp.OnProductionEnabledChanged(new MyDefinitionId?(this.m_oxygenGasId));
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_AirVent objectBuilderCubeBlock = (MyObjectBuilder_AirVent) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.IsDepressurizing = this.IsDepressurizing;
            if (this.m_onFullAction != null)
            {
                objectBuilderCubeBlock.OnFullAction = this.m_onFullAction.GetObjectBuilder();
            }
            if (this.m_onEmptyAction != null)
            {
                objectBuilderCubeBlock.OnEmptyAction = this.m_onEmptyAction.GetObjectBuilder();
            }
            return objectBuilderCubeBlock;
        }

        private MyOxygenBlock GetOxygenBlock()
        {
            if ((!MySession.Static.Settings.EnableOxygen || (!MySession.Static.Settings.EnableOxygenPressurization || (this.VentDummy == null))) || (base.CubeGrid.GridSystems.GasSystem == null))
            {
                return new MyOxygenBlock();
            }
            MatrixD xd = MatrixD.Multiply(MatrixD.Normalize(this.VentDummy.Matrix), base.WorldMatrix);
            return base.CubeGrid.GridSystems.GasSystem.GetOxygenBlock(xd.Translation);
        }

        public float GetOxygenLevel()
        {
            if (base.IsWorking)
            {
                MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
                if ((oxygenBlock != null) && (oxygenBlock.Room != null))
                {
                    float num = oxygenBlock.OxygenLevel(base.CubeGrid.GridSize);
                    return (!oxygenBlock.Room.IsAirtight ? oxygenBlock.Room.EnvironmentOxygen : num);
                }
            }
            return 0f;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_AirVent vent = (MyObjectBuilder_AirVent) objectBuilder;
            this.InitializeConveyorEndpoint();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            MyResourceSourceInfo sourceResourceData = new MyResourceSourceInfo {
                ResourceTypeId = this.m_oxygenGasId,
                DefinedOutput = this.BlockDefinition.VentilationCapacityPerSecond,
                ProductionToCapacityMultiplier = 1f
            };
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            this.SourceComp.OutputChanged += new MyResourceOutputChangedDelegate(this.Source_OutputChanged);
            MyResourceSinkInfo item = new MyResourceSinkInfo {
                ResourceTypeId = this.m_oxygenGasId,
                MaxRequiredInput = this.BlockDefinition.VentilationCapacityPerSecond,
                RequiredInputFunc = new Func<float>(this.Sink_ComputeRequiredGas)
            };
            this.OxygenSinkInfo = item;
            item = new MyResourceSinkInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                MaxRequiredInput = this.BlockDefinition.OperationalPowerConsumption,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredPower)
            };
            List<MyResourceSinkInfo> list1 = new List<MyResourceSinkInfo>();
            list1.Add(item);
            List<MyResourceSinkInfo> sinkData = list1;
            base.ResourceSink.Init(this.BlockDefinition.ResourceSinkGroup, sinkData);
            base.ResourceSink.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            base.ResourceSink.CurrentInputChanged += new MyCurrentResourceInputChangedDelegate(this.Sink_CurrentInputChanged);
            this.m_actionToolbar = new MyToolbar(MyToolbarType.ButtonPanel, 2, 1);
            this.m_actionToolbar.DrawNumbers = false;
            this.m_actionToolbar.Init(null, this, false);
            if (vent.OnFullAction != null)
            {
                this.m_onFullAction = MyToolbarItemFactory.CreateToolbarItem(vent.OnFullAction);
            }
            if (vent.OnEmptyAction != null)
            {
                this.m_onEmptyAction = MyToolbarItemFactory.CreateToolbarItem(vent.OnEmptyAction);
            }
            this.UpdateEmissivity();
            this.UpdateStatus();
            this.UpdateTexts();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyAirVent_IsWorkingChanged);
            this.m_isDepressurizing.SetLocalValue(vent.IsDepressurizing);
            this.SetDepressurizing();
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        public bool IsPressurized() => 
            this.CanPressurize;

        public bool IsRoomAirtight()
        {
            MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
            return (((oxygenBlock != null) && (oxygenBlock.Room != null)) && oxygenBlock.Room.IsAirtight);
        }

        private void MyAirVent_IsWorkingChanged(MyCubeBlock obj)
        {
            this.SourceComp.Enabled = base.IsWorking;
            this.UpdateEmissivity();
            this.UpdateStatus();
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.UpdateStatus();
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.SourceComp.Enabled = base.IsWorking;
            base.ResourceSink.Update();
            this.UpdateEmissivity();
            this.UpdateStatus();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.m_prevFillCount = -1;
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        bool IMyGasBlock.IsWorking() => 
            this.CanVentToRoom;

        [Event(null, 0x3d2), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        private void SendToolbarItemChanged(ToolbarItem sentItem, int index)
        {
            this.m_syncing = true;
            MyToolbarItem item = null;
            if (sentItem.EntityID != 0)
            {
                if (string.IsNullOrEmpty(sentItem.GroupName))
                {
                    MyTerminalBlock block;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyTerminalBlock>(sentItem.EntityID, out block, false))
                    {
                        MyObjectBuilder_ToolbarItemTerminalBlock data = MyToolbarItemFactory.TerminalBlockObjectBuilderFromBlock(block);
                        data._Action = sentItem.Action;
                        data.Parameters = sentItem.Parameters;
                        item = MyToolbarItemFactory.CreateToolbarItem(data);
                    }
                }
                else
                {
                    string groupName = sentItem.GroupName;
                    MyBlockGroup group = base.CubeGrid.GridSystems.TerminalSystem.BlockGroups.Find(x => x.Name.ToString() == groupName);
                    if (group != null)
                    {
                        MyObjectBuilder_ToolbarItemTerminalGroup data = MyToolbarItemFactory.TerminalGroupObjectBuilderFromGroup(group);
                        data._Action = sentItem.Action;
                        data.BlockEntityId = sentItem.EntityID;
                        data.Parameters = sentItem.Parameters;
                        item = MyToolbarItemFactory.CreateToolbarItem(data);
                    }
                }
            }
            if (index == 0)
            {
                this.m_onFullAction = item;
            }
            else
            {
                this.m_onEmptyAction = item;
            }
            base.RaisePropertiesChanged();
            this.m_syncing = false;
        }

        private void SetDepressurizing()
        {
            this.StopVentEffect();
            if (!this.IsDepressurizing)
            {
                base.ResourceSink.AddType(ref this.OxygenSinkInfo);
            }
            else
            {
                MyDefinitionId oxygenGasId = this.m_oxygenGasId;
                base.ResourceSink.RemoveType(ref oxygenGasId);
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            this.SourceComp.SetProductionEnabledByType(this.m_oxygenGasId, this.IsDepressurizing);
            base.ResourceSink.Update();
        }

        public override bool SetEmissiveStateDamaged() => 
            this.SetEmissiveStateForVent(MyCubeBlock.m_emissiveNames.Damaged, 1f);

        public override bool SetEmissiveStateDisabled() => 
            this.SetEmissiveStateForVent(MyCubeBlock.m_emissiveNames.Disabled, 1f);

        private bool SetEmissiveStateForVent(MyStringHash state, float fillLevel)
        {
            int num = (int) (fillLevel * m_emissiveTextureNames.Length);
            bool flag = false;
            if ((base.Render.RenderObjectIDs[0] != uint.MaxValue) && ((state != this.m_prevColor) || (num != this.m_prevFillCount)))
            {
                int index = 0;
                while (true)
                {
                    if (index >= m_emissiveTextureNames.Length)
                    {
                        this.m_prevColor = state;
                        this.m_prevFillCount = num;
                        break;
                    }
                    flag |= this.SetEmissiveState((index <= num) ? state : MyCubeBlock.m_emissiveNames.Damaged, base.Render.RenderObjectIDs[0], m_emissiveTextureNames[index]);
                    index++;
                }
            }
            return flag;
        }

        public override bool SetEmissiveStateWorking() => 
            this.UpdateEmissivity();

        private float Sink_ComputeRequiredGas()
        {
            if (!this.CanVentToRoom)
            {
                return 0f;
            }
            MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
            if (((oxygenBlock == null) || (oxygenBlock.Room == null)) || !oxygenBlock.Room.IsAirtight)
            {
                return 0f;
            }
            float num = oxygenBlock.Room.MissingOxygen(base.CubeGrid.GridSize);
            if (num >= 0.0001f)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
            else
            {
                oxygenBlock.Room.OxygenAmount = oxygenBlock.Room.MaxOxygen(base.CubeGrid.GridSize);
                num = 0f;
            }
            return Math.Min(((num * 60f) * this.SourceComp.ProductionToCapacityMultiplierByType(this.m_oxygenGasId)) + this.SourceComp.CurrentOutputByType(this.m_oxygenGasId), this.BlockDefinition.VentilationCapacityPerSecond);
        }

        private void Sink_CurrentInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            if (resourceTypeId == this.m_oxygenGasId)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void Source_OutputChanged(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source)
        {
            if (changedResourceId == this.m_oxygenGasId)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void StopVentEffect()
        {
            if (Sync.IsServer && this.m_isPlayingVentEffect)
            {
                this.m_isPlayingVentEffect = false;
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAirVent>(this, x => new Action(x.StopVentEffectImplementation), targetEndpoint);
                if (Sync.IsServer && !Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    this.StopVentEffectImplementation();
                }
            }
        }

        [Event(null, 0x14f), Reliable, Broadcast]
        private void StopVentEffectImplementation()
        {
            this.m_isPlayingVentEffect = false;
            if (this.m_effect != null)
            {
                this.m_effect.Stop(false);
                this.m_effect = null;
            }
        }

        private void Toolbar_ItemChanged(MyToolbar self, MyToolbar.IndexArgs index)
        {
            if (!this.m_syncing)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAirVent, ToolbarItem, int>(this, x => new Action<ToolbarItem, int>(x.SendToolbarItemChanged), ToolbarItem.FromItem(self.GetItemAtIndex(index.ItemIndex)), index.ItemIndex, targetEndpoint);
            }
        }

        private void Transfer(float transferAmount)
        {
            if (transferAmount > 0f)
            {
                this.VentToRoom(transferAmount);
            }
            else if (transferAmount < 0f)
            {
                this.DrainFromRoom(-transferAmount);
            }
        }

        private void UpdateActions()
        {
            float oxygenLevel = this.GetOxygenLevel();
            if ((this.m_wasRoomEmpty == null) || (this.m_wasRoomFull == null))
            {
                this.m_wasRoomEmpty = false;
                this.m_wasRoomFull = false;
                if (oxygenLevel > 0.99f)
                {
                    this.m_wasRoomFull = true;
                }
                else if (oxygenLevel < 0.01f)
                {
                    this.m_wasRoomEmpty = true;
                }
            }
            else if (oxygenLevel > 0.99f)
            {
                this.m_wasRoomEmpty = false;
                if (!this.m_wasRoomFull.Value)
                {
                    this.ExecuteAction(this.m_onFullAction);
                    this.m_wasRoomFull = true;
                }
            }
            else if (oxygenLevel >= 0.01f)
            {
                this.m_wasRoomFull = false;
                this.m_wasRoomEmpty = false;
            }
            else
            {
                this.m_wasRoomFull = false;
                if (!this.m_wasRoomEmpty.Value)
                {
                    this.ExecuteAction(this.m_onEmptyAction);
                    this.m_wasRoomEmpty = true;
                }
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.m_isProducing = this.m_producedSinceLastUpdate;
            this.m_producedSinceLastUpdate = false;
            this.ExecuteGasTransfer();
            this.UpdateStatus();
            this.UpdateEmissivity();
            this.UpdateTexts();
        }

        public override void UpdateAfterSimulation100()
        {
            float oxygenAmount;
            base.UpdateAfterSimulation100();
            MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
            if (Sync.IsServer)
            {
                int isAirtight;
                float environmentOxygen;
                if (base.IsWorking)
                {
                    this.UpdateActions();
                }
                if ((oxygenBlock == null) || (oxygenBlock.Room == null))
                {
                    isAirtight = 0;
                }
                else
                {
                    isAirtight = (int) oxygenBlock.Room.IsAirtight;
                }
                bool isRoomAirtight = (bool) isAirtight;
                if ((oxygenBlock == null) || (oxygenBlock.Room == null))
                {
                    environmentOxygen = 0f;
                }
                else
                {
                    environmentOxygen = oxygenBlock.Room.EnvironmentOxygen;
                }
                float roomEnvironmentOxygen = environmentOxygen;
                this.m_blockRoomInfo.Value = new MyAirVentBlockRoomInfo(isRoomAirtight, (oxygenBlock != null) ? oxygenBlock.OxygenLevel(base.CubeGrid.GridSize) : 0f, roomEnvironmentOxygen);
            }
            if (((oxygenBlock == null) || (oxygenBlock.Room == null)) || !oxygenBlock.Room.IsAirtight)
            {
                oxygenAmount = (MyOxygenProviderSystem.GetOxygenInPoint(base.WorldMatrix.Translation) != 0f) ? this.BlockDefinition.VentilationCapacityPerSecond : 0f;
            }
            else
            {
                oxygenAmount = oxygenBlock.Room.OxygenAmount;
            }
            this.SourceComp.SetRemainingCapacityByType(this.m_oxygenGasId, oxygenAmount);
            this.UpdateStatus();
            this.UpdateEmissivity();
            this.UpdateTexts();
            base.ResourceSink.Update();
            if (MyFakes.ENABLE_OXYGEN_SOUNDS)
            {
                this.UpdateSound();
            }
        }

        private bool UpdateEmissivity()
        {
            if (!base.IsWorking)
            {
                return false;
            }
            MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
            bool flag = (oxygenBlock != null) && (oxygenBlock.Room != null);
            bool isRoomAirtight = flag && oxygenBlock.Room.IsAirtight;
            float roomEnvironmentOxygen = flag ? oxygenBlock.Room.EnvironmentOxygen : 0f;
            float oxygenLevel = flag ? oxygenBlock.OxygenLevel(base.CubeGrid.GridSize) : 0f;
            if (!Sync.IsServer)
            {
                isRoomAirtight = this.m_blockRoomInfo.Value.IsRoomAirtight;
                roomEnvironmentOxygen = this.m_blockRoomInfo.Value.RoomEnvironmentOxygen;
                oxygenLevel = this.m_blockRoomInfo.Value.OxygenLevel;
                flag = true;
            }
            if (flag)
            {
                return (!isRoomAirtight ? this.SetEmissiveStateForVent(MyCubeBlock.m_emissiveNames.Warning, Math.Max(oxygenLevel, roomEnvironmentOxygen)) : this.SetEmissiveStateForVent(this.IsDepressurizing ? MyCubeBlock.m_emissiveNames.Alternative : MyCubeBlock.m_emissiveNames.Working, oxygenLevel));
            }
            float fillLevel = (int) (MyOxygenProviderSystem.GetOxygenInPoint(base.WorldMatrix.Translation) * m_emissiveTextureNames.Length);
            return this.SetEmissiveStateForVent((fillLevel == 0f) ? MyCubeBlock.m_emissiveNames.Warning : (this.IsDepressurizing ? MyCubeBlock.m_emissiveNames.Alternative : MyCubeBlock.m_emissiveNames.Working), fillLevel);
        }

        private void UpdateSound()
        {
            if (base.m_soundEmitter != null)
            {
                if (!base.IsWorking)
                {
                    if (base.m_soundEmitter.IsPlaying)
                    {
                        base.m_soundEmitter.StopSound(false, true);
                    }
                }
                else
                {
                    bool? nullable;
                    if (!this.m_isPlayingVentEffect)
                    {
                        if (!base.m_soundEmitter.IsPlaying || !base.m_soundEmitter.SoundPair.Equals(this.BlockDefinition.IdleSound))
                        {
                            if (base.m_soundEmitter.IsPlaying && (base.m_soundEmitter.SoundPair.Equals(this.BlockDefinition.PressurizeSound) || base.m_soundEmitter.SoundPair.Equals(this.BlockDefinition.DepressurizeSound)))
                            {
                                base.m_soundEmitter.StopSound(false, true);
                            }
                            nullable = null;
                            base.m_soundEmitter.PlaySound(this.BlockDefinition.IdleSound, true, false, false, false, false, nullable);
                        }
                    }
                    else if (this.IsDepressurizing)
                    {
                        if (!base.m_soundEmitter.IsPlaying || !base.m_soundEmitter.SoundPair.Equals(this.BlockDefinition.DepressurizeSound))
                        {
                            nullable = null;
                            base.m_soundEmitter.PlaySound(this.BlockDefinition.DepressurizeSound, true, false, false, false, false, nullable);
                        }
                    }
                    else if (!base.m_soundEmitter.IsPlaying || !base.m_soundEmitter.SoundPair.Equals(this.BlockDefinition.PressurizeSound))
                    {
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.PressurizeSound, true, false, false, false, false, nullable);
                    }
                }
                base.m_soundEmitter.Update();
            }
        }

        private void UpdateStatus()
        {
            MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
            if ((oxygenBlock == null) || (oxygenBlock.Room == null))
            {
                this.Status = VentStatus.Depressurized;
            }
            else if (!oxygenBlock.Room.IsAirtight)
            {
                this.Status = (oxygenBlock.Room.OxygenLevel(base.CubeGrid.GridSize) <= 0.01f) ? VentStatus.Depressurized : VentStatus.Depressurizing;
            }
            else if (oxygenBlock.Room.OxygenLevel(base.CubeGrid.GridSize) < 1f)
            {
                this.Status = this.IsDepressurizing ? VentStatus.Depressurizing : VentStatus.Pressurizing;
            }
            else
            {
                if ((Sync.IsServer && (MyVisualScriptLogicProvider.RoomFullyPressurized != null)) && (this.Status != VentStatus.Pressurized))
                {
                    MyVisualScriptLogicProvider.RoomFullyPressurized(base.EntityId, base.CubeGrid.EntityId, base.Name, base.CubeGrid.Name);
                }
                this.Status = VentStatus.Pressurized;
            }
            this.CheckEmissiveState(false);
        }

        private void UpdateTexts()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            if (!MySession.Static.Settings.EnableOxygen || !MySession.Static.Settings.EnableOxygenPressurization)
            {
                base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.Oxygen_Disabled));
            }
            else
            {
                MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
                bool flag = (oxygenBlock != null) && (oxygenBlock.Room != null);
                bool isRoomAirtight = flag && oxygenBlock.Room.IsAirtight;
                float oxygenLevel = flag ? oxygenBlock.OxygenLevel(base.CubeGrid.GridSize) : 0f;
                if (!Sync.IsServer)
                {
                    isRoomAirtight = this.m_blockRoomInfo.Value.IsRoomAirtight;
                    oxygenLevel = this.m_blockRoomInfo.Value.OxygenLevel;
                    flag = true;
                }
                if (!flag || !isRoomAirtight)
                {
                    base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.Oxygen_NotPressurized));
                }
                else
                {
                    base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.Oxygen_Pressure) + (oxygenLevel * 100f).ToString("F") + "%");
                }
            }
            base.RaisePropertiesChanged();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateStatus();
        }

        private void VentToRoom(float amount)
        {
            if ((amount != 0f) && !this.IsDepressurizing)
            {
                MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
                if (((oxygenBlock != null) && (oxygenBlock.Room != null)) && oxygenBlock.Room.IsAirtight)
                {
                    MyOxygenRoom room = oxygenBlock.Room;
                    room.OxygenAmount += amount;
                    if (oxygenBlock.Room.OxygenLevel(base.CubeGrid.GridSize) > 1f)
                    {
                        oxygenBlock.Room.OxygenAmount = oxygenBlock.Room.MaxOxygen(base.CubeGrid.GridSize);
                    }
                    base.ResourceSink.Update();
                    this.SourceComp.SetRemainingCapacityByType(this.m_oxygenGasId, oxygenBlock.Room.OxygenAmount);
                    this.CheckForVentEffect(amount);
                }
            }
        }

        private MyModelDummy VentDummy
        {
            get
            {
                MyModelDummy dummy;
                if ((base.Model == null) || (base.Model.Dummies == null))
                {
                    return null;
                }
                base.Model.Dummies.TryGetValue("vent_001", out dummy);
                return dummy;
            }
        }

        public bool CanVent =>
            (MySession.Static.Settings.EnableOxygen && (MySession.Static.Settings.EnableOxygenPressurization && (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.IsWorking)));

        public bool CanVentToRoom =>
            (this.CanVent && !this.IsDepressurizing);

        public bool CanVentFromRoom =>
            (this.CanVent && this.IsDepressurizing);

        public float GasOutputPerSecond =>
            (this.SourceComp.ProductionEnabledByType(this.m_oxygenGasId) ? this.SourceComp.CurrentOutputByType(this.m_oxygenGasId) : 0f);

        public float GasInputPerSecond =>
            (this.IsDepressurizing ? 0f : base.ResourceSink.CurrentInputByType(this.m_oxygenGasId));

        public float GasOutputPerUpdate =>
            (this.GasOutputPerSecond * 0.01666667f);

        public float GasInputPerUpdate =>
            (this.GasInputPerSecond * 0.01666667f);

        public bool IsDepressurizing
        {
            get => 
                ((bool) this.m_isDepressurizing);
            set => 
                (this.m_isDepressurizing.Value = value);
        }

        public VentStatus Status { get; private set; }

        public MyResourceSourceComponent SourceComp
        {
            get => 
                this.m_sourceComp;
            set
            {
                if (base.Components.Contains(typeof(MyResourceSourceComponent)))
                {
                    base.Components.Remove<MyResourceSourceComponent>();
                }
                base.Components.Add<MyResourceSourceComponent>(value);
                this.m_sourceComp = value;
            }
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public bool CanPressurizeRoom =>
            true;

        private MyAirVentDefinition BlockDefinition =>
            ((MyAirVentDefinition) base.BlockDefinition);

        public bool CanPressurize
        {
            get
            {
                MyOxygenBlock oxygenBlock = this.GetOxygenBlock();
                return ((oxygenBlock != null) && ((oxygenBlock.Room != null) && oxygenBlock.Room.IsAirtight));
            }
        }

        MyResourceSinkInfo SpaceEngineers.Game.ModAPI.IMyAirVent.OxygenSinkInfo
        {
            get => 
                this.OxygenSinkInfo;
            set => 
                (this.OxygenSinkInfo = value);
        }

        MyResourceSourceComponent SpaceEngineers.Game.ModAPI.IMyAirVent.SourceComp
        {
            get => 
                this.SourceComp;
            set => 
                (this.SourceComp = value);
        }

        float SpaceEngineers.Game.ModAPI.IMyAirVent.GasOutputPerSecond =>
            this.GasOutputPerSecond;

        float SpaceEngineers.Game.ModAPI.IMyAirVent.GasInputPerSecond =>
            this.GasInputPerSecond;

        VentStatus SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent.Status =>
            this.Status;

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent.Depressurize
        {
            get => 
                this.IsDepressurizing;
            set => 
                (this.IsDepressurizing = value);
        }

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent.PressurizationEnabled =>
            MySession.Static.Settings.EnableOxygenPressurization;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAirVent.<>c <>9 = new MyAirVent.<>c();
            public static MyTerminalValueControl<MyAirVent, bool>.GetterDelegate <>9__52_0;
            public static MyTerminalValueControl<MyAirVent, bool>.SetterDelegate <>9__52_1;
            public static Action<MyAirVent> <>9__52_2;
            public static Func<MyAirVent, Action> <>9__59_0;
            public static Func<MyAirVent, Action> <>9__65_0;
            public static Func<MyAirVent, Action<ToolbarItem, int>> <>9__69_0;

            internal Action <CreateEffect>b__65_0(MyAirVent x) => 
                new Action(x.CreateVentEffectImplementation);

            internal bool <CreateTerminalControls>b__52_0(MyAirVent x) => 
                x.IsDepressurizing;

            internal void <CreateTerminalControls>b__52_1(MyAirVent x, bool v)
            {
                x.IsDepressurizing = v;
                x.UpdateEmissivity();
            }

            internal void <CreateTerminalControls>b__52_2(MyAirVent self)
            {
                if (self.m_onFullAction != null)
                {
                    self.m_actionToolbar.SetItemAtIndex(0, self.m_onFullAction);
                }
                if (self.m_onEmptyAction != null)
                {
                    self.m_actionToolbar.SetItemAtIndex(1, self.m_onEmptyAction);
                }
                self.m_actionToolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(self.Toolbar_ItemChanged);
                if (MyGuiScreenToolbarConfigBase.Static == null)
                {
                    MyToolbarComponent.CurrentToolbar = self.m_actionToolbar;
                    object[] args = new object[] { 0, self };
                    MyToolbarComponent.AutoUpdate = false;
                    MyGuiScreenBase screen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.ToolbarConfigScreen, args);
                    screen.Closed += delegate (MyGuiScreenBase source) {
                        MyToolbarComponent.AutoUpdate = true;
                        self.m_actionToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(self.Toolbar_ItemChanged);
                        self.m_actionToolbar.Clear();
                    };
                    MyGuiSandbox.AddScreen(screen);
                }
            }

            internal Action <StopVentEffect>b__59_0(MyAirVent x) => 
                new Action(x.StopVentEffectImplementation);

            internal Action<ToolbarItem, int> <Toolbar_ItemChanged>b__69_0(MyAirVent x) => 
                new Action<ToolbarItem, int>(x.SendToolbarItemChanged);
        }
    }
}

