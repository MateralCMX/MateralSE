namespace Sandbox.Game.Replication
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    internal class MyCharacterReplicable : MyEntityReplicableBaseEvent<MyCharacter>
    {
        private MyPropertySyncStateGroup m_propertySync;
        private readonly HashSet<IMyReplicable> m_dependencies = new HashSet<IMyReplicable>();
        private readonly HashSet<IMyEntity> m_dependencyParents = new HashSet<IMyEntity>();
        private long m_ownerId;
        private long m_characterId;

        protected override IMyStateGroup CreatePhysicsGroup() => 
            new MyCharacterPhysicsStateGroup(base.Instance, this);

        public override HashSet<IMyReplicable> GetDependencies(bool forPlayer)
        {
            if (!forPlayer)
            {
                return null;
            }
            this.m_dependencies.Clear();
            this.m_dependencyParents.Clear();
            if (Sync.IsServer)
            {
                foreach (MyDataBroadcaster broadcaster in MyAntennaSystem.Static.GetAllRelayedBroadcasters(base.Instance, base.Instance.GetPlayerIdentityId(), false, null))
                {
                    if (ReferenceEquals(base.Instance.RadioBroadcaster, broadcaster))
                    {
                        continue;
                    }
                    if (!broadcaster.Closed)
                    {
                        MyFarBroadcasterReplicable item = FindByObject(broadcaster) as MyFarBroadcasterReplicable;
                        if (item != null)
                        {
                            this.m_dependencies.Add(item);
                            if ((item.Instance != null) && (item.Instance.Entity != null))
                            {
                                IMyEntity topMostParent = item.Instance.Entity.GetTopMostParent(null);
                                if (topMostParent != null)
                                {
                                    this.m_dependencyParents.Add(topMostParent);
                                }
                            }
                        }
                    }
                }
            }
            return this.m_dependencies;
        }

        public override IMyReplicable GetParent() => 
            ((base.Instance?.Parent == null) ? null : FindByObject(base.Instance.GetTopMostParent(null)));

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            base.GetStateGroups(resultList);
            if ((this.m_propertySync != null) && (this.m_propertySync.PropertyCount > 0))
            {
                resultList.Add(this.m_propertySync);
            }
        }

        public override ValidationResult HasRights(EndpointId endpointId, ValidationType validationFlags)
        {
            bool flag = true;
            if (validationFlags.HasFlag(ValidationType.Controlled))
            {
                flag &= endpointId.Value == base.Instance.GetClientIdentity().SteamId;
            }
            return (flag ? ValidationResult.Passed : (ValidationResult.Controlled | ValidationResult.Kick));
        }

        private static void LoadAsync(long ownerId, long characterId, Action<MyCharacter> loadingDoneHandler)
        {
            MyEntity entity;
            MyEntities.TryGetEntityById(ownerId, out entity, false);
            MyShipController controller = entity as MyShipController;
            if (controller == null)
            {
                loadingDoneHandler(null);
            }
            else if (controller.Pilot != null)
            {
                loadingDoneHandler(controller.Pilot);
                MySession.Static.Players.UpdatePlayerControllers(ownerId);
            }
            else
            {
                MyEntity entity2;
                MyEntities.TryGetEntityById(characterId, out entity2, false);
                MyCharacter character = entity2 as MyCharacter;
                loadingDoneHandler(character);
            }
        }

        protected override void OnHook()
        {
            base.OnHook();
            MyPropertySyncStateGroup group1 = new MyPropertySyncStateGroup(this, base.Instance.SyncType);
            group1.GlobalValidate = context => this.HasRights(context.ClientState.EndpointId.Id, ValidationType.Controlled);
            this.m_propertySync = group1;
            if (base.Instance != null)
            {
                base.Instance.Hierarchy.OnParentChanged += new Action<MyHierarchyComponentBase, MyHierarchyComponentBase>(this.OnParentChanged);
            }
        }

        protected override void OnLoad(BitStream stream, Action<MyCharacter> loadingDoneHandler)
        {
            bool flag = true;
            if (stream != null)
            {
                MySerializer.CreateAndRead<bool>(stream, out flag, null);
            }
            if (flag)
            {
                if (stream != null)
                {
                    MySerializer.CreateAndRead<long>(stream, out this.m_ownerId, null);
                    MySerializer.CreateAndRead<long>(stream, out this.m_characterId, null);
                }
                MyEntities.CallAsync(() => LoadAsync(this.m_ownerId, this.m_characterId, loadingDoneHandler));
            }
            else
            {
                byte islandIndex = stream.ReadByte(8);
                MyObjectBuilder_Character objectBuilder = (MyObjectBuilder_Character) MySerializer.CreateAndRead<MyObjectBuilder_EntityBase>(stream, MyObjectBuilderSerializer.Dynamic);
                base.TryRemoveExistingEntity(objectBuilder.EntityId);
                MyCharacter character = MyEntities.CreateFromObjectBuilderNoinit(objectBuilder) as MyCharacter;
                MyEntities.InitAsync(character, objectBuilder, true, e => loadingDoneHandler(character), islandIndex, 0.0, false);
            }
        }

        private void OnParentChanged(MyHierarchyComponentBase oldParent, MyHierarchyComponentBase newParent)
        {
            if (this.IsReadyForReplication)
            {
                (MyMultiplayer.ReplicationLayer as MyReplicationLayer).RefreshReplicableHierarchy(this);
            }
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            if (base.Instance == null)
            {
                return false;
            }
            stream.WriteBool(base.Instance.IsUsing is MyShipController);
            if (base.Instance.IsUsing is MyShipController)
            {
                long entityId = base.Instance.IsUsing.EntityId;
                MySerializer.Write<long>(stream, ref entityId, null);
                long num2 = base.Instance.EntityId;
                MySerializer.Write<long>(stream, ref num2, null);
            }
            else
            {
                byte clientReplicableIslandIndex = MyMultiplayer.GetReplicationServer().GetClientReplicableIslandIndex(this, clientEndpoint);
                stream.WriteByte(clientReplicableIslandIndex, 8);
                MyObjectBuilder_Character objectBuilder = (MyObjectBuilder_Character) base.Instance.GetObjectBuilder(false);
                MySerializer.Write<MyObjectBuilder_Character>(stream, ref objectBuilder, MyObjectBuilderSerializer.Dynamic);
            }
            return true;
        }

        public override bool ShouldReplicate(MyClientInfo client) => 
            !base.Instance.IsDead;

        public HashSetReader<IMyEntity> CachedParentDependencies =>
            new HashSetReader<IMyEntity>(this.m_dependencyParents);

        public override bool HasToBeChild =>
            (base.Instance.Parent != null);
    }
}

