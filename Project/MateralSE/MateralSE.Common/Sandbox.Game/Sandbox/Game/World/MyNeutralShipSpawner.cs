namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    internal class MyNeutralShipSpawner : MySessionComponentBase
    {
        public const float NEUTRAL_SHIP_SPAWN_DISTANCE = 8000f;
        public const float NEUTRAL_SHIP_FORBIDDEN_RADIUS = 2000f;
        public const float NEUTRAL_SHIP_DIRECTION_SPREAD = 0.5f;
        public const float NEUTRAL_SHIP_MINIMAL_ROUTE_LENGTH = 10000f;
        public const float NEUTRAL_SHIP_SPAWN_OFFSET = 500f;
        public static TimeSpan NEUTRAL_SHIP_RESCHEDULE_TIME = TimeSpan.FromSeconds(10.0);
        public static TimeSpan NEUTRAL_SHIP_MIN_TIME = TimeSpan.FromMinutes(13.0);
        public static TimeSpan NEUTRAL_SHIP_MAX_TIME = TimeSpan.FromMinutes(17.0);
        private const int EVENT_SPAWN_TRY_MAX = 3;
        private static List<MyPhysics.HitInfo> m_raycastHits = new List<MyPhysics.HitInfo>();
        private static List<float> m_spawnGroupCumulativeFrequencies = new List<float>();
        private static float m_spawnGroupTotalFrequencies = 0f;
        private static float[] m_upVecMultipliers = new float[] { 1f, 1f, -1f, -1f };
        private static float[] m_rightVecMultipliers = new float[] { 1f, -1f, -1f, 1f };
        private static List<MySpawnGroupDefinition> m_spawnGroups = new List<MySpawnGroupDefinition>();
        private static int m_eventSpawnTry = 0;

        public override void BeforeStart()
        {
            base.BeforeStart();
            if (Sync.IsServer)
            {
                bool flag = MyFakes.ENABLE_CARGO_SHIPS && MySession.Static.CargoShipsEnabled;
                MyGlobalEventBase eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip"));
                if (ReferenceEquals(eventById, null) & flag)
                {
                    MyGlobalEvents.AddGlobalEvent(MyGlobalEventFactory.CreateEvent(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip")));
                }
                else if (eventById != null)
                {
                    if (flag)
                    {
                        eventById.Enabled = true;
                    }
                    else
                    {
                        eventById.Enabled = false;
                    }
                }
            }
        }

        private static unsafe void GetSafeBoundingBoxForPlayers(Vector3D start, double spawnDistance, out BoundingBoxD output)
        {
            double radius = 10.0;
            BoundingSphereD ed = new BoundingSphereD(start, radius);
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            bool flag = true;
            while (flag)
            {
                flag = false;
                IEnumerator<MyPlayer> enumerator = onlinePlayers.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        Vector3D position = enumerator.Current.GetPosition();
                        Vector3D vectord2 = ed.Center - position;
                        double num2 = vectord2.Length() - ed.Radius;
                        if ((num2 > 0.0) && (num2 <= (spawnDistance * 2.0)))
                        {
                            ed.Include(new BoundingSphereD(position, radius));
                            flag = true;
                        }
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
            double* numPtr1 = (double*) ref ed.Radius;
            numPtr1[0] += spawnDistance;
            output = new BoundingBoxD(ed.Center - new Vector3D(ed.Radius), ed.Center + new Vector3D(ed.Radius));
            List<MyEntity> entitiesInAABB = MyEntities.GetEntitiesInAABB(ref output, false);
            foreach (MyEntity entity in entitiesInAABB)
            {
                if (!(entity is MyCubeGrid))
                {
                    continue;
                }
                MyCubeGrid grid = entity as MyCubeGrid;
                if (grid.IsStatic)
                {
                    Vector3D position = grid.PositionComp.GetPosition();
                    output.Include(new BoundingBoxD(position - spawnDistance, position + spawnDistance));
                }
            }
            entitiesInAABB.Clear();
        }

        private static void InitAutopilot(List<MyCubeGrid> tmpGridList, Vector3D shipDestination, Vector3D direction, string prefabSubtype)
        {
            List<MyCubeGrid>.Enumerator enumerator;
            using (enumerator = tmpGridList.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyCockpit firstBlockOfType = enumerator.Current.GetFirstBlockOfType<MyCockpit>();
                    if (firstBlockOfType != null)
                    {
                        firstBlockOfType.AttachAutopilot(new MySimpleAutopilot(shipDestination, (Vector3) direction, tmpGridList.ToArray<MyCubeGrid, long>(x => x.EntityId)), true);
                        return;
                    }
                }
            }
            MyLog.Default.Error("Missing cockpit on spawngroup " + prefabSubtype, Array.Empty<object>());
            using (enumerator = tmpGridList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
        }

        public override void LoadData()
        {
            MySandboxGame.Log.WriteLine("Pre-loading neutral ship spawn groups...");
            foreach (MySpawnGroupDefinition definition in MyDefinitionManager.Static.GetSpawnGroupDefinitions())
            {
                if (definition.IsCargoShip)
                {
                    m_spawnGroups.Add(definition);
                }
            }
            m_spawnGroupTotalFrequencies = 0f;
            m_spawnGroupCumulativeFrequencies.Clear();
            foreach (MySpawnGroupDefinition definition2 in m_spawnGroups)
            {
                m_spawnGroupTotalFrequencies += definition2.Frequency;
                m_spawnGroupCumulativeFrequencies.Add(m_spawnGroupTotalFrequencies);
            }
            MySandboxGame.Log.WriteLine("End pre-loading neutral ship spawn groups.");
        }

        [MyGlobalEventHandler(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip")]
        public static void OnGlobalSpawnEvent(object senderEvent)
        {
            BoundingBoxD xd;
            Vector3D vectord2;
            List<MySpawnGroupDefinition.SpawnGroupPrefab>.Enumerator enumerator2;
            MySpawnGroupDefinition definition = PickRandomSpawnGroup();
            if (definition == null)
            {
                return;
            }
            definition.ReloadPrefabs();
            long identityId = 0L;
            if (definition.IsPirate)
            {
                identityId = MyPirateAntennas.GetPiratesId();
            }
            if (identityId != 0)
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
                if ((identity == null) || !identity.BlockLimits.HasRemainingPCU)
                {
                    MySandboxGame.Log.Log(MyLogSeverity.Info, "Pirate PCUs exhausted. Cargo ship will not spawn.", Array.Empty<object>());
                    return;
                }
            }
            double num2 = 8000.0;
            Vector3D zero = Vector3D.Zero;
            bool flag = MyEntities.IsWorldLimited();
            if (flag)
            {
                num2 = Math.Min(num2, (double) (MyEntities.WorldSafeHalfExtent() - definition.SpawnRadius));
            }
            else
            {
                ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
                int randomInt = MyUtils.GetRandomInt(0, Math.Max(0, onlinePlayers.Count - 1));
                int num8 = 0;
                foreach (MyPlayer player in onlinePlayers)
                {
                    if (num8 == randomInt)
                    {
                        if (player.Character != null)
                        {
                            zero = player.GetPosition();
                        }
                        break;
                    }
                    num8++;
                }
            }
            if (num2 < 0.0)
            {
                MySandboxGame.Log.WriteLine("Not enough space in the world to spawn such a huge spawn group!");
                return;
            }
            double num4 = 2000.0;
            if (flag)
            {
                xd = new BoundingBoxD(zero - num2, zero + num2);
            }
            else
            {
                GetSafeBoundingBoxForPlayers(zero, num2, out xd);
                num4 += xd.HalfExtents.Max() - 2000.0;
            }
            Vector3D? nullable = new Vector3D?(MyUtils.GetRandomBorderPosition(ref xd));
            nullable = MyEntities.TestPlaceInSpace(nullable.Value, definition.SpawnRadius);
            if (nullable == null)
            {
                RetryEventWithMaxTry(senderEvent as MyGlobalEventBase);
                return;
            }
            Vector3 direction = -Vector3.Normalize(nullable.Value);
            float minValue = (float) Math.Atan(num4 / (nullable.Value - xd.Center).Length());
            float randomFloat = MyUtils.GetRandomFloat(minValue, minValue + 0.5f);
            float randomRadian = MyUtils.GetRandomRadian();
            Vector3 vector = Vector3.CalculatePerpendicularVector(direction);
            vector *= (float) (Math.Sin((double) randomFloat) * Math.Cos((double) randomRadian));
            Vector3 vector2 = Vector3.Cross(direction, vector) * ((float) (Math.Sin((double) randomFloat) * Math.Sin((double) randomRadian)));
            direction = ((direction * ((float) Math.Cos((double) randomFloat))) + vector) + vector2;
            double? nullable2 = new RayD(nullable.Value, direction).Intersects(xd);
            if ((nullable2 == null) || (nullable2.Value < 10000.0))
            {
                vectord2 = direction * 10000f;
            }
            else
            {
                vectord2 = direction * ((float) nullable2.Value);
            }
            Vector3D local1 = nullable.Value + vectord2;
            Vector3 vector3 = Vector3.CalculatePerpendicularVector(direction);
            Vector3 vector4 = Vector3.Cross(direction, vector3);
            MatrixD matrix = MatrixD.CreateWorld(nullable.Value, direction, vector3);
            m_raycastHits.Clear();
            using (enumerator2 = definition.Prefabs.GetEnumerator())
            {
                while (true)
                {
                    while (true)
                    {
                        if (enumerator2.MoveNext())
                        {
                            MySpawnGroupDefinition.SpawnGroupPrefab current = enumerator2.Current;
                            MyPrefabDefinition prefabDefinition = MyDefinitionManager.Static.GetPrefabDefinition(current.SubtypeId);
                            Vector3D position = Vector3.Transform(current.Position, matrix);
                            Vector3D vectord5 = position + vectord2;
                            float num9 = (prefabDefinition == null) ? 10f : prefabDefinition.BoundingSphere.Radius;
                            if (!MyGravityProviderSystem.IsPositionInNaturalGravity(position, (double) definition.SpawnRadius))
                            {
                                if (!MyGravityProviderSystem.IsPositionInNaturalGravity(vectord5, (double) definition.SpawnRadius))
                                {
                                    if (!MyGravityProviderSystem.DoesTrajectoryIntersectNaturalGravity(position, vectord5, (double) (definition.SpawnRadius + 500f)))
                                    {
                                        MyPhysics.CastRay(position, vectord5, m_raycastHits, 0x18);
                                        if (m_raycastHits.Count <= 0)
                                        {
                                            int index = 0;
                                            while (true)
                                            {
                                                if (index < 4)
                                                {
                                                    Vector3D vectord6 = ((vector3 * m_upVecMultipliers[index]) * num9) + ((vector4 * m_rightVecMultipliers[index]) * num9);
                                                    MyPhysics.CastRay(position + vectord6, vectord5 + vectord6, m_raycastHits, 0x18);
                                                    if (m_raycastHits.Count <= 0)
                                                    {
                                                        index++;
                                                        continue;
                                                    }
                                                    RetryEventWithMaxTry(senderEvent as MyGlobalEventBase);
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            RetryEventWithMaxTry(senderEvent as MyGlobalEventBase);
                                        }
                                    }
                                    else
                                    {
                                        RetryEventWithMaxTry(senderEvent as MyGlobalEventBase);
                                    }
                                }
                                else
                                {
                                    RetryEventWithMaxTry(senderEvent as MyGlobalEventBase);
                                }
                            }
                            else
                            {
                                RetryEventWithMaxTry(senderEvent as MyGlobalEventBase);
                            }
                            return;
                        }
                        else
                        {
                            goto TR_000B;
                        }
                    }
                }
            }
        TR_000B:
            using (enumerator2 = definition.Prefabs.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    MySpawnGroupDefinition.SpawnGroupPrefab shipPrefab;
                    Vector3D position = Vector3D.Transform((Vector3D) shipPrefab.Position, matrix);
                    Vector3D shipDestination = position + vectord2;
                    Vector3 up = Vector3.CalculatePerpendicularVector(-direction);
                    List<MyCubeGrid> tmpGridList = new List<MyCubeGrid>();
                    Stack<Action> callbacks = new Stack<Action>();
                    callbacks.Push(delegate {
                        InitAutopilot(tmpGridList, shipDestination, direction, shipPrefab.SubtypeId);
                        using (List<MyCubeGrid>.Enumerator enumerator = tmpGridList.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.ActivatePhysics();
                            }
                        }
                    });
                    Vector3 initialAngularVelocity = new Vector3();
                    MyPrefabManager.Static.SpawnPrefab(tmpGridList, shipPrefab.SubtypeId, position, direction, up, (Vector3) (shipPrefab.Speed * direction), initialAngularVelocity, shipPrefab.BeaconText, null, SpawningOptions.SetAuthorship | SpawningOptions.DisableDampeners | SpawningOptions.SpawnRandomCargo | SpawningOptions.RotateFirstCockpitTowardsDirection, identityId, false, callbacks);
                }
            }
            m_eventSpawnTry = 0;
        }

        private static MySpawnGroupDefinition PickRandomSpawnGroup()
        {
            if (m_spawnGroupCumulativeFrequencies.Count == 0)
            {
                return null;
            }
            float randomFloat = MyUtils.GetRandomFloat(0f, m_spawnGroupTotalFrequencies);
            int num2 = 0;
            while ((num2 < m_spawnGroupCumulativeFrequencies.Count) && ((randomFloat > m_spawnGroupCumulativeFrequencies[num2]) || !m_spawnGroups[num2].Enabled))
            {
                num2++;
            }
            if (num2 >= m_spawnGroupCumulativeFrequencies.Count)
            {
                num2 = m_spawnGroupCumulativeFrequencies.Count - 1;
            }
            return m_spawnGroups[num2];
        }

        private static void RetryEventWithMaxTry(MyGlobalEventBase evt)
        {
            if (++m_eventSpawnTry > 3)
            {
                m_eventSpawnTry = 0;
            }
            else
            {
                object[] objArray1 = new object[] { "Could not spawn event. Try ", m_eventSpawnTry, " of ", 3 };
                MySandboxGame.Log.WriteLine(string.Concat(objArray1));
                MyGlobalEvents.RescheduleEvent(evt, NEUTRAL_SHIP_RESCHEDULE_TIME);
            }
        }

        protected override void UnloadData()
        {
            m_spawnGroupTotalFrequencies = 0f;
            m_spawnGroupCumulativeFrequencies.Clear();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (MyDebugDrawSettings.DEBUG_DRAW_NEUTRAL_SHIPS)
            {
                foreach (MyCubeGrid grid in MyEntities.GetEntities())
                {
                    if (grid != null)
                    {
                        foreach (MyCockpit cockpit in grid.GetFatBlocks<MyCockpit>())
                        {
                            if (cockpit.AiPilot != null)
                            {
                                cockpit.AiPilot.DebugDraw();
                            }
                        }
                    }
                }
            }
        }

        public override bool IsRequiredByGame =>
            (MyPerGameSettings.Game == GameEnum.SE_GAME);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyNeutralShipSpawner.<>c <>9 = new MyNeutralShipSpawner.<>c();
            public static Func<MyCubeGrid, long> <>9__25_0;

            internal long <InitAutopilot>b__25_0(MyCubeGrid x) => 
                x.EntityId;
        }
    }
}

