namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;

    [MyCubeBlockType(typeof(MyObjectBuilder_FueledPowerProducer))]
    public abstract class MyFueledPowerProducer : MyFunctionalBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        public static float FUEL_CONSUMPTION_MULTIPLIER = 1f;
        private MyResourceSourceComponent m_sourceComponent;
        private readonly VRage.Sync.Sync<float, SyncDirection.FromServer> m_capacity;

        protected MyFueledPowerProducer()
        {
            this.SourceComp = new MyResourceSourceComponent(1);
            this.m_capacity.ValueChanged += new Action<SyncBase>(this.OnCapacityChanged);
            this.m_capacity.AlwaysReject<float, SyncDirection.FromServer>();
        }

        public virtual bool AllowSelfPulling() => 
            false;

        protected override bool CheckIsWorking()
        {
            MyResourceSourceComponent sourceComp = this.SourceComp;
            return (sourceComp.Enabled && (this.IsSupplied && (sourceComp.ProductionEnabled && base.CheckIsWorking())));
        }

        protected override void Closing()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
            base.Closing();
        }

        protected virtual float ComputeMaxProduction()
        {
            if (this.CheckIsWorking() || (MySession.Static.CreativeMode && base.CheckIsWorking()))
            {
                return this.MaxOutput;
            }
            return 0f;
        }

        protected virtual void ConstructDetailedInfo(StringBuilder sb)
        {
            sb.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            sb.Append(this.BlockDefinition.DisplayNameText);
            sb.Append('\n');
            sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxOutput));
            MyValueFormatter.AppendWorkInBestUnit(this.MaxOutput, sb);
            sb.Append('\n');
            sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentOutput));
            MyValueFormatter.AppendWorkInBestUnit(this.SourceComp.CurrentOutput, sb);
            sb.Append('\n');
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_FueledPowerProducer objectBuilderCubeBlock = (MyObjectBuilder_FueledPowerProducer) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Capacity = this.Capacity;
            return objectBuilderCubeBlock;
        }

        public virtual PullInformation GetPullInformation() => 
            null;

        public virtual PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyResourceSourceInfo sourceResourceData = new MyResourceSourceInfo {
                DefinedOutput = this.BlockDefinition.MaxPowerOutput,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                ProductionToCapacityMultiplier = this.BlockDefinition.FuelProductionToCapacityMultiplier
            };
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            this.SourceComp.Enabled = base.Enabled;
            base.Init(objectBuilder, cubeGrid);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.OnIsFunctionalChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            MyObjectBuilder_FueledPowerProducer producer = (MyObjectBuilder_FueledPowerProducer) objectBuilder;
            this.m_capacity.SetLocalValue(producer.Capacity);
        }

        public void InitializeConveyorEndpoint()
        {
            this.ConveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.ConveyorEndpoint));
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
        }

        private void OnCapacityChanged(SyncBase obj)
        {
            this.SourceComp.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, this.Capacity);
            base.UpdateIsWorking();
            this.OnProductionChanged();
        }

        protected virtual void OnCurrentOrMaxOutputChanged(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source)
        {
            if (!source.CurrentOutputByType(changedResourceId).IsEqual(oldOutput, (oldOutput * 0.1f)))
            {
                this.UpdateDisplay();
            }
        }

        protected override void OnEnabledChanged()
        {
            this.SourceComp.Enabled = base.Enabled;
            base.OnEnabledChanged();
            base.UpdateIsWorking();
            this.OnProductionChanged();
        }

        private void OnIsFunctionalChanged()
        {
            this.OnProductionChanged();
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
            else
            {
                this.OnStopWorking();
            }
        }

        protected virtual void OnProductionChanged()
        {
            float newMaxOutput = 0f;
            if ((base.Enabled && base.IsFunctional) && this.IsSupplied)
            {
                newMaxOutput = this.ComputeMaxProduction();
            }
            this.SourceComp.SetMaxOutput(newMaxOutput);
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            this.OnProductionChanged();
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            this.OnProductionChanged();
        }

        protected void UpdateDisplay()
        {
            this.ConstructDetailedInfo(base.DetailedInfo.Clear());
            base.RaisePropertiesChanged();
        }

        public MyFueledPowerProducerDefinition BlockDefinition =>
            ((MyFueledPowerProducerDefinition) base.BlockDefinition);

        public MyResourceSourceComponent SourceComp
        {
            get => 
                this.m_sourceComponent;
            set
            {
                if (this.m_sourceComponent != null)
                {
                    this.m_sourceComponent.OutputChanged -= new MyResourceOutputChangedDelegate(this.OnCurrentOrMaxOutputChanged);
                    this.m_sourceComponent.MaxOutputChanged -= new MyResourceOutputChangedDelegate(this.OnCurrentOrMaxOutputChanged);
                }
                if (base.ContainsDebugRenderComponent(typeof(MyDebugRenderComponentDrawPowerSource)))
                {
                    base.RemoveDebugRenderComponent(typeof(MyDebugRenderComponentDrawPowerSource));
                }
                this.m_sourceComponent = value;
                MyEntityComponentContainer components = base.Components;
                components.Remove<MyResourceSourceComponent>();
                components.Add<MyResourceSourceComponent>(value);
                if (this.m_sourceComponent != null)
                {
                    base.AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerSource(this.m_sourceComponent, this));
                    this.m_sourceComponent.OutputChanged += new MyResourceOutputChangedDelegate(this.OnCurrentOrMaxOutputChanged);
                    this.m_sourceComponent.MaxOutputChanged += new MyResourceOutputChangedDelegate(this.OnCurrentOrMaxOutputChanged);
                }
            }
        }

        public float Capacity
        {
            get => 
                this.m_capacity.Value;
            set => 
                (this.m_capacity.Value = Math.Max(value, 0f));
        }

        public bool IsSupplied =>
            (this.Capacity > 0f);

        public float CurrentOutput =>
            this.SourceComp.CurrentOutput;

        public virtual float MaxOutput =>
            this.BlockDefinition.MaxPowerOutput;

        public IMyConveyorEndpoint ConveyorEndpoint { get; private set; }
    }
}

