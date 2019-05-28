namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGasFueledPowerProducer : MyFueledPowerProducer, IMyGasTank
    {
        private MyResourceSinkComponent m_sinkComponent;
        private bool m_needsUpdate;

        protected MyGasFueledPowerProducer()
        {
        }

        protected override void ConstructDetailedInfo(StringBuilder sb)
        {
            base.ConstructDetailedInfo(sb);
            float fuelCapacity = this.BlockDefinition.FuelCapacity;
            float num2 = Math.Min(base.Capacity, fuelCapacity);
            float num3 = (num2 / fuelCapacity) * 100f;
            sb.Append(string.Format(MyTexts.GetString(MySpaceTexts.Oxygen_Filled), num3.ToString("F1"), num2.ToString("0"), fuelCapacity.ToString("0")));
        }

        private void DisableUpdate()
        {
            if (this.m_needsUpdate)
            {
                this.m_needsUpdate = false;
                if (!base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
            }
        }

        private float GetFillingOffset()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            float fuelCapacity = this.BlockDefinition.FuelCapacity;
            return MathHelper.Clamp((float) (fuelCapacity - base.Capacity), (float) 0f, (float) (fuelCapacity / 20f));
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyGasFueledPowerProducerDefinition.FuelInfo fuel = this.BlockDefinition.Fuel;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            MyResourceSinkInfo sinkData = new MyResourceSinkInfo {
                ResourceTypeId = fuel.FuelId,
                MaxRequiredInput = (fuel.Ratio * this.BlockDefinition.FuelProductionToCapacityMultiplier) * 2f
            };
            component.Init(this.BlockDefinition.ResourceSinkGroup, sinkData);
            this.SinkComp = component;
            base.Init(objectBuilder, cubeGrid);
            base.SourceComp.OutputChanged += new MyResourceOutputChangedDelegate(this.OnElectricityOutputChanged);
            this.MarkForUpdate();
        }

        private void MarkForUpdate()
        {
            if (!this.m_needsUpdate)
            {
                this.m_needsUpdate = true;
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void OnElectricityOutputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSourceComponent source)
        {
            this.MarkForUpdate();
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            if (base.Enabled)
            {
                this.MarkForUpdate();
            }
            this.CheckEmissiveState(false);
        }

        private void OnFuelInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            this.MarkForUpdate();
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            this.MarkForUpdate();
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            if (!base.Enabled || !base.IsFunctional)
            {
                foreach (MyDefinitionId id in this.SinkComp.AcceptedResources)
                {
                    this.SinkComp.SetRequiredInputByType(id, 0f);
                }
            }
            this.DisableUpdate();
            this.CheckEmissiveState(false);
        }

        bool IMyGasTank.IsResourceStorage(MyDefinitionId resourceDefinition)
        {
            using (List<MyDefinitionId>.Enumerator enumerator = this.SinkComp.AcceptedResources.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current == resourceDefinition)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool SetEmissiveStateDisabled()
        {
            if (!base.Enabled)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);
            }
            MyStringHash state = base.IsSupplied ? MyCubeBlock.m_emissiveNames.Disabled : MyCubeBlock.m_emissiveNames.Warning;
            return base.SetEmissiveState(state, base.Render.RenderObjectIDs[0], null);
        }

        public override bool SetEmissiveStateWorking()
        {
            MyStringHash state = base.IsSupplied ? MyCubeBlock.m_emissiveNames.Working : MyCubeBlock.m_emissiveNames.Warning;
            return base.SetEmissiveState(state, base.Render.RenderObjectIDs[0], null);
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (this.m_needsUpdate)
            {
                this.UpdateCapacity();
                bool isWorking = base.IsWorking;
                if (base.IsWorking != isWorking)
                {
                    if (isWorking)
                    {
                        this.OnStartWorking();
                    }
                    else
                    {
                        this.OnStopWorking();
                    }
                }
            }
        }

        private void UpdateCapacity()
        {
            MyGasFueledPowerProducerDefinition blockDefinition = this.BlockDefinition;
            MyDefinitionId fuelId = blockDefinition.Fuel.FuelId;
            float num = (base.SourceComp.CurrentOutput / blockDefinition.FuelProductionToCapacityMultiplier) / 60f;
            float num2 = (this.SinkComp.CurrentInputByType(fuelId) / 60f) / MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER;
            if ((num2 == 0f) && MySession.Static.CreativeMode)
            {
                num2 = num + this.GetFillingOffset();
            }
            bool flag = (num2 == 0f) && (this.SinkComp.RequiredInputByType(fuelId) > 0f);
            float num3 = num2 - num;
            bool flag1 = !(num3 == 0f);
            if (flag1)
            {
                if (Sync.IsServer)
                {
                    base.Capacity += num3;
                }
                base.UpdateDisplay();
            }
            float fillingOffset = this.GetFillingOffset();
            if (!flag1 && (flag || (fillingOffset == 0f)))
            {
                this.DisableUpdate();
            }
            this.SinkComp.SetRequiredInputByType(fuelId, (num + (fillingOffset * MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER)) * 60f);
            this.CheckEmissiveState(false);
        }

        public MyGasFueledPowerProducerDefinition BlockDefinition =>
            ((MyGasFueledPowerProducerDefinition) base.BlockDefinition);

        public MyResourceSinkComponent SinkComp
        {
            get => 
                this.m_sinkComponent;
            set
            {
                if (this.m_sinkComponent != null)
                {
                    this.m_sinkComponent.CurrentInputChanged -= new MyCurrentResourceInputChangedDelegate(this.OnFuelInputChanged);
                }
                this.m_sinkComponent = value;
                MyEntityComponentContainer components = base.Components;
                components.Remove<MyResourceSinkComponent>();
                components.Add<MyResourceSinkComponent>(value);
                if (this.m_sinkComponent != null)
                {
                    this.m_sinkComponent.CurrentInputChanged += new MyCurrentResourceInputChangedDelegate(this.OnFuelInputChanged);
                }
            }
        }

        public double FilledRatio =>
            ((double) (base.Capacity / this.BlockDefinition.FuelCapacity));

        public float GasCapacity =>
            this.BlockDefinition.FuelCapacity;
    }
}

