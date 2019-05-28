namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyCharacterOxygenComponent : MyCharacterComponent
    {
        public static readonly float LOW_OXYGEN_RATIO = 0.2f;
        public static readonly float GAS_REFILL_RATION = 0.3f;
        private Dictionary<MyDefinitionId, int> m_gasIdToIndex;
        private GasData[] m_storedGases;
        private float m_oldSuitOxygenLevel;
        private const int m_gasRefillInterval = 5;
        private int m_lastOxygenUpdateTime;
        private const int m_updateInterval = 100;
        private MyResourceSinkComponent m_characterGasSink;
        private MyResourceSourceComponent m_characterGasSource;
        public static readonly MyDefinitionId OxygenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        public static readonly MyDefinitionId HydrogenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");
        private MyEntity3DSoundEmitter m_soundEmitter;
        private MySoundPair m_helmetOpenSound = new MySoundPair("PlayHelmetOn", true);
        private MySoundPair m_helmetCloseSound = new MySoundPair("PlayHelmetOff", true);
        private MySoundPair m_helmetAirEscapeSound = new MySoundPair("PlayChokeInitiate", true);

        private void AnimateHelmet()
        {
            string str;
            string str2;
            base.Character.Definition.AnimationNameToSubtypeName.TryGetValue("HelmetOpen", out str);
            base.Character.Definition.AnimationNameToSubtypeName.TryGetValue("HelmetClose", out str2);
            if (base.Character.Definition != null)
            {
                if (this.NeedsOxygenFromSuit && (str != null))
                {
                    base.Character.PlayCharacterAnimation(str, MyBlendOption.Immediate, MyFrameOption.StayOnLastFrame, 0.2f, 1f, true, null, false);
                }
                else if (!this.NeedsOxygenFromSuit && (str2 != null))
                {
                    base.Character.PlayCharacterAnimation(str2, MyBlendOption.Immediate, MyFrameOption.StayOnLastFrame, 0.2f, 1f, true, null, false);
                }
            }
        }

        public void AppendSinkData(List<MyResourceSinkInfo> sinkData)
        {
            for (int i = 0; i < this.m_storedGases.Length; i++)
            {
                int captureIndex = i;
                MyResourceSinkInfo item = new MyResourceSinkInfo {
                    ResourceTypeId = this.m_storedGases[i].Id,
                    MaxRequiredInput = this.m_storedGases[i].Throughput,
                    RequiredInputFunc = () => this.Sink_ComputeRequiredGas(this.m_storedGases[captureIndex])
                };
                sinkData.Add(item);
            }
        }

        public void AppendSourceData(List<MyResourceSourceInfo> sourceData)
        {
            for (int i = 0; i < this.m_storedGases.Length; i++)
            {
                MyResourceSourceInfo item = new MyResourceSourceInfo {
                    ResourceTypeId = this.m_storedGases[i].Id,
                    DefinedOutput = this.m_storedGases[i].Throughput,
                    ProductionToCapacityMultiplier = 1f,
                    IsInfiniteCapacity = false
                };
                sourceData.Add(item);
            }
        }

        public bool ContainsGasStorage(MyDefinitionId gasId) => 
            this.m_gasIdToIndex.ContainsKey(gasId);

        public float GetGasFillLevel(MyDefinitionId gasId)
        {
            int typeIndex = -1;
            return (this.TryGetTypeIndex(ref gasId, out typeIndex) ? this.m_storedGases[typeIndex].FillLevel : 0f);
        }

        public virtual void GetObjectBuilder(MyObjectBuilder_Character objectBuilder)
        {
            objectBuilder.OxygenLevel = this.SuitOxygenLevel;
            objectBuilder.EnvironmentOxygenLevel = base.Character.EnvironmentOxygenLevel;
            objectBuilder.NeedsOxygenFromSuit = this.NeedsOxygenFromSuit;
            if ((this.m_storedGases != null) && (this.m_storedGases.Length != 0))
            {
                if (objectBuilder.StoredGases == null)
                {
                    objectBuilder.StoredGases = new List<MyObjectBuilder_Character.StoredGas>();
                }
                foreach (GasData storedGas in this.m_storedGases)
                {
                    if (objectBuilder.StoredGases.TrueForAll(obGas => obGas.Id != storedGas.Id))
                    {
                        MyObjectBuilder_Character.StoredGas item = new MyObjectBuilder_Character.StoredGas {
                            Id = (SerializableDefinitionId) storedGas.Id,
                            FillLevel = storedGas.FillLevel
                        };
                        objectBuilder.StoredGases.Add(item);
                    }
                }
            }
        }

        private int GetTypeIndex(ref MyDefinitionId gasId)
        {
            int num = 0;
            if (this.m_gasIdToIndex.Count > 1)
            {
                num = this.m_gasIdToIndex[gasId];
            }
            return num;
        }

        public virtual void Init(MyObjectBuilder_Character characterOb)
        {
            string str;
            string str2;
            this.m_lastOxygenUpdateTime = MySession.Static.GameplayFrameCounter;
            this.m_gasIdToIndex = new Dictionary<MyDefinitionId, int>();
            if (MyFakes.ENABLE_HYDROGEN_FUEL && (this.Definition.SuitResourceStorage != null))
            {
                this.m_storedGases = new GasData[this.Definition.SuitResourceStorage.Count];
                int index = 0;
                while (true)
                {
                    if (index >= this.m_storedGases.Length)
                    {
                        if ((characterOb.StoredGases != null) && !MySession.Static.CreativeMode)
                        {
                            foreach (MyObjectBuilder_Character.StoredGas gas in characterOb.StoredGases)
                            {
                                int num2;
                                if (this.m_gasIdToIndex.TryGetValue(gas.Id, out num2))
                                {
                                    this.m_storedGases[num2].FillLevel = gas.FillLevel;
                                }
                            }
                        }
                        break;
                    }
                    SuitResourceDefinition definition = this.Definition.SuitResourceStorage[index];
                    GasData data1 = new GasData();
                    data1.Id = definition.Id;
                    data1.FillLevel = 1f;
                    data1.MaxCapacity = definition.MaxCapacity;
                    data1.Throughput = definition.Throughput;
                    data1.LastOutputTime = MySession.Static.GameplayFrameCounter;
                    data1.LastInputTime = MySession.Static.GameplayFrameCounter;
                    this.m_storedGases[index] = data1;
                    this.m_gasIdToIndex.Add(definition.Id, index);
                    index++;
                }
            }
            if (this.m_storedGases == null)
            {
                this.m_storedGases = new GasData[0];
            }
            if (MySession.Static.Settings.EnableOxygen)
            {
                float gasFillLevel = this.GetGasFillLevel(OxygenId);
                this.m_oldSuitOxygenLevel = (gasFillLevel == 0f) ? this.OxygenCapacity : gasFillLevel;
            }
            if (Sync.IsServer)
            {
                base.Character.EnvironmentOxygenLevelSync.Value = characterOb.EnvironmentOxygenLevel;
                base.Character.OxygenLevelAtCharacterLocation.Value = 0f;
            }
            base.Character.Definition.AnimationNameToSubtypeName.TryGetValue("HelmetOpen", out str);
            base.Character.Definition.AnimationNameToSubtypeName.TryGetValue("HelmetClose", out str2);
            if (((str == null) || (str2 == null)) && (!base.Character.UseNewAnimationSystem || (base.Character.AnimationController.Controller.GetLayerByName("Helmet") == null)))
            {
                this.NeedsOxygenFromSuit = this.Definition.NeedsOxygen;
            }
            else
            {
                this.NeedsOxygenFromSuit = characterOb.NeedsOxygenFromSuit;
            }
            base.NeedsUpdateBeforeSimulation = true;
            base.NeedsUpdateBeforeSimulation100 = true;
            if (this.m_soundEmitter == null)
            {
                this.m_soundEmitter = new MyEntity3DSoundEmitter(base.Character, false, 1f);
            }
            if (!this.HelmetEnabled)
            {
                this.AnimateHelmet();
            }
        }

        private void RefillSuitGassesFromBottles()
        {
            foreach (GasData data in this.m_storedGases)
            {
                if (data.FillLevel >= GAS_REFILL_RATION)
                {
                    data.NextGasRefill = -1;
                }
                else
                {
                    if (data.NextGasRefill == -1)
                    {
                        data.NextGasRefill = MySandboxGame.TotalGamePlayTimeInMilliseconds + 0x1388;
                    }
                    if (MySandboxGame.TotalGamePlayTimeInMilliseconds >= data.NextGasRefill)
                    {
                        data.NextGasRefill = -1;
                        bool flag = false;
                        using (List<MyPhysicalInventoryItem>.Enumerator enumerator = base.Character.GetInventory(0).GetItems().GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                MyObjectBuilder_GasContainerObject content = enumerator.Current.Content as MyObjectBuilder_GasContainerObject;
                                if ((content != null) && (content.GasLevel != 0f))
                                {
                                    MyOxygenContainerDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(content) as MyOxygenContainerDefinition;
                                    if (physicalItemDefinition.StoredGasId == data.Id)
                                    {
                                        float num2 = content.GasLevel * physicalItemDefinition.Capacity;
                                        float gasInput = Math.Min(num2, (1f - data.FillLevel) * data.MaxCapacity);
                                        content.GasLevel = Math.Max((float) ((num2 - gasInput) / physicalItemDefinition.Capacity), (float) 0f);
                                        float gasLevel = content.GasLevel;
                                        base.Character.GetInventory(0).UpdateGasAmount();
                                        flag = true;
                                        this.TransferSuitGas(ref data.Id, gasInput, 0f);
                                        if (data.FillLevel == 1f)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (flag && !ReferenceEquals(MySession.Static.LocalCharacter, base.Character))
                        {
                            base.Character.SendRefillFromBottle(data.Id);
                        }
                        MyCharacterJetpackComponent jetpackComp = base.Character.JetpackComp;
                        if (((jetpackComp != null) && (jetpackComp.TurnedOn && ((jetpackComp.FuelDefinition != null) && ((jetpackComp.FuelDefinition.Id == data.Id) && (data.FillLevel <= 0f))))) && (((base.Character.ControllerInfo.Controller != null) && !MySession.Static.CreativeToolsEnabled(base.Character.ControllerInfo.Controller.Player.Id.SteamId)) || (!ReferenceEquals(MySession.Static.LocalCharacter, base.Character) && !Sync.IsServer)))
                        {
                            if (Sync.IsServer && !ReferenceEquals(MySession.Static.LocalCharacter, base.Character))
                            {
                                MyMultiplayer.RaiseEvent<MyCharacter>(base.Character, x => new Action(x.SwitchJetpack), new EndpointId(base.Character.ControllerInfo.Controller.Player.Id.SteamId));
                            }
                            jetpackComp.SwitchThrusts();
                        }
                    }
                }
            }
        }

        private void SetGasSink(MyResourceSinkComponent characterSinkComponent)
        {
            GasData[] storedGases = this.m_storedGases;
            for (int i = 0; i < storedGases.Length; i++)
            {
                storedGases[i].LastInputTime = MySession.Static.GameplayFrameCounter;
                if (Sync.IsServer)
                {
                    if (this.m_characterGasSink != null)
                    {
                        this.m_characterGasSink.CurrentInputChanged -= new MyCurrentResourceInputChangedDelegate(this.Sink_CurrentInputChanged);
                    }
                    if (characterSinkComponent != null)
                    {
                        characterSinkComponent.CurrentInputChanged += new MyCurrentResourceInputChangedDelegate(this.Sink_CurrentInputChanged);
                    }
                }
            }
            this.m_characterGasSink = characterSinkComponent;
        }

        private void SetGasSource(MyResourceSourceComponent characterSourceComponent)
        {
            foreach (GasData data in this.m_storedGases)
            {
                data.LastOutputTime = MySession.Static.GameplayFrameCounter;
                if (this.m_characterGasSource != null)
                {
                    this.m_characterGasSource.SetRemainingCapacityByType(data.Id, 0f);
                    if (Sync.IsServer)
                    {
                        this.m_characterGasSource.OutputChanged -= new MyResourceOutputChangedDelegate(this.Source_CurrentOutputChanged);
                    }
                }
                if (characterSourceComponent != null)
                {
                    characterSourceComponent.SetRemainingCapacityByType(data.Id, data.FillLevel * data.MaxCapacity);
                    characterSourceComponent.SetProductionEnabledByType(data.Id, data.FillLevel > 0f);
                    if (Sync.IsServer)
                    {
                        characterSourceComponent.OutputChanged += new MyResourceOutputChangedDelegate(this.Source_CurrentOutputChanged);
                    }
                }
            }
            this.m_characterGasSource = characterSourceComponent;
        }

        private float Sink_ComputeRequiredGas(GasData gas) => 
            Math.Min(((((1f - gas.FillLevel) * gas.MaxCapacity) + ((gas.Id == OxygenId) ? (this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier) : 0f)) / 60f) * 100f, gas.Throughput);

        private void Sink_CurrentInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            int num;
            if (this.TryGetTypeIndex(ref resourceTypeId, out num))
            {
                float num2 = (MySession.Static.GameplayFrameCounter - this.m_storedGases[num].LastInputTime) * 0.01666667f;
                this.m_storedGases[num].LastInputTime = MySession.Static.GameplayFrameCounter;
                float num3 = oldInput * num2;
                GasData data1 = this.m_storedGases[num];
                data1.NextGasTransfer += num3;
            }
        }

        private void Source_CurrentOutputChanged(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source)
        {
            int num;
            if (this.TryGetTypeIndex(ref changedResourceId, out num))
            {
                float num2 = (MySession.Static.GameplayFrameCounter - this.m_storedGases[num].LastOutputTime) * 0.01666667f;
                this.m_storedGases[num].LastOutputTime = MySession.Static.GameplayFrameCounter;
                float num3 = oldOutput * num2;
                GasData data1 = this.m_storedGases[num];
                data1.NextGasTransfer -= num3;
            }
        }

        public void SwitchHelmet()
        {
            if (((MySession.Static != null) && ((base.Character != null) && (!base.Character.IsDead && (base.Character.AnimationController != null)))) && (base.Character.AtmosphereDetectorComp != null))
            {
                string str;
                string str2;
                base.Character.Definition.AnimationNameToSubtypeName.TryGetValue("HelmetOpen", out str);
                base.Character.Definition.AnimationNameToSubtypeName.TryGetValue("HelmetClose", out str2);
                if (((str != null) && (str2 != null)) || (base.Character.UseNewAnimationSystem && (base.Character.AnimationController.Controller.GetLayerByName("Helmet") != null)))
                {
                    this.NeedsOxygenFromSuit = !this.NeedsOxygenFromSuit;
                    this.AnimateHelmet();
                }
                base.Character.SinkComp.Update();
                if (this.m_soundEmitter != null)
                {
                    bool flag = false;
                    if (this.NeedsOxygenFromSuit)
                    {
                        this.m_soundEmitter.PlaySound(this.m_helmetOpenSound, true, false, flag, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                    }
                    else
                    {
                        this.m_soundEmitter.PlaySound(this.m_helmetCloseSound, true, false, flag, false, false, new bool?(!MyFakes.FORCE_CHARACTER_2D_SOUND));
                    }
                    if ((!MySession.Static.CreativeMode && (this.NeedsOxygenFromSuit && ((base.Character.AtmosphereDetectorComp != null) && (!base.Character.AtmosphereDetectorComp.InAtmosphere && !base.Character.AtmosphereDetectorComp.InShipOrStation)))) && (this.SuitOxygenAmount >= 0.5f))
                    {
                        bool? nullable = null;
                        this.m_soundEmitter.PlaySound(this.m_helmetAirEscapeSound, false, false, flag, false, false, nullable);
                    }
                }
                if ((MyFakes.ENABLE_NEW_SOUNDS && MyFakes.ENABLE_NEW_SOUNDS_QUICK_UPDATE) && MySession.Static.Settings.RealisticSound)
                {
                    MyEntity3DSoundEmitter.UpdateEntityEmitters(true, true, false);
                }
            }
        }

        private void TransferSuitGas(ref MyDefinitionId gasId, float gasInput, float gasOutput)
        {
            int typeIndex = this.GetTypeIndex(ref gasId);
            float num2 = gasInput - gasOutput;
            if (MySession.Static.CreativeMode)
            {
                num2 = Math.Max(num2, 0f);
            }
            if (num2 != 0f)
            {
                GasData data = this.m_storedGases[typeIndex];
                data.FillLevel = MathHelper.Clamp((float) (data.FillLevel + (num2 / data.MaxCapacity)), (float) 0f, (float) 1f);
                this.CharacterGasSource.SetRemainingCapacityByType(data.Id, data.FillLevel * data.MaxCapacity);
                this.CharacterGasSource.SetProductionEnabledByType(data.Id, data.FillLevel > 0f);
            }
        }

        private bool TryGetGasData(MyDefinitionId gasId, out GasData data)
        {
            int typeIndex = -1;
            data = null;
            if (!this.TryGetTypeIndex(ref gasId, out typeIndex))
            {
                return false;
            }
            data = this.m_storedGases[typeIndex];
            return true;
        }

        private bool TryGetTypeIndex(ref MyDefinitionId gasId, out int typeIndex) => 
            this.m_gasIdToIndex.TryGetValue(gasId, out typeIndex);

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            this.UpdateOxygen();
        }

        private void UpdateGassesFillLevelsAndAmounts(MyOxygenRoom room)
        {
            foreach (GasData data in this.m_storedGases)
            {
                float num2 = (MySession.Static.GameplayFrameCounter - data.LastOutputTime) * 0.01666667f;
                float num3 = (MySession.Static.GameplayFrameCounter - data.LastInputTime) * 0.01666667f;
                data.LastOutputTime = MySession.Static.GameplayFrameCounter;
                data.LastInputTime = MySession.Static.GameplayFrameCounter;
                float num4 = this.CharacterGasSource.CurrentOutputByType(data.Id) * num2;
                float oxygenAmount = this.CharacterGasSink.CurrentInputByType(data.Id) * num3;
                if (((data.Id == OxygenId) && (MySession.Static.Settings.EnableOxygen && (this.Definition.OxygenSuitRefillTime > 0f))) && (data.FillLevel < 1f))
                {
                    float num8 = MySession.Static.Settings.EnableOxygenPressurization ? Math.Max(base.Character.EnvironmentOxygenLevel, base.Character.OxygenLevel) : base.Character.EnvironmentOxygenLevel;
                    if (num8 >= this.Definition.MinOxygenLevelForSuitRefill)
                    {
                        float num10 = data.MaxCapacity - (data.FillLevel * data.MaxCapacity);
                        oxygenAmount += MathHelper.Min(num8 * ((data.MaxCapacity / this.Definition.OxygenSuitRefillTime) * (num3 * 1000f)), num10);
                        if ((MySession.Static.Settings.EnableOxygenPressurization && (room != null)) && room.IsAirtight)
                        {
                            if (room.OxygenAmount >= oxygenAmount)
                            {
                                room.OxygenAmount -= oxygenAmount;
                            }
                            else
                            {
                                oxygenAmount = room.OxygenAmount;
                                room.OxygenAmount = 0f;
                            }
                        }
                    }
                }
                data.NextGasTransfer = 0f;
                this.TransferSuitGas(ref data.Id, oxygenAmount + MathHelper.Clamp(data.NextGasTransfer, 0f, float.PositiveInfinity), num4 + -MathHelper.Clamp(data.NextGasTransfer, float.NegativeInfinity, 0f));
            }
        }

        private void UpdateOxygen()
        {
            MyOxygenRoom room;
            List<MyEntity> result = new List<MyEntity>();
            BoundingBoxD worldAABB = base.Character.PositionComp.WorldAABB;
            bool enableOxygen = MySession.Static.Settings.EnableOxygen;
            bool noOxygenDamage = MySession.Static.Settings.EnableOxygen;
            bool isInEnvironment = true;
            bool flag4 = false;
            if (!Sync.IsServer)
            {
                goto TR_000A;
            }
            else
            {
                base.Character.EnvironmentOxygenLevelSync.Value = MyOxygenProviderSystem.GetOxygenInPoint(base.Character.PositionComp.GetPosition());
                base.Character.OxygenLevelAtCharacterLocation.Value = base.Character.EnvironmentOxygenLevel;
                room = null;
                if (!MySession.Static.Settings.EnableOxygen)
                {
                    goto TR_000C;
                }
                else
                {
                    GasData data;
                    if (this.TryGetGasData(OxygenId, out data))
                    {
                        float num = (MySession.Static.GameplayFrameCounter - data.LastOutputTime) * 0.01666667f;
                        flag4 = (this.CharacterGasSink.CurrentInputByType(OxygenId) * num) > this.Definition.OxygenConsumption;
                        if (flag4)
                        {
                            noOxygenDamage = false;
                            enableOxygen = false;
                        }
                    }
                    MyCockpit parent = base.Character.Parent as MyCockpit;
                    bool flag5 = false;
                    if ((parent != null) && parent.BlockDefinition.IsPressurized)
                    {
                        if ((!this.HelmetEnabled && (MySession.Static.SurvivalMode && !flag4)) && (parent.OxygenAmount >= (this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier)))
                        {
                            parent.OxygenAmount -= this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier;
                            noOxygenDamage = false;
                            enableOxygen = false;
                        }
                        base.Character.EnvironmentOxygenLevelSync.Value = parent.OxygenFillLevel;
                        isInEnvironment = false;
                        flag5 = true;
                    }
                    if (!flag5 || (MyFakes.ENABLE_NEW_SOUNDS && MySession.Static.Settings.RealisticSound))
                    {
                        base.Character.OxygenSourceGridEntityId.Value = 0L;
                        Vector3D center = base.Character.PositionComp.WorldAABB.Center;
                        MyGamePruningStructure.GetTopMostEntitiesInBox(ref worldAABB, result, MyEntityQueryType.Both);
                        using (List<MyEntity>.Enumerator enumerator = result.GetEnumerator())
                        {
                            while (true)
                            {
                                while (true)
                                {
                                    if (enumerator.MoveNext())
                                    {
                                        MyCubeGrid current = enumerator.Current as MyCubeGrid;
                                        if (current == null)
                                        {
                                            continue;
                                        }
                                        if (current.GridSystems.GasSystem == null)
                                        {
                                            continue;
                                        }
                                        MyOxygenBlock safeOxygenBlock = current.GridSystems.GasSystem.GetSafeOxygenBlock(center);
                                        if (safeOxygenBlock == null)
                                        {
                                            continue;
                                        }
                                        if (safeOxygenBlock.Room == null)
                                        {
                                            continue;
                                        }
                                        room = safeOxygenBlock.Room;
                                        if ((room.OxygenLevel(current.GridSize) > this.Definition.PressureLevelForLowDamage) && !this.HelmetEnabled)
                                        {
                                            enableOxygen = false;
                                        }
                                        if (!room.IsAirtight)
                                        {
                                            float environmentOxygen = room.EnvironmentOxygen;
                                            base.Character.OxygenLevelAtCharacterLocation.Value = environmentOxygen;
                                            if (flag5)
                                            {
                                                break;
                                            }
                                            base.Character.EnvironmentOxygenLevelSync.Value = environmentOxygen;
                                            if (this.HelmetEnabled)
                                            {
                                                break;
                                            }
                                            if (base.Character.EnvironmentOxygenLevelSync.Value <= (this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier))
                                            {
                                                break;
                                            }
                                            noOxygenDamage = false;
                                        }
                                        else
                                        {
                                            float num2 = room.OxygenLevel(current.GridSize);
                                            if (!flag5)
                                            {
                                                base.Character.EnvironmentOxygenLevelSync.Value = num2;
                                            }
                                            base.Character.OxygenLevelAtCharacterLocation.Value = num2;
                                            base.Character.OxygenSourceGridEntityId.Value = current.EntityId;
                                            if (room.OxygenAmount <= (this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier))
                                            {
                                                break;
                                            }
                                            if (!this.HelmetEnabled)
                                            {
                                                noOxygenDamage = false;
                                                safeOxygenBlock.PreviousOxygenAmount = safeOxygenBlock.OxygenAmount() - (this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier);
                                                safeOxygenBlock.OxygenChangeTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                                                if (!flag4)
                                                {
                                                    room.OxygenAmount -= this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier;
                                                }
                                            }
                                        }
                                        goto TR_000D;
                                    }
                                    else
                                    {
                                        goto TR_000D;
                                    }
                                    break;
                                }
                                isInEnvironment = false;
                            }
                        }
                    }
                }
            }
            goto TR_000D;
        TR_000A:
            this.CharacterGasSink.Update();
            if (Sync.IsServer && !MySession.Static.CreativeMode)
            {
                this.RefillSuitGassesFromBottles();
                if (MySession.Static.Settings.EnableOxygen)
                {
                    this.UpdateSuitOxygen(enableOxygen, noOxygenDamage, isInEnvironment);
                }
                foreach (GasData data2 in this.m_storedGases)
                {
                    base.Character.UpdateStoredGas(data2.Id, data2.FillLevel);
                }
            }
            return;
        TR_000C:
            this.UpdateGassesFillLevelsAndAmounts(room);
            goto TR_000A;
        TR_000D:
            this.m_oldSuitOxygenLevel = this.SuitOxygenLevel;
            goto TR_000C;
        }

        public void UpdateStoredGasLevel(ref MyDefinitionId gasId, float fillLevel)
        {
            int typeIndex = -1;
            if (this.TryGetTypeIndex(ref gasId, out typeIndex))
            {
                this.m_storedGases[typeIndex].FillLevel = fillLevel;
                this.CharacterGasSource.SetRemainingCapacityByType(gasId, fillLevel * this.m_storedGases[typeIndex].MaxCapacity);
                this.CharacterGasSource.SetProductionEnabledByType(gasId, fillLevel > 0f);
            }
        }

        private void UpdateSuitOxygen(bool lowOxygenDamage, bool noOxygenDamage, bool isInEnvironment)
        {
            if (noOxygenDamage | lowOxygenDamage)
            {
                if (this.HelmetEnabled && (this.SuitOxygenAmount > (this.Definition.OxygenConsumption * this.Definition.OxygenConsumptionMultiplier)))
                {
                    noOxygenDamage = false;
                    lowOxygenDamage = false;
                }
                if (isInEnvironment && !this.HelmetEnabled)
                {
                    if (base.Character.EnvironmentOxygenLevelSync.Value > this.Definition.PressureLevelForLowDamage)
                    {
                        lowOxygenDamage = false;
                    }
                    if (base.Character.EnvironmentOxygenLevelSync.Value > 0f)
                    {
                        noOxygenDamage = false;
                    }
                }
            }
            this.m_oldSuitOxygenLevel = this.SuitOxygenLevel;
            if (noOxygenDamage)
            {
                base.Character.DoDamage(this.Definition.DamageAmountAtZeroPressure, MyDamageType.LowPressure, true, 0L);
            }
            else if (lowOxygenDamage)
            {
                base.Character.DoDamage(1f, MyDamageType.Asphyxia, true, 0L);
            }
            base.Character.UpdateOxygen(this.SuitOxygenAmount);
        }

        public MyResourceSinkComponent CharacterGasSink
        {
            get => 
                this.m_characterGasSink;
            set => 
                this.SetGasSink(value);
        }

        public MyResourceSourceComponent CharacterGasSource
        {
            get => 
                this.m_characterGasSource;
            set => 
                this.SetGasSource(value);
        }

        private MyCharacterDefinition Definition =>
            base.Character.Definition;

        public float OxygenCapacity
        {
            get
            {
                int typeIndex = -1;
                MyDefinitionId oxygenId = OxygenId;
                return (this.TryGetTypeIndex(ref oxygenId, out typeIndex) ? this.m_storedGases[typeIndex].MaxCapacity : 0f);
            }
        }

        public float SuitOxygenAmount
        {
            get => 
                (this.GetGasFillLevel(OxygenId) * this.OxygenCapacity);
            set
            {
                MyDefinitionId oxygenId = OxygenId;
                this.UpdateStoredGasLevel(ref oxygenId, MyMath.Clamp(value / this.OxygenCapacity, 0f, 1f));
            }
        }

        public float SuitOxygenAmountMissing =>
            (this.OxygenCapacity - (this.GetGasFillLevel(OxygenId) * this.OxygenCapacity));

        public float SuitOxygenLevel
        {
            get => 
                ((this.OxygenCapacity != 0f) ? this.GetGasFillLevel(OxygenId) : 0f);
            set
            {
                MyDefinitionId oxygenId = OxygenId;
                this.UpdateStoredGasLevel(ref oxygenId, value);
            }
        }

        public bool HelmetEnabled =>
            !this.NeedsOxygenFromSuit;

        public bool NeedsOxygenFromSuit { get; set; }

        public override string ComponentTypeDebugString =>
            "Oxygen Component";

        public float EnvironmentOxygenLevel =>
            base.Character.EnvironmentOxygenLevel;

        public float OxygenLevelAtCharacterLocation =>
            base.Character.OxygenLevel;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCharacterOxygenComponent.<>c <>9 = new MyCharacterOxygenComponent.<>c();
            public static Func<MyCharacter, Action> <>9__52_0;

            internal Action <RefillSuitGassesFromBottles>b__52_0(MyCharacter x) => 
                new Action(x.SwitchJetpack);
        }

        private class GasData
        {
            public MyDefinitionId Id;
            public float FillLevel;
            public float MaxCapacity;
            public float Throughput;
            public float NextGasTransfer;
            public int LastOutputTime;
            public int LastInputTime;
            public int NextGasRefill = -1;

            public override string ToString() => 
                $"Subtype: {this.Id.SubtypeName}, FillLevel: {this.FillLevel}, CurrentCapacity: {(this.FillLevel * this.MaxCapacity)}, MaxCapacity: {this.MaxCapacity}";
        }
    }
}

