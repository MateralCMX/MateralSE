namespace Sandbox.Game.EntityComponents
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyResourceDistributorComponent : MyEntityComponentBase
    {
        public static readonly MyDefinitionId ElectricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        public static readonly MyDefinitionId HydrogenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");
        public static readonly MyDefinitionId OxygenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        private int m_typeGroupCount;
        private bool m_forceRecalculation;
        private readonly List<PerTypeData> m_dataPerType = new List<PerTypeData>();
        private readonly HashSet<MyDefinitionId> m_initializedTypes = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
        private readonly Dictionary<MyDefinitionId, int> m_typeIdToIndex = new Dictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);
        private readonly Dictionary<MyDefinitionId, bool> m_typeIdToConveyorConnectionRequired = new Dictionary<MyDefinitionId, bool>(MyDefinitionId.Comparer);
        private readonly HashSet<MyDefinitionId> m_typesToRemove = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
        private readonly MyConcurrentHashSet<MyResourceSinkComponent> m_sinksToAdd = new MyConcurrentHashSet<MyResourceSinkComponent>();
        private readonly MyConcurrentHashSet<MyTuple<MyResourceSinkComponent, bool>> m_sinksToRemove = new MyConcurrentHashSet<MyTuple<MyResourceSinkComponent, bool>>();
        private readonly MyConcurrentHashSet<MyResourceSourceComponent> m_sourcesToAdd = new MyConcurrentHashSet<MyResourceSourceComponent>();
        private readonly MyConcurrentHashSet<MyResourceSourceComponent> m_sourcesToRemove = new MyConcurrentHashSet<MyResourceSourceComponent>();
        private readonly MyConcurrentDictionary<MyDefinitionId, int> m_changedTypes = new MyConcurrentDictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);
        private readonly List<string> m_changesDebug = new List<string>();
        public static bool ShowTrace = false;
        public string DebugName;
        private static int m_typeGroupCountTotal = -1;
        private static int m_sinkGroupPrioritiesTotal = -1;
        private static int m_sourceGroupPrioritiesTotal = -1;
        private static readonly Dictionary<MyDefinitionId, int> m_typeIdToIndexTotal = new Dictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);
        private static readonly Dictionary<MyDefinitionId, bool> m_typeIdToConveyorConnectionRequiredTotal = new Dictionary<MyDefinitionId, bool>(MyDefinitionId.Comparer);
        private static readonly Dictionary<MyStringHash, int> m_sourceSubtypeToPriority = new Dictionary<MyStringHash, int>(MyStringHash.Comparer);
        private static readonly Dictionary<MyStringHash, int> m_sinkSubtypeToPriority = new Dictionary<MyStringHash, int>(MyStringHash.Comparer);
        private static readonly Dictionary<MyStringHash, bool> m_sinkSubtypeToAdaptability = new Dictionary<MyStringHash, bool>(MyStringHash.Comparer);
        public Action<bool> OnPowerGenerationChanged;
        private MyResourceStateEnum m_electricityState;
        private bool m_updateInProgress;
        private bool m_recomputeInProgress;

        public MyResourceDistributorComponent(string debugName)
        {
            InitializeMappings();
            this.DebugName = debugName;
            this.m_changesDebug.Clear();
        }

        public void AddSink(MyResourceSinkComponent sink)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS)
            {
                this.m_changesDebug.Add($"+Sink: {(sink.Entity != null) ? sink.Entity.ToString() : sink.Group.ToString()}");
            }
            MyTuple<MyResourceSinkComponent, bool> tuple = new MyTuple<MyResourceSinkComponent, bool>();
            MyConcurrentHashSet<MyTuple<MyResourceSinkComponent, bool>> sinksToRemove = this.m_sinksToRemove;
            lock (sinksToRemove)
            {
                foreach (MyTuple<MyResourceSinkComponent, bool> tuple2 in this.m_sinksToRemove)
                {
                    if (tuple2.Item1 == sink)
                    {
                        tuple = tuple2;
                        break;
                    }
                }
                if (tuple.Item1 != null)
                {
                    this.m_sinksToRemove.Remove(tuple);
                    this.RemoveTypesFromChanges(tuple.Item1.AcceptedResources);
                    return;
                }
            }
            MyConcurrentHashSet<MyResourceSinkComponent> sinksToAdd = this.m_sinksToAdd;
            lock (sinksToAdd)
            {
                this.m_sinksToAdd.Add(sink);
            }
            foreach (MyDefinitionId id in sink.AcceptedResources)
            {
                int num;
                if (!this.m_changedTypes.TryGetValue(id, out num))
                {
                    this.m_changedTypes.Add(id, 1);
                    continue;
                }
                this.m_changedTypes[id] = num + 1;
            }
        }

        private void AddSinkLazy(MyResourceSinkComponent sink)
        {
            foreach (MyDefinitionId id in sink.AcceptedResources)
            {
                if (!this.m_initializedTypes.Contains(id))
                {
                    this.InitializeNewType(ref id);
                }
                HashSet<MyResourceSinkComponent> sinksOfType = this.GetSinksOfType(ref id, sink.Group);
                if (sinksOfType != null)
                {
                    int typeIndex = this.GetTypeIndex(ref id);
                    PerTypeData data = this.m_dataPerType[typeIndex];
                    MyResourceSourceComponent item = null;
                    if (sink.Container != null)
                    {
                        foreach (HashSet<MyResourceSourceComponent> set2 in data.SourcesByPriority)
                        {
                            foreach (MyResourceSourceComponent component2 in set2)
                            {
                                if ((component2.Container != null) && (component2.Container.Get<MyResourceSinkComponent>() == sink))
                                {
                                    data.InputOutputList.Add(MyTuple.Create<MyResourceSinkComponent, MyResourceSourceComponent>(sink, component2));
                                    item = component2;
                                    break;
                                }
                            }
                            if (item != null)
                            {
                                set2.Remove(item);
                                data.InvalidateGridForUpdateCache();
                                break;
                            }
                        }
                    }
                    if (item == null)
                    {
                        sinksOfType.Add(sink);
                        data.InvalidateGridForUpdateCache();
                    }
                    data.NeedsRecompute = true;
                    data.GroupsDirty = true;
                    data.RemainingFuelTimeDirty = true;
                }
            }
            sink.RequiredInputChanged += new MyRequiredResourceChangeDelegate(this.Sink_RequiredInputChanged);
            sink.ResourceAvailable += new MyResourceAvailableDelegate(this.Sink_IsResourceAvailable);
            sink.OnAddType += new Action<MyResourceSinkComponent, MyDefinitionId>(this.Sink_OnAddType);
            sink.OnRemoveType += new Action<MyResourceSinkComponent, MyDefinitionId>(this.Sink_OnRemoveType);
        }

        public void AddSource(MyResourceSourceComponent source)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS)
            {
                string text1;
                if (source.Entity == null)
                {
                    text1 = source.Group.ToString();
                }
                else
                {
                    text1 = source.Entity.ToString();
                }
                this.m_changesDebug.Add($"+Source: {text1}");
            }
            MyConcurrentHashSet<MyResourceSourceComponent> sourcesToRemove = this.m_sourcesToRemove;
            lock (sourcesToRemove)
            {
                if (this.m_sourcesToRemove.Contains(source))
                {
                    this.m_sourcesToRemove.Remove(source);
                    this.RemoveTypesFromChanges(source.ResourceTypes);
                    return;
                }
            }
            sourcesToRemove = this.m_sourcesToAdd;
            lock (sourcesToRemove)
            {
                this.m_sourcesToAdd.Add(source);
            }
            foreach (MyDefinitionId id in source.ResourceTypes)
            {
                int num;
                if (!this.m_changedTypes.TryGetValue(id, out num))
                {
                    this.m_changedTypes.Add(id, 1);
                    continue;
                }
                this.m_changedTypes[id] = num + 1;
            }
        }

        private void AddSourceLazy(MyResourceSourceComponent source)
        {
            foreach (MyDefinitionId id in source.ResourceTypes)
            {
                if (!this.m_initializedTypes.Contains(id))
                {
                    this.InitializeNewType(ref id);
                }
                HashSet<MyResourceSourceComponent> sourcesOfType = this.GetSourcesOfType(ref id, source.Group);
                if (sourcesOfType != null)
                {
                    int typeIndex = this.GetTypeIndex(ref id);
                    PerTypeData data = this.m_dataPerType[typeIndex];
                    MyResourceSinkComponent item = null;
                    if (source.Container != null)
                    {
                        foreach (HashSet<MyResourceSinkComponent> set2 in data.SinksByPriority)
                        {
                            foreach (MyResourceSinkComponent component2 in set2)
                            {
                                if ((component2.Container != null) && (component2.Container.Get<MyResourceSourceComponent>() == source))
                                {
                                    data.InputOutputList.Add(MyTuple.Create<MyResourceSinkComponent, MyResourceSourceComponent>(component2, source));
                                    item = component2;
                                    break;
                                }
                            }
                            if (item != null)
                            {
                                set2.Remove(item);
                                data.InvalidateGridForUpdateCache();
                                break;
                            }
                        }
                    }
                    if (item == null)
                    {
                        sourcesOfType.Add(source);
                        data.InvalidateGridForUpdateCache();
                    }
                    data.NeedsRecompute = true;
                    data.GroupsDirty = true;
                    data.SourceCount++;
                    if (data.SourceCount == 1)
                    {
                        data.SourcesEnabled = source.Enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
                    }
                    else if (((data.SourcesEnabled == MyMultipleEnabledEnum.AllEnabled) && !source.Enabled) || ((data.SourcesEnabled == MyMultipleEnabledEnum.AllDisabled) && source.Enabled))
                    {
                        data.SourcesEnabled = MyMultipleEnabledEnum.Mixed;
                    }
                    data.RemainingFuelTimeDirty = true;
                }
            }
            source.HasCapacityRemainingChanged += new MyResourceCapacityRemainingChangedDelegate(this.source_HasRemainingCapacityChanged);
            source.MaxOutputChanged += new MyResourceOutputChangedDelegate(this.source_MaxOutputChanged);
            source.ProductionEnabledChanged += new MyResourceCapacityRemainingChangedDelegate(this.source_ProductionEnabledChanged);
        }

        public void ChangeSourcesState(MyDefinitionId resourceTypeId, MyMultipleEnabledEnum state, long playerId)
        {
            int num;
            if ((!this.m_recomputeInProgress && this.TryGetTypeIndex(resourceTypeId, out num)) && ((this.m_dataPerType[num].SourcesEnabled != state) && (this.m_dataPerType[num].SourcesEnabled != MyMultipleEnabledEnum.NoObjects)))
            {
                AdminSettingsEnum enum2;
                this.m_recomputeInProgress = true;
                this.m_dataPerType[num].SourcesEnabled = state;
                bool newValue = state == MyMultipleEnabledEnum.AllEnabled;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerId);
                bool flag2 = MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals);
                if (MySession.Static.RemoteAdminSettings.TryGetValue(MySession.Static.Players.TryGetSteamId(playerId), out enum2))
                {
                    flag2 |= enum2.HasFlag(AdminSettingsEnum.UseTerminals);
                }
                HashSet<MyResourceSourceComponent>[] sourcesByPriority = this.m_dataPerType[num].SourcesByPriority;
                for (int i = 0; i < sourcesByPriority.Length; i++)
                {
                    foreach (MyResourceSourceComponent component in sourcesByPriority[i])
                    {
                        if ((!flag2 && (playerId >= 0L)) && (component.Entity != null))
                        {
                            MyFunctionalBlock entity = component.Entity as MyFunctionalBlock;
                            if (((entity != null) && (entity.OwnerId != 0)) && (entity.OwnerId != playerId))
                            {
                                MyOwnershipShareModeEnum shareMode = entity.IDModule.ShareMode;
                                IMyFaction faction2 = MySession.Static.Factions.TryGetPlayerFaction(entity.OwnerId);
                                if (shareMode == MyOwnershipShareModeEnum.None)
                                {
                                    continue;
                                }
                                if (shareMode == MyOwnershipShareModeEnum.Faction)
                                {
                                    if (faction == null)
                                    {
                                        continue;
                                    }
                                    if ((faction2 != null) && (faction.FactionId != faction2.FactionId))
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        component.MaxOutputChanged -= new MyResourceOutputChangedDelegate(this.source_MaxOutputChanged);
                        component.SetEnabled(newValue, false);
                        component.MaxOutputChanged += new MyResourceOutputChangedDelegate(this.source_MaxOutputChanged);
                    }
                }
                using (List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>>.Enumerator enumerator2 = this.m_dataPerType[num].InputOutputList.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        MyResourceSourceComponent component2 = enumerator2.Current.Item2;
                        if ((!flag2 && (playerId >= 0L)) && (component2.Entity != null))
                        {
                            MyFunctionalBlock entity = component2.Entity as MyFunctionalBlock;
                            if (((entity != null) && (entity.OwnerId != 0)) && (entity.OwnerId != playerId))
                            {
                                MyOwnershipShareModeEnum shareMode = entity.IDModule.ShareMode;
                                IMyFaction faction3 = MySession.Static.Factions.TryGetPlayerFaction(entity.OwnerId);
                                if (shareMode == MyOwnershipShareModeEnum.None)
                                {
                                    continue;
                                }
                                if (shareMode == MyOwnershipShareModeEnum.Faction)
                                {
                                    if (faction == null)
                                    {
                                        continue;
                                    }
                                    if ((faction3 != null) && (faction.FactionId != faction3.FactionId))
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        component2.MaxOutputChanged -= new MyResourceOutputChangedDelegate(this.source_MaxOutputChanged);
                        component2.SetEnabled(newValue, false);
                        component2.MaxOutputChanged += new MyResourceOutputChangedDelegate(this.source_MaxOutputChanged);
                    }
                }
                this.m_dataPerType[num].SourcesEnabledDirty = false;
                this.m_dataPerType[num].NeedsRecompute = true;
                this.m_recomputeInProgress = false;
            }
        }

        private void CheckDistributionSystemChanges()
        {
            ListReader<MyDefinitionId> acceptedResources;
            MyConcurrentHashSet<MyResourceSourceComponent> sourcesToRemove;
            if (this.m_sinksToRemove.Count > 0)
            {
                MyConcurrentHashSet<MyTuple<MyResourceSinkComponent, bool>> sinksToRemove = this.m_sinksToRemove;
                lock (sinksToRemove)
                {
                    MyTuple<MyResourceSinkComponent, bool>[] tupleArray = this.m_sinksToRemove.ToArray<MyTuple<MyResourceSinkComponent, bool>>();
                    int index = 0;
                    while (true)
                    {
                        if (index >= tupleArray.Length)
                        {
                            this.m_sinksToRemove.Clear();
                            break;
                        }
                        MyTuple<MyResourceSinkComponent, bool> tuple = tupleArray[index];
                        this.RemoveSinkLazy(tuple.Item1, tuple.Item2);
                        acceptedResources = tuple.Item1.AcceptedResources;
                        foreach (MyDefinitionId id in acceptedResources)
                        {
                            this.m_changedTypes[id] = Math.Max(0, this.m_changedTypes[id] - 1);
                        }
                        index++;
                    }
                }
            }
            if (this.m_sourcesToRemove.Count > 0)
            {
                sourcesToRemove = this.m_sourcesToRemove;
                lock (sourcesToRemove)
                {
                    foreach (MyResourceSourceComponent component in this.m_sourcesToRemove)
                    {
                        this.RemoveSourceLazy(component);
                        acceptedResources = component.ResourceTypes;
                        foreach (MyDefinitionId id2 in acceptedResources)
                        {
                            this.m_changedTypes[id2] = Math.Max(0, this.m_changedTypes[id2] - 1);
                        }
                    }
                    this.m_sourcesToRemove.Clear();
                }
            }
            if (this.m_sourcesToAdd.Count > 0)
            {
                sourcesToRemove = this.m_sourcesToAdd;
                lock (sourcesToRemove)
                {
                    foreach (MyResourceSourceComponent component2 in this.m_sourcesToAdd)
                    {
                        this.AddSourceLazy(component2);
                        acceptedResources = component2.ResourceTypes;
                        foreach (MyDefinitionId id3 in acceptedResources)
                        {
                            this.m_changedTypes[id3] = Math.Max(0, this.m_changedTypes[id3] - 1);
                        }
                    }
                    this.m_sourcesToAdd.Clear();
                }
            }
            if (this.m_sinksToAdd.Count > 0)
            {
                MyConcurrentHashSet<MyResourceSinkComponent> sinksToAdd = this.m_sinksToAdd;
                lock (sinksToAdd)
                {
                    foreach (MyResourceSinkComponent component3 in this.m_sinksToAdd)
                    {
                        this.AddSinkLazy(component3);
                        acceptedResources = component3.AcceptedResources;
                        foreach (MyDefinitionId id4 in acceptedResources)
                        {
                            this.m_changedTypes[id4] = Math.Max(0, this.m_changedTypes[id4] - 1);
                        }
                    }
                    this.m_sinksToAdd.Clear();
                }
            }
        }

        private static unsafe void ComputeInitialDistributionData(ref MyDefinitionId typeId, MySinkGroupData[] sinkDataByPriority, MySourceGroupData[] sourceDataByPriority, ref MyTuple<MySinkGroupData, MySourceGroupData> sinkSourceData, HashSet<MyResourceSinkComponent>[] sinksByPriority, HashSet<MyResourceSourceComponent>[] sourcesByPriority, List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> sinkSourcePairs, List<int> stockpilingStorageList, List<int> otherStorageList, out float maxAvailableResource)
        {
            maxAvailableResource = 0f;
            for (int i = 0; i < sourceDataByPriority.Length; i++)
            {
                MySourceGroupData data = sourceDataByPriority[i];
                data.MaxAvailableResource = 0f;
                foreach (MyResourceSourceComponent component in sourcesByPriority[i])
                {
                    if (!component.Enabled)
                    {
                        continue;
                    }
                    if (component.HasCapacityRemainingByType(typeId))
                    {
                        float* singlePtr1 = (float*) ref data.MaxAvailableResource;
                        singlePtr1[0] += component.MaxOutputByType(typeId);
                        data.InfiniteCapacity = component.IsInfiniteCapacity;
                    }
                }
                maxAvailableResource += data.MaxAvailableResource;
                sourceDataByPriority[i] = data;
            }
            float num = 0f;
            for (int j = 0; j < sinksByPriority.Length; j++)
            {
                float num4 = 0f;
                bool flag = true;
                foreach (MyResourceSinkComponent component2 in sinksByPriority[j])
                {
                    num4 += component2.RequiredInputByType(typeId);
                    flag = flag && IsAdaptible(component2);
                }
                sinkDataByPriority[j].RequiredInput = num4;
                sinkDataByPriority[j].IsAdaptible = flag;
                sinkDataByPriority[j].RequiredInputCumulative = num + num4;
            }
            PrepareSinkSourceData(ref typeId, ref sinkSourceData, sinkSourcePairs, stockpilingStorageList, otherStorageList);
            maxAvailableResource += sinkSourceData.Item2.MaxAvailableResource;
        }

        private float ComputeRemainingFuelTime(MyDefinitionId resourceTypeId)
        {
            float positiveInfinity;
            try
            {
                float num2;
                int typeIndex = this.GetTypeIndex(ref resourceTypeId);
                if (this.m_dataPerType[typeIndex].MaxAvailableResource != 0f)
                {
                    num2 = 0f;
                    foreach (MySinkGroupData data in this.m_dataPerType[typeIndex].SinkDataByPriority)
                    {
                        if (data.RemainingAvailableResource >= data.RequiredInput)
                        {
                            num2 += data.RequiredInput;
                        }
                        else
                        {
                            if (!data.IsAdaptible)
                            {
                                break;
                            }
                            num2 += data.RemainingAvailableResource;
                        }
                    }
                }
                else
                {
                    return 0f;
                }
                num2 = (this.m_dataPerType[typeIndex].InputOutputData.Item1.RemainingAvailableResource <= this.m_dataPerType[typeIndex].InputOutputData.Item1.RequiredInput) ? (num2 + this.m_dataPerType[typeIndex].InputOutputData.Item1.RemainingAvailableResource) : (num2 + this.m_dataPerType[typeIndex].InputOutputData.Item1.RequiredInput);
                bool flag = false;
                bool flag2 = false;
                float num3 = 0f;
                int index = 0;
                while (true)
                {
                    if (index >= this.m_dataPerType[typeIndex].SourcesByPriority.Length)
                    {
                        if (this.m_dataPerType[typeIndex].InputOutputData.Item2.UsageRatio > 0f)
                        {
                            foreach (MyTuple<MyResourceSinkComponent, MyResourceSourceComponent> tuple in this.m_dataPerType[typeIndex].InputOutputList)
                            {
                                if (!tuple.Item2.Enabled)
                                {
                                    continue;
                                }
                                if (tuple.Item2.ProductionEnabledByType(resourceTypeId))
                                {
                                    flag2 = true;
                                    num3 += tuple.Item2.RemainingCapacityByType(resourceTypeId);
                                }
                            }
                        }
                        if (flag && !flag2)
                        {
                            positiveInfinity = float.PositiveInfinity;
                        }
                        else
                        {
                            float num4 = 0f;
                            if (num2 > 0f)
                            {
                                num4 = num3 / num2;
                            }
                            positiveInfinity = num4;
                        }
                        break;
                    }
                    MySourceGroupData data2 = this.m_dataPerType[typeIndex].SourceDataByPriority[index];
                    if (data2.UsageRatio > 0f)
                    {
                        if (data2.InfiniteCapacity)
                        {
                            flag = true;
                            num2 -= data2.UsageRatio * data2.MaxAvailableResource;
                        }
                        else
                        {
                            foreach (MyResourceSourceComponent component in this.m_dataPerType[typeIndex].SourcesByPriority[index])
                            {
                                if (!component.Enabled)
                                {
                                    continue;
                                }
                                if (component.ProductionEnabledByType(resourceTypeId))
                                {
                                    flag2 = true;
                                    num3 += component.RemainingCapacityByType(resourceTypeId);
                                }
                            }
                        }
                    }
                    index++;
                }
            }
            finally
            {
            }
            return positiveInfinity;
        }

        public void ConveyorSystem_OnPoweredChanged()
        {
            MySandboxGame.Static.Invoke(delegate {
                for (int i = 0; i < this.m_dataPerType.Count; i++)
                {
                    this.m_dataPerType[i].GroupsDirty = true;
                    this.m_dataPerType[i].NeedsRecompute = true;
                    this.m_dataPerType[i].RemainingFuelTimeDirty = true;
                    this.m_dataPerType[i].SourcesEnabledDirty = true;
                }
            }, "ConveyorSystem_OnPoweredChanged");
        }

        internal void CubeGrid_OnFatBlockAddedOrRemoved(MyCubeBlock fatblock)
        {
            IMyConveyorSegmentBlock block = fatblock as IMyConveyorSegmentBlock;
            if ((fatblock is IMyConveyorEndpointBlock) || (block != null))
            {
                foreach (PerTypeData local1 in this.m_dataPerType)
                {
                    local1.GroupsDirty = true;
                    local1.NeedsRecompute = true;
                }
            }
        }

        public void DebugDraw(VRage.Game.Entity.MyEntity entity)
        {
            // Invalid method body.
        }

        private void DebugDrawResource(MyDefinitionId resourceId, Vector3D origin, Vector3D rightVector, float textSize)
        {
            Vector3D vectord = (Vector3D) (0.05000000074505806 * rightVector);
            Vector3D worldCoord = (origin + vectord) + (rightVector * 0.014999999664723873);
            int num = 0;
            string subtypeName = resourceId.SubtypeName;
            if (this.m_typeIdToIndex.TryGetValue(resourceId, out num))
            {
                PerTypeData data = this.m_dataPerType[num];
                int num2 = 0;
                HashSet<MyResourceSinkComponent>[] sinksByPriority = data.SinksByPriority;
                int index = 0;
                while (true)
                {
                    if (index >= sinksByPriority.Length)
                    {
                        subtypeName = $"{resourceId.SubtypeName} Sources:{data.SourceCount} Sinks:{num2} Available:{data.MaxAvailableResource} State:{data.ResourceState}";
                        break;
                    }
                    HashSet<MyResourceSinkComponent> set = sinksByPriority[index];
                    num2 += set.Count;
                    index++;
                }
            }
            MyRenderProxy.DebugDrawLine3D(origin, origin + vectord, Color.White, Color.White, false, false);
            MyRenderProxy.DebugDrawText3D(worldCoord, subtypeName, Color.White, textSize, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
        }

        private MyResourceSourceComponent GetFirstSourceOfType(ref MyDefinitionId resourceTypeId)
        {
            int typeIndex = this.GetTypeIndex(ref resourceTypeId);
            for (int i = 0; i < this.m_dataPerType[typeIndex].SourcesByPriority.Length; i++)
            {
                HashSet<MyResourceSourceComponent> hashset = this.m_dataPerType[typeIndex].SourcesByPriority[i];
                if (hashset.Count > 0)
                {
                    return hashset.FirstElement<MyResourceSourceComponent>();
                }
            }
            return null;
        }

        internal static int GetPriority(MyResourceSinkComponent sink) => 
            m_sinkSubtypeToPriority[sink.Group];

        internal static int GetPriority(MyResourceSourceComponent source) => 
            m_sourceSubtypeToPriority[source.Group];

        private HashSet<MyResourceSinkComponent> GetSinksOfType(ref MyDefinitionId typeId, MyStringHash groupType)
        {
            int num;
            int num2;
            if (!this.TryGetTypeIndex((MyDefinitionId) typeId, out num) || !m_sinkSubtypeToPriority.TryGetValue(groupType, out num2))
            {
                return null;
            }
            return this.m_dataPerType[num].SinksByPriority[num2];
        }

        public int GetSourceCount(MyDefinitionId resourceTypeId, MyStringHash sourceGroupType)
        {
            int num2;
            int num = 0;
            if (!this.TryGetTypeIndex(ref resourceTypeId, out num2))
            {
                return 0;
            }
            int index = m_sourceSubtypeToPriority[sourceGroupType];
            foreach (MyTuple<MyResourceSinkComponent, MyResourceSourceComponent> tuple in this.m_dataPerType[num2].InputOutputList)
            {
                if (!(tuple.Item2.Group == sourceGroupType))
                {
                    continue;
                }
                if (tuple.Item2.CurrentOutputByType(resourceTypeId) > 0f)
                {
                    num++;
                }
            }
            return (this.m_dataPerType[num2].SourceDataByPriority[index].ActiveCount + num);
        }

        private HashSet<MyResourceSourceComponent> GetSourcesOfType(ref MyDefinitionId typeId, MyStringHash groupType)
        {
            int num;
            int num2;
            if (!this.TryGetTypeIndex((MyDefinitionId) typeId, out num) || !m_sourceSubtypeToPriority.TryGetValue(groupType, out num2))
            {
                return null;
            }
            return this.m_dataPerType[num].SourcesByPriority[num2];
        }

        private int GetTypeIndex(ref MyDefinitionId typeId)
        {
            int num = 0;
            if (this.m_typeGroupCount > 1)
            {
                num = this.m_typeIdToIndex[typeId];
            }
            return num;
        }

        private static int GetTypeIndexTotal(ref MyDefinitionId typeId)
        {
            int num = 0;
            if (m_typeGroupCountTotal > 1)
            {
                num = m_typeIdToIndexTotal[typeId];
            }
            return num;
        }

        internal static void InitializeMappings()
        {
            Dictionary<MyDefinitionId, int> typeIdToIndexTotal = m_typeIdToIndexTotal;
            lock (typeIdToIndexTotal)
            {
                if ((m_sinkGroupPrioritiesTotal < 0) && (m_sourceGroupPrioritiesTotal < 0))
                {
                    ListReader<MyResourceDistributionGroupDefinition> definitionsOfType = MyDefinitionManager.Static.GetDefinitionsOfType<MyResourceDistributionGroupDefinition>();
                    if (definitionsOfType.Count > 0)
                    {
                        m_sinkGroupPrioritiesTotal = 0;
                        m_sourceGroupPrioritiesTotal = 0;
                    }
                    foreach (MyResourceDistributionGroupDefinition definition in from def in definitionsOfType
                        orderby def.Priority
                        select def)
                    {
                        if (definition.IsSource)
                        {
                            m_sourceGroupPrioritiesTotal++;
                            m_sourceSubtypeToPriority.Add(definition.Id.SubtypeId, m_sourceGroupPrioritiesTotal);
                            continue;
                        }
                        m_sinkGroupPrioritiesTotal++;
                        m_sinkSubtypeToPriority.Add(definition.Id.SubtypeId, m_sinkGroupPrioritiesTotal);
                        m_sinkSubtypeToAdaptability.Add(definition.Id.SubtypeId, definition.IsAdaptible);
                    }
                    m_sinkGroupPrioritiesTotal = Math.Max(m_sinkGroupPrioritiesTotal, 1);
                    m_sourceGroupPrioritiesTotal = Math.Max(m_sourceGroupPrioritiesTotal, 1);
                    m_sinkSubtypeToPriority.Add(MyStringHash.NullOrEmpty, m_sinkGroupPrioritiesTotal - 1);
                    m_sinkSubtypeToAdaptability.Add(MyStringHash.NullOrEmpty, false);
                    m_sourceSubtypeToPriority.Add(MyStringHash.NullOrEmpty, m_sourceGroupPrioritiesTotal - 1);
                    m_typeGroupCountTotal = 0;
                    m_typeGroupCountTotal++;
                    m_typeIdToIndexTotal.Add(ElectricityId, m_typeGroupCountTotal);
                    m_typeIdToConveyorConnectionRequiredTotal.Add(ElectricityId, false);
                    foreach (MyGasProperties properties in MyDefinitionManager.Static.GetDefinitionsOfType<MyGasProperties>())
                    {
                        m_typeGroupCountTotal++;
                        m_typeIdToIndexTotal.Add(properties.Id, m_typeGroupCountTotal);
                        m_typeIdToConveyorConnectionRequiredTotal.Add(properties.Id, true);
                    }
                }
            }
        }

        private void InitializeNewType(ref MyDefinitionId typeId)
        {
            int typeGroupCount = this.m_typeGroupCount;
            this.m_typeGroupCount = typeGroupCount + 1;
            this.m_typeIdToIndex.Add(typeId, typeGroupCount);
            this.m_typeIdToConveyorConnectionRequired.Add(typeId, IsConveyorConnectionRequiredTotal(ref typeId));
            HashSet<MyResourceSinkComponent>[] setArray = new HashSet<MyResourceSinkComponent>[m_sinkGroupPrioritiesTotal];
            HashSet<MyResourceSourceComponent>[] setArray2 = new HashSet<MyResourceSourceComponent>[m_sourceGroupPrioritiesTotal];
            List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> list = new List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>>();
            for (int i = 0; i < setArray.Length; i++)
            {
                setArray[i] = new HashSet<MyResourceSinkComponent>();
            }
            for (int j = 0; j < setArray2.Length; j++)
            {
                setArray2[j] = new HashSet<MyResourceSourceComponent>();
            }
            List<MyPhysicalDistributionGroup> list2 = null;
            int num = 0;
            MySinkGroupData[] dataArray = null;
            MySourceGroupData[] dataArray2 = null;
            List<int> list3 = null;
            List<int> list4 = null;
            if (this.IsConveyorConnectionRequired(ref typeId))
            {
                list2 = new List<MyPhysicalDistributionGroup>();
            }
            else
            {
                dataArray = new MySinkGroupData[m_sinkGroupPrioritiesTotal];
                dataArray2 = new MySourceGroupData[m_sourceGroupPrioritiesTotal];
                list3 = new List<int>();
                list4 = new List<int>();
            }
            PerTypeData item = new PerTypeData();
            item.TypeId = typeId;
            item.SinkDataByPriority = dataArray;
            item.SourceDataByPriority = dataArray2;
            item.InputOutputData = new MyTuple<MySinkGroupData, MySourceGroupData>();
            item.SinksByPriority = setArray;
            item.SourcesByPriority = setArray2;
            item.InputOutputList = list;
            item.StockpilingStorageIndices = list3;
            item.OtherStorageIndices = list4;
            item.DistributionGroups = list2;
            item.DistributionGroupsInUse = num;
            item.NeedsRecompute = false;
            item.GroupsDirty = true;
            item.SourceCount = 0;
            item.RemainingFuelTime = 0f;
            item.RemainingFuelTimeDirty = true;
            item.MaxAvailableResource = 0f;
            item.SourcesEnabled = MyMultipleEnabledEnum.NoObjects;
            item.SourcesEnabledDirty = true;
            item.ResourceState = MyResourceStateEnum.NoPower;
            this.m_dataPerType.Add(item);
            this.m_initializedTypes.Add(typeId);
        }

        private static bool IsAdaptible(MyResourceSinkComponent sink) => 
            m_sinkSubtypeToAdaptability[sink.Group];

        private bool IsConveyorConnectionRequired(ref MyDefinitionId typeId) => 
            this.m_typeIdToConveyorConnectionRequired[typeId];

        public static bool IsConveyorConnectionRequiredTotal(MyDefinitionId typeId) => 
            IsConveyorConnectionRequiredTotal(ref typeId);

        public static bool IsConveyorConnectionRequiredTotal(ref MyDefinitionId typeId) => 
            m_typeIdToConveyorConnectionRequiredTotal[typeId];

        public void MarkForUpdate()
        {
            this.m_forceRecalculation = true;
        }

        private bool MatchesAdaptability(HashSet<MyResourceSinkComponent> group, MyResourceSinkComponent referenceSink)
        {
            bool flag = IsAdaptible(referenceSink);
            using (HashSet<MyResourceSinkComponent>.Enumerator enumerator = group.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (IsAdaptible(enumerator.Current) != flag)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool MatchesInfiniteCapacity(HashSet<MyResourceSourceComponent> group, MyResourceSourceComponent producer)
        {
            using (HashSet<MyResourceSourceComponent>.Enumerator enumerator = group.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyResourceSourceComponent current = enumerator.Current;
                    if (producer.IsInfiniteCapacity != current.IsInfiniteCapacity)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public float MaxAvailableResourceByType(MyDefinitionId resourceTypeId)
        {
            int num;
            return (this.TryGetTypeIndex(ref resourceTypeId, out num) ? this.m_dataPerType[num].MaxAvailableResource : 0f);
        }

        private bool NeedsRecompute(ref MyDefinitionId typeId)
        {
            int num2;
            if (!this.m_changedTypes.TryGetValue(typeId, out num2) || (num2 <= 0))
            {
                int num;
                return (this.TryGetTypeIndex(ref typeId, out num) && this.m_dataPerType[num].NeedsRecompute);
            }
            return true;
        }

        private bool NeedsRecompute(ref MyDefinitionId typeId, int typeIndex)
        {
            if (((this.m_typeGroupCount <= 0) || ((typeIndex < 0) || (this.m_dataPerType.Count <= typeIndex))) || !this.m_dataPerType[typeIndex].NeedsRecompute)
            {
                int num;
                return (this.m_changedTypes.TryGetValue(typeId, out num) && (num > 0));
            }
            return true;
        }

        private bool PowerStateIsOk(MyResourceStateEnum state) => 
            (state == MyResourceStateEnum.Ok);

        private bool PowerStateWorks(MyResourceStateEnum state) => 
            (state != MyResourceStateEnum.NoPower);

        private static unsafe void PrepareSinkSourceData(ref MyDefinitionId typeId, ref MyTuple<MySinkGroupData, MySourceGroupData> sinkSourceData, List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> sinkSourcePairs, List<int> stockpilingStorageList, List<int> otherStorageList)
        {
            stockpilingStorageList.Clear();
            otherStorageList.Clear();
            sinkSourceData.Item1.IsAdaptible = true;
            sinkSourceData.Item1.RequiredInput = 0f;
            sinkSourceData.Item1.RequiredInputCumulative = 0f;
            sinkSourceData.Item2.MaxAvailableResource = 0f;
            for (int i = 0; i < sinkSourcePairs.Count; i++)
            {
                int num1;
                MyTuple<MyResourceSinkComponent, MyResourceSourceComponent> tuple = sinkSourcePairs[i];
                bool flag = tuple.Item2.ProductionEnabledByType(typeId);
                if (!tuple.Item2.Enabled || flag)
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (tuple.Item1.RequiredInputByType(typeId) > 0f);
                }
                sinkSourceData.Item1.IsAdaptible = sinkSourceData.Item1.IsAdaptible && IsAdaptible(tuple.Item1);
                float* singlePtr1 = (float*) ref sinkSourceData.Item1.RequiredInput;
                singlePtr1[0] += tuple.Item1.RequiredInputByType(typeId);
                int local1 = num1;
                if (local1 != 0)
                {
                    float* singlePtr2 = (float*) ref sinkSourceData.Item1.RequiredInputCumulative;
                    singlePtr2[0] += tuple.Item1.RequiredInputByType(typeId);
                }
                sinkSourceData.Item2.InfiniteCapacity = float.IsInfinity(tuple.Item2.RemainingCapacityByType(typeId));
                if (local1 != 0)
                {
                    stockpilingStorageList.Add(i);
                }
                else
                {
                    otherStorageList.Add(i);
                    if (tuple.Item2.Enabled & flag)
                    {
                        float* singlePtr3 = (float*) ref sinkSourceData.Item2.MaxAvailableResource;
                        singlePtr3[0] += tuple.Item2.MaxOutputByType(typeId);
                    }
                }
            }
        }

        private unsafe void RecomputeResourceDistribution(ref MyDefinitionId typeId, bool updateChanges = true)
        {
            if (!this.m_recomputeInProgress)
            {
                this.m_recomputeInProgress = true;
                if (updateChanges && !this.m_updateInProgress)
                {
                    this.m_updateInProgress = true;
                    this.CheckDistributionSystemChanges();
                    this.m_updateInProgress = false;
                }
                int typeIndex = this.GetTypeIndex(ref typeId);
                if (((this.m_dataPerType[typeIndex].SinksByPriority.Length == 0) && (this.m_dataPerType[typeIndex].SourcesByPriority.Length == 0)) && (this.m_dataPerType[typeIndex].InputOutputList.Count == 0))
                {
                    this.m_typesToRemove.Add(typeId);
                    this.m_recomputeInProgress = false;
                }
                else
                {
                    if (!this.IsConveyorConnectionRequired(ref typeId))
                    {
                        ComputeInitialDistributionData(ref typeId, this.m_dataPerType[typeIndex].SinkDataByPriority, this.m_dataPerType[typeIndex].SourceDataByPriority, ref this.m_dataPerType[typeIndex].InputOutputData, this.m_dataPerType[typeIndex].SinksByPriority, this.m_dataPerType[typeIndex].SourcesByPriority, this.m_dataPerType[typeIndex].InputOutputList, this.m_dataPerType[typeIndex].StockpilingStorageIndices, this.m_dataPerType[typeIndex].OtherStorageIndices, out this.m_dataPerType[typeIndex].MaxAvailableResource);
                        this.m_dataPerType[typeIndex].ResourceState = RecomputeResourceDistributionPartial(ref typeId, 0, this.m_dataPerType[typeIndex].SinkDataByPriority, this.m_dataPerType[typeIndex].SourceDataByPriority, ref this.m_dataPerType[typeIndex].InputOutputData, this.m_dataPerType[typeIndex].SinksByPriority, this.m_dataPerType[typeIndex].SourcesByPriority, this.m_dataPerType[typeIndex].InputOutputList, this.m_dataPerType[typeIndex].StockpilingStorageIndices, this.m_dataPerType[typeIndex].OtherStorageIndices, this.m_dataPerType[typeIndex].MaxAvailableResource);
                    }
                    else
                    {
                        if (this.m_dataPerType[typeIndex].GroupsDirty)
                        {
                            this.m_dataPerType[typeIndex].GroupsDirty = false;
                            this.m_dataPerType[typeIndex].DistributionGroupsInUse = 0;
                            this.RecreatePhysicalDistributionGroups(ref typeId, this.m_dataPerType[typeIndex].SinksByPriority, this.m_dataPerType[typeIndex].SourcesByPriority, this.m_dataPerType[typeIndex].InputOutputList);
                        }
                        this.m_dataPerType[typeIndex].MaxAvailableResource = 0f;
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 >= this.m_dataPerType[typeIndex].DistributionGroupsInUse)
                            {
                                MyResourceStateEnum noPower;
                                if (this.m_dataPerType[typeIndex].MaxAvailableResource == 0f)
                                {
                                    noPower = MyResourceStateEnum.NoPower;
                                }
                                else
                                {
                                    noPower = MyResourceStateEnum.Ok;
                                    int num3 = 0;
                                    while (num3 < this.m_dataPerType[typeIndex].DistributionGroupsInUse)
                                    {
                                        if (this.m_dataPerType[typeIndex].DistributionGroups[num3].ResourceState == MyResourceStateEnum.OverloadAdaptible)
                                        {
                                            noPower = MyResourceStateEnum.OverloadAdaptible;
                                        }
                                        else
                                        {
                                            if (this.m_dataPerType[typeIndex].DistributionGroups[num3].ResourceState != MyResourceStateEnum.OverloadBlackout)
                                            {
                                                num3++;
                                                continue;
                                            }
                                            noPower = MyResourceStateEnum.OverloadAdaptible;
                                        }
                                        break;
                                    }
                                }
                                this.m_dataPerType[typeIndex].ResourceState = noPower;
                                break;
                            }
                            MyPhysicalDistributionGroup group = this.m_dataPerType[typeIndex].DistributionGroups[num2];
                            ComputeInitialDistributionData(ref typeId, group.SinkDataByPriority, group.SourceDataByPriority, ref group.InputOutputData, group.SinksByPriority, group.SourcesByPriority, group.SinkSourcePairs, group.StockpilingStorage, group.OtherStorage, out group.MaxAvailableResources);
                            MyPhysicalDistributionGroup* groupPtr1 = (MyPhysicalDistributionGroup*) ref group;
                            groupPtr1->ResourceState = RecomputeResourceDistributionPartial(ref typeId, 0, group.SinkDataByPriority, group.SourceDataByPriority, ref group.InputOutputData, group.SinksByPriority, group.SourcesByPriority, group.SinkSourcePairs, group.StockpilingStorage, group.OtherStorage, group.MaxAvailableResources);
                            PerTypeData local1 = this.m_dataPerType[typeIndex];
                            local1.MaxAvailableResource += group.MaxAvailableResources;
                            this.m_dataPerType[typeIndex].DistributionGroups[num2] = group;
                            num2++;
                        }
                    }
                    this.m_dataPerType[typeIndex].NeedsRecompute = false;
                    this.m_recomputeInProgress = false;
                }
            }
        }

        private static unsafe MyResourceStateEnum RecomputeResourceDistributionPartial(ref MyDefinitionId typeId, int startPriorityIdx, MySinkGroupData[] sinkDataByPriority, MySourceGroupData[] sourceDataByPriority, ref MyTuple<MySinkGroupData, MySourceGroupData> sinkSourceData, HashSet<MyResourceSinkComponent>[] sinksByPriority, HashSet<MyResourceSourceComponent>[] sourcesByPriority, List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> sinkSourcePairs, List<int> stockpilingStorageList, List<int> otherStorageList, float availableResource)
        {
            MyResourceStateEnum noPower;
            MyResourceSinkComponent[] componentArray;
            int num13;
            float num = availableResource;
            int index = startPriorityIdx;
            while (index < sinksByPriority.Length)
            {
                sinkDataByPriority[index].RemainingAvailableResource = availableResource;
                if (sinkDataByPriority[index].RequiredInput <= availableResource)
                {
                    availableResource -= sinkDataByPriority[index].RequiredInput;
                    componentArray = sinksByPriority[index].ToArray<MyResourceSinkComponent>();
                    num13 = 0;
                    while (num13 < componentArray.Length)
                    {
                        MyResourceSinkComponent component = componentArray[num13];
                        component.SetInputFromDistributor(typeId, component.RequiredInputByType(typeId), sinkDataByPriority[index].IsAdaptible, true);
                        num13++;
                    }
                }
                else if (!sinkDataByPriority[index].IsAdaptible || (availableResource <= 0f))
                {
                    componentArray = sinksByPriority[index].ToArray<MyResourceSinkComponent>();
                    num13 = 0;
                    while (true)
                    {
                        if (num13 >= componentArray.Length)
                        {
                            sinkDataByPriority[index].RemainingAvailableResource = availableResource;
                            break;
                        }
                        componentArray[num13].SetInputFromDistributor(typeId, 0f, sinkDataByPriority[index].IsAdaptible, true);
                        num13++;
                    }
                }
                else
                {
                    componentArray = sinksByPriority[index].ToArray<MyResourceSinkComponent>();
                    num13 = 0;
                    while (true)
                    {
                        if (num13 >= componentArray.Length)
                        {
                            availableResource = 0f;
                            break;
                        }
                        MyResourceSinkComponent component1 = componentArray[num13];
                        float num14 = component1.RequiredInputByType(typeId) / sinkDataByPriority[index].RequiredInput;
                        component1.SetInputFromDistributor(typeId, num14 * availableResource, true, true);
                        num13++;
                    }
                }
                index++;
            }
            while (index < sinkDataByPriority.Length)
            {
                sinkDataByPriority[index].RemainingAvailableResource = 0f;
                componentArray = sinksByPriority[index].ToArray<MyResourceSinkComponent>();
                num13 = 0;
                while (true)
                {
                    if (num13 >= componentArray.Length)
                    {
                        index++;
                        break;
                    }
                    componentArray[num13].SetInputFromDistributor(typeId, 0f, sinkDataByPriority[index].IsAdaptible, true);
                    num13++;
                }
            }
            float num3 = (num - availableResource) + ((startPriorityIdx != 0) ? (sinkDataByPriority[0].RemainingAvailableResource - sinkDataByPriority[startPriorityIdx].RemainingAvailableResource) : 0f);
            float num4 = Math.Max((float) (num - num3), (float) 0f);
            float num5 = num4;
            if (stockpilingStorageList.Count > 0)
            {
                int[] numArray;
                float requiredInputCumulative = sinkSourceData.Item1.RequiredInputCumulative;
                if (requiredInputCumulative <= num5)
                {
                    num5 -= requiredInputCumulative;
                    numArray = stockpilingStorageList.ToArray();
                    num13 = 0;
                    while (true)
                    {
                        if (num13 >= numArray.Length)
                        {
                            sinkSourceData.Item1.RemainingAvailableResource = num5;
                            break;
                        }
                        int num16 = numArray[num13];
                        MyResourceSinkComponent component2 = sinkSourcePairs[num16].Item1;
                        component2.SetInputFromDistributor(typeId, component2.RequiredInputByType(typeId), true, true);
                        num13++;
                    }
                }
                else
                {
                    numArray = stockpilingStorageList.ToArray();
                    num13 = 0;
                    while (true)
                    {
                        if (num13 >= numArray.Length)
                        {
                            num5 = 0f;
                            sinkSourceData.Item1.RemainingAvailableResource = num5;
                            break;
                        }
                        int num17 = numArray[num13];
                        float num18 = sinkSourcePairs[num17].Item1.RequiredInputByType(typeId) / requiredInputCumulative;
                        sinkSourcePairs[num17].Item1.SetInputFromDistributor(typeId, num18 * num4, true, true);
                        num13++;
                    }
                }
            }
            float num6 = num4 - num5;
            float num7 = Math.Max((float) (((num - (sinkSourceData.Item2.MaxAvailableResource - (sinkSourceData.Item2.MaxAvailableResource * sinkSourceData.Item2.UsageRatio))) - num3) - num6), (float) 0f);
            float num8 = num7;
            if (otherStorageList.Count > 0)
            {
                float num19 = sinkSourceData.Item1.RequiredInput - sinkSourceData.Item1.RequiredInputCumulative;
                if (num19 <= num8)
                {
                    num8 -= num19;
                    int num20 = 0;
                    while (true)
                    {
                        if (num20 >= otherStorageList.Count)
                        {
                            sinkSourceData.Item1.RemainingAvailableResource = num8;
                            break;
                        }
                        int num21 = otherStorageList[num20];
                        MyResourceSinkComponent component3 = sinkSourcePairs[num21].Item1;
                        component3.SetInputFromDistributor(typeId, component3.RequiredInputByType(typeId), true, true);
                        num20++;
                    }
                }
                else
                {
                    int num22 = 0;
                    while (true)
                    {
                        if (num22 >= otherStorageList.Count)
                        {
                            num8 = 0f;
                            sinkSourceData.Item1.RemainingAvailableResource = num8;
                            break;
                        }
                        int num23 = otherStorageList[num22];
                        float num24 = sinkSourcePairs[num23].Item1.RequiredInputByType(typeId) / num19;
                        sinkSourcePairs[num23].Item1.SetInputFromDistributor(typeId, num24 * num8, true, true);
                        num22++;
                    }
                }
            }
            float num9 = num7 - num8;
            float num10 = num6 + num3;
            if (sinkSourceData.Item2.MaxAvailableResource <= 0f)
            {
                sinkSourceData.Item2.UsageRatio = 0f;
            }
            else
            {
                float num25 = num10;
                sinkSourceData.Item2.UsageRatio = Math.Min((float) 1f, (float) (num25 / sinkSourceData.Item2.MaxAvailableResource));
                num10 -= Math.Min(num25, sinkSourceData.Item2.MaxAvailableResource);
            }
            num6 = num4 - num5;
            num8 = Math.Max((float) (((num - (sinkSourceData.Item2.MaxAvailableResource - (sinkSourceData.Item2.MaxAvailableResource * sinkSourceData.Item2.UsageRatio))) - num3) - num6), (float) 0f);
            if (otherStorageList.Count > 0)
            {
                float num26 = sinkSourceData.Item1.RequiredInput - sinkSourceData.Item1.RequiredInputCumulative;
                if (num26 <= num8)
                {
                    num8 -= num26;
                    int num27 = 0;
                    while (true)
                    {
                        if (num27 >= otherStorageList.Count)
                        {
                            sinkSourceData.Item1.RemainingAvailableResource = num8;
                            break;
                        }
                        int num28 = otherStorageList[num27];
                        MyResourceSinkComponent component4 = sinkSourcePairs[num28].Item1;
                        component4.SetInputFromDistributor(typeId, component4.RequiredInputByType(typeId), true, true);
                        num27++;
                    }
                }
                else
                {
                    int num29 = 0;
                    while (true)
                    {
                        if (num29 >= otherStorageList.Count)
                        {
                            num8 = 0f;
                            sinkSourceData.Item1.RemainingAvailableResource = num8;
                            break;
                        }
                        int num30 = otherStorageList[num29];
                        float num31 = sinkSourcePairs[num30].Item1.RequiredInputByType(typeId) / num26;
                        sinkSourcePairs[num30].Item1.SetInputFromDistributor(typeId, num31 * num8, true, true);
                        num29++;
                    }
                }
            }
            sinkSourceData.Item2.ActiveCount = 0;
            for (int i = 0; i < otherStorageList.Count; i++)
            {
                int num33 = otherStorageList[i];
                MyResourceSourceComponent component5 = sinkSourcePairs[num33].Item2;
                if ((component5.Enabled && component5.ProductionEnabledByType(typeId)) && component5.HasCapacityRemainingByType(typeId))
                {
                    int* numPtr1 = (int*) ref sinkSourceData.Item2.ActiveCount;
                    numPtr1[0]++;
                    component5.SetOutputByType(typeId, sinkSourceData.Item2.UsageRatio * component5.MaxOutputByType(typeId));
                }
            }
            int num11 = 0;
            float num12 = num10 + num9;
            while (num11 < sourcesByPriority.Length)
            {
                if (sourceDataByPriority[num11].MaxAvailableResource <= 0f)
                {
                    sourceDataByPriority[num11].UsageRatio = 0f;
                }
                else
                {
                    float num34 = Math.Max(num12, 0f);
                    sourceDataByPriority[num11].UsageRatio = Math.Min((float) 1f, (float) (num34 / sourceDataByPriority[num11].MaxAvailableResource));
                    num12 -= Math.Min(num34, sourceDataByPriority[num11].MaxAvailableResource);
                }
                sourceDataByPriority[num11].ActiveCount = 0;
                MyResourceSourceComponent[] componentArray2 = sourcesByPriority[num11].ToArray<MyResourceSourceComponent>();
                num13 = 0;
                while (true)
                {
                    if (num13 >= componentArray2.Length)
                    {
                        num11++;
                        break;
                    }
                    MyResourceSourceComponent component6 = componentArray2[num13];
                    if (component6.Enabled && component6.HasCapacityRemainingByType(typeId))
                    {
                        int* numPtr2 = (int*) ref sourceDataByPriority[num11].ActiveCount;
                        numPtr2[0]++;
                        component6.SetOutputByType(typeId, sourceDataByPriority[num11].UsageRatio * component6.MaxOutputByType(typeId));
                    }
                    num13++;
                }
            }
            if (num == 0f)
            {
                noPower = MyResourceStateEnum.NoPower;
            }
            else if (sinkDataByPriority[m_sinkGroupPrioritiesTotal - 1].RequiredInputCumulative <= num)
            {
                noPower = MyResourceStateEnum.Ok;
            }
            else
            {
                MySinkGroupData data = sinkDataByPriority.Last<MySinkGroupData>();
                if (!data.IsAdaptible || (data.RemainingAvailableResource == 0f))
                {
                    noPower = MyResourceStateEnum.OverloadBlackout;
                }
                else
                {
                    noPower = MyResourceStateEnum.OverloadAdaptible;
                }
            }
            return noPower;
        }

        private void RecreatePhysicalDistributionGroups(ref MyDefinitionId typeId, HashSet<MyResourceSinkComponent>[] allSinksByPriority, HashSet<MyResourceSourceComponent>[] allSourcesByPriority, List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> allSinkSources)
        {
            int num;
            HashSet<MyResourceSinkComponent>[] setArray = allSinksByPriority;
            for (num = 0; num < setArray.Length; num++)
            {
                foreach (MyResourceSinkComponent component in setArray[num])
                {
                    if (component.Entity != null)
                    {
                        this.SetEntityGroup(ref typeId, component.Entity);
                        continue;
                    }
                    if (component.TemporaryConnectedEntity != null)
                    {
                        this.SetEntityGroupForTempConnected(ref typeId, component);
                    }
                }
            }
            HashSet<MyResourceSourceComponent>[] setArray2 = allSourcesByPriority;
            for (num = 0; num < setArray2.Length; num++)
            {
                foreach (MyResourceSourceComponent component2 in setArray2[num])
                {
                    if (component2.Entity != null)
                    {
                        this.SetEntityGroup(ref typeId, component2.Entity);
                        continue;
                    }
                    if (component2.TemporaryConnectedEntity != null)
                    {
                        this.SetEntityGroupForTempConnected(ref typeId, component2);
                    }
                }
            }
            foreach (MyTuple<MyResourceSinkComponent, MyResourceSourceComponent> tuple in allSinkSources)
            {
                if (tuple.Item1.Entity != null)
                {
                    this.SetEntityGroup(ref typeId, tuple.Item1.Entity);
                }
            }
        }

        private void RefreshSourcesEnabled(MyDefinitionId resourceTypeId)
        {
            int num;
            if (!this.TryGetTypeIndex(ref resourceTypeId, out num))
            {
                return;
            }
            this.m_dataPerType[num].SourcesEnabledDirty = false;
            if (this.m_dataPerType[num].SourceCount == 0)
            {
                this.m_dataPerType[num].SourcesEnabled = MyMultipleEnabledEnum.NoObjects;
                return;
            }
            bool flag = true;
            bool flag2 = true;
            HashSet<MyResourceSourceComponent>[] sourcesByPriority = this.m_dataPerType[num].SourcesByPriority;
            int index = 0;
            goto TR_0018;
        TR_000D:
            index++;
        TR_0018:
            while (true)
            {
                if (index < sourcesByPriority.Length)
                {
                    using (HashSet<MyResourceSourceComponent>.Enumerator enumerator = sourcesByPriority[index].GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                MyResourceSourceComponent current = enumerator.Current;
                                flag = flag && current.Enabled;
                                flag2 = flag2 && !current.Enabled;
                                if (flag)
                                {
                                    continue;
                                }
                                if (flag2)
                                {
                                    continue;
                                }
                                this.m_dataPerType[num].SourcesEnabled = MyMultipleEnabledEnum.Mixed;
                            }
                            else
                            {
                                goto TR_000D;
                            }
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    using (List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>>.Enumerator enumerator2 = this.m_dataPerType[num].InputOutputList.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator2.MoveNext())
                            {
                                break;
                            }
                            MyTuple<MyResourceSinkComponent, MyResourceSourceComponent> current = enumerator2.Current;
                            flag = flag && current.Item2.Enabled;
                            flag2 = flag2 && !current.Item2.Enabled;
                            if (!flag && !flag2)
                            {
                                this.m_dataPerType[num].SourcesEnabled = MyMultipleEnabledEnum.Mixed;
                                break;
                            }
                        }
                    }
                    this.m_dataPerType[num].SourcesEnabled = flag2 ? MyMultipleEnabledEnum.AllDisabled : MyMultipleEnabledEnum.AllEnabled;
                    break;
                }
                goto TR_000D;
            }
        }

        public float RemainingFuelTimeByType(MyDefinitionId resourceTypeId)
        {
            int num;
            if (!this.TryGetTypeIndex(ref resourceTypeId, out num))
            {
                return 0f;
            }
            if (this.m_dataPerType[num].RemainingFuelTimeDirty)
            {
                this.m_dataPerType[num].RemainingFuelTime = this.ComputeRemainingFuelTime(resourceTypeId);
            }
            return this.m_dataPerType[num].RemainingFuelTime;
        }

        public void RemoveSink(MyResourceSinkComponent sink, bool resetSinkInput = true, bool markedForClose = false)
        {
            if (!markedForClose)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS)
                {
                    this.m_changesDebug.Add($"-Sink: {(sink.Entity != null) ? sink.Entity.ToString() : sink.Group.ToString()}");
                }
                MyConcurrentHashSet<MyResourceSinkComponent> sinksToAdd = this.m_sinksToAdd;
                lock (sinksToAdd)
                {
                    if (this.m_sinksToAdd.Contains(sink))
                    {
                        this.m_sinksToAdd.Remove(sink);
                        this.RemoveTypesFromChanges(sink.AcceptedResources);
                        return;
                    }
                }
                MyConcurrentHashSet<MyTuple<MyResourceSinkComponent, bool>> sinksToRemove = this.m_sinksToRemove;
                lock (sinksToRemove)
                {
                    this.m_sinksToRemove.Add(MyTuple.Create<MyResourceSinkComponent, bool>(sink, resetSinkInput));
                }
                foreach (MyDefinitionId id in sink.AcceptedResources)
                {
                    int num;
                    if (!this.m_changedTypes.TryGetValue(id, out num))
                    {
                        this.m_changedTypes.Add(id, 1);
                        continue;
                    }
                    this.m_changedTypes[id] = num + 1;
                }
            }
        }

        private void RemoveSinkLazy(MyResourceSinkComponent sink, bool resetSinkInput = true)
        {
            foreach (MyDefinitionId id in sink.AcceptedResources)
            {
                HashSet<MyResourceSinkComponent> sinksOfType = this.GetSinksOfType(ref id, sink.Group);
                if (sinksOfType != null)
                {
                    int typeIndex = this.GetTypeIndex(ref id);
                    PerTypeData data = this.m_dataPerType[typeIndex];
                    if (sinksOfType.Remove(sink))
                    {
                        data.InvalidateGridForUpdateCache();
                    }
                    else
                    {
                        int index = -1;
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 < data.InputOutputList.Count)
                            {
                                if (data.InputOutputList[num3].Item1 != sink)
                                {
                                    num3++;
                                    continue;
                                }
                                index = num3;
                            }
                            if (index != -1)
                            {
                                MyResourceSourceComponent item = data.InputOutputList[index].Item2;
                                data.InputOutputList.RemoveAtFast<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>>(index);
                                data.SourcesByPriority[GetPriority(item)].Add(item);
                                data.InvalidateGridForUpdateCache();
                            }
                            break;
                        }
                    }
                    data.NeedsRecompute = true;
                    data.GroupsDirty = true;
                    data.RemainingFuelTimeDirty = true;
                    if (resetSinkInput)
                    {
                        sink.SetInputFromDistributor(id, 0f, IsAdaptible(sink), true);
                    }
                }
            }
            sink.OnRemoveType -= new Action<MyResourceSinkComponent, MyDefinitionId>(this.Sink_OnRemoveType);
            sink.OnAddType -= new Action<MyResourceSinkComponent, MyDefinitionId>(this.Sink_OnAddType);
            sink.RequiredInputChanged -= new MyRequiredResourceChangeDelegate(this.Sink_RequiredInputChanged);
            sink.ResourceAvailable -= new MyResourceAvailableDelegate(this.Sink_IsResourceAvailable);
        }

        public void RemoveSource(MyResourceSourceComponent source)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS)
            {
                string text1;
                if (source.Entity == null)
                {
                    text1 = source.Group.ToString();
                }
                else
                {
                    text1 = source.Entity.ToString();
                }
                this.m_changesDebug.Add($"-Source: {text1}");
            }
            MyConcurrentHashSet<MyResourceSourceComponent> sourcesToAdd = this.m_sourcesToAdd;
            lock (sourcesToAdd)
            {
                if (this.m_sourcesToAdd.Contains(source))
                {
                    this.m_sourcesToAdd.Remove(source);
                    this.RemoveTypesFromChanges(source.ResourceTypes);
                    return;
                }
            }
            sourcesToAdd = this.m_sourcesToRemove;
            lock (sourcesToAdd)
            {
                this.m_sourcesToRemove.Add(source);
            }
            foreach (MyDefinitionId id in source.ResourceTypes)
            {
                int num;
                if (!this.m_changedTypes.TryGetValue(id, out num))
                {
                    this.m_changedTypes.Add(id, 1);
                    continue;
                }
                this.m_changedTypes[id] = num + 1;
            }
        }

        private void RemoveSourceLazy(MyResourceSourceComponent source)
        {
            foreach (MyDefinitionId id in source.ResourceTypes)
            {
                HashSet<MyResourceSourceComponent> sourcesOfType = this.GetSourcesOfType(ref id, source.Group);
                if (sourcesOfType != null)
                {
                    int typeIndex = this.GetTypeIndex(ref id);
                    PerTypeData data = this.m_dataPerType[typeIndex];
                    if (sourcesOfType.Remove(source))
                    {
                        data.InvalidateGridForUpdateCache();
                    }
                    else
                    {
                        int index = -1;
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 < data.InputOutputList.Count)
                            {
                                if (data.InputOutputList[num3].Item2 != source)
                                {
                                    num3++;
                                    continue;
                                }
                                index = num3;
                            }
                            if (index != -1)
                            {
                                MyResourceSinkComponent item = data.InputOutputList[index].Item1;
                                data.InputOutputList.RemoveAtFast<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>>(index);
                                data.SinksByPriority[GetPriority(item)].Add(item);
                                data.InvalidateGridForUpdateCache();
                            }
                            break;
                        }
                    }
                    data.NeedsRecompute = true;
                    data.GroupsDirty = true;
                    data.SourceCount--;
                    if (data.SourceCount == 0)
                    {
                        data.SourcesEnabled = MyMultipleEnabledEnum.NoObjects;
                    }
                    else if (data.SourceCount != 1)
                    {
                        if (data.SourcesEnabled == MyMultipleEnabledEnum.Mixed)
                        {
                            data.SourcesEnabledDirty = true;
                        }
                    }
                    else
                    {
                        MyResourceSourceComponent firstSourceOfType = this.GetFirstSourceOfType(ref id);
                        if (firstSourceOfType != null)
                        {
                            this.ChangeSourcesState(id, firstSourceOfType.Enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled, MySession.Static.LocalPlayerId);
                        }
                        else
                        {
                            data.SourceCount--;
                            data.SourcesEnabled = MyMultipleEnabledEnum.NoObjects;
                        }
                    }
                    data.RemainingFuelTimeDirty = true;
                }
            }
            source.ProductionEnabledChanged -= new MyResourceCapacityRemainingChangedDelegate(this.source_ProductionEnabledChanged);
            source.MaxOutputChanged -= new MyResourceOutputChangedDelegate(this.source_MaxOutputChanged);
            source.HasCapacityRemainingChanged -= new MyResourceCapacityRemainingChangedDelegate(this.source_HasRemainingCapacityChanged);
        }

        private void RemoveType(ref MyDefinitionId typeId)
        {
            int num;
            if (this.TryGetTypeIndex(ref typeId, out num))
            {
                this.m_dataPerType.RemoveAt(num);
                this.m_initializedTypes.Remove(typeId);
                this.m_typeGroupCount--;
                this.m_typeIdToIndex.Remove(typeId);
                this.m_typeIdToConveyorConnectionRequired.Remove(typeId);
            }
        }

        private void RemoveTypesFromChanges(ListReader<MyDefinitionId> types)
        {
            foreach (MyDefinitionId id in types)
            {
                int num;
                if (this.m_changedTypes.TryGetValue(id, out num))
                {
                    this.m_changedTypes[id] = Math.Max(0, num - 1);
                }
            }
        }

        public MyResourceStateEnum ResourceStateByType(MyDefinitionId typeId, bool withRecompute = true)
        {
            int typeIndex = this.GetTypeIndex(ref typeId);
            if (withRecompute && this.NeedsRecompute(ref typeId))
            {
                this.RecomputeResourceDistribution(ref typeId, true);
            }
            if (withRecompute || ((typeIndex >= 0) && (typeIndex < this.m_dataPerType.Count)))
            {
                return this.m_dataPerType[typeIndex].ResourceState;
            }
            return MyResourceStateEnum.NoPower;
        }

        private void SetEntityGroup(ref MyDefinitionId typeId, IMyEntity entity)
        {
            IMyConveyorEndpointBlock endpoint = entity as IMyConveyorEndpointBlock;
            if (endpoint != null)
            {
                int typeIndex = this.GetTypeIndex(ref typeId);
                bool flag = false;
                int num2 = 0;
                while (true)
                {
                    if (num2 < this.m_dataPerType[typeIndex].DistributionGroupsInUse)
                    {
                        if (!MyGridConveyorSystem.Reachable(this.m_dataPerType[typeIndex].DistributionGroups[num2].FirstEndpoint, endpoint.ConveyorEndpoint))
                        {
                            num2++;
                            continue;
                        }
                        MyPhysicalDistributionGroup group = this.m_dataPerType[typeIndex].DistributionGroups[num2];
                        group.Add(typeId, endpoint);
                        this.m_dataPerType[typeIndex].DistributionGroups[num2] = group;
                        flag = true;
                    }
                    if (!flag)
                    {
                        PerTypeData local1 = this.m_dataPerType[typeIndex];
                        int num3 = local1.DistributionGroupsInUse + 1;
                        local1.DistributionGroupsInUse = num3;
                        if (num3 > this.m_dataPerType[typeIndex].DistributionGroups.Count)
                        {
                            this.m_dataPerType[typeIndex].DistributionGroups.Add(new MyPhysicalDistributionGroup(typeId, endpoint));
                            return;
                        }
                        MyPhysicalDistributionGroup group2 = this.m_dataPerType[typeIndex].DistributionGroups[this.m_dataPerType[typeIndex].DistributionGroupsInUse - 1];
                        group2.Init(typeId, endpoint);
                        this.m_dataPerType[typeIndex].DistributionGroups[this.m_dataPerType[typeIndex].DistributionGroupsInUse - 1] = group2;
                    }
                    return;
                }
            }
        }

        private void SetEntityGroupForTempConnected(ref MyDefinitionId typeId, MyResourceSinkComponent sink)
        {
            int num3;
            IMyConveyorEndpointBlock temporaryConnectedEntity = sink.TemporaryConnectedEntity as IMyConveyorEndpointBlock;
            int typeIndex = this.GetTypeIndex(ref typeId);
            bool flag = false;
            int num2 = 0;
            while (true)
            {
                if (num2 < this.m_dataPerType[typeIndex].DistributionGroupsInUse)
                {
                    if ((temporaryConnectedEntity == null) || !MyGridConveyorSystem.Reachable(this.m_dataPerType[typeIndex].DistributionGroups[num2].FirstEndpoint, temporaryConnectedEntity.ConveyorEndpoint))
                    {
                        bool flag2 = false;
                        if (temporaryConnectedEntity == null)
                        {
                            HashSet<MyResourceSourceComponent>[] sourcesByPriority = this.m_dataPerType[typeIndex].DistributionGroups[num2].SourcesByPriority;
                            num3 = 0;
                            while (num3 < sourcesByPriority.Length)
                            {
                                foreach (MyResourceSourceComponent component in sourcesByPriority[num3])
                                {
                                    if (ReferenceEquals(sink.TemporaryConnectedEntity, component.TemporaryConnectedEntity))
                                    {
                                        flag2 = true;
                                        break;
                                    }
                                }
                                if (flag2)
                                {
                                    break;
                                }
                                num3++;
                            }
                        }
                        if (flag2)
                        {
                            break;
                        }
                        num2++;
                        continue;
                    }
                }
                else
                {
                    goto TR_0005;
                }
                break;
            }
            MyPhysicalDistributionGroup group = this.m_dataPerType[typeIndex].DistributionGroups[num2];
            group.AddTempConnected(typeId, sink);
            this.m_dataPerType[typeIndex].DistributionGroups[num2] = group;
            flag = true;
        TR_0005:
            if (!flag)
            {
                PerTypeData local1 = this.m_dataPerType[typeIndex];
                num3 = local1.DistributionGroupsInUse + 1;
                local1.DistributionGroupsInUse = num3;
                if (num3 > this.m_dataPerType[typeIndex].DistributionGroups.Count)
                {
                    this.m_dataPerType[typeIndex].DistributionGroups.Add(new MyPhysicalDistributionGroup(typeId, sink));
                    return;
                }
                MyPhysicalDistributionGroup group2 = this.m_dataPerType[typeIndex].DistributionGroups[this.m_dataPerType[typeIndex].DistributionGroupsInUse - 1];
                group2.InitFromTempConnected(typeId, sink);
                this.m_dataPerType[typeIndex].DistributionGroups[this.m_dataPerType[typeIndex].DistributionGroupsInUse - 1] = group2;
            }
        }

        private void SetEntityGroupForTempConnected(ref MyDefinitionId typeId, MyResourceSourceComponent source)
        {
            int num3;
            IMyConveyorEndpointBlock temporaryConnectedEntity = source.TemporaryConnectedEntity as IMyConveyorEndpointBlock;
            int typeIndex = this.GetTypeIndex(ref typeId);
            bool flag = false;
            int num2 = 0;
            while (true)
            {
                if (num2 < this.m_dataPerType[typeIndex].DistributionGroupsInUse)
                {
                    if ((temporaryConnectedEntity == null) || !MyGridConveyorSystem.Reachable(this.m_dataPerType[typeIndex].DistributionGroups[num2].FirstEndpoint, temporaryConnectedEntity.ConveyorEndpoint))
                    {
                        bool flag2 = false;
                        if (temporaryConnectedEntity == null)
                        {
                            HashSet<MyResourceSinkComponent>[] sinksByPriority = this.m_dataPerType[typeIndex].DistributionGroups[num2].SinksByPriority;
                            num3 = 0;
                            while (num3 < sinksByPriority.Length)
                            {
                                foreach (MyResourceSinkComponent component in sinksByPriority[num3])
                                {
                                    if (ReferenceEquals(source.TemporaryConnectedEntity, component.TemporaryConnectedEntity))
                                    {
                                        flag2 = true;
                                        break;
                                    }
                                }
                                if (flag2)
                                {
                                    break;
                                }
                                num3++;
                            }
                        }
                        if (flag2)
                        {
                            break;
                        }
                        num2++;
                        continue;
                    }
                }
                else
                {
                    goto TR_0005;
                }
                break;
            }
            MyPhysicalDistributionGroup group = this.m_dataPerType[typeIndex].DistributionGroups[num2];
            group.AddTempConnected(typeId, source);
            this.m_dataPerType[typeIndex].DistributionGroups[num2] = group;
            flag = true;
        TR_0005:
            if (!flag)
            {
                PerTypeData local1 = this.m_dataPerType[typeIndex];
                num3 = local1.DistributionGroupsInUse + 1;
                local1.DistributionGroupsInUse = num3;
                if (num3 > this.m_dataPerType[typeIndex].DistributionGroups.Count)
                {
                    this.m_dataPerType[typeIndex].DistributionGroups.Add(new MyPhysicalDistributionGroup(typeId, source));
                    return;
                }
                MyPhysicalDistributionGroup group2 = this.m_dataPerType[typeIndex].DistributionGroups[this.m_dataPerType[typeIndex].DistributionGroupsInUse - 1];
                group2.InitFromTempConnected(typeId, source);
                this.m_dataPerType[typeIndex].DistributionGroups[this.m_dataPerType[typeIndex].DistributionGroupsInUse - 1] = group2;
            }
        }

        private float Sink_IsResourceAvailable(MyDefinitionId resourceTypeId, MyResourceSinkComponent receiver)
        {
            int typeIndex = this.GetTypeIndex(ref resourceTypeId);
            int priority = GetPriority(receiver);
            if (!this.IsConveyorConnectionRequired(ref resourceTypeId))
            {
                return (this.m_dataPerType[typeIndex].SinkDataByPriority[priority].RemainingAvailableResource - this.m_dataPerType[typeIndex].SinkDataByPriority[priority].RequiredInput);
            }
            IMyConveyorEndpointBlock entity = receiver.Entity as IMyConveyorEndpointBlock;
            if (entity == null)
            {
                return 0f;
            }
            IMyConveyorEndpoint conveyorEndpoint = entity.ConveyorEndpoint;
            int num3 = 0;
            while ((num3 < this.m_dataPerType[typeIndex].DistributionGroupsInUse) && !this.m_dataPerType[typeIndex].DistributionGroups[num3].SinksByPriority[priority].Contains(receiver))
            {
                num3++;
            }
            return ((num3 != this.m_dataPerType[typeIndex].DistributionGroupsInUse) ? (this.m_dataPerType[typeIndex].DistributionGroups[num3].SinkDataByPriority[priority].RemainingAvailableResource - this.m_dataPerType[typeIndex].DistributionGroups[num3].SinkDataByPriority[priority].RequiredInput) : 0f);
        }

        private void Sink_OnAddType(MyResourceSinkComponent sink, MyDefinitionId resourceType)
        {
            this.RemoveSinkLazy(sink, false);
            this.CheckDistributionSystemChanges();
            this.AddSinkLazy(sink);
        }

        private void Sink_OnRemoveType(MyResourceSinkComponent sink, MyDefinitionId resourceType)
        {
            this.RemoveSinkLazy(sink, false);
            this.CheckDistributionSystemChanges();
            this.AddSinkLazy(sink);
        }

        private void Sink_RequiredInputChanged(MyDefinitionId changedResourceTypeId, MyResourceSinkComponent changedSink, float oldRequirement, float newRequirement)
        {
            if (this.m_typeIdToIndex.ContainsKey(changedResourceTypeId) && m_sinkSubtypeToPriority.ContainsKey(changedSink.Group))
            {
                int typeIndex = this.GetTypeIndex(ref changedResourceTypeId);
                if (this.TryGetTypeIndex(changedResourceTypeId, out typeIndex))
                {
                    this.m_dataPerType[typeIndex].NeedsRecompute = true;
                    if (this.NeedsRecompute(ref changedResourceTypeId))
                    {
                        this.RecomputeResourceDistribution(ref changedResourceTypeId, true);
                    }
                }
            }
        }

        private void source_HasRemainingCapacityChanged(MyDefinitionId changedResourceTypeId, MyResourceSourceComponent source)
        {
            int typeIndex = this.GetTypeIndex(ref changedResourceTypeId);
            this.m_dataPerType[typeIndex].NeedsRecompute = true;
            this.m_dataPerType[typeIndex].RemainingFuelTimeDirty = true;
        }

        private void source_MaxOutputChanged(MyDefinitionId changedResourceTypeId, float oldOutput, MyResourceSourceComponent obj)
        {
            int typeIndex = this.GetTypeIndex(ref changedResourceTypeId);
            this.m_dataPerType[typeIndex].NeedsRecompute = true;
            this.m_dataPerType[typeIndex].RemainingFuelTimeDirty = true;
            this.m_dataPerType[typeIndex].SourcesEnabledDirty = true;
            if (this.m_dataPerType[typeIndex].SourceCount == 1)
            {
                this.RecomputeResourceDistribution(ref changedResourceTypeId, true);
            }
        }

        private void source_ProductionEnabledChanged(MyDefinitionId changedResourceTypeId, MyResourceSourceComponent obj)
        {
            int typeIndex = this.GetTypeIndex(ref changedResourceTypeId);
            this.m_dataPerType[typeIndex].NeedsRecompute = true;
            this.m_dataPerType[typeIndex].RemainingFuelTimeDirty = true;
            this.m_dataPerType[typeIndex].SourcesEnabledDirty = true;
            if (this.m_dataPerType[typeIndex].SourceCount == 1)
            {
                this.RecomputeResourceDistribution(ref changedResourceTypeId, true);
            }
        }

        public MyMultipleEnabledEnum SourcesEnabledByType(MyDefinitionId resourceTypeId)
        {
            int num;
            if (!this.TryGetTypeIndex(ref resourceTypeId, out num))
            {
                return MyMultipleEnabledEnum.NoObjects;
            }
            if (this.m_dataPerType[num].SourcesEnabledDirty)
            {
                this.RefreshSourcesEnabled(resourceTypeId);
            }
            return this.m_dataPerType[num].SourcesEnabled;
        }

        public float TotalRequiredInputByType(MyDefinitionId resourceTypeId)
        {
            int num;
            return (this.TryGetTypeIndex(ref resourceTypeId, out num) ? this.m_dataPerType[num].SinkDataByPriority.Last<MySinkGroupData>().RequiredInputCumulative : 0f);
        }

        private bool TryGetTypeIndex(MyDefinitionId typeId, out int typeIndex) => 
            this.TryGetTypeIndex(ref typeId, out typeIndex);

        private bool TryGetTypeIndex(ref MyDefinitionId typeId, out int typeIndex)
        {
            typeIndex = 0;
            return ((this.m_typeGroupCount != 0) ? ((this.m_typeGroupCount <= 1) || this.m_typeIdToIndex.TryGetValue(typeId, out typeIndex)) : false);
        }

        public void UpdateBeforeSimulation()
        {
            this.CheckDistributionSystemChanges();
            foreach (MyDefinitionId id in this.m_typeIdToIndex.Keys)
            {
                if (this.m_forceRecalculation || this.NeedsRecompute(ref id))
                {
                    this.RecomputeResourceDistribution(ref id, false);
                }
            }
            this.m_forceRecalculation = false;
            foreach (MyDefinitionId id2 in this.m_typesToRemove)
            {
                this.RemoveType(ref id2);
            }
            bool showTrace = ShowTrace;
        }

        public void UpdateBeforeSimulation100()
        {
            MyResourceStateEnum state = this.ResourceStateByType(ElectricityId, true);
            if (this.m_electricityState != state)
            {
                if (this.PowerStateIsOk(state) != this.PowerStateIsOk(this.m_electricityState))
                {
                    this.ConveyorSystem_OnPoweredChanged();
                }
                bool flag = this.PowerStateWorks(state);
                if ((this.OnPowerGenerationChanged != null) && (flag != this.PowerStateWorks(this.m_electricityState)))
                {
                    this.OnPowerGenerationChanged(flag);
                }
                this.ConveyorSystem_OnPoweredChanged();
                this.m_electricityState = state;
            }
        }

        public void UpdateHud(MyHudSinkGroupInfo info)
        {
            int num3;
            bool flag = true;
            int num = 0;
            int index = 0;
            if (this.TryGetTypeIndex(ElectricityId, out num3))
            {
                while (index < this.m_dataPerType[num3].SinkDataByPriority.Length)
                {
                    if ((flag && (this.m_dataPerType[num3].SinkDataByPriority[index].RemainingAvailableResource < this.m_dataPerType[num3].SinkDataByPriority[index].RequiredInput)) && !this.m_dataPerType[num3].SinkDataByPriority[index].IsAdaptible)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        num++;
                    }
                    info.SetGroupDeficit(index, Math.Max((float) (this.m_dataPerType[num3].SinkDataByPriority[index].RequiredInput - this.m_dataPerType[num3].SinkDataByPriority[index].RemainingAvailableResource), (float) 0f));
                    index++;
                }
                info.WorkingGroupCount = num;
            }
        }

        [Conditional("DEBUG")]
        private void UpdateTrace()
        {
            int num = 0;
            while (num < this.m_typeGroupCount)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_dataPerType[num].DistributionGroupsInUse)
                    {
                        num++;
                        break;
                    }
                    MyPhysicalDistributionGroup group = this.m_dataPerType[num].DistributionGroups[num2];
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 >= group.SinkSourcePairs.Count)
                        {
                            num2++;
                            break;
                        }
                        num3++;
                    }
                }
            }
        }

        public MyMultipleEnabledEnum SourcesEnabled =>
            this.SourcesEnabledByType(m_typeIdToIndexTotal.Keys.First<MyDefinitionId>());

        public MyResourceStateEnum ResourceState =>
            this.ResourceStateByType(m_typeIdToIndexTotal.Keys.First<MyDefinitionId>(), true);

        public static int SinkGroupPrioritiesTotal =>
            m_sinkGroupPrioritiesTotal;

        public static DictionaryReader<MyStringHash, int> SinkSubtypesToPriority =>
            new DictionaryReader<MyStringHash, int>(m_sinkSubtypeToPriority);

        public bool NeedsPerFrameUpdate
        {
            get
            {
                int showTrace;
                if (((this.m_sinksToRemove.Count > 0) || ((this.m_sourcesToRemove.Count > 0) || ((this.m_sourcesToAdd.Count > 0) || (this.m_sinksToAdd.Count > 0)))) || (this.m_typesToRemove.Count > 0))
                {
                    showTrace = 1;
                }
                else
                {
                    showTrace = (int) ShowTrace;
                }
                bool flag = (bool) showTrace;
                if (!flag)
                {
                    foreach (KeyValuePair<MyDefinitionId, int> pair in this.m_typeIdToIndex)
                    {
                        MyDefinitionId key = pair.Key;
                        flag |= this.NeedsRecompute(ref key, pair.Value);
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                return flag;
            }
        }

        public override string ComponentTypeDebugString =>
            "Resource Distributor";

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyResourceDistributorComponent.<>c <>9 = new MyResourceDistributorComponent.<>c();
            public static Func<MyResourceDistributionGroupDefinition, int> <>9__39_0;

            internal int <InitializeMappings>b__39_0(MyResourceDistributionGroupDefinition def) => 
                def.Priority;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyPhysicalDistributionGroup
        {
            public IMyConveyorEndpoint FirstEndpoint;
            public HashSet<MyResourceSinkComponent>[] SinksByPriority;
            public HashSet<MyResourceSourceComponent>[] SourcesByPriority;
            public List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> SinkSourcePairs;
            public MyResourceDistributorComponent.MySinkGroupData[] SinkDataByPriority;
            public MyResourceDistributorComponent.MySourceGroupData[] SourceDataByPriority;
            public MyTuple<MyResourceDistributorComponent.MySinkGroupData, MyResourceDistributorComponent.MySourceGroupData> InputOutputData;
            public List<int> StockpilingStorage;
            public List<int> OtherStorage;
            public float MaxAvailableResources;
            public MyResourceStateEnum ResourceState;
            public MyPhysicalDistributionGroup(MyDefinitionId typeId, IMyConveyorEndpointBlock block)
            {
                this.SinksByPriority = null;
                this.SourcesByPriority = null;
                this.SinkSourcePairs = null;
                this.FirstEndpoint = null;
                this.SinkDataByPriority = null;
                this.SourceDataByPriority = null;
                this.StockpilingStorage = null;
                this.OtherStorage = null;
                this.InputOutputData = new MyTuple<MyResourceDistributorComponent.MySinkGroupData, MyResourceDistributorComponent.MySourceGroupData>();
                this.MaxAvailableResources = 0f;
                this.ResourceState = MyResourceStateEnum.NoPower;
                this.AllocateData();
                this.Init(typeId, block);
            }

            public MyPhysicalDistributionGroup(MyDefinitionId typeId, MyResourceSinkComponent tempConnectedSink)
            {
                this.SinksByPriority = null;
                this.SourcesByPriority = null;
                this.SinkSourcePairs = null;
                this.FirstEndpoint = null;
                this.SinkDataByPriority = null;
                this.SourceDataByPriority = null;
                this.StockpilingStorage = null;
                this.OtherStorage = null;
                this.InputOutputData = new MyTuple<MyResourceDistributorComponent.MySinkGroupData, MyResourceDistributorComponent.MySourceGroupData>();
                this.MaxAvailableResources = 0f;
                this.ResourceState = MyResourceStateEnum.NoPower;
                this.AllocateData();
                this.InitFromTempConnected(typeId, tempConnectedSink);
            }

            public MyPhysicalDistributionGroup(MyDefinitionId typeId, MyResourceSourceComponent tempConnectedSource)
            {
                this.SinksByPriority = null;
                this.SourcesByPriority = null;
                this.SinkSourcePairs = null;
                this.FirstEndpoint = null;
                this.SinkDataByPriority = null;
                this.SourceDataByPriority = null;
                this.StockpilingStorage = null;
                this.OtherStorage = null;
                this.InputOutputData = new MyTuple<MyResourceDistributorComponent.MySinkGroupData, MyResourceDistributorComponent.MySourceGroupData>();
                this.MaxAvailableResources = 0f;
                this.ResourceState = MyResourceStateEnum.NoPower;
                this.AllocateData();
                this.InitFromTempConnected(typeId, tempConnectedSource);
            }

            public void Init(MyDefinitionId typeId, IMyConveyorEndpointBlock block)
            {
                this.FirstEndpoint = block.ConveyorEndpoint;
                this.ClearData();
                this.Add(typeId, block);
            }

            public void InitFromTempConnected(MyDefinitionId typeId, MyResourceSinkComponent tempConnectedSink)
            {
                IMyConveyorEndpointBlock temporaryConnectedEntity = tempConnectedSink.TemporaryConnectedEntity as IMyConveyorEndpointBlock;
                if (temporaryConnectedEntity != null)
                {
                    this.FirstEndpoint = temporaryConnectedEntity.ConveyorEndpoint;
                }
                this.ClearData();
                this.AddTempConnected(typeId, tempConnectedSink);
            }

            public void InitFromTempConnected(MyDefinitionId typeId, MyResourceSourceComponent tempConnectedSource)
            {
                IMyConveyorEndpointBlock temporaryConnectedEntity = tempConnectedSource.TemporaryConnectedEntity as IMyConveyorEndpointBlock;
                if (temporaryConnectedEntity != null)
                {
                    this.FirstEndpoint = temporaryConnectedEntity.ConveyorEndpoint;
                }
                this.ClearData();
                this.AddTempConnected(typeId, tempConnectedSource);
            }

            public void Add(MyDefinitionId typeId, IMyConveyorEndpointBlock endpoint)
            {
                if (this.FirstEndpoint == null)
                {
                    this.FirstEndpoint = endpoint.ConveyorEndpoint;
                }
                MyEntityComponentContainer components = (endpoint as IMyEntity).Components;
                MyResourceSinkComponent component = components.Get<MyResourceSinkComponent>();
                MyResourceSourceComponent component2 = components.Get<MyResourceSourceComponent>();
                bool flag = (component != null) && component.AcceptedResources.Contains<MyDefinitionId>(typeId);
                bool flag2 = (component2 != null) && component2.ResourceTypes.Contains<MyDefinitionId>(typeId);
                if (flag & flag2)
                {
                    this.SinkSourcePairs.Add(new MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>(component, component2));
                }
                else if (flag)
                {
                    this.SinksByPriority[MyResourceDistributorComponent.GetPriority(component)].Add(component);
                }
                else if (flag2)
                {
                    this.SourcesByPriority[MyResourceDistributorComponent.GetPriority(component2)].Add(component2);
                }
            }

            public void AddTempConnected(MyDefinitionId typeId, MyResourceSinkComponent tempConnectedSink)
            {
                if ((tempConnectedSink != null) && tempConnectedSink.AcceptedResources.Contains<MyDefinitionId>(typeId))
                {
                    this.SinksByPriority[MyResourceDistributorComponent.GetPriority(tempConnectedSink)].Add(tempConnectedSink);
                }
            }

            public void AddTempConnected(MyDefinitionId typeId, MyResourceSourceComponent tempConnectedSource)
            {
                if ((tempConnectedSource != null) && tempConnectedSource.ResourceTypes.Contains<MyDefinitionId>(typeId))
                {
                    this.SourcesByPriority[MyResourceDistributorComponent.GetPriority(tempConnectedSource)].Add(tempConnectedSource);
                }
            }

            private void AllocateData()
            {
                this.FirstEndpoint = null;
                this.SinksByPriority = new HashSet<MyResourceSinkComponent>[MyResourceDistributorComponent.m_sinkGroupPrioritiesTotal];
                this.SourcesByPriority = new HashSet<MyResourceSourceComponent>[MyResourceDistributorComponent.m_sourceGroupPrioritiesTotal];
                this.SinkSourcePairs = new List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>>();
                this.SinkDataByPriority = new MyResourceDistributorComponent.MySinkGroupData[MyResourceDistributorComponent.m_sinkGroupPrioritiesTotal];
                this.SourceDataByPriority = new MyResourceDistributorComponent.MySourceGroupData[MyResourceDistributorComponent.m_sourceGroupPrioritiesTotal];
                this.StockpilingStorage = new List<int>();
                this.OtherStorage = new List<int>();
                for (int i = 0; i < MyResourceDistributorComponent.m_sinkGroupPrioritiesTotal; i++)
                {
                    this.SinksByPriority[i] = new HashSet<MyResourceSinkComponent>();
                }
                for (int j = 0; j < MyResourceDistributorComponent.m_sourceGroupPrioritiesTotal; j++)
                {
                    this.SourcesByPriority[j] = new HashSet<MyResourceSourceComponent>();
                }
            }

            private void ClearData()
            {
                int num;
                HashSet<MyResourceSinkComponent>[] sinksByPriority = this.SinksByPriority;
                for (num = 0; num < sinksByPriority.Length; num++)
                {
                    sinksByPriority[num].Clear();
                }
                HashSet<MyResourceSourceComponent>[] sourcesByPriority = this.SourcesByPriority;
                for (num = 0; num < sourcesByPriority.Length; num++)
                {
                    sourcesByPriority[num].Clear();
                }
                this.SinkSourcePairs.Clear();
                this.StockpilingStorage.SetSize<int>(0);
                this.OtherStorage.SetSize<int>(0);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MySinkGroupData
        {
            public bool IsAdaptible;
            public float RequiredInput;
            public float RequiredInputCumulative;
            public float RemainingAvailableResource;
            public override string ToString() => 
                $"IsAdaptible: {this.IsAdaptible}, RequiredInput: {this.RequiredInput}, RemainingAvailableResource: {this.RemainingAvailableResource}";
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MySourceGroupData
        {
            public float MaxAvailableResource;
            public float UsageRatio;
            public bool InfiniteCapacity;
            public int ActiveCount;
            public override string ToString() => 
                $"MaxAvailableResource: {this.MaxAvailableResource}, UsageRatio: {this.UsageRatio}";
        }

        private class PerTypeData
        {
            private bool m_needsRecompute;
            public MyDefinitionId TypeId;
            public MyResourceDistributorComponent.MySinkGroupData[] SinkDataByPriority;
            public MyResourceDistributorComponent.MySourceGroupData[] SourceDataByPriority;
            public MyTuple<MyResourceDistributorComponent.MySinkGroupData, MyResourceDistributorComponent.MySourceGroupData> InputOutputData;
            public HashSet<MyResourceSinkComponent>[] SinksByPriority;
            public HashSet<MyResourceSourceComponent>[] SourcesByPriority;
            public List<MyTuple<MyResourceSinkComponent, MyResourceSourceComponent>> InputOutputList;
            public List<int> StockpilingStorageIndices;
            public List<int> OtherStorageIndices;
            public List<MyResourceDistributorComponent.MyPhysicalDistributionGroup> DistributionGroups;
            public int DistributionGroupsInUse;
            public bool GroupsDirty;
            public int SourceCount;
            public float RemainingFuelTime;
            public bool RemainingFuelTimeDirty;
            public float MaxAvailableResource;
            public MyMultipleEnabledEnum SourcesEnabled;
            public bool SourcesEnabledDirty;
            public MyResourceStateEnum ResourceState;
            private bool m_gridsForUpdateValid;
            private HashSet<MyCubeGrid> m_gridsForUpdate = new HashSet<MyCubeGrid>();
            private bool m_gridUpdateScheduled;
            private Action m_UpdateGridsCallback;

            public PerTypeData()
            {
                this.m_UpdateGridsCallback = new Action(this.UpdateGrids);
            }

            private void AddGridForUpdate(MyEntityComponentBase component)
            {
                IMyEntity entity = component.Entity;
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    this.m_gridsForUpdate.Add(block.CubeGrid);
                }
                else
                {
                    MyCubeGrid item = entity as MyCubeGrid;
                    if (item != null)
                    {
                        this.m_gridsForUpdate.Add(item);
                    }
                }
            }

            public void InvalidateGridForUpdateCache()
            {
                this.m_gridsForUpdate.Clear();
                this.m_gridsForUpdateValid = false;
            }

            private void ScheduleGridUpdate()
            {
                if (!this.m_gridUpdateScheduled)
                {
                    this.m_gridUpdateScheduled = true;
                    MySandboxGame.Static.Invoke(this.m_UpdateGridsCallback, "UpdateResourcesOnGrids");
                }
            }

            private void UpdateGrids()
            {
                this.m_gridUpdateScheduled = false;
                if (!this.m_gridsForUpdateValid)
                {
                    this.m_gridsForUpdateValid = true;
                    HashSet<MyResourceSourceComponent>[] sourcesByPriority = this.SourcesByPriority;
                    int index = 0;
                    while (true)
                    {
                        if (index >= sourcesByPriority.Length)
                        {
                            HashSet<MyResourceSinkComponent>[] sinksByPriority = this.SinksByPriority;
                            index = 0;
                            while (index < sinksByPriority.Length)
                            {
                                foreach (MyResourceSinkComponent component2 in sinksByPriority[index])
                                {
                                    this.AddGridForUpdate(component2);
                                }
                                index++;
                            }
                            break;
                        }
                        foreach (MyResourceSourceComponent component in sourcesByPriority[index])
                        {
                            this.AddGridForUpdate(component);
                        }
                        index++;
                    }
                }
                using (HashSet<MyCubeGrid>.Enumerator enumerator3 = this.m_gridsForUpdate.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        enumerator3.Current.MarkForUpdate();
                    }
                }
            }

            public bool NeedsRecompute
            {
                get => 
                    this.m_needsRecompute;
                set
                {
                    if (this.m_needsRecompute != value)
                    {
                        this.m_needsRecompute = value;
                        if (this.m_needsRecompute)
                        {
                            this.ScheduleGridUpdate();
                        }
                    }
                }
            }
        }
    }
}

