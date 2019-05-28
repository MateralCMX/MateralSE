namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Definitions;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Utils;

    public abstract class MyEnvironmentalPowerProducer : MyFunctionalBlock, Sandbox.ModAPI.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        protected MySoundPair m_processSound = new MySoundPair();
        private MyResourceSourceComponent m_sourceComponent;

        protected MyEnvironmentalPowerProducer()
        {
            this.SourceComp = new MyResourceSourceComponent(1);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.OnIsWorkingChanged);
        }

        protected override void Closing()
        {
            base.Closing();
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
                base.m_soundEmitter = null;
            }
        }

        protected virtual void ConstructDetailedInfo(StringBuilder sb)
        {
            float maxOutput = this.SourceComp.MaxOutput;
            float workInMegaWatts = Math.Min(maxOutput, this.SourceComp.CurrentOutput);
            sb.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            sb.Append(this.BlockDefinition.DisplayNameText);
            sb.Append('\n');
            sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxOutput));
            MyValueFormatter.AppendWorkInBestUnit(maxOutput, sb);
            sb.Append('\n');
            sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentOutput));
            MyValueFormatter.AppendWorkInBestUnit(workInMegaWatts, sb);
            sb.Append('\n');
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyResourceSourceInfo sourceResourceData = new MyResourceSourceInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                DefinedOutput = this.BlockDefinition.MaxPowerOutput,
                IsInfiniteCapacity = true,
                ProductionToCapacityMultiplier = 3600f
            };
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            this.m_processSound = this.BlockDefinition.ActionSound;
            this.SourceComp.SetMaxOutput(0f);
            base.Init(objectBuilder, cubeGrid);
        }

        protected void OnCurrentOutputChanged(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source)
        {
            if (!source.CurrentOutputByType(changedResourceId).IsEqual(oldOutput, (oldOutput * 0.1f)))
            {
                this.UpdateDisplay();
            }
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.OnProductionChanged();
        }

        private void OnIsWorkingChanged(MyCubeBlock obj)
        {
            this.OnProductionChanged();
        }

        protected virtual void OnProductionChanged()
        {
            if (base.InScene && !base.CubeGrid.IsPreview)
            {
                float newMaxOutput = this.CurrentProductionRatio * this.BlockDefinition.MaxPowerOutput;
                this.SourceComp.SetMaxOutput(newMaxOutput);
                this.SourceComp.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, newMaxOutput > 0f);
                this.UpdateDisplay();
                base.RaisePropertiesChanged();
                if ((base.m_soundEmitter != null) && (this.m_processSound != null))
                {
                    if (newMaxOutput > 0f)
                    {
                        bool? nullable = null;
                        base.m_soundEmitter.PlaySound(this.m_processSound, true, false, false, false, false, nullable);
                    }
                    else
                    {
                        base.m_soundEmitter.StopSound(true, true);
                    }
                }
            }
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

        public MyResourceSourceComponent SourceComp
        {
            get => 
                this.m_sourceComponent;
            set
            {
                if (this.m_sourceComponent != null)
                {
                    this.m_sourceComponent.OutputChanged -= new MyResourceOutputChangedDelegate(this.OnCurrentOutputChanged);
                }
                if (base.ContainsDebugRenderComponent(typeof(MyDebugRenderComponentDrawPowerSource)))
                {
                    base.RemoveDebugRenderComponent(typeof(MyDebugRenderComponentDrawPowerSource));
                }
                if (base.Components.Contains(typeof(MyResourceSourceComponent)))
                {
                    base.Components.Remove<MyResourceSourceComponent>();
                }
                base.Components.Add<MyResourceSourceComponent>(value);
                this.m_sourceComponent = value;
                if (this.m_sourceComponent != null)
                {
                    base.AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerSource(this.m_sourceComponent, this));
                    this.m_sourceComponent.OutputChanged += new MyResourceOutputChangedDelegate(this.OnCurrentOutputChanged);
                }
            }
        }

        public MyPowerProducerDefinition BlockDefinition =>
            ((MyPowerProducerDefinition) base.BlockDefinition);

        public float CurrentOutput =>
            this.SourceComp.CurrentOutput;

        public float MaxOutput =>
            this.SourceComp.MaxOutput;

        protected abstract float CurrentProductionRatio { get; }
    }
}

