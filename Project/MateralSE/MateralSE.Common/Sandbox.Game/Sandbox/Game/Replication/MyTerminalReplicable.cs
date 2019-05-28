namespace Sandbox.Game.Replication
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRageMath;

    internal class MyTerminalReplicable : MyExternalReplicableEvent<MySyncedBlock>
    {
        private MyPropertySyncStateGroup m_propertySync;
        private long m_blockEntityId;

        private MySyncedBlock FindBlock()
        {
            MySyncedBlock block;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById<MySyncedBlock>(this.m_blockEntityId, out block, false);
            if ((block == null) || !block.GetTopMostParent(null).MarkedForClose)
            {
                return block;
            }
            return null;
        }

        public override BoundingBoxD GetAABB() => 
            BoundingBoxD.CreateInvalid();

        public override IMyReplicable GetParent() => 
            base.m_parent;

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            resultList.Add(this.m_propertySync);
        }

        public override ValidationResult HasRights(EndpointId endpointId, ValidationType validationFlags)
        {
            AdminSettingsEnum enum2;
            if (this.Block == null)
            {
                return ValidationResult.Kick;
            }
            if (!validationFlags.HasFlag(ValidationType.IgnoreDLC) && !MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(this.Block.BlockDefinition, endpointId.Value))
            {
                return ValidationResult.Kick;
            }
            ValidationResult passed = ValidationResult.Passed;
            long identityId = MySession.Static.Players.TryGetIdentityId(endpointId.Value, 0);
            if (validationFlags.HasFlag(ValidationType.Ownership) && (!MySession.Static.RemoteAdminSettings.TryGetValue(endpointId.Value, out enum2) || !enum2.HasFlag(AdminSettingsEnum.UseTerminals)))
            {
                MyRelationsBetweenPlayerAndBlock userRelationToOwner = this.Block.GetUserRelationToOwner(identityId);
                if (((userRelationToOwner != MyRelationsBetweenPlayerAndBlock.FactionShare) && (userRelationToOwner != MyRelationsBetweenPlayerAndBlock.Owner)) && (userRelationToOwner != MyRelationsBetweenPlayerAndBlock.NoOwnership))
                {
                    return (ValidationResult.Ownership | ValidationResult.Kick);
                }
            }
            if (validationFlags.HasFlag(ValidationType.BigOwner) && !MyReplicableRightsValidator.GetBigOwner(this.Block.CubeGrid, endpointId, identityId, false))
            {
                return (ValidationResult.BigOwner | ValidationResult.Kick);
            }
            if (validationFlags.HasFlag(ValidationType.BigOwnerSpaceMaster) && !MyReplicableRightsValidator.GetBigOwner(this.Block.CubeGrid, endpointId, identityId, true))
            {
                return (ValidationResult.BigOwnerSpaceMaster | ValidationResult.Kick);
            }
            if (validationFlags.HasFlag(ValidationType.Controlled))
            {
                passed = MyReplicableRightsValidator.GetControlled(this.Block.CubeGrid, endpointId);
                if (passed == ValidationResult.Kick)
                {
                    return (passed | ValidationResult.Controlled);
                }
            }
            if (validationFlags.HasFlag(ValidationType.Access))
            {
                if (this.Block.CubeGrid == null)
                {
                    return (ValidationResult.Access | ValidationResult.Kick);
                }
                MyCubeGrid cubeGrid = this.Block.CubeGrid;
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
                if (identity == null)
                {
                    goto TR_0007;
                }
                else if (identity.Character != null)
                {
                    MyCharacterReplicable characterReplicable = FindByObject(identity.Character) as MyCharacterReplicable;
                    if (characterReplicable == null)
                    {
                        return (ValidationResult.Access | ValidationResult.Kick);
                    }
                    Vector3D position = identity.Character.PositionComp.GetPosition();
                    MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(cubeGrid);
                    bool flag = MyReplicableRightsValidator.GetAccess(characterReplicable, position, cubeGrid, group, true);
                    if (!flag)
                    {
                        characterReplicable.GetDependencies(true);
                        flag |= MyReplicableRightsValidator.GetAccess(characterReplicable, position, cubeGrid, group, false);
                    }
                    if (!flag)
                    {
                        return ValidationResult.Access;
                    }
                }
                else
                {
                    goto TR_0007;
                }
            }
            return passed;
        TR_0007:
            return (ValidationResult.Access | ValidationResult.Kick);
        }

        private void MarkDirty(VRage.Game.Entity.MyEntity entity)
        {
            this.m_propertySync.MarkDirty();
        }

        private void OnBlockCubeGridChanged(MySlimBlock slimBlock, MyCubeGrid grid)
        {
            base.m_parent = FindByObject(this.Block.CubeGrid);
            (MyMultiplayer.ReplicationLayer as MyReplicationLayer).RefreshReplicableHierarchy(this);
        }

        public override void OnDestroyClient()
        {
        }

        protected override void OnHook()
        {
            base.OnHook();
            MyPropertySyncStateGroup group1 = new MyPropertySyncStateGroup(this, this.Block.SyncType);
            group1.GlobalValidate = context => this.HasRights(context.ClientState.EndpointId.Id, ValidationType.Ownership | ValidationType.Access);
            this.m_propertySync = group1;
            this.Block.OnClose += entity => this.RaiseDestroyed();
            this.Block.SlimBlock.CubeGridChanged += new Action<MySlimBlock, MyCubeGrid>(this.OnBlockCubeGridChanged);
            if (Sync.IsServer)
            {
                this.Block.AddedToScene += new Action<VRage.Game.Entity.MyEntity>(this.MarkDirty);
            }
            base.m_parent = FindByObject(this.Block.CubeGrid);
        }

        protected override void OnLoad(BitStream stream, Action<MySyncedBlock> loadingDoneHandler)
        {
            if (stream != null)
            {
                this.m_blockEntityId = stream.ReadInt64(0x40);
            }
            Sandbox.Game.Entities.MyEntities.CallAsync(() => loadingDoneHandler(this.FindBlock()));
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            stream.WriteInt64(this.Block.EntityId, 0x40);
            return true;
        }

        private MySyncedBlock Block =>
            base.Instance;

        public override bool IsValid =>
            ((this.Block != null) && !this.Block.MarkedForClose);

        public override bool HasToBeChild =>
            true;
    }
}

