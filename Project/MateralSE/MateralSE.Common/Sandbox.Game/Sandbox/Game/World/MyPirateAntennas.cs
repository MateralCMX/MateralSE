namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0x3e8, typeof(MyObjectBuilder_PirateAntennas), (Type) null)]
    public class MyPirateAntennas : MySessionComponentBase
    {
        private static readonly string IDENTITY_NAME = "Pirate";
        private static readonly string PIRATE_FACTION_TAG = "SPRT";
        private static readonly int DRONE_DESPAWN_TIMER = 0x927c0;
        private static readonly int DRONE_DESPAWN_RETRY = 0x1388;
        private static CachingDictionary<long, PirateAntennaInfo> m_pirateAntennas;
        private static bool m_iteratingAntennas;
        private static Dictionary<string, MyPirateAntennaDefinition> m_definitionsByAntennaName;
        private static int m_ctr = 0;
        private static int m_ctr2 = 0;
        private static CachingDictionary<long, DroneInfo> m_droneInfos;
        private static long m_piratesIdentityId = 0L;

        public override void BeforeStart()
        {
            base.BeforeStart();
            MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(PIRATE_FACTION_TAG, null);
            if (faction != null)
            {
                if (m_piratesIdentityId == 0)
                {
                    m_piratesIdentityId = faction.FounderId;
                }
                else if (Sync.IsServer)
                {
                    MyIdentity identity1 = Sync.Players.TryGetIdentity(m_piratesIdentityId);
                    if (identity1 == null)
                    {
                        Vector3? colorMask = null;
                        Sync.Players.CreateNewIdentity(IDENTITY_NAME, m_piratesIdentityId, null, colorMask);
                    }
                    identity1.LastLoginTime = DateTime.Now;
                    if (MySession.Static.Factions.GetPlayerFaction(m_piratesIdentityId) == null)
                    {
                        MyFactionCollection.SendJoinRequest(faction.FactionId, m_piratesIdentityId);
                    }
                }
                if (!Sync.Players.IdentityIsNpc(m_piratesIdentityId))
                {
                    Sync.Players.MarkIdentityAsNPC(m_piratesIdentityId);
                }
            }
            foreach (KeyValuePair<long, DroneInfo> pair in m_droneInfos)
            {
                VRage.Game.Entity.MyEntity entity;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(pair.Key, out entity, false);
                if (entity == null)
                {
                    DroneInfo.Deallocate(pair.Value);
                    m_droneInfos.Remove(pair.Key, false);
                    continue;
                }
                if (MySession.Static.Settings.EnableDrones)
                {
                    this.RegisterDrone(pair.Value.AntennaEntityId, entity, false);
                    continue;
                }
                MyCubeGrid cubeGrid = entity as MyCubeGrid;
                MyRemoteControl control = entity as MyRemoteControl;
                if (cubeGrid == null)
                {
                    cubeGrid = control.CubeGrid;
                }
                this.UnregisterDrone(entity, false);
                cubeGrid.Close();
            }
            m_droneInfos.ApplyRemovals();
        }

        public unsafe bool CanDespawn(MyCubeGrid grid, MyRemoteControl remote)
        {
            if ((remote != null) && !remote.IsFunctional)
            {
                return false;
            }
            BoundingSphereD worldVolume = grid.PositionComp.WorldVolume;
            double* numPtr1 = (double*) ref worldVolume.Radius;
            numPtr1[0] += 4000.0;
            using (IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPlayer current = enumerator.Current;
                    if (worldVolume.Contains(current.GetPosition()) == ContainmentType.Contains)
                    {
                        return false;
                    }
                }
            }
            using (Dictionary<MyDefinitionId, HashSet<IMyGunObject<MyDeviceBase>>>.ValueCollection.Enumerator enumerator2 = grid.GridSystems.WeaponSystem.GetGunSets().Values.GetEnumerator())
            {
                bool flag;
                while (true)
                {
                    if (enumerator2.MoveNext())
                    {
                        HashSet<IMyGunObject<MyDeviceBase>>.Enumerator enumerator3 = enumerator2.Current.GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                if (!enumerator3.MoveNext())
                                {
                                    break;
                                }
                                if (enumerator3.Current.IsShooting)
                                {
                                    return false;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator3.Dispose();
                            continue;
                        }
                    }
                    else
                    {
                        goto TR_0001;
                    }
                    break;
                }
                return flag;
            }
        TR_0001:
            return true;
        }

        private void ChangeDroneOwnership(List<MyCubeGrid> gridList, long ownerId, long antennaEntityId)
        {
            foreach (MyCubeGrid grid in gridList)
            {
                grid.ChangeGridOwnership(ownerId, MyOwnershipShareModeEnum.None);
                MyRemoteControl control = null;
                foreach (MySlimBlock block in grid.CubeBlocks)
                {
                    if (block.FatBlock != null)
                    {
                        MyProgrammableBlock fatBlock = block.FatBlock as MyProgrammableBlock;
                        if (fatBlock != null)
                        {
                            fatBlock.SendRecompile();
                        }
                        MyRemoteControl control2 = block.FatBlock as MyRemoteControl;
                        if (control == null)
                        {
                            control = control2;
                        }
                    }
                }
                this.RegisterDrone(antennaEntityId, control ?? grid, true);
            }
        }

        private static void DebugDraw()
        {
            foreach (KeyValuePair<long, PirateAntennaInfo> pair in m_pirateAntennas)
            {
                MyRadioAntenna entity = null;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRadioAntenna>(pair.Key, out entity, false);
                if (entity != null)
                {
                    MyRenderProxy.DebugDrawText3D(entity.WorldMatrix.Translation, "Time remaining: " + Math.Max(0, (pair.Value.AntennaDefinition.SpawnTimeMs - MySandboxGame.TotalGamePlayTimeInMilliseconds) + pair.Value.LastGenerationGameTime).ToString(), Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
            foreach (KeyValuePair<long, PirateAntennaInfo> pair2 in m_pirateAntennas)
            {
                VRage.Game.Entity.MyEntity entity;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(pair2.Key, out entity, false);
                if (entity != null)
                {
                    MyRenderProxy.DebugDrawSphere(entity.WorldMatrix.Translation, (float) entity.PositionComp.WorldVolume.Radius, Color.BlueViolet, 1f, false, false, true, false);
                }
            }
            foreach (KeyValuePair<long, DroneInfo> pair3 in m_droneInfos)
            {
                VRage.Game.Entity.MyEntity entity2;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(pair3.Key, out entity2, false);
                if (entity2 != null)
                {
                    MyCubeGrid cubeGrid = entity2 as MyCubeGrid;
                    if (cubeGrid == null)
                    {
                        cubeGrid = (entity2 as MyRemoteControl).CubeGrid;
                    }
                    MyRenderProxy.DebugDrawSphere(cubeGrid.PositionComp.WorldVolume.Center, (float) cubeGrid.PositionComp.WorldVolume.Radius, Color.Cyan, 1f, false, false, true, false);
                    MyRenderProxy.DebugDrawText3D(cubeGrid.PositionComp.WorldVolume.Center, ((pair3.Value.DespawnTime - MySandboxGame.TotalGamePlayTimeInMilliseconds) / 0x3e8).ToString(), Color.Cyan, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
        }

        private void DroneMainEntityOnClosing(VRage.Game.Entity.MyEntity entity)
        {
            this.UnregisterDrone(entity, true);
        }

        private void DroneRemoteOwnershipChanged(MyTerminalBlock remote)
        {
            long ownerId = remote.OwnerId;
            if (!Sync.Players.IdentityIsNpc(ownerId))
            {
                this.UnregisterDrone(remote, true);
            }
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_PirateAntennas objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_PirateAntennas;
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            objectBuilder.PiratesIdentity = m_piratesIdentityId;
            DictionaryReader<long, DroneInfo> reader = m_droneInfos.Reader;
            objectBuilder.Drones = new MyObjectBuilder_PirateAntennas.MyPirateDrone[reader.Count];
            int index = 0;
            foreach (KeyValuePair<long, DroneInfo> pair in reader)
            {
                objectBuilder.Drones[index] = new MyObjectBuilder_PirateAntennas.MyPirateDrone();
                objectBuilder.Drones[index].EntityId = pair.Key;
                objectBuilder.Drones[index].AntennaEntityId = pair.Value.AntennaEntityId;
                objectBuilder.Drones[index].DespawnTimer = Math.Max(0, pair.Value.DespawnTime - totalGamePlayTimeInMilliseconds);
                index++;
            }
            return objectBuilder;
        }

        public static long GetPiratesId() => 
            m_piratesIdentityId;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_PirateAntennas antennas = sessionComponent as MyObjectBuilder_PirateAntennas;
            m_piratesIdentityId = antennas.PiratesIdentity;
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (antennas.Drones != null)
            {
                foreach (MyObjectBuilder_PirateAntennas.MyPirateDrone drone in antennas.Drones)
                {
                    m_droneInfos.Add(drone.EntityId, DroneInfo.Allocate(drone.AntennaEntityId, totalGamePlayTimeInMilliseconds + drone.DespawnTimer), true);
                }
            }
            m_iteratingAntennas = false;
        }

        public override void LoadData()
        {
            base.LoadData();
            m_piratesIdentityId = 0L;
            m_pirateAntennas = new CachingDictionary<long, PirateAntennaInfo>();
            m_definitionsByAntennaName = new Dictionary<string, MyPirateAntennaDefinition>();
            m_droneInfos = new CachingDictionary<long, DroneInfo>();
            foreach (MyPirateAntennaDefinition definition in MyDefinitionManager.Static.GetPirateAntennaDefinitions())
            {
                m_definitionsByAntennaName[definition.Name] = definition;
            }
        }

        private static void RandomShuffle<T>(List<T> input)
        {
            for (int i = input.Count - 1; i > 1; i--)
            {
                int randomInt = MyUtils.GetRandomInt(0, i);
                T local = input[i];
                input[i] = input[randomInt];
                input[randomInt] = local;
            }
        }

        private void RegisterDrone(long antennaEntityId, VRage.Game.Entity.MyEntity droneMainEntity, bool immediate = true)
        {
            VRage.Game.Entity.MyEntity entity;
            DroneInfo info = DroneInfo.Allocate(antennaEntityId, MySandboxGame.TotalGamePlayTimeInMilliseconds + DRONE_DESPAWN_TIMER);
            m_droneInfos.Add(droneMainEntity.EntityId, info, immediate);
            droneMainEntity.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.DroneMainEntityOnClosing);
            PirateAntennaInfo info2 = null;
            if (!m_pirateAntennas.TryGetValue(antennaEntityId, out info2) && Sandbox.Game.Entities.MyEntities.TryGetEntityById(antennaEntityId, out entity, false))
            {
                MyRadioAntenna antenna = entity as MyRadioAntenna;
                if (antenna != null)
                {
                    antenna.UpdatePirateAntenna(false);
                    m_pirateAntennas.TryGetValue(antennaEntityId, out info2);
                }
            }
            if (info2 != null)
            {
                info2.SpawnedDrones++;
            }
            MyRemoteControl control = droneMainEntity as MyRemoteControl;
            if (control != null)
            {
                control.OwnershipChanged += new Action<MyTerminalBlock>(this.DroneRemoteOwnershipChanged);
            }
        }

        private bool SpawnDrone(MyRadioAntenna antenna, long ownerId, Vector3D position, MySpawnGroupDefinition spawnGroup, Vector3? spawnUp = new Vector3?(), Vector3? spawnForward = new Vector3?())
        {
            Vector3D vectord2;
            long antennaEntityId = antenna.EntityId;
            Vector3D vectord = antenna.PositionComp.GetPosition();
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
            if (closestPlanet != null)
            {
                if (!MyGravityProviderSystem.IsPositionInNaturalGravity(vectord, 0.0) && MyGravityProviderSystem.IsPositionInNaturalGravity(position, 0.0))
                {
                    MySandboxGame.Log.WriteLine("Couldn't spawn drone; antenna is not in natural gravity but spawn location is.");
                    return false;
                }
                closestPlanet.CorrectSpawnLocation(ref position, spawnGroup.SpawnRadius * 2.0);
                vectord2 = position - closestPlanet.PositionComp.GetPosition();
                vectord2.Normalize();
            }
            else
            {
                Vector3 vector = MyGravityProviderSystem.CalculateTotalGravityInPoint(position);
                if (!(vector != Vector3.Zero))
                {
                    vectord2 = (spawnUp == null) ? MyUtils.GetRandomVector3Normalized() : ((Vector3D) spawnUp.Value);
                }
                else
                {
                    vectord2 = -vector;
                    vectord2.Normalize();
                }
            }
            Vector3D randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref vectord2);
            if (spawnForward != null)
            {
                Vector3 vector2 = spawnForward.Value;
                if (Math.Abs(Vector3.Dot(vector2, (Vector3) vectord2)) >= 0.98f)
                {
                    vector2 = Vector3.CalculatePerpendicularVector((Vector3) vectord2);
                }
                else
                {
                    Vector3 vector3 = Vector3.Cross(vector2, (Vector3) vectord2);
                    vector3.Normalize();
                    vector2 = Vector3.Cross((Vector3) vectord2, vector3);
                    vector2.Normalize();
                }
                randomPerpendicularVector = vector2;
            }
            MatrixD matrix = MatrixD.CreateWorld(position, randomPerpendicularVector, vectord2);
            using (List<MySpawnGroupDefinition.SpawnGroupPrefab>.Enumerator enumerator = spawnGroup.Prefabs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MySpawnGroupDefinition.SpawnGroupPrefab shipPrefab;
                    Vector3D vectord4 = Vector3D.Transform((Vector3D) shipPrefab.Position, matrix);
                    Stack<Action> callbacks = new Stack<Action>();
                    List<MyCubeGrid> createdGrids = new List<MyCubeGrid>();
                    if (!string.IsNullOrEmpty(shipPrefab.Behaviour))
                    {
                        callbacks.Push(delegate {
                            foreach (MyCubeGrid grid in createdGrids)
                            {
                                if (!MyDroneAI.SetAIToGrid(grid, shipPrefab.Behaviour, shipPrefab.BehaviourActivationDistance))
                                {
                                    object[] args = new object[] { grid.DisplayName };
                                    MyLog.Default.Error("Could not inject AI to encounter {0}. No remote control.", args);
                                }
                            }
                        });
                    }
                    callbacks.Push(delegate {
                        this.ChangeDroneOwnership(createdGrids, ownerId, antennaEntityId);
                    });
                    Vector3 initialLinearVelocity = new Vector3();
                    initialLinearVelocity = new Vector3();
                    MyPrefabManager.Static.SpawnPrefab(createdGrids, shipPrefab.SubtypeId, vectord4, (Vector3) randomPerpendicularVector, (Vector3) vectord2, initialLinearVelocity, initialLinearVelocity, null, null, SpawningOptions.SetAuthorship | SpawningOptions.SpawnRandomCargo, ownerId, true, callbacks);
                }
            }
            return true;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            m_definitionsByAntennaName = null;
            foreach (KeyValuePair<long, DroneInfo> pair in m_droneInfos)
            {
                VRage.Game.Entity.MyEntity entity;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(pair.Key, out entity, false);
                if (entity != null)
                {
                    this.UnregisterDrone(entity, false);
                }
            }
            m_droneInfos.Clear();
            m_droneInfos = null;
            m_pirateAntennas = null;
        }

        private void UnregisterDrone(VRage.Game.Entity.MyEntity entity, bool immediate = true)
        {
            long key = 0L;
            DroneInfo info = null;
            m_droneInfos.TryGetValue(entity.EntityId, out info);
            if (info != null)
            {
                key = info.AntennaEntityId;
                DroneInfo.Deallocate(info);
            }
            m_droneInfos.Remove(entity.EntityId, immediate);
            PirateAntennaInfo info2 = null;
            m_pirateAntennas.TryGetValue(key, out info2);
            if (info2 != null)
            {
                info2.SpawnedDrones--;
            }
            entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.DroneMainEntityOnClosing);
            MyRemoteControl control = entity as MyRemoteControl;
            if (control != null)
            {
                control.OwnershipChanged -= new Action<MyTerminalBlock>(this.DroneRemoteOwnershipChanged);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            bool flag1 = MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
            if (Sync.IsServer)
            {
                if (++m_ctr > 30)
                {
                    m_ctr = 0;
                    this.UpdateDroneSpawning();
                }
                if (++m_ctr2 > 100)
                {
                    m_ctr2 = 0;
                    this.UpdateDroneDespawning();
                }
            }
        }

        private void UpdateDroneDespawning()
        {
            foreach (KeyValuePair<long, DroneInfo> pair in m_droneInfos)
            {
                if (pair.Value.DespawnTime < MySandboxGame.TotalGamePlayTimeInMilliseconds)
                {
                    VRage.Game.Entity.MyEntity entity = null;
                    Sandbox.Game.Entities.MyEntities.TryGetEntityById(pair.Key, out entity, false);
                    if (entity == null)
                    {
                        DroneInfo.Deallocate(pair.Value);
                        m_droneInfos.Remove(pair.Key, false);
                        continue;
                    }
                    MyCubeGrid cubeGrid = entity as MyCubeGrid;
                    MyRemoteControl remote = entity as MyRemoteControl;
                    if (cubeGrid == null)
                    {
                        cubeGrid = remote.CubeGrid;
                    }
                    if (!this.CanDespawn(cubeGrid, remote))
                    {
                        pair.Value.DespawnTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + DRONE_DESPAWN_RETRY;
                    }
                    else
                    {
                        this.UnregisterDrone(entity, false);
                        Sandbox.Game.Entities.MyEntities.SendCloseRequest(cubeGrid);
                    }
                }
            }
            m_droneInfos.ApplyChanges();
        }

        private void UpdateDroneSpawning()
        {
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            m_iteratingAntennas = true;
            using (Dictionary<long, PirateAntennaInfo>.Enumerator enumerator = m_pirateAntennas.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<long, PirateAntennaInfo> current = enumerator.Current;
                    PirateAntennaInfo info = current.Value;
                    if (info.IsActive && ((info.AntennaDefinition != null) && ((totalGamePlayTimeInMilliseconds - info.LastGenerationGameTime) > info.AntennaDefinition.SpawnTimeMs)))
                    {
                        MyRadioAntenna entity = null;
                        Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRadioAntenna>(current.Key, out entity, false);
                        if (info.AntennaDefinition.SpawnGroupSampler != null)
                        {
                            MySpawnGroupDefinition spawnGroup = info.AntennaDefinition.SpawnGroupSampler.Sample();
                            if ((entity == null) || (spawnGroup == null))
                            {
                                info.LastGenerationGameTime = totalGamePlayTimeInMilliseconds;
                                continue;
                            }
                            bool flag = true;
                            if (entity.OwnerId != 0)
                            {
                                MyIdentity identity = MySession.Static.Players.TryGetIdentity(entity.OwnerId);
                                flag = (identity != null) && identity.BlockLimits.HasRemainingPCU;
                            }
                            if ((!MySession.Static.Settings.EnableDrones || ((info.SpawnedDrones >= info.AntennaDefinition.MaxDrones) || !flag)) || (m_droneInfos.Reader.Count >= MySession.Static.Settings.MaxDrones))
                            {
                                info.LastGenerationGameTime = totalGamePlayTimeInMilliseconds;
                            }
                            else
                            {
                                spawnGroup.ReloadPrefabs();
                                MatrixD worldMatrix = entity.WorldMatrix;
                                BoundingSphereD ed = new BoundingSphereD(worldMatrix.Translation, (double) entity.GetRadius());
                                bool flag2 = false;
                                foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                                {
                                    if (ed.Contains(player.GetPosition()) == ContainmentType.Contains)
                                    {
                                        Vector3D? nullable = null;
                                        int num2 = 0;
                                        while (true)
                                        {
                                            if (num2 < 10)
                                            {
                                                worldMatrix = entity.WorldMatrix;
                                                nullable = Sandbox.Game.Entities.MyEntities.FindFreePlace(worldMatrix.Translation + (MyUtils.GetRandomVector3Normalized() * info.AntennaDefinition.SpawnDistance), spawnGroup.SpawnRadius, 20, 5, 1f);
                                                if (nullable == null)
                                                {
                                                    num2++;
                                                    continue;
                                                }
                                            }
                                            Vector3? spawnUp = null;
                                            spawnUp = null;
                                            flag2 = this.SpawnDrone(entity, entity.OwnerId, nullable.Value, spawnGroup, spawnUp, spawnUp);
                                            break;
                                        }
                                        break;
                                    }
                                }
                                if (flag2)
                                {
                                    info.LastGenerationGameTime = totalGamePlayTimeInMilliseconds;
                                }
                            }
                            continue;
                        }
                        return;
                    }
                }
            }
            m_pirateAntennas.ApplyChanges();
            m_iteratingAntennas = false;
        }

        public static void UpdatePirateAntenna(long antennaEntityId, bool remove, bool activeState, StringBuilder antennaName)
        {
            if (m_pirateAntennas != null)
            {
                if (remove)
                {
                    m_pirateAntennas.Remove(antennaEntityId, !m_iteratingAntennas);
                }
                else
                {
                    string key = antennaName.ToString();
                    PirateAntennaInfo info = null;
                    if (!m_pirateAntennas.TryGetValue(antennaEntityId, out info))
                    {
                        MyPirateAntennaDefinition definition = null;
                        if (m_definitionsByAntennaName.TryGetValue(key, out definition))
                        {
                            info = PirateAntennaInfo.Allocate(definition);
                            info.IsActive = activeState;
                            m_pirateAntennas.Add(antennaEntityId, info, !m_iteratingAntennas);
                        }
                    }
                    else if (info.AntennaDefinition.Name != key)
                    {
                        MyPirateAntennaDefinition definition2 = null;
                        if (!m_definitionsByAntennaName.TryGetValue(key, out definition2))
                        {
                            PirateAntennaInfo.Deallocate(info);
                            m_pirateAntennas.Remove(antennaEntityId, !m_iteratingAntennas);
                        }
                        else
                        {
                            info.Reset(definition2);
                            info.IsActive = activeState;
                        }
                    }
                    else
                    {
                        info.IsActive = activeState;
                    }
                }
            }
        }

        public override bool IsRequiredByGame =>
            ((MyPerGameSettings.Game == GameEnum.SE_GAME) || (MyPerGameSettings.Game == GameEnum.VRS_GAME));

        private class DroneInfo
        {
            public long AntennaEntityId;
            public int DespawnTime;
            public static List<MyPirateAntennas.DroneInfo> m_pool = new List<MyPirateAntennas.DroneInfo>();

            public static MyPirateAntennas.DroneInfo Allocate(long antennaEntityId, int despawnTime)
            {
                MyPirateAntennas.DroneInfo info = null;
                if (m_pool.Count == 0)
                {
                    info = new MyPirateAntennas.DroneInfo();
                }
                else
                {
                    info = m_pool[m_pool.Count - 1];
                    m_pool.RemoveAt(m_pool.Count - 1);
                }
                info.AntennaEntityId = antennaEntityId;
                info.DespawnTime = despawnTime;
                return info;
            }

            public static void Deallocate(MyPirateAntennas.DroneInfo toDeallocate)
            {
                toDeallocate.AntennaEntityId = 0L;
                toDeallocate.DespawnTime = 0;
                m_pool.Add(toDeallocate);
            }
        }

        private class PirateAntennaInfo
        {
            public MyPirateAntennaDefinition AntennaDefinition;
            public int LastGenerationGameTime;
            public int SpawnedDrones;
            public bool IsActive;
            public List<int> SpawnPositionsIndexes;
            public int CurrentSpawnPositionsIndex = -1;
            public static List<MyPirateAntennas.PirateAntennaInfo> m_pool = new List<MyPirateAntennas.PirateAntennaInfo>();

            public static MyPirateAntennas.PirateAntennaInfo Allocate(MyPirateAntennaDefinition antennaDef)
            {
                MyPirateAntennas.PirateAntennaInfo info = null;
                if (m_pool.Count == 0)
                {
                    info = new MyPirateAntennas.PirateAntennaInfo();
                }
                else
                {
                    info = m_pool[m_pool.Count - 1];
                    m_pool.RemoveAt(m_pool.Count - 1);
                }
                info.Reset(antennaDef);
                return info;
            }

            public static void Deallocate(MyPirateAntennas.PirateAntennaInfo toDeallocate)
            {
                toDeallocate.AntennaDefinition = null;
                toDeallocate.SpawnPositionsIndexes = null;
                m_pool.Add(toDeallocate);
            }

            public void Reset(MyPirateAntennaDefinition antennaDef)
            {
                this.AntennaDefinition = antennaDef;
                this.LastGenerationGameTime = (MySandboxGame.TotalGamePlayTimeInMilliseconds + antennaDef.FirstSpawnTimeMs) - antennaDef.SpawnTimeMs;
                this.SpawnedDrones = 0;
                this.IsActive = false;
                this.SpawnPositionsIndexes = null;
                this.CurrentSpawnPositionsIndex = -1;
            }
        }
    }
}

