namespace SpaceEngineers.AI
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.AI;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.Spatial;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x1f6, typeof(MyObjectBuilder_SpaceFaunaComponent), (Type) null)]
    public class MySpaceFaunaComponent : MySessionComponentBase
    {
        private const string Wolf_SUBTYPE_ID = "Wolf";
        private static readonly int UPDATE_DELAY = 120;
        private static readonly int CLEAN_DELAY = 0x960;
        private static readonly int ABANDON_DELAY = 0xafc8;
        private static readonly float DESPAWN_DIST = 1000f;
        private static readonly float SPHERE_SPAWN_DIST = 150f;
        private static readonly float PROXIMITY_DIST = 50f;
        private static readonly float TIMEOUT_DIST = 150f;
        private static readonly int MAX_BOTS_PER_PLANET = 10;
        private int m_waitForUpdate = UPDATE_DELAY;
        private int m_waitForClean = CLEAN_DELAY;
        private Dictionary<long, PlanetAIInfo> m_planets = new Dictionary<long, PlanetAIInfo>();
        private List<Vector3D> m_tmpPlayerPositions = new List<Vector3D>();
        private MyVector3DGrid<SpawnInfo> m_spawnInfoGrid = new MyVector3DGrid<SpawnInfo>((double) SPHERE_SPAWN_DIST);
        private List<SpawnInfo> m_allSpawnInfos = new List<SpawnInfo>();
        private MyVector3DGrid<SpawnTimeoutInfo> m_timeoutInfoGrid = new MyVector3DGrid<SpawnTimeoutInfo>((double) TIMEOUT_DIST);
        private List<SpawnTimeoutInfo> m_allTimeoutInfos = new List<SpawnTimeoutInfo>();
        private MyObjectBuilder_SpaceFaunaComponent m_obForLoading;
        private Action<MyCharacter> m_botCharacterDied;

        public override void BeforeStart()
        {
            base.BeforeStart();
            if (this.m_obForLoading != null)
            {
                int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_allSpawnInfos.Capacity = this.m_obForLoading.SpawnInfos.Count;
                using (List<MyObjectBuilder_SpaceFaunaComponent.SpawnInfo>.Enumerator enumerator = this.m_obForLoading.SpawnInfos.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        SpawnInfo item = new SpawnInfo(enumerator.Current, totalGamePlayTimeInMilliseconds);
                        this.m_allSpawnInfos.Add(item);
                        this.m_spawnInfoGrid.AddPoint(ref item.Position, item);
                    }
                }
                this.m_allTimeoutInfos.Capacity = this.m_obForLoading.TimeoutInfos.Count;
                using (List<MyObjectBuilder_SpaceFaunaComponent.TimeoutInfo>.Enumerator enumerator2 = this.m_obForLoading.TimeoutInfos.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        SpawnTimeoutInfo item = new SpawnTimeoutInfo(enumerator2.Current, totalGamePlayTimeInMilliseconds);
                        if (item.AnimalSpawnInfo != null)
                        {
                            this.m_allTimeoutInfos.Add(item);
                            this.m_timeoutInfoGrid.AddPoint(ref item.Position, item);
                        }
                    }
                }
                this.m_obForLoading = null;
            }
        }

        private void BotCharacterDied(MyCharacter obj)
        {
            Vector3D position = obj.PositionComp.GetPosition();
            obj.CharacterDied -= new Action<MyCharacter>(this.BotCharacterDied);
            int num = 0;
            MyVector3DGrid<SpawnTimeoutInfo>.Enumerator pointsCloserThan = this.m_timeoutInfoGrid.GetPointsCloserThan(ref position, (double) TIMEOUT_DIST);
            while (pointsCloserThan.MoveNext())
            {
                num++;
                pointsCloserThan.Current.AddKillTimeout();
            }
            if (num == 0)
            {
                SpawnTimeoutInfo data = new SpawnTimeoutInfo(position, MySandboxGame.TotalGamePlayTimeInMilliseconds);
                data.AddKillTimeout();
                this.m_timeoutInfoGrid.AddPoint(ref position, data);
                this.m_allTimeoutInfos.Add(data);
            }
        }

        public void DebugDraw()
        {
            int num = 0 + 1;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "Cleanup in " + this.m_waitForClean.ToString(), Color.Red, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            num++;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "Planet infos:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (KeyValuePair<long, PlanetAIInfo> pair in this.m_planets)
            {
                num++;
                object[] objArray1 = new object[] { "  Name: ", pair.Value.Planet.Generator.FolderName, ", Id: ", pair.Key, ", Bots: ", pair.Value.BotNumber.ToString() };
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), string.Concat(objArray1), Color.LightYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            num++;
            object[] objArray2 = new object[] { "Num. of spawn infos: ", this.m_allSpawnInfos.Count, "/", this.m_timeoutInfoGrid.Count };
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), string.Concat(objArray2), Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            foreach (SpawnInfo info in this.m_allSpawnInfos)
            {
                Vector3D position = info.Position;
                Vector3 vector = (Vector3) (info.Planet.PositionComp.GetPosition() - position);
                vector.Normalize();
                int num3 = Math.Max(0, (info.SpawnTime - totalGamePlayTimeInMilliseconds) / 0x3e8);
                int num4 = Math.Max(0, (info.AbandonTime - totalGamePlayTimeInMilliseconds) / 0x3e8);
                if ((num3 != 0) && (num4 != 0))
                {
                    MyRenderProxy.DebugDrawSphere(position, SPHERE_SPAWN_DIST, Color.Yellow, 1f, false, false, true, false);
                    MyRenderProxy.DebugDrawText3D(position, "Spawning in: " + num3.ToString(), Color.Yellow, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    MyRenderProxy.DebugDrawText3D(position - (vector * 0.5f), "Abandoned in: " + num4.ToString(), Color.Yellow, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
            foreach (SpawnTimeoutInfo info2 in this.m_allTimeoutInfos)
            {
                int num5 = Math.Max(0, (info2.TimeoutTime - totalGamePlayTimeInMilliseconds) / 0x3e8);
                MyRenderProxy.DebugDrawSphere(info2.Position, TIMEOUT_DIST, Color.Blue, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawText3D(info2.Position, "Timeout: " + num5.ToString(), Color.Blue, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        private void EntityAdded(MyEntity entity)
        {
            MyPlanet planet = entity as MyPlanet;
            if ((planet != null) && this.PlanetHasFauna(planet))
            {
                this.m_planets.Add(entity.EntityId, new PlanetAIInfo(planet));
            }
        }

        private void EntityRemoved(MyEntity entity)
        {
            if (entity is MyPlanet)
            {
                this.m_planets.Remove(entity.EntityId);
            }
        }

        private void EraseAllInfos()
        {
            using (List<SpawnInfo>.Enumerator enumerator = this.m_allSpawnInfos.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.SpawnTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
            }
            foreach (SpawnTimeoutInfo info in this.m_allTimeoutInfos)
            {
                this.m_timeoutInfoGrid.RemovePoint(ref info.Position);
            }
            this.m_allTimeoutInfos.Clear();
        }

        private MyBotDefinition GetAnimalDefinition(MyPlanetAnimalSpawnInfo animalSpawnInfo)
        {
            int randomInt = MyUtils.GetRandomInt(0, animalSpawnInfo.Animals.Length);
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_AnimalBot), animalSpawnInfo.Animals[randomInt].AnimalType);
            return (MyDefinitionManager.Static.GetBotDefinition(id) as MyAgentDefinition);
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_SpaceFaunaComponent objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_SpaceFaunaComponent;
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            int num2 = 0;
            using (List<SpawnInfo>.Enumerator enumerator = this.m_allSpawnInfos.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.SpawnDone)
                    {
                        continue;
                    }
                    num2++;
                }
            }
            objectBuilder.SpawnInfos.Capacity = num2;
            foreach (SpawnInfo info in this.m_allSpawnInfos)
            {
                if (!info.SpawnDone)
                {
                    MyObjectBuilder_SpaceFaunaComponent.SpawnInfo item = new MyObjectBuilder_SpaceFaunaComponent.SpawnInfo {
                        X = info.Position.X,
                        Y = info.Position.Y,
                        Z = info.Position.Z,
                        AbandonTime = Math.Max(0, info.AbandonTime - totalGamePlayTimeInMilliseconds),
                        SpawnTime = Math.Max(0, info.SpawnTime - totalGamePlayTimeInMilliseconds)
                    };
                    objectBuilder.SpawnInfos.Add(item);
                }
            }
            objectBuilder.TimeoutInfos.Capacity = this.m_allTimeoutInfos.Count;
            foreach (SpawnTimeoutInfo info3 in this.m_allTimeoutInfos)
            {
                MyObjectBuilder_SpaceFaunaComponent.TimeoutInfo item = new MyObjectBuilder_SpaceFaunaComponent.TimeoutInfo {
                    X = info3.Position.X,
                    Y = info3.Position.Y,
                    Z = info3.Position.Z,
                    Timeout = Math.Max(0, info3.TimeoutTime - totalGamePlayTimeInMilliseconds)
                };
                objectBuilder.TimeoutInfos.Add(item);
            }
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            this.m_obForLoading = sessionComponent as MyObjectBuilder_SpaceFaunaComponent;
        }

        public override void LoadData()
        {
            base.LoadData();
            if (Sync.IsServer)
            {
                MyEntities.OnEntityAdd += new Action<MyEntity>(this.EntityAdded);
                MyEntities.OnEntityRemove += new Action<MyEntity>(this.EntityRemoved);
                MyAIComponent.Static.BotCreatedEvent += new Action<int, MyBotDefinition>(this.OnBotCreatedEvent);
                this.m_botCharacterDied = new Action<MyCharacter>(this.BotCharacterDied);
            }
        }

        private void OnBotControlledEntityChanged(IMyControllableEntity oldControllable, IMyControllableEntity newControllable)
        {
            MyCharacter character = oldControllable as MyCharacter;
            MyCharacter character2 = newControllable as MyCharacter;
            if (character != null)
            {
                character.CharacterDied -= new Action<MyCharacter>(this.BotCharacterDied);
            }
            if (character2 != null)
            {
                character2.CharacterDied += new Action<MyCharacter>(this.BotCharacterDied);
            }
        }

        private void OnBotCreatedEvent(int botSerialNum, MyBotDefinition botDefinition)
        {
            MyAgentDefinition definition = botDefinition as MyAgentDefinition;
            if ((definition != null) && (definition.FactionTag == "SPID"))
            {
                MyPlayer player = null;
                if (Sync.Players.TryGetPlayerById(new MyPlayer.PlayerId(Sync.MyId, botSerialNum), out player))
                {
                    player.Controller.ControlledEntityChanged += new Action<IMyControllableEntity, IMyControllableEntity>(this.OnBotControlledEntityChanged);
                    MyCharacter controlledEntity = player.Controller.ControlledEntity as MyCharacter;
                    if (controlledEntity != null)
                    {
                        controlledEntity.CharacterDied += new Action<MyCharacter>(this.BotCharacterDied);
                    }
                }
            }
        }

        private bool PlanetHasFauna(MyPlanet planet) => 
            ((planet.Generator.AnimalSpawnInfo != null) && ((planet.Generator.AnimalSpawnInfo.Animals != null) && (planet.Generator.AnimalSpawnInfo.Animals.Length != 0)));

        private void SpawnBot(SpawnInfo spawnInfo, MyPlanet planet, MyPlanetAnimalSpawnInfo animalSpawnInfo)
        {
            PlanetAIInfo info = null;
            if (this.m_planets.TryGetValue(planet.EntityId, out info) && (info.BotNumber < MAX_BOTS_PER_PLANET))
            {
                double spawnDistMin = animalSpawnInfo.SpawnDistMin;
                double spawnDistMax = animalSpawnInfo.SpawnDistMax;
                Vector3D position = spawnInfo.Position;
                Vector3D v = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
                if (v == Vector3D.Zero)
                {
                    v = Vector3D.Up;
                }
                v.Normalize();
                Vector3D vectord3 = Vector3D.CalculatePerpendicularVector(v);
                Vector3D bitangent = Vector3D.Cross(v, vectord3);
                vectord3.Normalize();
                bitangent.Normalize();
                Vector3D globalPos = MyUtils.GetRandomDiscPosition(ref position, spawnDistMin, spawnDistMax, ref vectord3, ref bitangent);
                globalPos = planet.GetClosestSurfacePointGlobal(ref globalPos);
                Vector3D? nullable = MyEntities.FindFreePlace(globalPos, 2f, 20, 5, 1f);
                if (nullable != null)
                {
                    globalPos = nullable.Value;
                }
                planet.CorrectSpawnLocation(ref globalPos, 2.0);
                MyAgentDefinition animalDefinition = this.GetAnimalDefinition(animalSpawnInfo) as MyAgentDefinition;
                if (animalDefinition != null)
                {
                    if ((animalDefinition.Id.SubtypeName == "Wolf") && MySession.Static.EnableWolfs)
                    {
                        MyAIComponent.Static.SpawnNewBot(animalDefinition, globalPos, true);
                    }
                    else if ((animalDefinition.Id.SubtypeName != "Wolf") && MySession.Static.EnableSpiders)
                    {
                        MyAIComponent.Static.SpawnNewBot(animalDefinition, globalPos, true);
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (Sync.IsServer)
            {
                MyEntities.OnEntityAdd -= new Action<MyEntity>(this.EntityAdded);
                MyEntities.OnEntityRemove -= new Action<MyEntity>(this.EntityRemoved);
                MyAIComponent.Static.BotCreatedEvent -= new Action<int, MyBotDefinition>(this.OnBotCreatedEvent);
                this.m_botCharacterDied = null;
                this.m_planets.Clear();
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (Sync.IsServer)
            {
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_FAUNA_COMPONENT)
                {
                    this.DebugDraw();
                }
                this.m_waitForUpdate--;
                if (this.m_waitForUpdate <= 0)
                {
                    this.m_waitForUpdate = UPDATE_DELAY;
                    ICollection<MyPlayer> onlinePlayers = Sync.Players.GetOnlinePlayers();
                    this.m_tmpPlayerPositions.Capacity = Math.Max(this.m_tmpPlayerPositions.Capacity, onlinePlayers.Count);
                    this.m_tmpPlayerPositions.Clear();
                    foreach (KeyValuePair<long, PlanetAIInfo> pair in this.m_planets)
                    {
                        pair.Value.BotNumber = 0;
                    }
                    foreach (MyPlayer player in onlinePlayers)
                    {
                        if (player.Id.SerialId == 0)
                        {
                            if (player.Controller.ControlledEntity == null)
                            {
                                continue;
                            }
                            Vector3D position = player.GetPosition();
                            this.m_tmpPlayerPositions.Add(position);
                            continue;
                        }
                        if (player.Controller.ControlledEntity != null)
                        {
                            PlanetAIInfo info;
                            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(player.GetPosition());
                            if ((closestPlanet != null) && this.m_planets.TryGetValue(closestPlanet.EntityId, out info))
                            {
                                info.BotNumber++;
                            }
                        }
                    }
                    int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    if (MyFakes.SPAWN_SPACE_FAUNA_IN_CREATIVE)
                    {
                        foreach (MyPlayer player2 in onlinePlayers)
                        {
                            if (player2.Controller.ControlledEntity == null)
                            {
                                continue;
                            }
                            Vector3D position = player2.GetPosition();
                            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                            if ((closestPlanet != null) && this.PlanetHasFauna(closestPlanet))
                            {
                                PlanetAIInfo info2 = null;
                                if (this.m_planets.TryGetValue(closestPlanet.EntityId, out info2))
                                {
                                    if (player2.Id.SerialId != 0)
                                    {
                                        double maxValue = double.MaxValue;
                                        foreach (Vector3D vectord4 in this.m_tmpPlayerPositions)
                                        {
                                            maxValue = Math.Min(Vector3D.DistanceSquared(position, vectord4), maxValue);
                                        }
                                        if (maxValue > (DESPAWN_DIST * DESPAWN_DIST))
                                        {
                                            MyAIComponent.Static.RemoveBot(player2.Id.SerialId, true);
                                        }
                                        continue;
                                    }
                                    Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref position);
                                    if (((closestSurfacePointGlobal - position).LengthSquared() < (PROXIMITY_DIST * PROXIMITY_DIST)) && (info2.BotNumber < MAX_BOTS_PER_PLANET))
                                    {
                                        int num2 = 0;
                                        MyVector3DGrid<SpawnInfo>.Enumerator pointsCloserThan = this.m_spawnInfoGrid.GetPointsCloserThan(ref position, (double) SPHERE_SPAWN_DIST);
                                        while (true)
                                        {
                                            if (!pointsCloserThan.MoveNext())
                                            {
                                                if (num2 == 0)
                                                {
                                                    SpawnInfo data = new SpawnInfo(position, totalGamePlayTimeInMilliseconds, closestPlanet);
                                                    this.m_spawnInfoGrid.AddPoint(ref position, data);
                                                    this.m_allSpawnInfos.Add(data);
                                                }
                                                break;
                                            }
                                            SpawnInfo current = pointsCloserThan.Current;
                                            num2++;
                                            if (!current.SpawnDone)
                                            {
                                                if (!current.ShouldSpawn(totalGamePlayTimeInMilliseconds))
                                                {
                                                    current.UpdateAbandoned(totalGamePlayTimeInMilliseconds);
                                                    continue;
                                                }
                                                current.SpawnDone = true;
                                                MyVector3DGrid<SpawnTimeoutInfo>.Enumerator enumerator4 = this.m_timeoutInfoGrid.GetPointsCloserThan(ref position, (double) TIMEOUT_DIST);
                                                bool flag = false;
                                                while (true)
                                                {
                                                    if (enumerator4.MoveNext())
                                                    {
                                                        if (enumerator4.Current.IsTimedOut(totalGamePlayTimeInMilliseconds))
                                                        {
                                                            continue;
                                                        }
                                                        flag = true;
                                                    }
                                                    if (!flag)
                                                    {
                                                        MyPlanetAnimalSpawnInfo dayOrNightAnimalSpawnInfo = MySpaceBotFactory.GetDayOrNightAnimalSpawnInfo(closestPlanet, current.Position);
                                                        if (dayOrNightAnimalSpawnInfo != null)
                                                        {
                                                            int randomInt = MyUtils.GetRandomInt(dayOrNightAnimalSpawnInfo.WaveCountMin, dayOrNightAnimalSpawnInfo.WaveCountMax);
                                                            for (int i = 0; i < randomInt; i++)
                                                            {
                                                                this.SpawnBot(current, closestPlanet, dayOrNightAnimalSpawnInfo);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    this.m_tmpPlayerPositions.Clear();
                    this.m_waitForClean -= UPDATE_DELAY;
                    if (this.m_waitForClean <= 0)
                    {
                        MyAIComponent.Static.CleanUnusedIdentities();
                        this.m_waitForClean = CLEAN_DELAY;
                        int index = 0;
                        while (true)
                        {
                            if (index >= this.m_allSpawnInfos.Count)
                            {
                                for (int i = 0; i < this.m_allTimeoutInfos.Count; i++)
                                {
                                    SpawnTimeoutInfo info7 = this.m_allTimeoutInfos[i];
                                    if (info7.IsTimedOut(totalGamePlayTimeInMilliseconds))
                                    {
                                        this.m_allTimeoutInfos.RemoveAtFast<SpawnTimeoutInfo>(i);
                                        Vector3D position = info7.Position;
                                        this.m_timeoutInfoGrid.RemovePoint(ref position);
                                        i--;
                                    }
                                }
                                break;
                            }
                            SpawnInfo info6 = this.m_allSpawnInfos[index];
                            if (info6.IsAbandoned(totalGamePlayTimeInMilliseconds) || info6.SpawnDone)
                            {
                                this.m_allSpawnInfos.RemoveAtFast<SpawnInfo>(index);
                                Vector3D position = info6.Position;
                                this.m_spawnInfoGrid.RemovePoint(ref position);
                                index--;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MyAIComponent) };

        public override bool IsRequiredByGame =>
            ((MyPerGameSettings.Game == GameEnum.SE_GAME) && MyPerGameSettings.EnableAi);

        private class PlanetAIInfo
        {
            public MyPlanet Planet;
            public int BotNumber;

            public PlanetAIInfo(MyPlanet planet)
            {
                this.Planet = planet;
                this.BotNumber = 0;
            }
        }

        private class SpawnInfo
        {
            public int SpawnTime;
            public int AbandonTime;
            public Vector3D Position;
            public MyPlanet Planet;
            public bool SpawnDone;

            public SpawnInfo(MyObjectBuilder_SpaceFaunaComponent.SpawnInfo info, int currentTime)
            {
                this.SpawnTime = currentTime + info.SpawnTime;
                this.AbandonTime = currentTime + info.SpawnTime;
                this.Position = new Vector3D(info.X, info.Y, info.Z);
                this.Planet = MyGamePruningStructure.GetClosestPlanet(this.Position);
                this.SpawnDone = false;
            }

            public SpawnInfo(Vector3D position, int gameTime, MyPlanet planet)
            {
                MyPlanetAnimalSpawnInfo dayOrNightAnimalSpawnInfo = MySpaceBotFactory.GetDayOrNightAnimalSpawnInfo(planet, position);
                this.SpawnTime = gameTime + MyUtils.GetRandomInt(dayOrNightAnimalSpawnInfo.SpawnDelayMin, dayOrNightAnimalSpawnInfo.SpawnDelayMax);
                this.AbandonTime = gameTime + MySpaceFaunaComponent.ABANDON_DELAY;
                this.Position = position;
                this.Planet = planet;
                this.SpawnDone = false;
            }

            public bool IsAbandoned(int currentTime) => 
                ((this.AbandonTime - currentTime) < 0);

            public bool ShouldSpawn(int currentTime) => 
                ((this.SpawnTime - currentTime) < 0);

            public void UpdateAbandoned(int currentTime)
            {
                this.AbandonTime = currentTime + MySpaceFaunaComponent.ABANDON_DELAY;
            }
        }

        private class SpawnTimeoutInfo
        {
            public int TimeoutTime;
            public Vector3D Position;
            public MyPlanetAnimalSpawnInfo AnimalSpawnInfo;

            public SpawnTimeoutInfo(MyObjectBuilder_SpaceFaunaComponent.TimeoutInfo info, int currentTime)
            {
                this.TimeoutTime = currentTime + info.Timeout;
                this.Position = new Vector3D(info.X, info.Y, info.Z);
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(this.Position);
                this.AnimalSpawnInfo = MySpaceBotFactory.GetDayOrNightAnimalSpawnInfo(closestPlanet, this.Position);
                if (this.AnimalSpawnInfo == null)
                {
                    this.TimeoutTime = currentTime;
                }
            }

            public SpawnTimeoutInfo(Vector3D position, int currentTime)
            {
                this.TimeoutTime = currentTime;
                this.Position = position;
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(this.Position);
                this.AnimalSpawnInfo = MySpaceBotFactory.GetDayOrNightAnimalSpawnInfo(closestPlanet, this.Position);
                if (this.AnimalSpawnInfo == null)
                {
                    this.TimeoutTime = currentTime;
                }
            }

            internal void AddKillTimeout()
            {
                if (this.AnimalSpawnInfo != null)
                {
                    this.TimeoutTime += this.AnimalSpawnInfo.KillDelay;
                }
            }

            internal bool IsTimedOut(int currentTime) => 
                ((this.TimeoutTime - currentTime) < 0);
        }
    }
}

