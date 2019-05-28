namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment.Modules;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x457, typeof(MyObjectBuilder_EnvironmentBotSpawningSystem), (Type) null)]
    public class MyEnvironmentBotSpawningSystem : MySessionComponentBase
    {
        private static readonly int DELAY_BETWEEN_TICKS_IN_MS = 0x1d4c0;
        private static readonly float BOT_SPAWN_RANGE_MIN = 80f;
        private static readonly float BOT_SPAWN_RANGE_MIN_SQ = (BOT_SPAWN_RANGE_MIN * BOT_SPAWN_RANGE_MIN);
        private static readonly float BOT_DESPAWN_DISTANCE = 400f;
        private static readonly float BOT_DESPAWN_DISTANCE_SQ = (BOT_DESPAWN_DISTANCE * BOT_DESPAWN_DISTANCE);
        private static readonly int MAX_SPAWN_ATTEMPTS = 5;
        public static MyEnvironmentBotSpawningSystem Static;
        private MyRandom m_random = new MyRandom();
        private List<Vector3D> m_tmpPlayerPositions;
        private HashSet<MyBotSpawningEnvironmentProxy> m_activeBotSpawningProxies;
        private int m_lastSpawnEventTimeInMs;
        private int m_timeSinceLastEventInMs;
        private int m_tmpSpawnAttempts;

        public override void BeforeStart()
        {
            base.BeforeStart();
            bool isServer = Sync.IsServer;
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_EnvironmentBotSpawningSystem objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_EnvironmentBotSpawningSystem;
            objectBuilder.TimeSinceLastEventInMs = this.m_timeSinceLastEventInMs;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_EnvironmentBotSpawningSystem system = sessionComponent as MyObjectBuilder_EnvironmentBotSpawningSystem;
            this.m_timeSinceLastEventInMs = system.TimeSinceLastEventInMs;
            this.m_lastSpawnEventTimeInMs = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_timeSinceLastEventInMs;
        }

        public bool IsHumanPlayerWithinRange(Vector3 position)
        {
            using (IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPlayer current = enumerator.Current;
                    if ((current.Id.SerialId == 0) && ((current.Controller.ControlledEntity != null) && (Vector3.DistanceSquared((Vector3) current.GetPosition(), position) < BOT_SPAWN_RANGE_MIN_SQ)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
            this.m_tmpPlayerPositions = new List<Vector3D>();
            this.m_activeBotSpawningProxies = new HashSet<MyBotSpawningEnvironmentProxy>();
            bool isServer = Sync.IsServer;
        }

        public void RegisterBotSpawningProxy(MyBotSpawningEnvironmentProxy proxy)
        {
            this.m_activeBotSpawningProxies.Add(proxy);
        }

        public void RemoveDistantBots()
        {
            ICollection<MyPlayer> onlinePlayers = Sync.Players.GetOnlinePlayers();
            this.m_tmpPlayerPositions.Capacity = Math.Max(this.m_tmpPlayerPositions.Capacity, onlinePlayers.Count);
            this.m_tmpPlayerPositions.Clear();
            foreach (MyPlayer player in onlinePlayers)
            {
                if (player.Id.SerialId != 0)
                {
                    continue;
                }
                if (player.Controller.ControlledEntity != null)
                {
                    Vector3D position = player.GetPosition();
                    this.m_tmpPlayerPositions.Add(position);
                }
            }
            foreach (MyPlayer player2 in onlinePlayers)
            {
                if (player2.Controller.ControlledEntity == null)
                {
                    continue;
                }
                if (player2.Id.SerialId != 0)
                {
                    bool flag = true;
                    Vector3D position = player2.GetPosition();
                    foreach (Vector3D vectord3 in this.m_tmpPlayerPositions)
                    {
                        if (Vector3D.DistanceSquared(position, vectord3) < BOT_DESPAWN_DISTANCE_SQ)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        MyAIComponent.Static.RemoveBot(player2.Id.SerialId, true);
                    }
                }
            }
        }

        public void SpawnTick()
        {
            if ((this.m_activeBotSpawningProxies.Count != 0) && (this.m_tmpSpawnAttempts <= MAX_SPAWN_ATTEMPTS))
            {
                this.m_tmpSpawnAttempts++;
                int randomInt = MyUtils.GetRandomInt(0, this.m_activeBotSpawningProxies.Count);
                if (!this.m_activeBotSpawningProxies.ElementAt<MyBotSpawningEnvironmentProxy>(randomInt).OnSpawnTick())
                {
                    this.SpawnTick();
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
            this.m_tmpPlayerPositions = null;
            this.m_activeBotSpawningProxies = null;
            bool isServer = Sync.IsServer;
        }

        public void UnregisterBotSpawningProxy(MyBotSpawningEnvironmentProxy proxy)
        {
            this.m_activeBotSpawningProxies.Remove(proxy);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (Sync.IsServer)
            {
                this.m_timeSinceLastEventInMs = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastSpawnEventTimeInMs;
                if (this.m_timeSinceLastEventInMs >= DELAY_BETWEEN_TICKS_IN_MS)
                {
                    this.RemoveDistantBots();
                    MyAIComponent.Static.CleanUnusedIdentities();
                    this.m_tmpSpawnAttempts = 0;
                    this.SpawnTick();
                    this.m_lastSpawnEventTimeInMs = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    this.m_timeSinceLastEventInMs = 0;
                }
            }
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MyAIComponent) };

        public override bool IsRequiredByGame =>
            MyPerGameSettings.EnableAi;
    }
}

