namespace SpaceEngineers.Game.World
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using Sandbox.Graphics.GUI;
    using SpaceEngineers.Game.Entities.Blocks;
    using SpaceEngineers.Game.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MySpaceRespawnComponent : MyRespawnComponentBase
    {
        private int m_lastUpdate;
        private bool m_updatingStopped;
        private int m_updateCtr;
        private bool m_synced;
        private readonly List<RespawnCooldownEntry> m_tmpRespawnTimes = new List<RespawnCooldownEntry>();
        [CompilerGenerated]
        private Action<ulong> RespawnDoneEvent;
        private const int REPEATED_DEATH_TIME_SECONDS = 10;
        private readonly CachingDictionary<RespawnKey, int> m_globalRespawnTimesMs = new CachingDictionary<RespawnKey, int>();
        private static List<MyRespawnShipDefinition> m_respawnShipsCache;
        private static readonly List<MyRespawnPointInfo> m_respanwPointsCache = new List<MyRespawnPointInfo>();
        private static List<MyObjectSeed> m_asteroidsCache;
        private static readonly List<Vector3D> m_playerPositionsCache = new List<Vector3D>();

        public event Action<ulong> RespawnDoneEvent
        {
            [CompilerGenerated] add
            {
                Action<ulong> respawnDoneEvent = this.RespawnDoneEvent;
                while (true)
                {
                    Action<ulong> a = respawnDoneEvent;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Combine(a, value);
                    respawnDoneEvent = Interlocked.CompareExchange<Action<ulong>>(ref this.RespawnDoneEvent, action3, a);
                    if (ReferenceEquals(respawnDoneEvent, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong> respawnDoneEvent = this.RespawnDoneEvent;
                while (true)
                {
                    Action<ulong> source = respawnDoneEvent;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Remove(source, value);
                    respawnDoneEvent = Interlocked.CompareExchange<Action<ulong>>(ref this.RespawnDoneEvent, action3, source);
                    if (ReferenceEquals(respawnDoneEvent, source))
                    {
                        return;
                    }
                }
            }
        }

        public override void AfterRemovePlayer(MyPlayer player)
        {
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            this.m_lastUpdate = MySandboxGame.TotalTimeInMilliseconds;
            this.m_updatingStopped = true;
            this.m_updateCtr = 0;
            if (!Sync.IsServer)
            {
                this.m_synced = false;
                this.RequestSync();
            }
            else
            {
                this.RequestSync();
                this.m_synced = true;
            }
        }

        public override void CloseRespawnScreen()
        {
            MyGuiScreenMedicals.Close();
        }

        public override void CloseRespawnScreenNow()
        {
            if (MyGuiScreenMedicals.Static != null)
            {
                MyGuiScreenMedicals.Static.CloseScreenNow();
            }
        }

        public override MyIdentity CreateNewIdentity(string identityName, MyPlayer.PlayerId playerId, string modelName, bool initialPlayer = false)
        {
            Vector3? colorMask = null;
            return Sync.Players.CreateNewIdentity(identityName, modelName, colorMask, initialPlayer);
        }

        private static Vector3D? FindFreeLocationCloseToAsteroid(BoundingSphereD searchArea, BoundingSphereD suppressedArea, bool takeOccupiedPositions, float collisionRadius, float minFreeRange, out Vector3 forward, out Vector3 up)
        {
            int num = 3;
            while (true)
            {
                if (num <= 0)
                {
                    Vector3D vectord = new Vector3D();
                    up = (Vector3) vectord;
                    vectord = new Vector3D();
                    forward = (Vector3) vectord;
                    return null;
                }
                bool flag = num == 1;
                BoundingSphereD sphere = new BoundingSphereD(searchArea.Center, searchArea.Radius / ((double) num));
                if (flag || (suppressedArea.Contains(sphere) != ContainmentType.Contains))
                {
                    using (MyUtils.ReuseCollection<MyObjectSeed>(ref m_asteroidsCache))
                    {
                        List<MyObjectSeed> asteroidsCache = m_asteroidsCache;
                        MyProceduralWorldGenerator.Static.OverlapAllAsteroidSeedsInSphere(sphere, asteroidsCache);
                        asteroidsCache.RemoveAll(delegate (MyObjectSeed x) {
                            MyObjectSeedType type = x.Params.Type;
                            return (type != MyObjectSeedType.Asteroid) && (type != MyObjectSeedType.AsteroidCluster);
                        });
                        if (asteroidsCache.Count != 0)
                        {
                            Vector3D spawnPosition = searchArea.Center;
                            asteroidsCache.Sort((a, b) => Vector3D.DistanceSquared(spawnPosition, a.BoundingVolume.Center).CompareTo(Vector3D.DistanceSquared(spawnPosition, b.BoundingVolume.Center)));
                            bool flag2 = false;
                            int index = asteroidsCache.Count - 1;
                            while (true)
                            {
                                if (index >= 0)
                                {
                                    bool flag3 = false;
                                    BoundingBoxD boundingVolume = asteroidsCache[index].BoundingVolume;
                                    if (suppressedArea.Contains(boundingVolume) != ContainmentType.Disjoint)
                                    {
                                        flag3 = true;
                                    }
                                    else
                                    {
                                        double radius = Math.Max(boundingVolume.HalfExtents.AbsMax() * 2.0, (double) minFreeRange);
                                        flag3 = !IsZoneFree(new BoundingSphereD(boundingVolume.Center, radius));
                                    }
                                    if (!flag3)
                                    {
                                        flag2 = true;
                                    }
                                    else
                                    {
                                        if (takeOccupiedPositions)
                                        {
                                            asteroidsCache.Add(asteroidsCache[index]);
                                        }
                                        asteroidsCache.RemoveAt(index);
                                    }
                                    index--;
                                    continue;
                                }
                                if (flag2 || flag)
                                {
                                    using (List<MyObjectSeed>.Enumerator enumerator = asteroidsCache.GetEnumerator())
                                    {
                                        while (true)
                                        {
                                            if (!enumerator.MoveNext())
                                            {
                                                break;
                                            }
                                            BoundingBoxD boundingVolume = enumerator.Current.BoundingVolume;
                                            Vector3D vectord2 = spawnPosition - boundingVolume.Center;
                                            if (vectord2.Normalize() < 0.01)
                                            {
                                                vectord2 = Vector3D.Forward;
                                            }
                                            Vector3D? freePosition = Sandbox.Game.Entities.MyEntities.FindFreePlace(Vector3D.Clamp(boundingVolume.Center + ((vectord2 * boundingVolume.HalfExtents.AbsMax()) * 10.0), boundingVolume.Min, boundingVolume.Max) + (((boundingVolume.HalfExtents.AbsMax() / 2.0) + collisionRadius) * vectord2), collisionRadius, 20, 5, 1f);
                                            if (((freePosition != null) && !MyPlanets.Static.GetPlanetAABBs().Any<BoundingBoxD>(x => (x.Contains(freePosition.Value) != ContainmentType.Disjoint))) && !asteroidsCache.Any<MyObjectSeed>(x => (x.BoundingVolume.Contains(freePosition.Value) != ContainmentType.Disjoint)))
                                            {
                                                forward = (Vector3) Vector3D.Normalize(boundingVolume.Center - freePosition.Value);
                                                up = MyUtils.GetRandomPerpendicularVector(ref forward);
                                                return new Vector3D?(freePosition.Value);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                num--;
            }
        }

        private static Vector3D? FindPositionAbovePlanet(Vector3D friendPosition, ref SpawnInfo info, bool testFreeZone, int distanceIteration, int maxDistanceIterations)
        {
            MyPlanet planet = info.Planet;
            float collisionRadius = info.CollisionRadius;
            Vector3D center = planet.PositionComp.WorldAABB.Center;
            Vector3D axis = Vector3D.Normalize(friendPosition - center);
            float optimalSpawnDistance = MySession.Static.Settings.OptimalSpawnDistance;
            float num3 = optimalSpawnDistance * 0.9f;
            int num4 = 0;
            while (true)
            {
                if (num4 >= 20)
                {
                    break;
                }
                Vector3D randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref axis);
                float num5 = optimalSpawnDistance * (MyUtils.GetRandomFloat(1.05f, 1.15f) + (distanceIteration * 0.05f));
                Vector3D globalPos = friendPosition + (randomPerpendicularVector * num5);
                globalPos = planet.GetClosestSurfacePointGlobal(ref globalPos);
                if ((!testFreeZone || (info.MinimalAirDensity <= 0f)) || (planet.GetAirDensity(globalPos) >= info.MinimalAirDensity))
                {
                    Vector3D vectord5 = Vector3D.Normalize(globalPos - center);
                    randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref vectord5);
                    bool flag = true;
                    Vector3 vector = (Vector3) (randomPerpendicularVector * collisionRadius);
                    Vector3 vector2 = Vector3.Cross(vector, (Vector3) vectord5);
                    MyOrientedBoundingBoxD xd2 = new MyOrientedBoundingBoxD(globalPos, new Vector3D((double) (collisionRadius * 2f), (double) Math.Min((float) 10f, (float) (collisionRadius * 0.5f)), (double) (collisionRadius * 2f)), Quaternion.CreateFromForwardUp((Vector3) randomPerpendicularVector, (Vector3) vectord5));
                    int num6 = -1;
                    int num7 = 0;
                    while (true)
                    {
                        if (num7 < 4)
                        {
                            num6 = -num6;
                            Vector3D closestSurfacePointGlobal = planet.GetClosestSurfacePointGlobal((globalPos + (vector * num6)) + (vector2 * ((num7 > 1) ? ((float) (-1)) : ((float) 1))));
                            if (xd2.Contains(ref closestSurfacePointGlobal))
                            {
                                num7++;
                                continue;
                            }
                            flag = false;
                        }
                        if (flag)
                        {
                            if (testFreeZone && !IsZoneFree(new BoundingSphereD(globalPos, (double) num3)))
                            {
                                distanceIteration++;
                                if (distanceIteration > maxDistanceIterations)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Vector3D? nullable = Sandbox.Game.Entities.MyEntities.FindFreePlace(globalPos + (Vector3D.Normalize(globalPos - center) * info.PlanetDeployAltitude), collisionRadius, 20, 5, 1f);
                                if (nullable != null)
                                {
                                    return new Vector3D?(nullable.Value);
                                }
                            }
                        }
                        break;
                    }
                }
                num4++;
            }
            return null;
        }

        private MyRespawnComponent FindRespawnById(long respawnBlockId, MyPlayer player)
        {
            MyCubeBlock entity = null;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(respawnBlockId, out entity, false))
            {
                if (!entity.IsWorking)
                {
                    return null;
                }
                MyRespawnComponent component = (MyRespawnComponent) entity.Components.Get<MyEntityRespawnComponentBase>();
                if (component == null)
                {
                    return null;
                }
                if (!component.SpawnWithoutOxygen && (component.GetOxygenLevel() == 0f))
                {
                    return null;
                }
                if ((player == null) || component.CanPlayerSpawn(player.Identity.IdentityId, true))
                {
                    return component;
                }
            }
            return null;
        }

        public static ClearToken<MyRespawnPointInfo> GetAvailableRespawnPoints(long? identityId, bool includePublicSpawns)
        {
            m_respanwPointsCache.AssertEmpty<MyRespawnPointInfo>();
            foreach (MyRespawnComponent component in MyRespawnComponent.GetAllRespawns())
            {
                MyTerminalBlock entity = component.Entity;
                if ((entity != null) && !entity.Closed)
                {
                    if (Sync.IsServer)
                    {
                        MyCubeGrid cubeGrid = entity.CubeGrid;
                        if (cubeGrid != null)
                        {
                            MyCubeGridSystems gridSystems = cubeGrid.GridSystems;
                            if (gridSystems != null)
                            {
                                gridSystems.UpdatePower();
                            }
                        }
                        entity.UpdateIsWorking();
                    }
                    if (entity.IsWorking && ((identityId == null) || component.CanPlayerSpawn(identityId.Value, includePublicSpawns)))
                    {
                        MyRespawnPointInfo item = new MyRespawnPointInfo();
                        IMySpawnBlock block2 = (IMySpawnBlock) entity;
                        item.MedicalRoomId = entity.EntityId;
                        item.MedicalRoomGridId = entity.CubeGrid.EntityId;
                        item.MedicalRoomName = !string.IsNullOrEmpty(block2.SpawnName) ? block2.SpawnName : ((entity.CustomName != null) ? entity.CustomName.ToString() : ((entity.Name != null) ? entity.Name : entity.ToString()));
                        item.OxygenLevel = component.GetOxygenLevel();
                        item.OwnerId = entity.IDModule.Owner;
                        MatrixD worldMatrix = entity.WorldMatrix;
                        Vector3D translation = worldMatrix.Translation;
                        Vector3D basePos = ((translation + (worldMatrix.Up * 20.0)) + (entity.WorldMatrix.Right * 20.0)) + (worldMatrix.Forward * 20.0);
                        Vector3D? nullable = Sandbox.Game.Entities.MyEntities.FindFreePlace(basePos, 1f, 20, 5, 1f);
                        if (nullable == null)
                        {
                            nullable = new Vector3D?(basePos);
                        }
                        item.PrefferedCameraPosition = nullable.Value;
                        item.MedicalRoomPosition = translation;
                        item.MedicalRoomUp = worldMatrix.Up;
                        if (entity.CubeGrid.Physics != null)
                        {
                            item.MedicalRoomVelocity = entity.CubeGrid.Physics.LinearVelocity;
                        }
                        m_respanwPointsCache.Add(item);
                    }
                }
            }
            return new ClearToken<MyRespawnPointInfo> { List = m_respanwPointsCache };
        }

        private static ClearToken<Vector3D> GetFriendlyPlayerPositions(long identityId)
        {
            int? nullable;
            m_playerPositionsCache.AssertEmpty<Vector3D>();
            if (MyFakes.USE_GPS_AS_FRIENDLY_SPAWN_LOCATIONS)
            {
                if (!MySession.Static.Gpss.ExistsForPlayer(identityId))
                {
                    return new List<Vector3D>().GetClearToken<Vector3D>();
                }
                nullable = null;
                List<Vector3D> list = (from x in MySession.Static.Gpss[identityId].Values select x.Coords).ToList<Vector3D>();
                list.ShuffleList<Vector3D>(0, nullable);
                return list.GetClearToken<Vector3D>();
            }
            int index = 0;
            using (IEnumerator<MyIdentity> enumerator = MySession.Static.Players.GetAllIdentities().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCharacter character = enumerator.Current.Character;
                    if ((character != null) && (!character.IsDead && !character.MarkedForClose))
                    {
                        MyIDModule module;
                        ((IMyComponentOwner<MyIDModule>) character).GetComponent(out module);
                        MyRelationsBetweenPlayerAndBlock block = MyIDModule.GetRelation(module.Owner, identityId, MyOwnershipShareModeEnum.Faction, MyRelationsBetweenPlayerAndBlock.Neutral, MyRelationsBetweenFactions.Neutral, MyRelationsBetweenPlayerAndBlock.FactionShare);
                        Vector3D position = character.PositionComp.GetPosition();
                        if (block == MyRelationsBetweenPlayerAndBlock.Neutral)
                        {
                            m_playerPositionsCache.Add(position);
                            continue;
                        }
                        if (block == MyRelationsBetweenPlayerAndBlock.FactionShare)
                        {
                            index++;
                            m_playerPositionsCache.Insert(index, position);
                        }
                    }
                }
            }
            m_playerPositionsCache.ShuffleList<Vector3D>(0, new int?(index));
            if ((index - 1) > 0)
            {
                nullable = null;
                m_playerPositionsCache.ShuffleList<Vector3D>(index - 1, nullable);
            }
            return m_playerPositionsCache.GetClearToken<Vector3D>();
        }

        public static MyRespawnShipDefinition GetRandomRespawnShip(MyPlanet planet)
        {
            using (ClearToken<MyRespawnShipDefinition> token = GetRespawnShips(planet))
            {
                List<MyRespawnShipDefinition> list = token.List;
                return ((list.Count != 0) ? list[MyUtils.GetRandomInt(list.Count)] : null);
            }
        }

        public int GetRespawnCooldownSeconds(MyPlayer.PlayerId controllerId, string respawnShipId)
        {
            if (MyDefinitionManager.Static.GetRespawnShipDefinition(respawnShipId) == null)
            {
                return 0;
            }
            RespawnKey key = new RespawnKey {
                ControllerId = controllerId,
                RespawnShipId = respawnShipId
            };
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            int num2 = totalTimeInMilliseconds;
            this.m_globalRespawnTimesMs.TryGetValue(key, out num2);
            return Math.Max((num2 - totalTimeInMilliseconds) / 0x3e8, 0);
        }

        public static ClearToken<MyRespawnShipDefinition> GetRespawnShips(MyPlanet planet)
        {
            MyUtils.Init<List<MyRespawnShipDefinition>>(ref m_respawnShipsCache).AssertEmpty<MyRespawnShipDefinition>();
            IEnumerable<MyRespawnShipDefinition> values = MyDefinitionManager.Static.GetRespawnShipDefinitions().Values;
            if (planet == null)
            {
                foreach (MyRespawnShipDefinition definition in values)
                {
                    if (definition.UseForSpace)
                    {
                        m_respawnShipsCache.Add(definition);
                    }
                }
            }
            else if (planet.HasAtmosphere)
            {
                float num = planet.Generator.Atmosphere.Density * 0.95f;
                foreach (MyRespawnShipDefinition definition2 in values)
                {
                    if (!definition2.UseForPlanetsWithAtmosphere)
                    {
                        continue;
                    }
                    if (num >= definition2.MinimalAirDensity)
                    {
                        m_respawnShipsCache.Add(definition2);
                    }
                }
            }
            else
            {
                foreach (MyRespawnShipDefinition definition3 in values)
                {
                    if (definition3.UseForPlanetsWithoutAtmosphere)
                    {
                        m_respawnShipsCache.Add(definition3);
                    }
                }
            }
            return m_respawnShipsCache.GetClearToken<MyRespawnShipDefinition>();
        }

        private static unsafe void GetSpawnPositionInSpace(SpawnInfo info, out Vector3D position, out Vector3 forward, out Vector3 up)
        {
            float optimalSpawnDistance = MySession.Static.Settings.OptimalSpawnDistance;
            float minFreeRange = optimalSpawnDistance * 0.9f;
            float collisionRadius = info.CollisionRadius;
            using (ClearToken<Vector3D> token = GetFriendlyPlayerPositions(info.IdentityId))
            {
                using (List<Vector3D>.Enumerator enumerator = token.List.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        Vector3D friendPosition = enumerator.Current;
                        if (!MyPlanets.Static.GetPlanetAABBs().Any<BoundingBoxD>(x => (x.Contains(friendPosition) != ContainmentType.Disjoint)))
                        {
                            Vector3D center = friendPosition + (MyUtils.GetRandomVector3Normalized() * optimalSpawnDistance);
                            BoundingSphereD suppressedArea = new BoundingSphereD(friendPosition, (double) minFreeRange);
                            Vector3D? nullable2 = FindFreeLocationCloseToAsteroid(new BoundingSphereD(center, 100000.0), suppressedArea, false, collisionRadius, minFreeRange, out forward, out up);
                            if (nullable2 != null)
                            {
                                position = nullable2.Value;
                                return;
                            }
                        }
                    }
                }
            }
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            BoundingBoxD box = new BoundingBoxD(new Vector3D(-25000.0), new Vector3D(25000.0));
            foreach (VRage.Game.Entity.MyEntity entity in Sandbox.Game.Entities.MyEntities.GetEntities())
            {
                if (entity.Parent != null)
                {
                    continue;
                }
                BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
                if (!(entity is MyPlanet))
                {
                    box.Include(worldAABB);
                    continue;
                }
                if (worldAABB.Contains(Vector3D.Zero) != ContainmentType.Disjoint)
                {
                    xd.Include(worldAABB);
                }
            }
            box.Include(xd.GetInflated((double) 25000.0));
            if (Sandbox.Game.Entities.MyEntities.IsWorldLimited())
            {
                Vector3D max = new Vector3D((double) Sandbox.Game.Entities.MyEntities.WorldSafeHalfExtent());
                BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref box;
                xdPtr1 = (BoundingBoxD*) new BoundingBoxD(Vector3D.Clamp(box.Min, -max, Vector3D.Zero), Vector3D.Clamp(box.Max, Vector3D.Zero, max));
            }
            Vector3D zero = Vector3D.Zero;
            int num4 = 0;
            while (true)
            {
                if (num4 < 50)
                {
                    zero = MyUtils.GetRandomPosition(ref box);
                    if (xd.Contains(zero) != ContainmentType.Disjoint)
                    {
                        num4++;
                        continue;
                    }
                }
                BoundingSphereD suppressedArea = new BoundingSphereD(xd.Center, Math.Max(0.0, xd.HalfExtents.Min()));
                Vector3D? nullable = FindFreeLocationCloseToAsteroid(new BoundingSphereD(zero, 100000.0), suppressedArea, true, collisionRadius, minFreeRange, out forward, out up);
                if (nullable != null)
                {
                    position = nullable.Value;
                    return;
                }
                if (MyGamePruningStructure.GetClosestPlanet(zero) != null)
                {
                    GetSpawnPositionNearPlanet(info, out position, out forward, out up);
                    return;
                }
                forward = MyUtils.GetRandomVector3Normalized();
                up = Vector3.CalculatePerpendicularVector((Vector3) forward);
                Vector3D? nullable3 = Sandbox.Game.Entities.MyEntities.FindFreePlace(zero, collisionRadius, 20, 5, 1f);
                position = (nullable3 != null) ? nullable3.GetValueOrDefault() : zero;
                break;
            }
        }

        private static void GetSpawnPositionNearPlanet(SpawnInfo info, out Vector3D position, out Vector3 forward, out Vector3 up)
        {
            Vector3 vector;
            MyPlanet planet;
            Vector3D center;
            int num3;
            bool flag = false;
            position = Vector3D.Zero;
            using (ClearToken<Vector3D> token = GetFriendlyPlayerPositions(info.IdentityId))
            {
                List<Vector3D> list = token.List;
                BoundingBoxD worldAABB = info.Planet.PositionComp.WorldAABB;
                int index = list.Count - 1;
                while (true)
                {
                    if (index < 0)
                    {
                        for (int i = 0; (i < 30) && !flag; i += 3)
                        {
                            using (List<Vector3D>.Enumerator enumerator = list.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    Vector3D? nullable = FindPositionAbovePlanet(enumerator.Current, ref info, true, i, i + 3);
                                    if (nullable != null)
                                    {
                                        position = nullable.Value;
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    if (worldAABB.Contains(list[index]) == ContainmentType.Disjoint)
                    {
                        list.RemoveAt(index);
                    }
                    index--;
                }
            }
            if (flag)
            {
                goto TR_0002;
            }
            else
            {
                planet = info.Planet;
                center = planet.PositionComp.WorldVolume.Center;
                num3 = 0;
            }
            goto TR_000E;
        TR_0002:
            vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            if (Vector3.IsZero(vector))
            {
                vector = Vector3.Up;
            }
            Vector3D vectord = Vector3D.Normalize(vector);
            forward = Vector3.CalculatePerpendicularVector((Vector3) -vectord);
            up = (Vector3) -vectord;
            return;
        TR_000E:
            while (true)
            {
                if (num3 < 0x19)
                {
                    Vector3 vector2 = MyUtils.GetRandomVector3Normalized();
                    if ((vector2.Dot(MySector.DirectionToSunNormalized) < 0f) && (num3 < 20))
                    {
                        vector2 = -vector2;
                    }
                    position = center + (vector2 * planet.AverageRadius);
                    Vector3D? nullable2 = FindPositionAbovePlanet(position, ref info, num3 < 20, 0, 30);
                    if (nullable2 == null)
                    {
                        break;
                    }
                    position = nullable2.Value;
                    if ((position - center).Dot(MySector.DirectionToSunNormalized) <= 0.0)
                    {
                        break;
                    }
                    goto TR_0002;
                }
                else
                {
                    goto TR_0002;
                }
                break;
            }
            num3++;
            goto TR_000E;
        }

        public override bool HandleRespawnRequest(bool joinGame, bool newIdentity, long respawnEntityId, string respawnShipId, MyPlayer.PlayerId playerId, Vector3D? spawnPosition, SerializableDefinitionId? botDefinitionId, bool realPlayer, string modelName, Color color)
        {
            MyRespawnComponent component;
            MyPlayer playerById = Sync.Players.GetPlayerById(playerId);
            bool flag = newIdentity || ReferenceEquals(playerById, null);
            if (!MySessionComponentMissionTriggers.CanRespawn(playerId))
            {
                return false;
            }
            Vector3D zero = Vector3D.Zero;
            if ((playerById != null) && (playerById.Character != null))
            {
                zero = playerById.Character.PositionComp.GetPosition();
            }
            if (TryFindExistingCharacter(playerById))
            {
                return true;
            }
            MyBotDefinition botDefinition = null;
            if (botDefinitionId != null)
            {
                MyDefinitionManager.Static.TryGetBotDefinition(botDefinitionId.Value, out botDefinition);
            }
            long? planetId = null;
            MyPlanet entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(respawnEntityId, false) as MyPlanet;
            if (entityById != null)
            {
                planetId = new long?(respawnEntityId);
                if (string.IsNullOrEmpty(respawnShipId))
                {
                    MyRespawnShipDefinition randomRespawnShip = GetRandomRespawnShip(entityById);
                    if (randomRespawnShip != null)
                    {
                        respawnShipId = randomRespawnShip.Id.SubtypeName;
                    }
                }
            }
            if (flag)
            {
                goto TR_0017;
            }
            else
            {
                if (respawnShipId != null)
                {
                    this.SpawnAtShip(playerById, respawnShipId, botDefinition, modelName, new Color?(color), planetId);
                    return true;
                }
                if (spawnPosition != null)
                {
                    Vector3D vectord3;
                    Vector3D down = MyGravityProviderSystem.CalculateTotalGravityInPoint(spawnPosition.Value);
                    if (Vector3D.IsZero(down))
                    {
                        down = Vector3D.Down;
                    }
                    else
                    {
                        down.Normalize();
                    }
                    down.CalculatePerpendicularVector(out vectord3);
                    playerById.SpawnAt(MatrixD.CreateWorld(spawnPosition.Value, vectord3, -down), Vector3.Zero, null, botDefinition, true, modelName, new Color?(color));
                    return true;
                }
                component = null;
                if ((respawnEntityId == 0) || !MyFakes.SHOW_FACTIONS_GUI)
                {
                    long? nullable1;
                    if (!MySession.Static.CreativeMode)
                    {
                        nullable1 = new long?(playerById.Identity.IdentityId);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    using (ClearToken<MyRespawnPointInfo> token = GetAvailableRespawnPoints(nullable1, false))
                    {
                        List<MyRespawnPointInfo> list = token.List;
                        if (joinGame && (list.Count > 0))
                        {
                            component = this.FindRespawnById(list[MyRandom.Instance.Next(0, list.Count)].MedicalRoomId, null);
                        }
                        goto TR_001A;
                    }
                }
                component = this.FindRespawnById(respawnEntityId, playerById);
                if (component == null)
                {
                    return false;
                }
            }
            goto TR_001A;
        TR_0017:
            if (flag)
            {
                bool resetIdentity = false;
                if (MySession.Static.Settings.PermanentDeath.Value)
                {
                    MyIdentity identity = Sync.Players.TryGetPlayerIdentity(playerId);
                    if (identity != null)
                    {
                        resetIdentity = identity.FirstSpawnDone;
                    }
                }
                if (playerById == null)
                {
                    Vector3? colorMask = null;
                    MyIdentity identity = Sync.Players.CreateNewIdentity(playerId.SteamId.ToString(), null, colorMask, false);
                    playerById = Sync.Players.CreateNewPlayer(identity, playerId, playerId.SteamId.ToString(), realPlayer, false, false);
                    resetIdentity = false;
                }
                if (!MySession.Static.CreativeMode)
                {
                    this.SpawnAsNewPlayer(playerById, zero, respawnShipId, planetId, resetIdentity, botDefinition, modelName, color);
                }
                else
                {
                    Vector3D? nullable4 = Sandbox.Game.Entities.MyEntities.FindFreePlace(zero, 2f, 200, 5, 1f);
                    if (nullable4 != null)
                    {
                        zero = nullable4.Value;
                    }
                    playerById.SpawnAt(Matrix.CreateTranslation((Vector3) zero), Vector3.Zero, null, botDefinition, true, modelName, new Color?(color));
                }
            }
            return true;
        TR_001A:
            if (component != null)
            {
                this.SpawnInRespawn(playerById, component, botDefinition, modelName, color);
            }
            else
            {
                flag = true;
            }
            goto TR_0017;
        }

        public override void InitFromCheckpoint(MyObjectBuilder_Checkpoint checkpoint)
        {
            List<MyObjectBuilder_Checkpoint.RespawnCooldownItem> respawnCooldowns = checkpoint.RespawnCooldowns;
            this.m_lastUpdate = MySandboxGame.TotalTimeInMilliseconds;
            this.m_globalRespawnTimesMs.Clear();
            if (respawnCooldowns != null)
            {
                foreach (MyObjectBuilder_Checkpoint.RespawnCooldownItem item in respawnCooldowns)
                {
                    MyPlayer.PlayerId id = new MyPlayer.PlayerId {
                        SteamId = item.PlayerSteamId,
                        SerialId = item.PlayerSerialId
                    };
                    RespawnKey key = new RespawnKey {
                        ControllerId = id,
                        RespawnShipId = item.RespawnShipId
                    };
                    this.m_globalRespawnTimesMs.Add(key, item.Cooldown + this.m_lastUpdate, true);
                }
            }
        }

        public override bool IsInRespawnScreen() => 
            ((MyGuiScreenMedicals.Static != null) && (MyGuiScreenMedicals.Static.State == MyGuiScreenState.OPENED));

        private static bool IsZoneFree(BoundingSphereD safeZone)
        {
            using (ClearToken<VRage.Game.Entity.MyEntity> token = Sandbox.Game.Entities.MyEntities.GetTopMostEntitiesInSphere(ref safeZone).GetClearToken<VRage.Game.Entity.MyEntity>())
            {
                using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = token.List.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current is MyCubeGrid)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public override void LoadData()
        {
            base.LoadData();
            Sync.Players.RespawnComponent = this;
            Sync.Players.LocalRespawnRequested += new Action<string, Color>(this.OnLocalRespawnRequest);
            MyRespawnComponentBase.ShowPermaWarning = false;
        }

        private void OnLocalRespawnRequest(string model, Color color)
        {
            if (!MyFakes.SHOW_FACTIONS_GUI)
            {
                MyPlayerCollection.RespawnRequest(ReferenceEquals(MySession.Static.LocalHumanPlayer, null), false, 0L, null, 0, model, color);
            }
            else
            {
                ulong num = (MySession.Static.LocalHumanPlayer != null) ? MySession.Static.LocalHumanPlayer.Id.SteamId : Sync.MyId;
                int num2 = (MySession.Static.LocalHumanPlayer != null) ? MySession.Static.LocalHumanPlayer.Id.SerialId : 0;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int>(s => new Action<ulong, int>(MySpaceRespawnComponent.RespawnRequest_Implementation), num, num2, targetEndpoint, position);
            }
        }

        [Event(null, 0xab), Reliable, Server]
        private static void OnSyncCooldownRequest()
        {
            if (MyEventContext.Current.IsLocallyInvoked)
            {
                Static.SyncCooldownToPlayer(Sync.MyId, true);
            }
            else
            {
                Static.SyncCooldownToPlayer(MyEventContext.Current.Sender.Value, false);
            }
        }

        [Event(null, 0xb9), Reliable, Client]
        private static void OnSyncCooldownResponse(List<RespawnCooldownEntry> entries)
        {
            Static.SyncCooldownResponse(entries);
        }

        private void PutPlayerInRespawnGrid(MyPlayer player, List<MyCubeGrid> respawnGrids, MyBotDefinition botDefinition, string modelName, Color? color, bool spawnWithDefaultItems)
        {
            List<MyCockpit> list = new List<MyCockpit>();
            using (List<MyCubeGrid>.Enumerator enumerator = respawnGrids.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyFatBlockReader<MyCockpit> fatBlocks = enumerator.Current.GetFatBlocks<MyCockpit>();
                    foreach (MyCockpit cockpit2 in fatBlocks)
                    {
                        if (cockpit2.IsFunctional)
                        {
                            list.Add(cockpit2);
                        }
                    }
                }
            }
            if (list.Count > 1)
            {
                list.Sort(delegate (MyCockpit cockpitA, MyCockpit cockpitB) {
                    int num = cockpitB.EnableShipControl.CompareTo(cockpitA.EnableShipControl);
                    if (num != 0)
                    {
                        return num;
                    }
                    int num2 = cockpitB.IsMainCockpit.CompareTo(cockpitA.IsMainCockpit);
                    return (num2 == 0) ? 0 : num2;
                });
            }
            MyCockpit cockpit = null;
            if (list.Count > 0)
            {
                cockpit = list[0];
            }
            MatrixD identity = MatrixD.Identity;
            if (cockpit != null)
            {
                identity = cockpit.WorldMatrix;
                identity.Translation = (cockpit.WorldMatrix.Translation - Vector3.Up) - Vector3.Forward;
            }
            else if (respawnGrids.Count > 0)
            {
                identity.Translation = respawnGrids[0].PositionComp.WorldAABB.Center + respawnGrids[0].PositionComp.WorldAABB.HalfExtents;
            }
            MySessionComponentTrash.CloseRespawnShip(player);
            foreach (MyCubeGrid grid in respawnGrids)
            {
                grid.ChangeGridOwnership(player.Identity.IdentityId, MyOwnershipShareModeEnum.None);
                grid.IsRespawnGrid = true;
                grid.m_playedTime = 0;
                player.RespawnShip.Add(grid.EntityId);
            }
            this.SpawnInCockpit(player, cockpit, botDefinition, identity, modelName, color, spawnWithDefaultItems);
        }

        private void RemoveOldRespawnTimes()
        {
            MyDefinitionManager.Static.GetRespawnShipDefinitions();
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            foreach (RespawnKey key in this.m_globalRespawnTimesMs.Keys)
            {
                int num2 = this.m_globalRespawnTimesMs[key];
                if ((totalTimeInMilliseconds - num2) >= 0)
                {
                    this.m_globalRespawnTimesMs.Remove(key, false);
                }
            }
            this.m_globalRespawnTimesMs.ApplyRemovals();
        }

        [Event(null, 0x1c3), Reliable, Client]
        private static void RequestRespawnAtSpawnPoint(long spawnPointId)
        {
            string model = null;
            Color red = Color.Red;
            MyLocalCache.GetCharacterInfoFromInventoryConfig(ref model, ref red);
            MyPlayerCollection.RespawnRequest(ReferenceEquals(MySession.Static.LocalCharacter, null), false, spawnPointId, null, 0, model, red);
        }

        public void RequestSync()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MySpaceRespawnComponent.OnSyncCooldownRequest), targetEndpoint, position);
        }

        public void ResetRespawnCooldown(MyPlayer.PlayerId controllerId)
        {
            DictionaryReader<string, MyRespawnShipDefinition> respawnShipDefinitions = MyDefinitionManager.Static.GetRespawnShipDefinitions();
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            float spawnShipTimeMultiplier = MySession.Static.Settings.SpawnShipTimeMultiplier;
            foreach (KeyValuePair<string, MyRespawnShipDefinition> pair in respawnShipDefinitions)
            {
                RespawnKey key = new RespawnKey {
                    ControllerId = controllerId,
                    RespawnShipId = pair.Key
                };
                if (spawnShipTimeMultiplier != 0f)
                {
                    this.m_globalRespawnTimesMs.Add(key, totalTimeInMilliseconds + ((int) ((pair.Value.Cooldown * 0x3e8) * spawnShipTimeMultiplier)), true);
                    continue;
                }
                this.m_globalRespawnTimesMs.Remove(key, false);
            }
        }

        [Event(null, 0x15d), Reliable, Server]
        private static void RespawnRequest_Implementation(ulong steamPlayerId, int serialId)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MyEventContext.Current.Sender.Value != steamPlayerId))
            {
                ((MyMultiplayerServerBase) MyMultiplayer.Static).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyPlayer.PlayerId id = new MyPlayer.PlayerId(steamPlayerId, serialId);
                MyPlayer playerById = Sync.Players.GetPlayerById(id);
                if (((MyScenarioSystem.Static == null) || ((MyScenarioSystem.Static.GameState != MyScenarioSystem.MyState.JoinScreen) && (MyScenarioSystem.Static.GameState != MyScenarioSystem.MyState.WaitingForClients))) && (playerById != null))
                {
                    Vector3D? lastDeathPosition;
                    if (TryFindExistingCharacter(playerById))
                    {
                        if (!Sandbox.Engine.Platform.Game.IsDedicated && (Sync.MyId == steamPlayerId))
                        {
                            ShowMotD(MOTDData.Construct());
                        }
                        else
                        {
                            lastDeathPosition = null;
                            MyMultiplayer.RaiseStaticEvent<MOTDData>(s => new Action<MOTDData>(MySpaceRespawnComponent.ShowMotD), MOTDData.Construct(), new EndpointId(steamPlayerId), lastDeathPosition);
                        }
                    }
                    else
                    {
                        bool flag = true;
                        long medicalRoomId = 0L;
                        if (MySession.Static.Settings.EnableAutorespawn)
                        {
                            lastDeathPosition = playerById.Identity.LastDeathPosition;
                            if (lastDeathPosition != null)
                            {
                                using (ClearToken<MyRespawnPointInfo> token = GetAvailableRespawnPoints(new long?(playerById.Identity.IdentityId), false))
                                {
                                    if (token.List.Count > 0)
                                    {
                                        lastDeathPosition = playerById.Identity.LastDeathPosition;
                                        Vector3D lastPlayerPosition = lastDeathPosition.Value;
                                        MyRespawnPointInfo info = token.List.MinBy<MyRespawnPointInfo>(x => (float) Vector3D.Distance(x.MedicalRoomPosition, lastPlayerPosition));
                                        if ((MySession.Static.ElapsedGameTime - playerById.Identity.LastRespawnTime) < TimeSpan.FromSeconds(10.0))
                                        {
                                            medicalRoomId = info.MedicalRoomId;
                                        }
                                        else
                                        {
                                            lastDeathPosition = null;
                                            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySpaceRespawnComponent.RequestRespawnAtSpawnPoint), info.MedicalRoomId, new EndpointId(steamPlayerId), lastDeathPosition);
                                            flag = false;
                                        }
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            lastDeathPosition = null;
                            MyMultiplayer.RaiseStaticEvent<MOTDData, long>(s => new Action<MOTDData, long>(MySpaceRespawnComponent.ShowMedicalScreen_Implementation), MOTDData.Construct(), medicalRoomId, new EndpointId(steamPlayerId), lastDeathPosition);
                        }
                    }
                    NotifyRespawnRequested(playerById);
                }
            }
        }

        private static void SaveRespawnShip(MyPlayer player)
        {
            MyCubeGrid oldHome;
            if ((MySession.Static.Settings.RespawnShipDelete && (player.RespawnShip != null)) && Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(player.RespawnShip[0], out oldHome, false))
            {
                ulong sizeInBytes = 0UL;
                string sessionPath = MySession.Static.CurrentPath;
                Console.WriteLine(sessionPath);
                string fileName = "RS_" + player.Client.SteamUserId + ".sbr";
                Parallel.Start(() => MyLocalCache.SaveRespawnShip((MyObjectBuilder_CubeGrid) oldHome.GetObjectBuilder(false), sessionPath, fileName, out sizeInBytes));
            }
        }

        public override void SaveToCheckpoint(MyObjectBuilder_Checkpoint checkpoint)
        {
            List<MyObjectBuilder_Checkpoint.RespawnCooldownItem> respawnCooldowns = checkpoint.RespawnCooldowns;
            foreach (KeyValuePair<RespawnKey, int> pair in this.m_globalRespawnTimesMs)
            {
                int num = pair.Value - this.m_lastUpdate;
                if (num > 0)
                {
                    MyObjectBuilder_Checkpoint.RespawnCooldownItem item = new MyObjectBuilder_Checkpoint.RespawnCooldownItem {
                        PlayerSteamId = pair.Key.ControllerId.SteamId,
                        PlayerSerialId = pair.Key.ControllerId.SerialId,
                        RespawnShipId = pair.Key.RespawnShipId,
                        Cooldown = num
                    };
                    respawnCooldowns.Add(item);
                }
            }
        }

        public override void SetNoRespawnText(StringBuilder text, int timeSec)
        {
            MyGuiScreenMedicals.SetNoRespawnText(text, timeSec);
        }

        public override void SetupCharacterDefault(MyPlayer player, MyWorldGenerator.Args args)
        {
            string firstRespawnShip = MyDefinitionManager.Static.GetFirstRespawnShip();
            Color? color = null;
            long? planetId = null;
            this.SpawnAtShip(player, firstRespawnShip, null, null, color, planetId);
        }

        public override void SetupCharacterFromStarts(MyPlayer player, MyWorldGeneratorStartingStateBase[] playerStarts, MyWorldGenerator.Args args)
        {
            playerStarts[MyUtils.GetRandomInt(playerStarts.Length)].SetupCharacter(args);
        }

        [Event(null, 460), Reliable, Client]
        private static void ShowMedicalScreen_Implementation(MOTDData motd, long restrictedRespawn)
        {
            int num1;
            if (MySession.Static.Factions.JoinableFactionsPresent || (MySession.Static.Settings.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION))
            {
                num1 = (int) ReferenceEquals(MySession.Static.Factions.GetPlayerFaction(MySession.Static.LocalPlayerId), null);
            }
            else
            {
                num1 = 0;
            }
            MyGuiScreenMedicals screen = new MyGuiScreenMedicals((bool) num1, restrictedRespawn);
            MyGuiSandbox.AddScreen(screen);
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                if (motd.HasMessage)
                {
                    screen.SetMotD(motd.GetMessage().ToString());
                }
                else
                {
                    screen.HideMotdButton();
                }
                if (motd.HasUrl)
                {
                    MyGuiScreenMedicals.ShowMotDUrl(motd.Url);
                }
                MySession.ShowMotD = false;
            }
        }

        [Event(null, 0x1e7), Reliable, Client]
        private static void ShowMotD(MOTDData motd)
        {
            if (motd.HasMessage)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenMotD(motd.GetMessage()));
            }
            if (motd.HasUrl)
            {
                MyGuiScreenMedicals.ShowMotDUrl(motd.Url);
            }
            MySession.ShowMotD = false;
        }

        public void SpawnAsNewPlayer(MyPlayer player, Vector3D currentPosition, string respawnShipId, long? planetId, bool resetIdentity, MyBotDefinition botDefinition, string modelName, Color color)
        {
            if ((Sync.IsServer && (player != null)) && (player.Identity != null))
            {
                if (resetIdentity)
                {
                    base.ResetPlayerIdentity(player, modelName, color);
                }
                if (respawnShipId != null)
                {
                    this.SpawnAtShip(player, respawnShipId, botDefinition, modelName, new Color?(color), planetId);
                }
                else
                {
                    this.SpawnInSuit(player, null, botDefinition, modelName, color);
                }
                if (((MySession.Static != null) && ((player.Character != null) && (MySession.Static.Settings.EnableOxygen && (player.Character.OxygenComponent != null)))) && player.Character.OxygenComponent.NeedsOxygenFromSuit)
                {
                    player.Character.OxygenComponent.SwitchHelmet();
                }
            }
        }

        public unsafe void SpawnAtShip(MyPlayer player, string respawnShipId, MyBotDefinition botDefinition, string modelName, Color? color, long? planetId = new long?())
        {
            if (Sync.IsServer)
            {
                this.ResetRespawnCooldown(player.Id);
                if (Sync.MultiplayerActive)
                {
                    this.SyncCooldownToPlayer(player.Id.SteamId, player.Id.SteamId == Sync.MyId);
                }
                MyRespawnShipDefinition respawnShip = MyDefinitionManager.Static.GetRespawnShipDefinition(respawnShipId);
                if (respawnShip != null)
                {
                    SpawnInfo* infoPtr1;
                    SpawnInfo info = new SpawnInfo {
                        IdentityId = player.Identity.IdentityId,
                        PlanetDeployAltitude = respawnShip.PlanetDeployAltitude,
                        CollisionRadius = respawnShip.Prefab.BoundingSphere.Radius
                    };
                    infoPtr1->MinimalAirDensity = respawnShip.UseForPlanetsWithoutAtmosphere ? 0f : respawnShip.MinimalAirDensity;
                    infoPtr1 = (SpawnInfo*) ref info;
                    SpawnInfo spawnInfo = info;
                    Vector3 zero = Vector3.Zero;
                    Vector3 forward = Vector3.Zero;
                    Vector3D position = Vector3D.Zero;
                    if (planetId != null)
                    {
                        MyPlanet entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(planetId.Value, false) as MyPlanet;
                        if (entityById == null)
                        {
                            planetId = null;
                        }
                        else
                        {
                            spawnInfo.Planet = entityById;
                            GetSpawnPositionNearPlanet(spawnInfo, out position, out forward, out zero);
                        }
                    }
                    if (planetId == null)
                    {
                        spawnInfo.Planet = null;
                        GetSpawnPositionInSpace(spawnInfo, out position, out forward, out zero);
                    }
                    Vector3D? nullable = null;
                    MyMultiplayer.RaiseStaticEvent<Vector3D>(s => new Action<Vector3D>(MySession.SetSpectatorPositionFromServer), position, new EndpointId(player.Id.SteamId), nullable);
                    Stack<Action> callbacks = new Stack<Action>();
                    List<MyCubeGrid> respawnGrids = new List<MyCubeGrid>();
                    if (!MyFakes.USE_GPS_AS_FRIENDLY_SPAWN_LOCATIONS)
                    {
                        callbacks.Push(delegate {
                            if (respawnGrids.Count != 0)
                            {
                                MyCubeGrid grid = respawnGrids[0];
                                MyGps gps1 = new MyGps();
                                gps1.ShowOnHud = true;
                                gps1.Name = new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.GPS_Respawn_Location_Name)).Append(" - ").Append(grid.EntityId).ToString();
                                gps1.DisplayName = MyTexts.GetString(MySpaceTexts.GPS_Respawn_Location_Name);
                                gps1.DiscardAt = null;
                                gps1.Coords = new Vector3(0f, 0f, 0f);
                                gps1.Description = MyTexts.GetString(MySpaceTexts.GPS_Respawn_Location_Desc);
                                gps1.AlwaysVisible = true;
                                gps1.GPSColor = new Color(0x75, 0xc9, 0xf1);
                                gps1.IsContainerGPS = true;
                                MyGps gps = gps1;
                                MySession.Static.Gpss.SendAddGps(spawnInfo.IdentityId, ref gps, grid.EntityId, false);
                            }
                        });
                    }
                    if (!Vector3.IsZero(ref respawnShip.InitialAngularVelocity) || !Vector3.IsZero(ref respawnShip.InitialLinearVelocity))
                    {
                        callbacks.Push(delegate {
                            if (respawnGrids.Count != 0)
                            {
                                MyCubeGrid local1 = respawnGrids[0];
                                MatrixD worldMatrix = local1.WorldMatrix;
                                MyGridPhysics physics = local1.Physics;
                                physics.LinearVelocity = (Vector3) Vector3D.TransformNormal(respawnShip.InitialLinearVelocity, worldMatrix);
                                physics.AngularVelocity = (Vector3) Vector3D.TransformNormal(respawnShip.InitialAngularVelocity, worldMatrix);
                                for (int i = 1; i < respawnGrids.Count; i++)
                                {
                                    MyGridPhysics physics2 = respawnGrids[i].Physics;
                                    physics2.AngularVelocity = physics.AngularVelocity;
                                    physics2.LinearVelocity = physics.GetVelocityAtPoint(physics2.CenterOfMassWorld);
                                }
                            }
                        });
                    }
                    callbacks.Push(delegate {
                        this.PutPlayerInRespawnGrid(player, respawnGrids, botDefinition, modelName, color, respawnShip.SpawnWithDefaultItems);
                        if (respawnGrids.Count != 0)
                        {
                            MySession.SendVicinityInformation(respawnGrids[0].EntityId, new EndpointId(player.Client.SteamUserId));
                        }
                        this.RespawnDoneEvent.InvokeIfNotNull<ulong>(player.Client.SteamUserId);
                    });
                    Vector3 initialLinearVelocity = new Vector3();
                    initialLinearVelocity = new Vector3();
                    MyPrefabManager.Static.SpawnPrefab(respawnGrids, respawnShip.Prefab.Id.SubtypeName, position, forward, zero, initialLinearVelocity, initialLinearVelocity, null, null, SpawningOptions.SetAuthorship | SpawningOptions.RotateFirstCockpitTowardsDirection, spawnInfo.IdentityId, true, callbacks);
                }
            }
        }

        private void SpawnInCockpit(MyPlayer player, MyCockpit cockpit, MyBotDefinition botDefinition, MatrixD matrix, string modelName, Color? color, bool spawnWithDefaultItems)
        {
            Vector3? nullable1;
            if (color != null)
            {
                nullable1 = new Vector3?(color.Value.ToVector3());
            }
            else
            {
                nullable1 = null;
            }
            MyCharacter newCharacter = MyCharacter.CreateCharacter(matrix, Vector3.Zero, player.Identity.DisplayName, modelName, nullable1, botDefinition, true, false, cockpit, true, player.Identity.IdentityId, spawnWithDefaultItems);
            if (cockpit == null)
            {
                Sync.Players.SetPlayerCharacter(player, newCharacter, null);
            }
            else
            {
                cockpit.AttachPilot(newCharacter, false, false, true);
                newCharacter.SetPlayer(player, true);
                Sync.Players.SetControlledEntity(player.Id, cockpit);
                if (MyVisualScriptLogicProvider.PlayerEnteredCockpit != null)
                {
                    MyVisualScriptLogicProvider.PlayerEnteredCockpit(cockpit.Name, player.Identity.IdentityId, cockpit.CubeGrid.Name);
                }
            }
            Sync.Players.RevivePlayer(player);
        }

        private void SpawnInRespawn(MyPlayer player, MyRespawnComponent respawn, MyBotDefinition botDefinition, string modelName, Color color)
        {
            if (respawn.Entity == null)
            {
                this.SpawnInSuit(player, null, botDefinition, modelName, color);
            }
            else
            {
                VRage.Game.Entity.MyEntity topMostParent = respawn.Entity.GetTopMostParent(null);
                if (topMostParent.Physics == null)
                {
                    this.SpawnInSuit(player, topMostParent, botDefinition, modelName, color);
                }
                else
                {
                    MyCockpit entity = respawn.Entity as MyCockpit;
                    if (entity != null)
                    {
                        this.SpawnInCockpit(player, entity, botDefinition, entity.WorldMatrix, modelName, new Color?(color), true);
                    }
                    else
                    {
                        MatrixD spawnPosition = respawn.GetSpawnPosition();
                        player.SpawnAt(spawnPosition, topMostParent.Physics.GetVelocityAtPoint(spawnPosition.Translation), topMostParent, botDefinition, true, modelName, new Color?(color));
                        MyMedicalRoom room = respawn.Entity as MyMedicalRoom;
                        if (room != null)
                        {
                            room.TryTakeSpawneeOwnership(player);
                            room.TrySetFaction(player);
                            if (room.ForceSuitChangeOnRespawn)
                            {
                                player.Character.ChangeModelAndColor(room.RespawnSuitName, player.Character.ColorMask, false, 0L);
                                if ((MySession.Static.Settings.EnableOxygen && (player.Character.OxygenComponent != null)) && player.Character.OxygenComponent.NeedsOxygenFromSuit)
                                {
                                    player.Character.OxygenComponent.SwitchHelmet();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SpawnInSuit(MyPlayer player, VRage.Game.Entity.MyEntity spawnedBy, MyBotDefinition botDefinition, string modelName, Color color)
        {
            Vector3D vectord;
            Vector3 vector;
            Vector3 vector2;
            SpawnInfo info = new SpawnInfo {
                CollisionRadius = 10f,
                PlanetDeployAltitude = 10f,
                IdentityId = player.Identity.IdentityId
            };
            GetSpawnPositionInSpace(info, out vectord, out vector, out vector2);
            MyCharacter newCharacter = MyCharacter.CreateCharacter(Matrix.CreateWorld((Vector3) vectord, vector, vector2), Vector3.Zero, player.Identity.DisplayName, modelName, new Vector3?(color.ToVector3()), botDefinition, true, false, null, true, player.Identity.IdentityId, true);
            Sync.Players.SetPlayerCharacter(player, newCharacter, spawnedBy);
            Sync.Players.RevivePlayer(player);
        }

        private void SyncCooldownResponse(List<RespawnCooldownEntry> entries)
        {
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            if (entries != null)
            {
                foreach (RespawnCooldownEntry entry in entries)
                {
                    MyPlayer.PlayerId id = new MyPlayer.PlayerId {
                        SteamId = Sync.MyId,
                        SerialId = entry.ControllerId
                    };
                    RespawnKey key = new RespawnKey {
                        ControllerId = id,
                        RespawnShipId = entry.ShipId
                    };
                    this.m_globalRespawnTimesMs.Add(key, totalTimeInMilliseconds + entry.RelativeRespawnTime, true);
                }
            }
            this.m_synced = true;
        }

        public void SyncCooldownToPlayer(ulong steamId, bool isLocal)
        {
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            this.m_tmpRespawnTimes.Clear();
            foreach (KeyValuePair<RespawnKey, int> pair in this.m_globalRespawnTimesMs)
            {
                if (pair.Key.ControllerId.SteamId == steamId)
                {
                    RespawnCooldownEntry item = new RespawnCooldownEntry {
                        ControllerId = pair.Key.ControllerId.SerialId,
                        ShipId = pair.Key.RespawnShipId,
                        RelativeRespawnTime = pair.Value - totalTimeInMilliseconds
                    };
                    this.m_tmpRespawnTimes.Add(item);
                }
            }
            if (isLocal)
            {
                OnSyncCooldownResponse(this.m_tmpRespawnTimes);
            }
            else
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<List<RespawnCooldownEntry>>(s => new Action<List<RespawnCooldownEntry>>(MySpaceRespawnComponent.OnSyncCooldownResponse), this.m_tmpRespawnTimes, new EndpointId(steamId), position);
            }
            this.m_tmpRespawnTimes.Clear();
        }

        private static bool TryFindExistingCharacter(MyPlayer player)
        {
            if (player != null)
            {
                using (HashSet<long>.Enumerator enumerator = player.Identity.SavedCharacters.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyCharacter entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(enumerator.Current, false) as MyCharacter;
                        if ((entityById != null) && !entityById.IsDead)
                        {
                            bool flag;
                            if (entityById.Parent == null)
                            {
                                MySession.Static.Players.SetControlledEntity(player.Id, entityById);
                                player.Identity.ChangeCharacter(entityById);
                                MySession.SendVicinityInformation(entityById.EntityId, new EndpointId(player.Client.SteamUserId));
                                flag = true;
                            }
                            else
                            {
                                if (!(entityById.Parent is MyCockpit))
                                {
                                    continue;
                                }
                                MyCockpit parent = entityById.Parent as MyCockpit;
                                MySession.Static.Players.SetControlledEntity(player.Id, parent);
                                player.Identity.ChangeCharacter(parent.Pilot);
                                MySession.SendVicinityInformation(entityById.EntityId, new EndpointId(player.Client.SteamUserId));
                                flag = true;
                            }
                            return flag;
                        }
                    }
                }
            }
            return false;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Sync.Players.LocalRespawnRequested -= new Action<string, Color>(this.OnLocalRespawnRequest);
            Sync.Players.RespawnComponent = null;
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            int delta = totalTimeInMilliseconds - this.m_lastUpdate;
            if (this.m_updatingStopped)
            {
                this.UpdateRespawnTimes(delta);
                this.m_lastUpdate = totalTimeInMilliseconds;
                this.m_updatingStopped = false;
            }
            else
            {
                this.m_updateCtr++;
                this.m_lastUpdate = totalTimeInMilliseconds;
                if ((this.m_updateCtr % 100) == 0)
                {
                    this.RemoveOldRespawnTimes();
                }
            }
        }

        private void UpdateRespawnTimes(int delta)
        {
            foreach (RespawnKey key in this.m_globalRespawnTimesMs.Keys)
            {
                this.m_globalRespawnTimesMs[key] += delta;
            }
            this.m_globalRespawnTimesMs.ApplyAdditionsAndModifications();
        }

        public override void UpdatingStopped()
        {
            base.UpdatingStopped();
            this.m_updatingStopped = true;
        }

        public bool IsSynced =>
            this.m_synced;

        public static MySpaceRespawnComponent Static =>
            (Sync.Players.RespawnComponent as MySpaceRespawnComponent);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySpaceRespawnComponent.<>c <>9 = new MySpaceRespawnComponent.<>c();
            public static Func<IMyEventOwner, Action> <>9__17_0;
            public static Func<IMyEventOwner, Action<List<MySpaceRespawnComponent.RespawnCooldownEntry>>> <>9__26_0;
            public static Func<IMyEventOwner, Action<ulong, int>> <>9__33_0;
            public static Func<IMyEventOwner, Action<long>> <>9__34_3;
            public static Func<IMyEventOwner, Action<MySpaceRespawnComponent.MOTDData, long>> <>9__34_1;
            public static Func<IMyEventOwner, Action<MySpaceRespawnComponent.MOTDData>> <>9__34_0;
            public static Func<IMyEventOwner, Action<Vector3D>> <>9__49_0;
            public static Comparison<MyCockpit> <>9__51_0;
            public static Predicate<MyObjectSeed> <>9__60_0;
            public static Func<MyGps, Vector3D> <>9__63_0;

            internal bool <FindFreeLocationCloseToAsteroid>b__60_0(MyObjectSeed x)
            {
                MyObjectSeedType type = x.Params.Type;
                return ((type != MyObjectSeedType.Asteroid) && (type != MyObjectSeedType.AsteroidCluster));
            }

            internal Vector3D <GetFriendlyPlayerPositions>b__63_0(MyGps x) => 
                x.Coords;

            internal Action<ulong, int> <OnLocalRespawnRequest>b__33_0(IMyEventOwner s) => 
                new Action<ulong, int>(MySpaceRespawnComponent.RespawnRequest_Implementation);

            internal int <PutPlayerInRespawnGrid>b__51_0(MyCockpit cockpitA, MyCockpit cockpitB)
            {
                int num = cockpitB.EnableShipControl.CompareTo(cockpitA.EnableShipControl);
                if (num != 0)
                {
                    return num;
                }
                int num2 = cockpitB.IsMainCockpit.CompareTo(cockpitA.IsMainCockpit);
                return ((num2 == 0) ? 0 : num2);
            }

            internal Action <RequestSync>b__17_0(IMyEventOwner s) => 
                new Action(MySpaceRespawnComponent.OnSyncCooldownRequest);

            internal Action<MySpaceRespawnComponent.MOTDData> <RespawnRequest_Implementation>b__34_0(IMyEventOwner s) => 
                new Action<MySpaceRespawnComponent.MOTDData>(MySpaceRespawnComponent.ShowMotD);

            internal Action<MySpaceRespawnComponent.MOTDData, long> <RespawnRequest_Implementation>b__34_1(IMyEventOwner s) => 
                new Action<MySpaceRespawnComponent.MOTDData, long>(MySpaceRespawnComponent.ShowMedicalScreen_Implementation);

            internal Action<long> <RespawnRequest_Implementation>b__34_3(IMyEventOwner s) => 
                new Action<long>(MySpaceRespawnComponent.RequestRespawnAtSpawnPoint);

            internal Action<Vector3D> <SpawnAtShip>b__49_0(IMyEventOwner s) => 
                new Action<Vector3D>(MySession.SetSpectatorPositionFromServer);

            internal Action<List<MySpaceRespawnComponent.RespawnCooldownEntry>> <SyncCooldownToPlayer>b__26_0(IMyEventOwner s) => 
                new Action<List<MySpaceRespawnComponent.RespawnCooldownEntry>>(MySpaceRespawnComponent.OnSyncCooldownResponse);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOTDData
        {
            public string Url;
            public string Message;
            public bool HasMessage =>
                !string.IsNullOrEmpty(this.Message);
            public bool HasUrl =>
                (string.IsNullOrEmpty(this.Url) && MyGuiSandbox.IsUrlValid(this.Url));
            public bool HasAnything() => 
                (this.HasMessage || this.HasUrl);

            public MOTDData(string url, string message)
            {
                this.Url = url;
                this.Message = message;
            }

            public StringBuilder GetMessage()
            {
                StringBuilder builder = new StringBuilder(this.Message);
                if (MySession.Static.LocalHumanPlayer != null)
                {
                    builder.Replace(MyPerGameSettings.MotDCurrentPlayerVariable, MySession.Static.LocalHumanPlayer.DisplayName);
                }
                return builder;
            }

            public static MySpaceRespawnComponent.MOTDData Construct()
            {
                if (!Sync.IsDedicated)
                {
                    string description = MySession.Static.Description;
                    return new MySpaceRespawnComponent.MOTDData(string.Empty, description ?? string.Empty);
                }
                string messageOfTheDay = MySandboxGame.ConfigDedicated.MessageOfTheDay;
                if (!string.IsNullOrEmpty(messageOfTheDay))
                {
                    StringBuilder builder1 = new StringBuilder();
                    builder1.Append(messageOfTheDay);
                    builder1.Replace(MyPerGameSettings.MotDServerNameVariable, MySandboxGame.ConfigDedicated.ServerName);
                    builder1.Replace(MyPerGameSettings.MotDWorldNameVariable, MySandboxGame.ConfigDedicated.WorldName);
                    builder1.Replace(MyPerGameSettings.MotDServerDescriptionVariable, MySandboxGame.ConfigDedicated.ServerDescription);
                    builder1.Replace(MyPerGameSettings.MotDPlayerCountVariable, Sync.Players.GetOnlinePlayerCount().ToString());
                    messageOfTheDay = builder1.ToString();
                }
                return new MySpaceRespawnComponent.MOTDData(MySandboxGame.ConfigDedicated.MessageOfTheDayUrl, messageOfTheDay);
            }
        }

        public class MyRespawnPointInfo
        {
            public long MedicalRoomId;
            public long MedicalRoomGridId;
            public string MedicalRoomName;
            public float OxygenLevel;
            public long OwnerId;
            public Vector3D PrefferedCameraPosition;
            public Vector3D MedicalRoomPosition;
            public Vector3D MedicalRoomUp;
            public Vector3 MedicalRoomVelocity;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RespawnCooldownEntry
        {
            public int ControllerId;
            public string ShipId;
            public int RelativeRespawnTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RespawnKey : IEquatable<MySpaceRespawnComponent.RespawnKey>
        {
            public MyPlayer.PlayerId ControllerId;
            public string RespawnShipId;
            public bool Equals(MySpaceRespawnComponent.RespawnKey other) => 
                ((this.ControllerId == other.ControllerId) && (this.RespawnShipId == other.RespawnShipId));

            public override int GetHashCode() => 
                (this.ControllerId.GetHashCode() ^ ((this.RespawnShipId == null) ? 0 : this.RespawnShipId.GetHashCode()));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SpawnInfo
        {
            public long IdentityId;
            public MyPlanet Planet;
            public float CollisionRadius;
            public float PlanetDeployAltitude;
            public float MinimalAirDensity;
        }
    }
}

