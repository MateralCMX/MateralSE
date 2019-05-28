namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
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
    using VRage.Groups;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x7d0)]
    public class MySessionComponentTrash : MySessionComponentBase
    {
        private static MyDistributedUpdater<CachingList<MyCubeGrid>, MyCubeGrid> m_gridsToCheck = new MyDistributedUpdater<CachingList<MyCubeGrid>, MyCubeGrid>(100);
        private static float m_playerDistanceHysteresis = 0f;
        private int m_identityCheckIndex;
        private List<MyIdentity> m_allIdentities = new List<MyIdentity>();
        private int m_trashedGridsCount;
        private bool m_voxelTrash_StartFromBegining = true;
        private List<long> m_voxel_BaseIds = new List<long>();
        private int m_voxel_BaseCurrentIndex;
        private MyVoxelBase m_voxel_CurrentBase;
        private MyStorageBase m_voxel_CurrentStorage;
        private IEnumerator<KeyValuePair<Vector3I, MyTimeSpan>> m_voxel_CurrentAccessEnumerator;
        private KeyValuePair<Vector3I, MyTimeSpan>? m_voxel_CurrentChunk;
        private int m_voxel_Timer;
        private static int CONST_VOXEL_WAIT_CYCLE = 600;
        private static int CONST_VOXEL_WAIT_CHUNK = 10;
        private static Dictionary<MyTrashRemovalFlags, MyStringId> m_names;

        static MySessionComponentTrash()
        {
            Dictionary<MyTrashRemovalFlags, MyStringId> dictionary1 = new Dictionary<MyTrashRemovalFlags, MyStringId>();
            dictionary1.Add(MyTrashRemovalFlags.Fixed, MySpaceTexts.ScreenDebugAdminMenu_Stations);
            dictionary1.Add(MyTrashRemovalFlags.Stationary, MySpaceTexts.ScreenDebugAdminMenu_Stationary);
            dictionary1.Add(MyTrashRemovalFlags.Linear, MyCommonTexts.ScreenDebugAdminMenu_Linear);
            dictionary1.Add(MyTrashRemovalFlags.Accelerating, MyCommonTexts.ScreenDebugAdminMenu_Accelerating);
            dictionary1.Add(MyTrashRemovalFlags.Powered, MySpaceTexts.ScreenDebugAdminMenu_Powered);
            dictionary1.Add(MyTrashRemovalFlags.Controlled, MySpaceTexts.ScreenDebugAdminMenu_Controlled);
            dictionary1.Add(MyTrashRemovalFlags.WithProduction, MySpaceTexts.ScreenDebugAdminMenu_WithProduction);
            dictionary1.Add(MyTrashRemovalFlags.WithMedBay, MySpaceTexts.ScreenDebugAdminMenu_WithMedBay);
            dictionary1.Add(MyTrashRemovalFlags.WithBlockCount, MyCommonTexts.ScreenDebugAdminMenu_WithBlockCount);
            dictionary1.Add(MyTrashRemovalFlags.DistanceFromPlayer, MyCommonTexts.ScreenDebugAdminMenu_DistanceFromPlayer);
            dictionary1.Add(MyTrashRemovalFlags.RevertMaterials, MyCommonTexts.ScreenDebugAdminMenu_RevertMaterials);
            dictionary1.Add(MyTrashRemovalFlags.RevertAsteroids, MyCommonTexts.ScreenDebugAdminMenu_RevertAsteroids);
            dictionary1.Add(MyTrashRemovalFlags.RevertWithFloatingsPresent, MyCommonTexts.ScreenDebugAdminMenu_RevertWithFloatingsPresent);
            m_names = dictionary1;
        }

        private void CheckAbandonedCharacters()
        {
            if (MySession.Static.Settings.PlayerCharacterRemovalThreshold <= 0)
            {
                return;
            }
            if (this.m_identityCheckIndex >= this.m_allIdentities.Count)
            {
                this.m_identityCheckIndex = 0;
                return;
            }
            MyIdentity identity = this.m_allIdentities[this.m_identityCheckIndex];
            if (((int) (MySession.GetIdentityLogoutTimeSeconds(identity.IdentityId) / 60f)) >= MySession.Static.Settings.PlayerCharacterRemovalThreshold)
            {
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(identity.IdentityId, out id))
                {
                    MyPlayer player;
                    if (MySession.Static.Players.TryGetPlayerById(id, out player))
                    {
                        goto TR_0001;
                    }
                    else
                    {
                        if (identity.Character == null)
                        {
                            CloseAbandonedRespawnShip(identity);
                        }
                        else if (this.RemoveCharacter(identity.Character))
                        {
                            CloseAbandonedRespawnShip(identity);
                        }
                        using (HashSet<long>.Enumerator enumerator = identity.SavedCharacters.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                MyCharacter character;
                                if (!MyEntities.TryGetEntityById<MyCharacter>(enumerator.Current, out character, true))
                                {
                                    continue;
                                }
                                if (!character.Closed || character.MarkedForClose)
                                {
                                    this.RemoveCharacter(character);
                                }
                            }
                            goto TR_0001;
                        }
                    }
                }
                CloseAbandonedRespawnShip(identity);
            }
        TR_0001:
            this.m_identityCheckIndex++;
        }

        public static void CloseAbandonedRespawnShip(MyIdentity identity)
        {
            if (MySession.Static.Settings.RespawnShipDelete)
            {
                CloseRespawnShip(identity);
            }
        }

        private static void CloseRespawnShip(MyIdentity identity)
        {
            if (identity.RespawnShips != null)
            {
                using (List<long>.Enumerator enumerator = identity.RespawnShips.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyCubeGrid grid;
                        if (!MyEntities.TryGetEntityById<MyCubeGrid>(enumerator.Current, out grid, false))
                        {
                            continue;
                        }
                        using (HashSet<MySlimBlock>.Enumerator enumerator2 = grid.GetBlocks().GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                MyCockpit fatBlock = enumerator2.Current.FatBlock as MyCockpit;
                                if ((fatBlock != null) && (fatBlock.Pilot != null))
                                {
                                    fatBlock.Use();
                                }
                            }
                        }
                        MyEntities.SendCloseRequest(grid);
                    }
                }
                identity.RespawnShips.Clear();
            }
        }

        public static void CloseRespawnShip(MyPlayer player)
        {
            if (player.RespawnShip != null)
            {
                CloseRespawnShip(player.Identity);
            }
        }

        public static string GetName(MyTrashRemovalFlags flag)
        {
            MyStringId id;
            return (!m_names.TryGetValue(flag, out id) ? MyEnum<MyTrashRemovalFlags>.GetName(flag) : MyTexts.GetString(id));
        }

        public static MyTrashRemovalFlags GetTrashState(MyCubeGrid grid)
        {
            float num;
            return GetTrashState(grid, out num, true);
        }

        private static MyTrashRemovalFlags GetTrashState(MyCubeGrid grid, out float metric, bool checkGroup = false)
        {
            metric = -1f;
            float num = MySession.GetOwnerLogoutTimeSeconds(grid) / 3600f;
            if (((num > 0f) && (MySession.Static.Settings.PlayerInactivityThreshold > 0f)) && (num > MySession.Static.Settings.PlayerInactivityThreshold))
            {
                return MyTrashRemovalFlags.None;
            }
            if (PlayerDistanceHysteresis == 0f)
            {
                int num1;
                int num4;
                int num5;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = true;
                if (grid.Physics == null)
                {
                    return MyTrashRemovalFlags.Default;
                }
                if (grid.Physics.AngularVelocity.AbsMax() < 0.05f)
                {
                    num1 = (int) (grid.Physics.LinearVelocity.AbsMax() < 0.05f);
                }
                else
                {
                    num1 = 0;
                }
                flag3 = (bool) num1;
                if (flag3)
                {
                    num4 = 0;
                }
                else if (grid.Physics.AngularAcceleration.AbsMax() <= 0.05f)
                {
                    num4 = (int) (grid.Physics.LinearAcceleration.AbsMax() > 0.05f);
                }
                else
                {
                    num4 = 1;
                }
                flag = (bool) num5;
                flag2 = !flag && !flag3;
                if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.Stationary) & flag3)
                {
                    return MyTrashRemovalFlags.Stationary;
                }
                if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.Linear) & flag2)
                {
                    return MyTrashRemovalFlags.Linear;
                }
                if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.Accelerating) & flag)
                {
                    return MyTrashRemovalFlags.Accelerating;
                }
            }
            HashSet<MySlimBlock> blocks = grid.GetBlocks();
            if (PlayerDistanceHysteresis == 0f)
            {
                if ((!grid.IsRespawnGrid && (blocks != null)) && (blocks.Count > MySession.Static.Settings.BlockCountThreshold))
                {
                    metric = MySession.Static.Settings.BlockCountThreshold;
                    return MyTrashRemovalFlags.WithBlockCount;
                }
                if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.Fixed) && grid.IsStatic)
                {
                    return MyTrashRemovalFlags.Fixed;
                }
                if (grid.GridSystems != null)
                {
                    bool flag4 = grid.GridSystems.ResourceDistributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true) != MyResourceStateEnum.NoPower;
                    if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.Powered) & flag4)
                    {
                        bool flag5 = true;
                        long piratesId = MyPirateAntennas.GetPiratesId();
                        MyIdentity identity = MySession.Static.Players.TryGetIdentity(piratesId);
                        if (((identity != null) && (!identity.BlockLimits.HasRemainingPCU && grid.BigOwners.Contains(piratesId))) && grid.Save)
                        {
                            bool flag6 = false;
                            foreach (long num3 in grid.SmallOwners)
                            {
                                if (!MySession.Static.Players.IdentityIsNpc(num3))
                                {
                                    flag6 = true;
                                    break;
                                }
                            }
                            if (!flag6)
                            {
                                flag5 = false;
                            }
                        }
                        if (flag5)
                        {
                            return MyTrashRemovalFlags.Powered;
                        }
                    }
                }
            }
            if (grid.GridSystems != null)
            {
                if ((!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.Controlled) && (grid.GridSystems.ControlSystem != null)) && grid.GridSystems.ControlSystem.IsControlled)
                {
                    return MyTrashRemovalFlags.Controlled;
                }
                if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.WithProduction) && (((grid.BlocksCounters.GetValueOrDefault<MyObjectBuilderType, int>(typeof(MyObjectBuilder_ProductionBlock)) > 0) || (grid.BlocksCounters.GetValueOrDefault<MyObjectBuilderType, int>(typeof(MyObjectBuilder_Assembler)) > 0)) || (grid.BlocksCounters.GetValueOrDefault<MyObjectBuilderType, int>(typeof(MyObjectBuilder_Refinery)) > 0)))
                {
                    return MyTrashRemovalFlags.WithProduction;
                }
            }
            if (!MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.WithMedBay) && (grid.BlocksCounters.GetValueOrDefault<MyObjectBuilderType, int>(typeof(MyObjectBuilder_MedicalRoom)) > 0))
            {
                return MyTrashRemovalFlags.WithMedBay;
            }
            if (checkGroup && (MyCubeGridGroups.Static.Physical != null))
            {
                MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(grid);
                if (group != null)
                {
                    using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node>.Enumerator enumerator2 = group.Nodes.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator2.MoveNext())
                            {
                                break;
                            }
                            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node current = enumerator2.Current;
                            if ((current.NodeData != null) && ((current.NodeData.Physics != null) && ((current.NodeData.Physics.Shape != null) && (current.NodeData != grid))))
                            {
                                MyTrashRemovalFlags flags = GetTrashState(current.NodeData, out metric, false);
                                if (flags != MyTrashRemovalFlags.None)
                                {
                                    return flags;
                                }
                            }
                        }
                    }
                }
            }
            if (!IsCloseToPlayerOrCamera(grid, MySession.Static.Settings.PlayerDistanceThreshold + PlayerDistanceHysteresis))
            {
                return MyTrashRemovalFlags.None;
            }
            metric = MySession.Static.Settings.PlayerDistanceThreshold;
            return MyTrashRemovalFlags.DistanceFromPlayer;
        }

        public static bool IsCloseToPlayerOrCamera(MyEntity entity, float distanceThreshold)
        {
            MatrixD worldMatrix = entity.WorldMatrix;
            Vector3D translation = worldMatrix.Translation;
            float num = distanceThreshold * distanceThreshold;
            if (Sync.Players.GetOnlinePlayers().Count <= 0)
            {
                return true;
            }
            int num2 = 0;
            using (IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IMyControllableEntity controlledEntity = enumerator.Current.Controller.ControlledEntity;
                    if (controlledEntity != null)
                    {
                        num2++;
                        if (Vector3D.DistanceSquared(controlledEntity.Entity.WorldMatrix.Translation, translation) < num)
                        {
                            return true;
                        }
                    }
                }
            }
            return (num2 <= 0);
        }

        public override void LoadData()
        {
            if (Sync.IsServer)
            {
                MyEntities.OnEntityAdd += new Action<MyEntity>(this.MyEntities_OnEntityAdd);
                MyEntities.OnEntityRemove += new Action<MyEntity>(this.MyEntities_OnEntityRemove);
                MySession.Static.Players.IdentitiesChanged += new Action(this.Players_IdentitiesChanged);
                this.m_trashedGridsCount = 0;
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity entity)
        {
            if (entity is MyCubeGrid)
            {
                m_gridsToCheck.List.Add(entity as MyCubeGrid);
            }
        }

        private void MyEntities_OnEntityRemove(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                m_gridsToCheck.List.Remove(grid, false);
                if (grid.MarkedAsTrash)
                {
                    this.m_trashedGridsCount--;
                }
            }
        }

        private void Players_IdentitiesChanged()
        {
            this.m_allIdentities = MySession.Static.Players.GetAllIdentities().ToList<MyIdentity>();
        }

        private bool RemoveCharacter(MyCharacter character)
        {
            if (character.IsUsing is MyCryoChamber)
            {
                return false;
            }
            if (character.IsUsing is MyCockpit)
            {
                (character.IsUsing as MyCockpit).RemovePilot();
            }
            character.Close();
            return true;
        }

        public static void RemoveGrid(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(grid);
            if (group != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node local1 in group.Nodes)
                {
                    local1.NodeData.DismountAllCockpits();
                    local1.NodeData.Close();
                }
            }
            grid.Close();
            if (grid.BigOwners.Count > 0)
            {
                MyPlayer.PlayerId id;
                MyPlayer player;
                long identityId = grid.BigOwners[0];
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
                if ((MySession.Static.Players.TryGetPlayerId(identityId, out id) && (MySession.Static.Players.TryGetPlayerById(id, out player) && (!MySession.Static.Players.GetOnlinePlayers().Contains(player) && (identity != null)))) && (identity.BlockLimits.BlocksBuilt == 0))
                {
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId();
                    MySession.Static.Players.RemoveIdentity(identityId, playerId);
                }
            }
        }

        protected override void UnloadData()
        {
            if (Sync.IsServer)
            {
                MyEntities.OnEntityAdd -= new Action<MyEntity>(this.MyEntities_OnEntityAdd);
                MyEntities.OnEntityRemove -= new Action<MyEntity>(this.MyEntities_OnEntityRemove);
                MySession.Static.Players.IdentitiesChanged -= new Action(this.Players_IdentitiesChanged);
                this.m_trashedGridsCount = 0;
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (Sync.IsServer)
            {
                m_gridsToCheck.List.ApplyChanges();
                if (MySession.Static.Settings.TrashRemovalEnabled && (Sync.IsDedicated || !MySession.Static.IsCameraUserAnySpectator()))
                {
                    bool trashFound = false;
                    m_gridsToCheck.Update();
                    m_gridsToCheck.Iterate(x => trashFound |= this.UpdateTrash(x));
                    if ((MySession.Static.Settings.OptimalGridCount > 0) && !trashFound)
                    {
                        if ((m_gridsToCheck.List.Count - this.m_trashedGridsCount) > MySession.Static.Settings.OptimalGridCount)
                        {
                            m_playerDistanceHysteresis--;
                        }
                        else if (((m_gridsToCheck.List.Count - this.m_trashedGridsCount) < MySession.Static.Settings.OptimalGridCount) && (m_playerDistanceHysteresis < 0f))
                        {
                            m_playerDistanceHysteresis++;
                        }
                        m_playerDistanceHysteresis = MathHelper.Clamp(m_playerDistanceHysteresis, -MySession.Static.Settings.PlayerDistanceThreshold, 0f);
                    }
                    this.CheckAbandonedCharacters();
                }
                this.VoxelRevertor_Update();
            }
        }

        private bool UpdateTrash(MyCubeGrid grid)
        {
            if (grid.MarkedAsTrash)
            {
                return false;
            }
            if (grid.IsPreview)
            {
                return false;
            }
            if (!MyEntities.IsInsideWorld(grid.PositionComp.GetPosition()))
            {
                RemoveGrid(grid);
                return true;
            }
            if ((MyEncounterGenerator.Static != null) && MyEncounterGenerator.Static.IsEncounter(grid))
            {
                return false;
            }
            if (GetTrashState(grid) != MyTrashRemovalFlags.None)
            {
                return false;
            }
            if (PlayerDistanceHysteresis == 0f)
            {
                RemoveGrid(grid);
            }
            else
            {
                grid.MarkAsTrash();
                this.m_trashedGridsCount++;
            }
            return true;
        }

        private bool VoxelRevertor_AdvanceToNext()
        {
            while (true)
            {
                if ((this.m_voxel_CurrentBase == null) || (this.m_voxel_CurrentStorage == null))
                {
                    this.m_voxel_BaseCurrentIndex++;
                    if (this.m_voxel_BaseCurrentIndex >= this.m_voxel_BaseIds.Count)
                    {
                        this.m_voxel_Timer = CONST_VOXEL_WAIT_CYCLE;
                        return false;
                    }
                    MyVoxelBase vox = MySession.Static.VoxelMaps.TryGetVoxelBaseById(this.m_voxel_BaseIds[this.m_voxel_BaseCurrentIndex]);
                    if (vox == null)
                    {
                        continue;
                    }
                    MyStorageBase storage = vox.Storage as MyStorageBase;
                    if (storage == null)
                    {
                        continue;
                    }
                    if (!this.VoxelsAreSuitableForReversion(ref vox, ref storage))
                    {
                        continue;
                    }
                    this.m_voxel_CurrentBase = vox;
                    this.m_voxel_CurrentStorage = storage;
                    this.m_voxel_CurrentAccessEnumerator = null;
                    this.m_voxel_CurrentChunk = null;
                }
                if ((this.m_voxel_CurrentBase != null) && (this.m_voxel_CurrentStorage != null))
                {
                    if (this.m_voxel_CurrentAccessEnumerator == null)
                    {
                        this.m_voxel_CurrentAccessEnumerator = this.m_voxel_CurrentStorage.AccessEnumerator;
                    }
                    if (this.m_voxel_CurrentAccessEnumerator.MoveNext())
                    {
                        this.m_voxel_CurrentChunk = new KeyValuePair<Vector3I, MyTimeSpan>?(this.m_voxel_CurrentAccessEnumerator.Current);
                        this.m_voxel_Timer = CONST_VOXEL_WAIT_CHUNK;
                        return true;
                    }
                    this.m_voxel_CurrentBase = null;
                    this.m_voxel_CurrentStorage = null;
                    this.m_voxel_CurrentChunk = null;
                }
            }
        }

        private bool VoxelRevertor_CanRevertCurrent()
        {
            BoundingBoxD xd;
            Vector3I key = this.m_voxel_CurrentChunk.Value.Key;
            MyTimeSpan span = this.m_voxel_CurrentChunk.Value.Value;
            if (MyTimeSpan.FromTicks(Stopwatch.GetTimestamp() - span.Ticks).Minutes < MySession.Static.Settings.VoxelAgeThreshold)
            {
                return false;
            }
            this.m_voxel_CurrentStorage.ConvertAccessCoordinates(ref key, out xd);
            xd.Translate(this.m_voxel_CurrentBase.PositionLeftBottomCorner);
            if (!ReferenceEquals(this.m_voxel_CurrentBase.RootVoxel, this.m_voxel_CurrentBase))
            {
                xd.Translate(-this.m_voxel_CurrentBase.PositionLeftBottomCorner);
            }
            bool flag = true;
            foreach (MyEntity entity in MyEntities.GetEntities())
            {
                if (entity is MyCubeGrid)
                {
                    if (entity.PositionComp.WorldAABB.DistanceSquared(ref xd) >= (MySession.Static.Settings.VoxelGridDistanceThreshold * MySession.Static.Settings.VoxelGridDistanceThreshold))
                    {
                        continue;
                    }
                    flag = false;
                }
                else if (entity is MyCharacter)
                {
                    if (xd.DistanceSquared(entity.PositionComp.WorldMatrix.Translation) >= (MySession.Static.Settings.VoxelPlayerDistanceThreshold * MySession.Static.Settings.VoxelPlayerDistanceThreshold))
                    {
                        continue;
                    }
                    flag = false;
                }
                else
                {
                    ContainmentType type;
                    if (MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.RevertWithFloatingsPresent))
                    {
                        continue;
                    }
                    if (!(entity is MyFloatingObject) && !(entity is MyInventoryBagEntity))
                    {
                        continue;
                    }
                    Vector3D translation = entity.PositionComp.WorldMatrix.Translation;
                    xd.Contains(ref translation, out type);
                    if (type == ContainmentType.Disjoint)
                    {
                        continue;
                    }
                    flag = false;
                }
                break;
            }
            return flag;
        }

        private void VoxelRevertor_Update()
        {
            if (MySession.Static.Settings.VoxelTrashRemovalEnabled)
            {
                if (this.m_voxel_Timer >= 0)
                {
                    this.m_voxel_Timer--;
                }
                else
                {
                    if (this.m_voxelTrash_StartFromBegining)
                    {
                        this.m_voxelTrash_StartFromBegining = false;
                        this.m_voxel_BaseIds.Clear();
                        MySession.Static.VoxelMaps.GetAllIds(ref this.m_voxel_BaseIds);
                        this.m_voxel_BaseCurrentIndex = -1;
                        this.m_voxel_CurrentBase = null;
                        this.m_voxel_CurrentStorage = null;
                        this.m_voxel_CurrentAccessEnumerator = null;
                    }
                    if (!this.VoxelRevertor_AdvanceToNext())
                    {
                        this.m_voxelTrash_StartFromBegining = true;
                    }
                    else if ((this.m_voxel_CurrentChunk != null) && this.VoxelRevertor_CanRevertCurrent())
                    {
                        Vector3I key = this.m_voxel_CurrentChunk.Value.Key;
                        MyStorageDataTypeFlags dataType = MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.RevertMaterials) ? MyStorageDataTypeFlags.All : MyStorageDataTypeFlags.Content;
                        this.m_voxel_CurrentStorage.AccessDelete(ref key, dataType, true);
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyVoxelBase, Vector3I, MyStorageDataTypeFlags>(this.m_voxel_CurrentBase.RootVoxel, x => new Action<Vector3I, MyStorageDataTypeFlags>(x.RevertVoxelAccess), key, dataType, targetEndpoint);
                    }
                }
            }
        }

        private bool VoxelsAreSuitableForReversion(ref MyVoxelBase vox, ref MyStorageBase storage)
        {
            if (vox.Closed)
            {
                return false;
            }
            if (storage.DataProvider == null)
            {
                return false;
            }
            bool flag = storage.DataProvider is MyPlanetStorageProvider;
            bool flag2 = storage.DataProvider is MyCompositeShapeProvider;
            if (vox.RootVoxel != vox)
            {
                return false;
            }
            if (flag2 && !MySession.Static.Settings.TrashFlags.HasFlag(MyTrashRemovalFlags.RevertAsteroids))
            {
                return false;
            }
            bool flag1 = flag;
            return true;
        }

        public static float PlayerDistanceHysteresis =>
            m_playerDistanceHysteresis;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentTrash.<>c <>9 = new MySessionComponentTrash.<>c();
            public static Func<MyVoxelBase, Action<Vector3I, MyStorageDataTypeFlags>> <>9__31_0;

            internal Action<Vector3I, MyStorageDataTypeFlags> <VoxelRevertor_Update>b__31_0(MyVoxelBase x) => 
                new Action<Vector3I, MyStorageDataTypeFlags>(x.RevertVoxelAccess);
        }
    }
}

