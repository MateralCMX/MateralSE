namespace Sandbox.Game.GameSystems.Electricity
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner]
    public class MyBattery
    {
        public static float BATTERY_DEPLETION_MULTIPLIER = 1f;
        private int m_lastUpdateTime;
        private MyEntity m_lastParent;
        public const float EnergyCriticalThresholdCharacter = 0.1f;
        public const float EnergyLowThresholdCharacter = 0.25f;
        public const float EnergyCriticalThresholdShip = 0.05f;
        public const float EnergyLowThresholdShip = 0.125f;
        private const int m_productionUpdateInterval = 100;
        private readonly MyCharacter m_owner;
        private readonly MyStringHash m_resourceSinkGroup = MyStringHash.GetOrCompute("Charging");
        private readonly MyStringHash m_resourceSourceGroup = MyStringHash.GetOrCompute("Battery");
        public float RechargeMultiplier = 1f;

        public MyBattery(MyCharacter owner)
        {
            this.m_owner = owner;
            this.ResourceSink = new MyResourceSinkComponent(1);
            this.ResourceSource = new MyResourceSourceComponent(1);
        }

        public void DebugDepleteBattery()
        {
            this.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, 0f);
        }

        public MyObjectBuilder_Battery GetObjectBuilder()
        {
            MyObjectBuilder_Battery local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Battery>();
            local1.ProducerEnabled = this.ResourceSource.Enabled;
            local1.CurrentCapacity = this.ResourceSource.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId);
            return local1;
        }

        public void Init(MyObjectBuilder_Battery builder, List<MyResourceSinkInfo> additionalSinks = null, List<MyResourceSourceInfo> additionalSources = null)
        {
            MyResourceSinkInfo sinkData = new MyResourceSinkInfo {
                MaxRequiredInput = 0.0018f,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                RequiredInputFunc = new Func<float>(this.Sink_ComputeRequiredPower)
            };
            if (additionalSinks == null)
            {
                this.ResourceSink.Init(this.m_resourceSinkGroup, sinkData);
            }
            else
            {
                additionalSinks.Insert(0, sinkData);
                this.ResourceSink.Init(this.m_resourceSinkGroup, additionalSinks);
            }
            this.ResourceSink.TemporaryConnectedEntity = this.m_owner;
            MyResourceSourceInfo sourceResourceData = new MyResourceSourceInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                DefinedOutput = 0.009f,
                ProductionToCapacityMultiplier = 3600f
            };
            if (additionalSources == null)
            {
                this.ResourceSource.Init(this.m_resourceSourceGroup, sourceResourceData);
            }
            else
            {
                additionalSources.Insert(0, sourceResourceData);
                this.ResourceSource.Init(this.m_resourceSourceGroup, additionalSources);
            }
            this.ResourceSource.TemporaryConnectedEntity = this.m_owner;
            this.m_lastUpdateTime = MySession.Static.GameplayFrameCounter;
            if (builder == null)
            {
                this.ResourceSource.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, true);
                this.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, 1E-05f);
                this.ResourceSink.Update();
            }
            else
            {
                this.ResourceSource.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, builder.ProducerEnabled);
                if (MySession.Static.SurvivalMode)
                {
                    this.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, MathHelper.Clamp(builder.CurrentCapacity, 0f, 1E-05f));
                }
                else
                {
                    this.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, 1E-05f);
                }
                this.ResourceSink.Update();
            }
        }

        public float Sink_ComputeRequiredPower() => 
            Math.Min((float) (((((1E-05f - this.ResourceSource.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId)) * 60f) / 100f) * this.ResourceSource.ProductionToCapacityMultiplierByType(MyResourceDistributorComponent.ElectricityId)) + (this.ResourceSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId) * (MySession.Static.CreativeMode ? 0f : 1f))), (float) 0.0018f);

        [Event(null, 0xa8), Reliable, ServerInvoked, Broadcast]
        private static void SyncCapacitySuccess(long entityId, float remainingCapacity)
        {
            MyCharacter character;
            MyEntities.TryGetEntityById<MyCharacter>(entityId, out character, false);
            if (character != null)
            {
                character.SuitBattery.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, remainingCapacity);
            }
        }

        public void UpdateOnServer100()
        {
            if (Sync.IsServer)
            {
                MyEntity parent = this.m_owner.Parent;
                if (!ReferenceEquals(this.m_lastParent, parent))
                {
                    this.ResourceSink.Update();
                    this.m_lastParent = parent;
                }
                if (this.ResourceSource.HasCapacityRemainingByType(MyResourceDistributorComponent.ElectricityId) || (this.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) > 0f))
                {
                    float num = (MySession.Static.GameplayFrameCounter - this.m_lastUpdateTime) * 0.01666667f;
                    this.m_lastUpdateTime = MySession.Static.GameplayFrameCounter;
                    float num2 = this.ResourceSource.ProductionToCapacityMultiplierByType(MyResourceDistributorComponent.ElectricityId);
                    float num3 = this.ResourceSource.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId) / num2;
                    float num4 = MyFakes.ENABLE_BATTERY_SELF_RECHARGE ? this.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId) : ((this.RechargeMultiplier * this.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId)) / num2);
                    float num5 = MySession.Static.CreativeMode ? 0f : (num * num3);
                    float num6 = (num * num4) - num5;
                    float num7 = this.ResourceSource.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId) + num6;
                    this.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, MathHelper.Clamp(num7, 0f, 1E-05f));
                }
                if (!this.ResourceSource.HasCapacityRemainingByType(MyResourceDistributorComponent.ElectricityId))
                {
                    this.ResourceSink.Update();
                }
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, float>(s => new Action<long, float>(MyBattery.SyncCapacitySuccess), this.Owner.EntityId, this.ResourceSource.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId), targetEndpoint, position);
            }
        }

        public bool IsEnergyCriticalShip =>
            ((this.ResourceSource.RemainingCapacity / 1E-05f) < 0.05f);

        public bool IsEnergyLowShip =>
            ((this.ResourceSource.RemainingCapacity / 1E-05f) < 0.125f);

        public MyCharacter Owner =>
            this.m_owner;

        public MyResourceSinkComponent ResourceSink { get; private set; }

        public MyResourceSourceComponent ResourceSource { get; private set; }

        public bool OwnedByLocalPlayer { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyBattery.<>c <>9 = new MyBattery.<>c();
            public static Func<IMyEventOwner, Action<long, float>> <>9__34_0;

            internal Action<long, float> <UpdateOnServer100>b__34_0(IMyEventOwner s) => 
                new Action<long, float>(MyBattery.SyncCapacitySuccess);
        }
    }
}

