namespace Sandbox.Game.Replication
{
    using ParallelTasks;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Replication;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    internal class MyCubeGridReplicable : MyEntityReplicableBaseEvent<MyCubeGrid>, IMyStreamableReplicable
    {
        private Action<MyCubeGrid> m_loadingDoneHandler;
        private MyStreamingEntityStateGroup<MyCubeGridReplicable> m_streamingGroup;
        private readonly HashSet<IMyReplicable> m_dependencies = new HashSet<IMyReplicable>();
        private readonly List<MyCubeGrid> m_tmpCubeGrids = new List<MyCubeGrid>();
        private MyPropertySyncStateGroup m_propertySync;

        public void CreateStreamingStateGroup()
        {
            this.m_streamingGroup = new MyStreamingEntityStateGroup<MyCubeGridReplicable>(this, this);
        }

        public override HashSet<IMyReplicable> GetDependencies(bool forPlayer)
        {
            this.m_dependencies.Clear();
            if (Sync.IsServer)
            {
                if (base.Instance == null)
                {
                    return this.m_dependencies;
                }
                using (HashSet<MyLaserReceiver>.Enumerator enumerator = base.Instance.GridSystems.RadioSystem.LaserReceivers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MyDataBroadcaster broadcaster in enumerator.Current.BroadcastersInRange)
                        {
                            if (broadcaster.Closed)
                            {
                                continue;
                            }
                            MyExternalReplicable item = FindByObject(broadcaster);
                            if (item != null)
                            {
                                this.m_dependencies.Add(item);
                            }
                        }
                    }
                }
            }
            return this.m_dependencies;
        }

        public override HashSet<IMyReplicable> GetPhysicalDependencies(MyTimeSpan timeStamp, MyReplicablesBase replicables)
        {
            HashSet<IMyReplicable> physicalDependencies = base.GetPhysicalDependencies(timeStamp, replicables);
            if (base.Instance != null)
            {
                MyGridPhysicalHierarchy.Static.GetGroupNodes(base.Instance, this.m_tmpCubeGrids);
                using (List<MyCubeGrid>.Enumerator enumerator = this.m_tmpCubeGrids.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyExternalReplicable item = FindByObject(enumerator.Current);
                        if (item != null)
                        {
                            physicalDependencies.Add(item);
                        }
                    }
                }
                this.m_tmpCubeGrids.Clear();
            }
            return physicalDependencies;
        }

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            if (this.m_streamingGroup != null)
            {
                resultList.Add(this.m_streamingGroup);
            }
            base.GetStateGroups(resultList);
            resultList.Add(this.m_propertySync);
        }

        public IMyStateGroup GetStreamingStateGroup() => 
            this.m_streamingGroup;

        public override ValidationResult HasRights(EndpointId endpointId, ValidationType validationFlags)
        {
            ValidationResult passed = ValidationResult.Passed;
            long identityId = MySession.Static.Players.TryGetIdentityId(endpointId.Value, 0);
            if (validationFlags.HasFlag(ValidationType.Controlled))
            {
                passed |= MyReplicableRightsValidator.GetControlled(base.Instance, endpointId);
                if (passed.HasFlag(ValidationResult.Kick))
                {
                    return passed;
                }
            }
            if ((validationFlags.HasFlag(ValidationType.Ownership) || validationFlags.HasFlag(ValidationType.BigOwner)) && !MyReplicableRightsValidator.GetBigOwner(base.Instance, endpointId, identityId, false))
            {
                return (ValidationResult.BigOwner | ValidationResult.Ownership | ValidationResult.Kick);
            }
            if (validationFlags.HasFlag(ValidationType.BigOwnerSpaceMaster) && !MyReplicableRightsValidator.GetBigOwner(base.Instance, endpointId, identityId, true))
            {
                return (ValidationResult.BigOwnerSpaceMaster | ValidationResult.Kick);
            }
            if (validationFlags.HasFlag(ValidationType.Access))
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
                if (identity == null)
                {
                    goto TR_0003;
                }
                else if (identity.Character != null)
                {
                    if (this.Grid == null)
                    {
                        return (ValidationResult.Access | ValidationResult.Kick);
                    }
                    MyCharacterReplicable characterReplicable = FindByObject(identity.Character) as MyCharacterReplicable;
                    if (characterReplicable == null)
                    {
                        return (ValidationResult.Access | ValidationResult.Kick);
                    }
                    Vector3D position = identity.Character.PositionComp.GetPosition();
                    MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(this.Grid);
                    bool flag = MyReplicableRightsValidator.GetAccess(characterReplicable, position, this.Grid, group, true);
                    if (!flag)
                    {
                        characterReplicable.GetDependencies(true);
                        flag |= MyReplicableRightsValidator.GetAccess(characterReplicable, position, this.Grid, group, false);
                    }
                    if (!flag)
                    {
                        return ValidationResult.Access;
                    }
                }
                else
                {
                    goto TR_0003;
                }
            }
            return passed;
        TR_0003:
            return (ValidationResult.Access | ValidationResult.Kick);
        }

        public void LoadCancel()
        {
            this.m_loadingDoneHandler(null);
        }

        public void LoadDone(BitStream stream)
        {
            this.OnLoad(stream, this.m_loadingDoneHandler);
        }

        protected override void OnHook()
        {
            base.OnHook();
            MyPropertySyncStateGroup group1 = new MyPropertySyncStateGroup(this, this.Grid.SyncType);
            group1.GlobalValidate = context => this.HasRights(context.ClientState.EndpointId.Id, ValidationType.Controlled | ValidationType.Access);
            this.m_propertySync = group1;
        }

        protected override void OnLoad(BitStream stream, Action<MyCubeGrid> loadingDoneHandler)
        {
            if (stream.ReadBool())
            {
                long gridId = stream.ReadInt64(0x40);
                Action<MyCubeGrid> findGrid = null;
                findGrid = delegate (MyCubeGrid grid) {
                    if (grid.EntityId == gridId)
                    {
                        loadingDoneHandler(grid);
                        MyCubeGrid.OnSplitGridCreated -= findGrid;
                    }
                };
                MyCubeGrid.OnSplitGridCreated += findGrid;
            }
            else
            {
                MyObjectBuilder_EntityBase objectBuilder = MySerializer.CreateAndRead<MyObjectBuilder_EntityBase>(stream, MyObjectBuilderSerializer.Dynamic);
                base.TryRemoveExistingEntity(objectBuilder.EntityId);
                MyCubeGrid grid = MyEntities.CreateFromObjectBuilderNoinit(objectBuilder) as MyCubeGrid;
                bool fadeIn = false;
                if ((objectBuilder.PositionAndOrientation != null) && ((objectBuilder.PositionAndOrientation.Value.Position - MySector.MainCamera.Position).LengthSquared() > 1000000.0))
                {
                    fadeIn = true;
                }
                byte islandIndex = stream.ReadByte(8);
                MyEntities.InitAsync(grid, objectBuilder, true, e => loadingDoneHandler(grid), islandIndex, stream.ReadDouble(), fadeIn);
            }
        }

        public void OnLoadBegin(Action<bool> loadingDoneHandler)
        {
            this.m_loadingDoneHandler = instance => this.OnLoadDone(instance, loadingDoneHandler);
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            if (!this.Grid.IsSplit)
            {
                return false;
            }
            stream.WriteBool(true);
            stream.WriteInt64(this.Grid.EntityId, 0x40);
            return true;
        }

        public void Serialize(BitStream stream, HashSet<string> cachedData, Endpoint forClient, Action writeData)
        {
            if (!this.Grid.Closed)
            {
                stream.WriteBool(false);
                MyObjectBuilder_EntityBase builder = this.Grid.GetObjectBuilder(false);
                byte replicableIsland = MyMultiplayer.GetReplicationServer().GetClientReplicableIslandIndex(this, forClient);
                double time = MyMultiplayer.GetReplicationServer().GetClientRelevantServerTimestamp(forClient).Milliseconds;
                Parallel.Start(delegate {
                    try
                    {
                        MySerializer.Write<MyObjectBuilder_EntityBase>(stream, ref builder, MyObjectBuilderSerializer.Dynamic);
                    }
                    catch (Exception)
                    {
                        XmlSerializer serializer = MyXmlSerializerManager.GetSerializer(builder.GetType());
                        MyLog.Default.WriteLine("Grid data - START");
                        try
                        {
                            serializer.Serialize(MyLog.Default.GetTextWriter(), builder);
                        }
                        catch
                        {
                            MyLog.Default.WriteLine("Failed");
                        }
                        MyLog.Default.WriteLine("Grid data - END");
                        throw;
                    }
                    stream.WriteByte(replicableIsland, 8);
                    stream.WriteDouble(time);
                    writeData();
                });
            }
        }

        private MyCubeGrid Grid =>
            base.Instance;

        public bool NeedsToBeStreamed =>
            (!Sync.IsServer ? (this.m_streamingGroup != null) : !this.Grid.IsSplit);
    }
}

