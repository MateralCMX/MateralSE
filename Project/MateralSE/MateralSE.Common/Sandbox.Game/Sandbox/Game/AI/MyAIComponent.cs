namespace Sandbox.Game.AI
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI.BehaviorTree;
    using Sandbox.Game.AI.Commands;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation, 0x3e8, typeof(MyObjectBuilder_AIComponent), (System.Type) null)]
    public class MyAIComponent : MySessionComponentBase
    {
        private MyBotCollection m_botCollection;
        private IMyPathfinding m_pathfinding;
        private MyBehaviorTreeCollection m_behaviorTreeCollection;
        private Dictionary<int, MyObjectBuilder_Bot> m_loadedBotObjectBuildersByHandle;
        private List<int> m_loadedLocalPlayers;
        private List<Vector3D> m_tmpSpawnPoints = new List<Vector3D>();
        public static MyAIComponent Static;
        public static MyBotFactoryBase BotFactory;
        private int m_lastBotId;
        private Dictionary<int, AgentSpawnData> m_agentsToSpawn;
        private MyHudNotification m_maxBotNotification;
        private bool m_debugDrawPathfinding;
        public MyAgentDefinition BotToSpawn;
        public MyAiCommandDefinition CommandDefinition;
        [CompilerGenerated]
        private Action<int, MyBotDefinition> BotCreatedEvent;
        private MyConcurrentQueue<BotRemovalRequest> m_removeQueue;
        private MyConcurrentQueue<AgentSpawnData> m_processQueue;
        private FastResourceLock m_lock;
        private BoundingBoxD m_debugTargetAABB;

        public event Action<int, MyBotDefinition> BotCreatedEvent
        {
            [CompilerGenerated] add
            {
                Action<int, MyBotDefinition> botCreatedEvent = this.BotCreatedEvent;
                while (true)
                {
                    Action<int, MyBotDefinition> a = botCreatedEvent;
                    Action<int, MyBotDefinition> action3 = (Action<int, MyBotDefinition>) Delegate.Combine(a, value);
                    botCreatedEvent = Interlocked.CompareExchange<Action<int, MyBotDefinition>>(ref this.BotCreatedEvent, action3, a);
                    if (ReferenceEquals(botCreatedEvent, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int, MyBotDefinition> botCreatedEvent = this.BotCreatedEvent;
                while (true)
                {
                    Action<int, MyBotDefinition> source = botCreatedEvent;
                    Action<int, MyBotDefinition> action3 = (Action<int, MyBotDefinition>) Delegate.Remove(source, value);
                    botCreatedEvent = Interlocked.CompareExchange<Action<int, MyBotDefinition>>(ref this.BotCreatedEvent, action3, source);
                    if (ReferenceEquals(botCreatedEvent, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyAIComponent()
        {
            Static = this;
            BotFactory = Activator.CreateInstance(MyPerGameSettings.BotFactoryType) as MyBotFactoryBase;
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            if (MyPerGameSettings.EnableAi)
            {
                foreach (int num in this.m_loadedLocalPlayers)
                {
                    MyObjectBuilder_Bot bot = null;
                    this.m_loadedBotObjectBuildersByHandle.TryGetValue(num, out bot);
                    if ((bot == null) || (bot.TypeId == bot.BotDefId.TypeId))
                    {
                        this.CreateBot(num, bot);
                    }
                }
                this.m_loadedLocalPlayers.Clear();
                this.m_loadedBotObjectBuildersByHandle.Clear();
                Sync.Players.LocalPlayerRemoved += new Action<int>(this.LocalPlayerRemoved);
                if ((MyPerGameSettings.Game == GameEnum.ME_GAME) && Sync.IsServer)
                {
                    this.CleanUnusedIdentities();
                }
            }
        }

        public bool CanSpawnMoreBots(MyPlayer.PlayerId pid)
        {
            if (!Sync.IsServer)
            {
                return false;
            }
            if (MyFakes.DEVELOPMENT_PRESET)
            {
                return true;
            }
            if (Sync.MyId == pid.SteamId)
            {
                AgentSpawnData data = new AgentSpawnData();
                return (this.m_agentsToSpawn.TryGetValue(pid.SerialId, out data) && (!data.CreatedByPlayer ? (this.Bots.GetGeneratedBotCount() < BotFactory.MaximumUncontrolledBotCount) : (this.Bots.GetCreatedBotCount() < BotFactory.MaximumBotPerPlayer)));
            }
            int num = 0;
            ulong steamId = pid.SteamId;
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                if (player.Id.SteamId != steamId)
                {
                    continue;
                }
                if (player.Id.SerialId != 0)
                {
                    num++;
                }
            }
            return (num < BotFactory.MaximumBotPerPlayer);
        }

        public void CleanUnusedIdentities()
        {
            List<MyPlayer.PlayerId> list = new List<MyPlayer.PlayerId>();
            foreach (MyPlayer.PlayerId id in Sync.Players.GetAllPlayers())
            {
                list.Add(id);
            }
            foreach (MyPlayer.PlayerId id2 in list)
            {
                if (id2.SteamId != Sync.MyId)
                {
                    continue;
                }
                if ((id2.SerialId != 0) && (Sync.Players.GetPlayerById(id2) == null))
                {
                    long identityId = Sync.Players.TryGetIdentityId(id2.SteamId, id2.SerialId);
                    if (identityId != 0)
                    {
                        Sync.Players.RemoveIdentity(identityId, id2);
                    }
                }
            }
        }

        private void CreateBot(int playerNumber)
        {
            this.CreateBot(playerNumber, null);
        }

        private void CreateBot(int playerNumber, MyObjectBuilder_Bot botBuilder)
        {
            if (BotFactory != null)
            {
                MyPlayer player = Sync.Clients.LocalClient.GetPlayer(playerNumber);
                if (player != null)
                {
                    int num1;
                    bool flag = this.m_agentsToSpawn.ContainsKey(playerNumber);
                    bool load = botBuilder != null;
                    bool spawnedByPlayer = false;
                    MyBotDefinition botDefinition = null;
                    AgentSpawnData data = new AgentSpawnData();
                    if (flag)
                    {
                        data = this.m_agentsToSpawn[playerNumber];
                        spawnedByPlayer = data.CreatedByPlayer;
                        botDefinition = data.AgentDefinition;
                        this.m_agentsToSpawn.Remove(playerNumber);
                    }
                    else
                    {
                        if ((botBuilder == null) || botBuilder.BotDefId.TypeId.IsNull)
                        {
                            MyPlayer player2 = null;
                            if (Sync.Players.TryGetPlayerById(new MyPlayer.PlayerId(Sync.MyId, playerNumber), out player2))
                            {
                                Sync.Players.RemovePlayer(player2, true);
                            }
                            return;
                        }
                        MyDefinitionManager.Static.TryGetBotDefinition(botBuilder.BotDefId, out botDefinition);
                        if (botDefinition == null)
                        {
                            return;
                        }
                    }
                    if ((player.Character == null) || !player.Character.IsDead)
                    {
                        num1 = (int) BotFactory.CanCreateBotOfType(botDefinition.BehaviorType, load);
                    }
                    else
                    {
                        num1 = 0;
                    }
                    if ((num1 | spawnedByPlayer) == 0)
                    {
                        MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(Sync.MyId, playerNumber));
                        Sync.Players.RemovePlayer(playerById, true);
                    }
                    else
                    {
                        IMyBot newBot = null;
                        newBot = !flag ? BotFactory.CreateBot(player, botBuilder, botDefinition) : BotFactory.CreateBot(player, botBuilder, data.AgentDefinition);
                        if (newBot == null)
                        {
                            MyLog.Default.WriteLine("Could not create a bot for player " + player + "!");
                        }
                        else
                        {
                            this.m_botCollection.AddBot(playerNumber, newBot);
                            if (flag && (newBot is IMyEntityBot))
                            {
                                (newBot as IMyEntityBot).Spawn(data.SpawnPosition, spawnedByPlayer);
                            }
                            if (this.BotCreatedEvent != null)
                            {
                                this.BotCreatedEvent(playerNumber, newBot.BotDefinition);
                            }
                        }
                    }
                }
            }
        }

        private void CurrentToolbar_SelectedSlotChanged(MyToolbar toolbar, MyToolbar.SlotArgs args)
        {
            if (!(toolbar.SelectedItem is MyToolbarItemBot))
            {
                this.BotToSpawn = null;
            }
            if (!(toolbar.SelectedItem is MyToolbarItemAiCommand))
            {
                this.CommandDefinition = null;
            }
        }

        private void CurrentToolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs args, bool userActivated)
        {
            if (!(toolbar.GetItemAtIndex(toolbar.SlotToIndex(args.SlotNumber.Value)) is MyToolbarItemBot))
            {
                this.BotToSpawn = null;
            }
            if (!(toolbar.GetItemAtIndex(toolbar.SlotToIndex(args.SlotNumber.Value)) is MyToolbarItemAiCommand))
            {
                this.CommandDefinition = null;
            }
        }

        private void CurrentToolbar_Unselected(MyToolbar toolbar)
        {
            this.BotToSpawn = null;
            this.CommandDefinition = null;
        }

        public void DebugDrawBots()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                this.m_botCollection.DebugDrawBots();
            }
        }

        public void DebugRemoveFirstBot()
        {
            if (this.m_botCollection.HasBot)
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(Sync.MyId, this.m_botCollection.GetHandleToFirstBot()));
                Sync.Players.RemovePlayer(playerById, true);
            }
        }

        public void DebugSelectNextBot()
        {
            this.m_botCollection.DebugSelectNextBot();
        }

        public void DebugSelectPreviousBot()
        {
            this.m_botCollection.DebugSelectPreviousBot();
        }

        public void DespawnBotsOfType(string botType)
        {
            foreach (KeyValuePair<int, IMyBot> pair in this.m_botCollection.GetAllBots())
            {
                if (pair.Value.BotDefinition.BehaviorType == botType)
                {
                    Sync.Players.GetPlayerById(new MyPlayer.PlayerId(Sync.MyId, pair.Key));
                    this.RemoveBot(pair.Key, true);
                }
            }
            this.PerformBotRemovals();
        }

        private void DrawDebugTarget()
        {
            if (this.DebugTarget != null)
            {
                MyRenderProxy.DebugDrawSphere(this.DebugTarget.Value, 0.2f, Color.Red, 0f, false, false, true, false);
                MyRenderProxy.DebugDrawAABB(this.m_debugTargetAABB, Color.Green, 1f, 1f, true, false, false);
            }
        }

        public static int GenerateBotId()
        {
            int lastBotId = Static.m_lastBotId;
            Static.m_lastBotId = GenerateBotId(lastBotId);
            return Static.m_lastBotId;
        }

        public static int GenerateBotId(int lastSpawnedBot)
        {
            int num = lastSpawnedBot;
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                if (player.Id.SteamId == Sync.MyId)
                {
                    num = Math.Max(num, player.Id.SerialId);
                }
            }
            return (num + 1);
        }

        public void GenerateNavmeshTile(Vector3D? target)
        {
            if (target != null)
            {
                Vector3D vectord2;
                float num;
                IMyEntity entity;
                MyDestinationSphere end = new MyDestinationSphere(ref target.Value + 0.1f, 1f);
                Static.Pathfinding.FindPathGlobal(target.Value - 0.10000000149011612, end, null).GetNextTarget(target.Value, out vectord2, out num, out entity);
            }
            this.DebugTarget = target;
        }

        public int GetAvailableUncontrolledBotsCount() => 
            (BotFactory.MaximumUncontrolledBotCount - this.Bots.GetGeneratedBotCount());

        public int GetBotCount(string behaviorType) => 
            this.m_botCollection.GetCurrentBotsCount(behaviorType);

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            if (!MyPerGameSettings.EnableAi)
            {
                return null;
            }
            MyObjectBuilder_AIComponent objectBuilder = (MyObjectBuilder_AIComponent) base.GetObjectBuilder();
            objectBuilder.BotBrains = new List<MyObjectBuilder_AIComponent.BotData>();
            this.m_botCollection.GetBotsData(objectBuilder.BotBrains);
            return objectBuilder;
        }

        public override void HandleInput()
        {
            base.HandleInput();
            if ((MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay) && MyControllerHelper.IsControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
            {
                if ((MySession.Static.ControlledEntity != null) && (this.BotToSpawn != null))
                {
                    this.TrySpawnBot();
                }
                if ((MySession.Static.ControlledEntity != null) && (this.CommandDefinition != null))
                {
                    this.UseCommand();
                }
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponentBuilder)
        {
            if (MyPerGameSettings.EnableAi)
            {
                base.Init(sessionComponentBuilder);
                MyObjectBuilder_AIComponent component = (MyObjectBuilder_AIComponent) sessionComponentBuilder;
                if (component.BotBrains != null)
                {
                    foreach (MyObjectBuilder_AIComponent.BotData data in component.BotBrains)
                    {
                        this.m_loadedBotObjectBuildersByHandle[data.PlayerHandle] = data.BotBrain;
                    }
                }
            }
        }

        public void InvalidateNavmeshPosition(Vector3D? target)
        {
            if (target != null)
            {
                MyRDPathfinding pathfinding = (MyRDPathfinding) Static.Pathfinding;
                if (pathfinding != null)
                {
                    BoundingBoxD areaBox = new BoundingBoxD(target.Value - 0.1, target.Value + 0.1);
                    pathfinding.InvalidateArea(areaBox);
                }
            }
            this.DebugTarget = target;
        }

        public override void LoadData()
        {
            base.LoadData();
            if (MyPerGameSettings.EnableAi)
            {
                Sync.Players.NewPlayerRequestSucceeded += new Action<MyPlayer.PlayerId>(this.PlayerCreated);
                Sync.Players.LocalPlayerLoaded += new Action<int>(this.LocalPlayerLoaded);
                Sync.Players.NewPlayerRequestFailed += new Action<int>(this.Players_NewPlayerRequestFailed);
                if (Sync.IsServer)
                {
                    Sync.Players.PlayerRemoved += new Action<MyPlayer.PlayerId>(this.Players_PlayerRemoved);
                    Sync.Players.PlayerRequesting += new PlayerRequestDelegate(this.Players_PlayerRequesting);
                }
                if (MyPerGameSettings.PathfindingType != null)
                {
                    this.m_pathfinding = Activator.CreateInstance(MyPerGameSettings.PathfindingType) as IMyPathfinding;
                }
                this.m_behaviorTreeCollection = new MyBehaviorTreeCollection();
                this.m_botCollection = new MyBotCollection(this.m_behaviorTreeCollection);
                this.m_loadedLocalPlayers = new List<int>();
                this.m_loadedBotObjectBuildersByHandle = new Dictionary<int, MyObjectBuilder_Bot>();
                this.m_agentsToSpawn = new Dictionary<int, AgentSpawnData>();
                this.m_removeQueue = new MyConcurrentQueue<BotRemovalRequest>();
                this.m_maxBotNotification = new MyHudNotification(MyCommonTexts.NotificationMaximumNumberBots, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                this.m_processQueue = new MyConcurrentQueue<AgentSpawnData>();
                this.m_lock = new FastResourceLock();
                if (MyFakes.ENABLE_BEHAVIOR_TREE_TOOL_COMMUNICATION)
                {
                    MyMessageLoop.AddMessageHandler((uint) 0x40a, new ActionRef<System.Windows.Forms.Message>(this.OnUploadNewTree));
                    MyMessageLoop.AddMessageHandler((uint) 0x40c, new ActionRef<System.Windows.Forms.Message>(this.OnBreakDebugging));
                    MyMessageLoop.AddMessageHandler((uint) 0x40b, new ActionRef<System.Windows.Forms.Message>(this.OnResumeDebugging));
                }
                MyToolbarComponent.CurrentToolbar.SelectedSlotChanged += new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
                MyToolbarComponent.CurrentToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
                MyToolbarComponent.CurrentToolbar.Unselected += new Action<MyToolbar>(this.CurrentToolbar_Unselected);
            }
        }

        private void LocalPlayerLoaded(int playerNumber)
        {
            if ((playerNumber != 0) && !this.m_loadedLocalPlayers.Contains(playerNumber))
            {
                this.m_loadedLocalPlayers.Add(playerNumber);
            }
        }

        private void LocalPlayerRemoved(int playerNumber)
        {
            if (playerNumber != 0)
            {
                this.m_botCollection.TryRemoveBot(playerNumber);
            }
        }

        private void OnBreakDebugging(ref System.Windows.Forms.Message msg)
        {
            if (this.m_behaviorTreeCollection != null)
            {
                this.m_behaviorTreeCollection.DebugBreakDebugging = true;
            }
        }

        private void OnResumeDebugging(ref System.Windows.Forms.Message msg)
        {
            if (this.m_behaviorTreeCollection != null)
            {
                this.m_behaviorTreeCollection.DebugBreakDebugging = false;
            }
        }

        private void OnUploadNewTree(ref System.Windows.Forms.Message msg)
        {
            if (this.m_behaviorTreeCollection != null)
            {
                MyBehaviorTree outBehaviorTree = null;
                MyBehaviorDefinition definition = null;
                if (MyBehaviorTreeCollection.LoadUploadedBehaviorTree(out definition) && this.m_behaviorTreeCollection.HasBehavior(definition.Id.SubtypeId))
                {
                    this.m_botCollection.ResetBots(definition.Id.SubtypeName);
                    this.m_behaviorTreeCollection.RebuildBehaviorTree(definition, out outBehaviorTree);
                    this.m_botCollection.CheckCompatibilityWithBots(outBehaviorTree);
                }
                IntPtr zero = IntPtr.Zero;
                if (this.m_behaviorTreeCollection.TryGetValidToolWindow(out zero))
                {
                    WinApi.PostMessage(zero, 0x404, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        public void PathfindingSetDrawDebug(bool drawDebug)
        {
            this.m_debugDrawPathfinding = drawDebug;
        }

        public void PathfindingSetDrawNavmesh(bool drawNavmesh)
        {
            MyRDPathfinding pathfinding = this.m_pathfinding as MyRDPathfinding;
            if (pathfinding != null)
            {
                pathfinding.SetDrawNavmesh(drawNavmesh);
            }
        }

        private void PerformBotRemovals()
        {
            BotRemovalRequest request;
            while (this.m_removeQueue.TryDequeue(out request))
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(Sync.MyId, request.SerialId));
                if (playerById != null)
                {
                    Sync.Players.RemovePlayer(playerById, request.RemoveCharacter);
                }
            }
        }

        private void PlayerCreated(MyPlayer.PlayerId playerId)
        {
            if ((Sync.Players.GetPlayerById(playerId) != null) && !Sync.Players.GetPlayerById(playerId).IsRealPlayer)
            {
                this.CreateBot(playerId.SerialId);
            }
        }

        private void Players_NewPlayerRequestFailed(int serialId)
        {
            if ((serialId != 0) && this.m_agentsToSpawn.ContainsKey(serialId))
            {
                this.m_agentsToSpawn.Remove(serialId);
                if (this.m_agentsToSpawn[serialId].CreatedByPlayer)
                {
                    MyHud.Notifications.Add(this.m_maxBotNotification);
                }
            }
        }

        private void Players_PlayerRemoved(MyPlayer.PlayerId pid)
        {
            if (Sync.IsServer && (pid.SerialId != 0))
            {
                MyBotCollection bots = this.Bots;
                bots.TotalBotCount--;
            }
        }

        private void Players_PlayerRequesting(PlayerRequestArgs args)
        {
            if (args.PlayerId.SerialId != 0)
            {
                if (!this.CanSpawnMoreBots(args.PlayerId))
                {
                    args.Cancel = true;
                }
                else
                {
                    MyBotCollection bots = this.Bots;
                    bots.TotalBotCount++;
                }
            }
        }

        public void RemoveBot(int playerNumber, bool removeCharacter = false)
        {
            BotRemovalRequest instance = new BotRemovalRequest {
                SerialId = playerNumber,
                RemoveCharacter = removeCharacter
            };
            this.m_removeQueue.Enqueue(instance);
        }

        public void SetPathfindingDebugTarget(Vector3D? target)
        {
            MyExternalPathfinding pathfinding = this.m_pathfinding as MyExternalPathfinding;
            if (pathfinding != null)
            {
                pathfinding.SetTarget(target);
            }
            else if (target != null)
            {
                this.m_debugTargetAABB = new MyOrientedBoundingBoxD(target.Value, new Vector3D(5.0, 5.0, 5.0), Quaternion.Identity).GetAABB();
                List<MyEntity> result = new List<MyEntity>();
                MyGamePruningStructure.GetAllEntitiesInBox(ref this.m_debugTargetAABB, result, MyEntityQueryType.Both);
            }
            this.DebugTarget = target;
        }

        public override void Simulate()
        {
            if (MyPerGameSettings.EnableAi)
            {
                if (MyFakes.DEBUG_ONE_VOXEL_PATHFINDING_STEP_SETTING)
                {
                    if (!MyFakes.DEBUG_ONE_VOXEL_PATHFINDING_STEP)
                    {
                        return;
                    }
                }
                else if (MyFakes.DEBUG_ONE_AI_STEP_SETTING)
                {
                    if (!MyFakes.DEBUG_ONE_AI_STEP)
                    {
                        return;
                    }
                    MyFakes.DEBUG_ONE_AI_STEP = false;
                }
                MySimpleProfiler.Begin("AI", MySimpleProfiler.ProfilingBlockType.OTHER, "Simulate");
                if (this.m_pathfinding != null)
                {
                    this.m_pathfinding.Update();
                }
                base.Simulate();
                this.m_behaviorTreeCollection.Update();
                this.m_botCollection.Update();
                MySimpleProfiler.End("Simulate");
            }
        }

        public int SpawnNewBot(MyAgentDefinition agentDefinition)
        {
            Vector3D spawnPosition = new Vector3D();
            return (BotFactory.GetBotSpawnPosition(agentDefinition.BehaviorType, out spawnPosition) ? this.SpawnNewBotInternal(agentDefinition, new Vector3D?(spawnPosition), false) : 0);
        }

        public int SpawnNewBot(MyAgentDefinition agentDefinition, Vector3D? spawnPosition) => 
            this.SpawnNewBotInternal(agentDefinition, spawnPosition, true);

        public int SpawnNewBot(MyAgentDefinition agentDefinition, Vector3D position, bool createdByPlayer = true) => 
            this.SpawnNewBotInternal(agentDefinition, new Vector3D?(position), createdByPlayer);

        public bool SpawnNewBotGroup(string type, List<AgentGroupData> groupData, List<int> outIds)
        {
            int count = 0;
            foreach (AgentGroupData data in groupData)
            {
                count += data.Count;
            }
            BotFactory.GetBotGroupSpawnPositions(type, count, this.m_tmpSpawnPoints);
            int num2 = this.m_tmpSpawnPoints.Count;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            while (num3 < num2)
            {
                int item = this.SpawnNewBotInternal(groupData[num4].AgentDefinition, new Vector3D?(this.m_tmpSpawnPoints[num3]), false);
                if (outIds != null)
                {
                    outIds.Add(item);
                }
                if (groupData[num4].Count == ++num5)
                {
                    num5 = 0;
                    num4++;
                }
                num3++;
            }
            this.m_tmpSpawnPoints.Clear();
            return (num2 == count);
        }

        private int SpawnNewBotInternal(MyAgentDefinition agentDefinition, Vector3D? spawnPosition = new Vector3D?(), bool createdByPlayer = false)
        {
            int lastBotId;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
                {
                    if (player.Id.SteamId != Sync.MyId)
                    {
                        continue;
                    }
                    if (player.Id.SerialId > this.m_lastBotId)
                    {
                        this.m_lastBotId = player.Id.SerialId;
                    }
                }
                this.m_lastBotId++;
                lastBotId = this.m_lastBotId;
            }
            this.m_processQueue.Enqueue(new AgentSpawnData(agentDefinition, lastBotId, spawnPosition, createdByPlayer));
            return lastBotId;
        }

        private void TrySpawnBot()
        {
            Vector3D position;
            if ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity))
            {
                position = MySector.MainCamera.Position;
                Vector3D forward = MySector.MainCamera.WorldMatrix.Forward;
            }
            else
            {
                MatrixD xd = MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
                position = xd.Translation;
                Vector3D forward = xd.Forward;
            }
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            LineD ed = new LineD(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 1000f));
            MyPhysics.CastRay(ed.From, ed.To, toList, 15);
            if (toList.Count == 0)
            {
                Static.SpawnNewBot(this.BotToSpawn, position, true);
            }
            else
            {
                MyPhysics.HitInfo? nullable = null;
                foreach (MyPhysics.HitInfo info in toList)
                {
                    IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                    if (hitEntity is MyCubeGrid)
                    {
                        nullable = new MyPhysics.HitInfo?(info);
                    }
                    else if (hitEntity is MyVoxelBase)
                    {
                        nullable = new MyPhysics.HitInfo?(info);
                    }
                    else
                    {
                        if (!(hitEntity is MyVoxelPhysics))
                        {
                            continue;
                        }
                        nullable = new MyPhysics.HitInfo?(info);
                    }
                    break;
                }
                Vector3D position = (nullable == null) ? MySector.MainCamera.Position : nullable.Value.Position;
                Static.SpawnNewBot(this.BotToSpawn, position, true);
            }
        }

        public void TrySpawnBot(MyAgentDefinition agentDefinition)
        {
            this.BotToSpawn = agentDefinition;
            this.TrySpawnBot();
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (MyPerGameSettings.EnableAi)
            {
                Sync.Players.NewPlayerRequestSucceeded -= new Action<MyPlayer.PlayerId>(this.PlayerCreated);
                Sync.Players.LocalPlayerRemoved -= new Action<int>(this.LocalPlayerRemoved);
                Sync.Players.LocalPlayerLoaded -= new Action<int>(this.LocalPlayerLoaded);
                Sync.Players.NewPlayerRequestFailed -= new Action<int>(this.Players_NewPlayerRequestFailed);
                if (Sync.IsServer)
                {
                    Sync.Players.PlayerRequesting -= new PlayerRequestDelegate(this.Players_PlayerRequesting);
                    Sync.Players.PlayerRemoved -= new Action<MyPlayer.PlayerId>(this.Players_PlayerRemoved);
                }
                if (this.m_pathfinding != null)
                {
                    this.m_pathfinding.UnloadData();
                }
                this.m_botCollection.UnloadData();
                this.m_botCollection = null;
                this.m_pathfinding = null;
                if (MyFakes.ENABLE_BEHAVIOR_TREE_TOOL_COMMUNICATION)
                {
                    MyMessageLoop.RemoveMessageHandler((uint) 0x40a, new ActionRef<System.Windows.Forms.Message>(this.OnUploadNewTree));
                    MyMessageLoop.RemoveMessageHandler((uint) 0x40c, new ActionRef<System.Windows.Forms.Message>(this.OnBreakDebugging));
                    MyMessageLoop.RemoveMessageHandler((uint) 0x40b, new ActionRef<System.Windows.Forms.Message>(this.OnResumeDebugging));
                }
                if (MyToolbarComponent.CurrentToolbar != null)
                {
                    MyToolbarComponent.CurrentToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
                    MyToolbarComponent.CurrentToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
                    MyToolbarComponent.CurrentToolbar.Unselected -= new Action<MyToolbar>(this.CurrentToolbar_Unselected);
                }
            }
            Static = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (MyPerGameSettings.EnableAi)
            {
                this.PerformBotRemovals();
                while (true)
                {
                    AgentSpawnData data;
                    if (!this.m_processQueue.TryDequeue(out data))
                    {
                        if (this.m_debugDrawPathfinding && (this.m_pathfinding != null))
                        {
                            this.m_pathfinding.DebugDraw();
                        }
                        this.m_botCollection.DebugDraw();
                        this.DebugDrawBots();
                        this.DrawDebugTarget();
                        break;
                    }
                    this.m_agentsToSpawn[data.BotId] = data;
                    Sync.Players.RequestNewPlayer(data.BotId, MyDefinitionManager.Static.GetRandomCharacterName(), data.AgentDefinition.BotModel, false, false);
                }
            }
        }

        private void UseCommand()
        {
            MyAiCommandBehavior behavior1 = new MyAiCommandBehavior();
            behavior1.InitCommand(this.CommandDefinition);
            behavior1.ActivateCommand();
        }

        public MyBotCollection Bots =>
            this.m_botCollection;

        public IMyPathfinding Pathfinding =>
            this.m_pathfinding;

        public MyBehaviorTreeCollection BehaviorTrees =>
            this.m_behaviorTreeCollection;

        public override System.Type[] Dependencies =>
            new System.Type[] { typeof(MyToolbarComponent) };

        public Vector3D? DebugTarget { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct AgentGroupData
        {
            public MyAgentDefinition AgentDefinition;
            public int Count;
            public AgentGroupData(MyAgentDefinition agentDefinition, int count)
            {
                this.AgentDefinition = agentDefinition;
                this.Count = count;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AgentSpawnData
        {
            public MyAgentDefinition AgentDefinition;
            public Vector3D? SpawnPosition;
            public bool CreatedByPlayer;
            public int BotId;
            public AgentSpawnData(MyAgentDefinition agentDefinition, int botId, Vector3D? spawnPosition = new Vector3D?(), bool createAlways = false)
            {
                this.AgentDefinition = agentDefinition;
                this.SpawnPosition = spawnPosition;
                this.CreatedByPlayer = createAlways;
                this.BotId = botId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BotRemovalRequest
        {
            public int SerialId;
            public bool RemoveCharacter;
        }
    }
}

