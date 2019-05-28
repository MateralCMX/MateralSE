namespace Sandbox.Game.Multiplayer
{
    using ProtoBuf;
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner]
    public class MyPlayerCollection : MyIdentity.Friend, IMyPlayerCollection
    {
        private readonly ConcurrentDictionary<MyPlayer.PlayerId, MyPlayer> m_players = new ConcurrentDictionary<MyPlayer.PlayerId, MyPlayer>(MyPlayer.PlayerId.Comparer);
        private List<MyPlayer> m_tmpRemovedPlayers = new List<MyPlayer>();
        private CachingDictionary<long, MyPlayer.PlayerId> m_controlledEntities = new CachingDictionary<long, MyPlayer.PlayerId>();
        private Dictionary<long, MyPlayer.PlayerId> m_previousControlledEntities = new Dictionary<long, MyPlayer.PlayerId>();
        private ConcurrentDictionary<long, MyIdentity> m_allIdentities = new ConcurrentDictionary<long, MyIdentity>();
        private readonly ConcurrentDictionary<MyPlayer.PlayerId, long> m_playerIdentityIds = new ConcurrentDictionary<MyPlayer.PlayerId, long>(MyPlayer.PlayerId.Comparer);
        private readonly Dictionary<long, MyPlayer.PlayerId> m_identityPlayerIds = new Dictionary<long, MyPlayer.PlayerId>();
        private HashSet<long> m_npcIdentities = new HashSet<long>();
        private List<EndpointId> m_tmpPlayersLinkedToBlockLimit = new List<EndpointId>();
        private Action<MyEntity> m_entityClosingHandler;
        private static Dictionary<long, MyPlayer.PlayerId> m_controlledEntitiesClientCache;
        [CompilerGenerated]
        private static Action<ulong> OnRespawnRequestFailureEvent;
        [CompilerGenerated]
        private Action<MyPlayer.PlayerId> NewPlayerRequestSucceeded;
        [CompilerGenerated]
        private Action<int> NewPlayerRequestFailed;
        [CompilerGenerated]
        private Action<int> LocalPlayerRemoved;
        [CompilerGenerated]
        private Action<int> LocalPlayerLoaded;
        [CompilerGenerated]
        private Action<string, Color> LocalRespawnRequested;
        [CompilerGenerated]
        private Action<MyPlayer.PlayerId> PlayerRemoved;
        [CompilerGenerated]
        private PlayerRequestDelegate PlayerRequesting;
        [CompilerGenerated]
        private Action<bool, MyPlayer.PlayerId> PlayersChanged;
        [CompilerGenerated]
        private Action<long> PlayerCharacterDied;
        [CompilerGenerated]
        private Action IdentitiesChanged;

        public event Action IdentitiesChanged
        {
            [CompilerGenerated] add
            {
                Action identitiesChanged = this.IdentitiesChanged;
                while (true)
                {
                    Action a = identitiesChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    identitiesChanged = Interlocked.CompareExchange<Action>(ref this.IdentitiesChanged, action3, a);
                    if (ReferenceEquals(identitiesChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action identitiesChanged = this.IdentitiesChanged;
                while (true)
                {
                    Action source = identitiesChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    identitiesChanged = Interlocked.CompareExchange<Action>(ref this.IdentitiesChanged, action3, source);
                    if (ReferenceEquals(identitiesChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<int> LocalPlayerLoaded
        {
            [CompilerGenerated] add
            {
                Action<int> localPlayerLoaded = this.LocalPlayerLoaded;
                while (true)
                {
                    Action<int> a = localPlayerLoaded;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    localPlayerLoaded = Interlocked.CompareExchange<Action<int>>(ref this.LocalPlayerLoaded, action3, a);
                    if (ReferenceEquals(localPlayerLoaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> localPlayerLoaded = this.LocalPlayerLoaded;
                while (true)
                {
                    Action<int> source = localPlayerLoaded;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    localPlayerLoaded = Interlocked.CompareExchange<Action<int>>(ref this.LocalPlayerLoaded, action3, source);
                    if (ReferenceEquals(localPlayerLoaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<int> LocalPlayerRemoved
        {
            [CompilerGenerated] add
            {
                Action<int> localPlayerRemoved = this.LocalPlayerRemoved;
                while (true)
                {
                    Action<int> a = localPlayerRemoved;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    localPlayerRemoved = Interlocked.CompareExchange<Action<int>>(ref this.LocalPlayerRemoved, action3, a);
                    if (ReferenceEquals(localPlayerRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> localPlayerRemoved = this.LocalPlayerRemoved;
                while (true)
                {
                    Action<int> source = localPlayerRemoved;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    localPlayerRemoved = Interlocked.CompareExchange<Action<int>>(ref this.LocalPlayerRemoved, action3, source);
                    if (ReferenceEquals(localPlayerRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<string, Color> LocalRespawnRequested
        {
            [CompilerGenerated] add
            {
                Action<string, Color> localRespawnRequested = this.LocalRespawnRequested;
                while (true)
                {
                    Action<string, Color> a = localRespawnRequested;
                    Action<string, Color> action3 = (Action<string, Color>) Delegate.Combine(a, value);
                    localRespawnRequested = Interlocked.CompareExchange<Action<string, Color>>(ref this.LocalRespawnRequested, action3, a);
                    if (ReferenceEquals(localRespawnRequested, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<string, Color> localRespawnRequested = this.LocalRespawnRequested;
                while (true)
                {
                    Action<string, Color> source = localRespawnRequested;
                    Action<string, Color> action3 = (Action<string, Color>) Delegate.Remove(source, value);
                    localRespawnRequested = Interlocked.CompareExchange<Action<string, Color>>(ref this.LocalRespawnRequested, action3, source);
                    if (ReferenceEquals(localRespawnRequested, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<int> NewPlayerRequestFailed
        {
            [CompilerGenerated] add
            {
                Action<int> newPlayerRequestFailed = this.NewPlayerRequestFailed;
                while (true)
                {
                    Action<int> a = newPlayerRequestFailed;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    newPlayerRequestFailed = Interlocked.CompareExchange<Action<int>>(ref this.NewPlayerRequestFailed, action3, a);
                    if (ReferenceEquals(newPlayerRequestFailed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> newPlayerRequestFailed = this.NewPlayerRequestFailed;
                while (true)
                {
                    Action<int> source = newPlayerRequestFailed;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    newPlayerRequestFailed = Interlocked.CompareExchange<Action<int>>(ref this.NewPlayerRequestFailed, action3, source);
                    if (ReferenceEquals(newPlayerRequestFailed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyPlayer.PlayerId> NewPlayerRequestSucceeded
        {
            [CompilerGenerated] add
            {
                Action<MyPlayer.PlayerId> newPlayerRequestSucceeded = this.NewPlayerRequestSucceeded;
                while (true)
                {
                    Action<MyPlayer.PlayerId> a = newPlayerRequestSucceeded;
                    Action<MyPlayer.PlayerId> action3 = (Action<MyPlayer.PlayerId>) Delegate.Combine(a, value);
                    newPlayerRequestSucceeded = Interlocked.CompareExchange<Action<MyPlayer.PlayerId>>(ref this.NewPlayerRequestSucceeded, action3, a);
                    if (ReferenceEquals(newPlayerRequestSucceeded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPlayer.PlayerId> newPlayerRequestSucceeded = this.NewPlayerRequestSucceeded;
                while (true)
                {
                    Action<MyPlayer.PlayerId> source = newPlayerRequestSucceeded;
                    Action<MyPlayer.PlayerId> action3 = (Action<MyPlayer.PlayerId>) Delegate.Remove(source, value);
                    newPlayerRequestSucceeded = Interlocked.CompareExchange<Action<MyPlayer.PlayerId>>(ref this.NewPlayerRequestSucceeded, action3, source);
                    if (ReferenceEquals(newPlayerRequestSucceeded, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<ulong> OnRespawnRequestFailureEvent
        {
            [CompilerGenerated] add
            {
                Action<ulong> onRespawnRequestFailureEvent = OnRespawnRequestFailureEvent;
                while (true)
                {
                    Action<ulong> a = onRespawnRequestFailureEvent;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Combine(a, value);
                    onRespawnRequestFailureEvent = Interlocked.CompareExchange<Action<ulong>>(ref OnRespawnRequestFailureEvent, action3, a);
                    if (ReferenceEquals(onRespawnRequestFailureEvent, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong> onRespawnRequestFailureEvent = OnRespawnRequestFailureEvent;
                while (true)
                {
                    Action<ulong> source = onRespawnRequestFailureEvent;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Remove(source, value);
                    onRespawnRequestFailureEvent = Interlocked.CompareExchange<Action<ulong>>(ref OnRespawnRequestFailureEvent, action3, source);
                    if (ReferenceEquals(onRespawnRequestFailureEvent, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<long> PlayerCharacterDied
        {
            [CompilerGenerated] add
            {
                Action<long> playerCharacterDied = this.PlayerCharacterDied;
                while (true)
                {
                    Action<long> a = playerCharacterDied;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    playerCharacterDied = Interlocked.CompareExchange<Action<long>>(ref this.PlayerCharacterDied, action3, a);
                    if (ReferenceEquals(playerCharacterDied, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> playerCharacterDied = this.PlayerCharacterDied;
                while (true)
                {
                    Action<long> source = playerCharacterDied;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    playerCharacterDied = Interlocked.CompareExchange<Action<long>>(ref this.PlayerCharacterDied, action3, source);
                    if (ReferenceEquals(playerCharacterDied, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyPlayer.PlayerId> PlayerRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyPlayer.PlayerId> playerRemoved = this.PlayerRemoved;
                while (true)
                {
                    Action<MyPlayer.PlayerId> a = playerRemoved;
                    Action<MyPlayer.PlayerId> action3 = (Action<MyPlayer.PlayerId>) Delegate.Combine(a, value);
                    playerRemoved = Interlocked.CompareExchange<Action<MyPlayer.PlayerId>>(ref this.PlayerRemoved, action3, a);
                    if (ReferenceEquals(playerRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPlayer.PlayerId> playerRemoved = this.PlayerRemoved;
                while (true)
                {
                    Action<MyPlayer.PlayerId> source = playerRemoved;
                    Action<MyPlayer.PlayerId> action3 = (Action<MyPlayer.PlayerId>) Delegate.Remove(source, value);
                    playerRemoved = Interlocked.CompareExchange<Action<MyPlayer.PlayerId>>(ref this.PlayerRemoved, action3, source);
                    if (ReferenceEquals(playerRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event PlayerRequestDelegate PlayerRequesting
        {
            [CompilerGenerated] add
            {
                PlayerRequestDelegate playerRequesting = this.PlayerRequesting;
                while (true)
                {
                    PlayerRequestDelegate a = playerRequesting;
                    PlayerRequestDelegate delegate4 = (PlayerRequestDelegate) Delegate.Combine(a, value);
                    playerRequesting = Interlocked.CompareExchange<PlayerRequestDelegate>(ref this.PlayerRequesting, delegate4, a);
                    if (ReferenceEquals(playerRequesting, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                PlayerRequestDelegate playerRequesting = this.PlayerRequesting;
                while (true)
                {
                    PlayerRequestDelegate source = playerRequesting;
                    PlayerRequestDelegate delegate4 = (PlayerRequestDelegate) Delegate.Remove(source, value);
                    playerRequesting = Interlocked.CompareExchange<PlayerRequestDelegate>(ref this.PlayerRequesting, delegate4, source);
                    if (ReferenceEquals(playerRequesting, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<bool, MyPlayer.PlayerId> PlayersChanged
        {
            [CompilerGenerated] add
            {
                Action<bool, MyPlayer.PlayerId> playersChanged = this.PlayersChanged;
                while (true)
                {
                    Action<bool, MyPlayer.PlayerId> a = playersChanged;
                    Action<bool, MyPlayer.PlayerId> action3 = (Action<bool, MyPlayer.PlayerId>) Delegate.Combine(a, value);
                    playersChanged = Interlocked.CompareExchange<Action<bool, MyPlayer.PlayerId>>(ref this.PlayersChanged, action3, a);
                    if (ReferenceEquals(playersChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool, MyPlayer.PlayerId> playersChanged = this.PlayersChanged;
                while (true)
                {
                    Action<bool, MyPlayer.PlayerId> source = playersChanged;
                    Action<bool, MyPlayer.PlayerId> action3 = (Action<bool, MyPlayer.PlayerId>) Delegate.Remove(source, value);
                    playersChanged = Interlocked.CompareExchange<Action<bool, MyPlayer.PlayerId>>(ref this.PlayersChanged, action3, source);
                    if (ReferenceEquals(playersChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyPlayerCollection()
        {
            this.m_entityClosingHandler = new Action<MyEntity>(this.EntityClosing);
            m_controlledEntitiesClientCache = !Sync.IsServer ? new Dictionary<long, MyPlayer.PlayerId>() : null;
        }

        private void AddPlayer(MyPlayer.PlayerId playerId, MyPlayer newPlayer)
        {
            if (Sync.IsServer && (MyVisualScriptLogicProvider.PlayerConnected != null))
            {
                MyVisualScriptLogicProvider.PlayerConnected(newPlayer.Identity.IdentityId);
            }
            newPlayer.Identity.LastLoginTime = DateTime.Now;
            newPlayer.Identity.BlockLimits.SetAllDirty();
            this.m_players.TryAdd(playerId, newPlayer);
            this.OnPlayersChanged(true, playerId);
        }

        private void AfterCreateIdentity(MyIdentity identity, bool addToNpcs = false, bool sendToClients = true)
        {
            if (addToNpcs)
            {
                this.MarkIdentityAsNPC(identity.IdentityId);
            }
            if (!this.m_allIdentities.ContainsKey(identity.IdentityId))
            {
                this.m_allIdentities.TryAdd(identity.IdentityId, identity);
                identity.CharacterChanged += new Action<MyCharacter, MyCharacter>(this.Identity_CharacterChanged);
                if (identity.Character != null)
                {
                    identity.Character.CharacterDied += new Action<MyCharacter>(this.Character_CharacterDied);
                }
            }
            if ((Sync.IsServer && (Sync.MyId != 0L)) & sendToClients)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<bool, long, string>(s => new Action<bool, long, string>(MyPlayerCollection.OnIdentityCreated), addToNpcs, identity.IdentityId, identity.DisplayName, targetEndpoint, position);
            }
            Action identitiesChanged = this.IdentitiesChanged;
            if (identitiesChanged != null)
            {
                identitiesChanged();
            }
        }

        private void ChangeDisplayNameOfPlayerAndIdentity(MyObjectBuilder_Player playerOb, string name)
        {
            playerOb.DisplayName = MyGameService.UserName;
            MyIdentity identity = this.TryGetIdentity(playerOb.IdentityId);
            if (identity != null)
            {
                identity.SetDisplayName(MyGameService.UserName);
            }
        }

        public static void ChangePlayerCharacter(MyPlayer player, MyCharacter characterEntity, MyEntity entity)
        {
            if (player == null)
            {
                MySandboxGame.Log.WriteLine("Player not found");
            }
            else
            {
                if (player.Identity == null)
                {
                    MySandboxGame.Log.WriteLine("Player identity was null");
                }
                player.Identity.ChangeCharacter(characterEntity);
                if ((player.Controller == null) || (player.Controller.ControlledEntity == null))
                {
                    Sync.Players.SetControlledEntityInternal(player.Id, entity, false);
                }
                if (ReferenceEquals(player, MySession.Static.LocalHumanPlayer))
                {
                    MyShipController controlledEntity = MySession.Static.ControlledEntity as MyShipController;
                    if ((controlledEntity == null) || !ReferenceEquals(controlledEntity.Pilot, characterEntity))
                    {
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(MySession.Static.LocalCharacter.IsInFirstPersonView ? MyCameraControllerEnum.Entity : MyCameraControllerEnum.ThirdPersonSpectator, MySession.Static.LocalCharacter, position);
                    }
                }
            }
        }

        private void Character_CharacterDied(MyCharacter diedCharacter)
        {
            if (((this.PlayerCharacterDied != null) && (diedCharacter != null)) && (diedCharacter.ControllerInfo.ControllingIdentityId != 0))
            {
                this.PlayerCharacterDied(diedCharacter.ControllerInfo.ControllingIdentityId);
            }
        }

        [Event(null, 0x60e), Reliable, Server]
        public static void ClearDampeningEntity(long controlledEntityId)
        {
            Sandbox.Game.Entities.IMyControllableEntity entity = MyEntities.GetEntityByIdOrDefault(controlledEntityId, null, false) as Sandbox.Game.Entities.IMyControllableEntity;
            if (entity != null)
            {
                entity.RelativeDampeningEntity = null;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient), entity.Entity.EntityId, 0L, targetEndpoint, position);
            }
        }

        public void ClearIdentities()
        {
            this.m_allIdentities.Clear();
            this.m_npcIdentities.Clear();
        }

        public void ClearPlayers()
        {
            this.m_players.Clear();
            this.m_controlledEntities.Clear();
            this.m_playerIdentityIds.Clear();
            this.m_identityPlayerIds.Clear();
        }

        private void controller_ControlledEntityChanged(Sandbox.Game.Entities.IMyControllableEntity oldEntity, Sandbox.Game.Entities.IMyControllableEntity newEntity)
        {
            EndpointId id2;
            Vector3D? nullable;
            MyEntityController controller = (newEntity == null) ? oldEntity.ControllerInfo.Controller : newEntity.ControllerInfo.Controller;
            MyEntity entity = oldEntity as MyEntity;
            if (entity != null)
            {
                MyPlayer.PlayerId id;
                if (this.m_controlledEntities.TryGetValue(entity.EntityId, out id))
                {
                    this.m_previousControlledEntities[entity.EntityId] = id;
                }
                this.m_controlledEntities.Remove(entity.EntityId, true);
                if (Sync.IsServer)
                {
                    id2 = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<ulong, int, long, bool>(s => new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess), 0L, 0, entity.EntityId, true, id2, nullable);
                }
            }
            MyEntity entity2 = newEntity as MyEntity;
            if ((entity2 != null) && (controller != null))
            {
                this.m_controlledEntities.Add(entity2.EntityId, controller.Player.Id, true);
                if (Sync.IsServer)
                {
                    id2 = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<ulong, int, long, bool>(s => new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess), controller.Player.Id.SteamId, controller.Player.Id.SerialId, entity2.EntityId, true, id2, nullable);
                }
            }
        }

        public override MyIdentity CreateNewIdentity(MyObjectBuilder_Identity objectBuilder)
        {
            bool addToNpcs = false;
            MyEntityIdentifier.ID_OBJECT_TYPE idObjectType = MyEntityIdentifier.GetIdObjectType(objectBuilder.IdentityId);
            if ((idObjectType == MyEntityIdentifier.ID_OBJECT_TYPE.NPC) || (idObjectType == MyEntityIdentifier.ID_OBJECT_TYPE.SPAWN_GROUP))
            {
                addToNpcs = true;
            }
            MyIdentity identity = base.CreateNewIdentity(objectBuilder);
            this.AfterCreateIdentity(identity, addToNpcs, true);
            return identity;
        }

        public override MyIdentity CreateNewIdentity(string name, long identityId, string model, Vector3? colorMask)
        {
            bool addToNpcs = false;
            MyEntityIdentifier.ID_OBJECT_TYPE idObjectType = MyEntityIdentifier.GetIdObjectType(identityId);
            if ((idObjectType == MyEntityIdentifier.ID_OBJECT_TYPE.NPC) || (idObjectType == MyEntityIdentifier.ID_OBJECT_TYPE.SPAWN_GROUP))
            {
                addToNpcs = true;
            }
            MyIdentity identity = base.CreateNewIdentity(name, identityId, model, colorMask);
            this.AfterCreateIdentity(identity, addToNpcs, true);
            return identity;
        }

        public MyIdentity CreateNewIdentity(string name, string model = null, Vector3? colorMask = new Vector3?(), bool initialPlayer = false)
        {
            Vector3? nullable = null;
            MyIdentity identity = base.CreateNewIdentity(name, model, nullable);
            this.AfterCreateIdentity(identity, false, !initialPlayer);
            return identity;
        }

        public MyIdentity CreateNewNpcIdentity(string name, long identityId = 0L)
        {
            MyIdentity identity;
            Vector3? nullable;
            if (identityId == 0)
            {
                nullable = null;
                identity = base.CreateNewIdentity(name, null, nullable);
            }
            else
            {
                nullable = null;
                identity = base.CreateNewIdentity(name, identityId, null, nullable);
            }
            this.AfterCreateIdentity(identity, true, true);
            return identity;
        }

        public MyPlayer CreateNewPlayer(MyIdentity identity, MyNetworkClient steamClient, string playerName, bool realPlayer)
        {
            MyPlayer.PlayerId playerId = this.FindFreePlayerId(steamClient.SteamUserId);
            MyObjectBuilder_Player playerBuilder = new MyObjectBuilder_Player();
            playerBuilder.DisplayName = playerName;
            playerBuilder.IdentityId = identity.IdentityId;
            return this.CreateNewPlayerInternal(steamClient, playerId, playerBuilder);
        }

        public MyPlayer CreateNewPlayer(MyIdentity identity, MyPlayer.PlayerId id, string playerName, bool realPlayer, bool initialPlayer, bool newIdentity)
        {
            MyNetworkClient client;
            Sync.Clients.TryGetClient(id.SteamId, out client);
            if (client == null)
            {
                return null;
            }
            MyObjectBuilder_Player player1 = new MyObjectBuilder_Player();
            player1.DisplayName = playerName;
            player1.IdentityId = identity.IdentityId;
            player1.ForceRealPlayer = realPlayer;
            MyObjectBuilder_Player playerBuilder = player1;
            MyPlayer player2 = this.CreateNewPlayerInternal(client, id, playerBuilder);
            if (player2 != null)
            {
                List<Vector3> buildColorSlots = null;
                if (!MyPlayer.IsColorsSetToDefaults(player2.BuildColorSlots))
                {
                    buildColorSlots = player2.BuildColorSlots;
                }
                if (!initialPlayer || (MyMultiplayer.Static == null))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<ulong, int, long, string, List<Vector3>, bool>(s => new Action<ulong, int, long, string, List<Vector3>, bool>(MyPlayerCollection.OnPlayerCreated), id.SteamId, id.SerialId, identity.IdentityId, playerName, buildColorSlots, realPlayer, targetEndpoint, position);
                }
                else
                {
                    PlayerDataMsg msg = new PlayerDataMsg {
                        ClientSteamId = id.SteamId,
                        PlayerSerialId = id.SerialId,
                        IdentityId = identity.IdentityId,
                        DisplayName = playerName,
                        BuildColors = buildColorSlots,
                        RealPlayer = realPlayer,
                        NewIdentity = newIdentity
                    };
                    MyMultiplayer.GetReplicationServer().SendPlayerData(ref msg);
                }
            }
            return player2;
        }

        private MyPlayer CreateNewPlayerInternal(MyNetworkClient steamClient, MyPlayer.PlayerId playerId, MyObjectBuilder_Player playerBuilder)
        {
            if (!this.m_playerIdentityIds.ContainsKey(playerId))
            {
                this.m_playerIdentityIds.TryAdd(playerId, playerBuilder.IdentityId);
                if (!this.m_identityPlayerIds.ContainsKey(playerBuilder.IdentityId))
                {
                    this.m_identityPlayerIds.Add(playerBuilder.IdentityId, playerId);
                }
            }
            MyPlayer playerById = this.GetPlayerById(playerId);
            if (playerById == null)
            {
                playerById = new MyPlayer(steamClient, playerId);
                playerById.Init(playerBuilder);
                playerById.IdentityChanged += new Action<MyPlayer, MyIdentity>(this.player_IdentityChanged);
                playerById.Controller.ControlledEntityChanged += new Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>(this.controller_ControlledEntityChanged);
                this.AddPlayer(playerId, playerById);
                if (MyFakes.ENABLE_MISSION_TRIGGERS && (MySessionComponentMissionTriggers.Static != null))
                {
                    MySessionComponentMissionTriggers.Static.TryCreateFromDefault(playerId, false);
                }
            }
            return playerById;
        }

        [Conditional("DEBUG")]
        public void DebugDraw()
        {
            int num = 0 + 1;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "Steam clients:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (MyNetworkClient client in Sync.Clients.GetClients())
            {
                num++;
                object[] objArray1 = new object[] { "  SteamId: ", client.SteamUserId, ", Name: ", client.DisplayName };
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), string.Concat(objArray1), Color.LightYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            num++;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "Online players:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (KeyValuePair<MyPlayer.PlayerId, MyPlayer> pair in this.m_players)
            {
                num++;
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "  PlayerId: " + pair.Key.ToString() + ", Name: " + pair.Value.DisplayName, Color.LightYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            num++;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "Player identities:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (KeyValuePair<MyPlayer.PlayerId, long> pair2 in this.m_playerIdentityIds)
            {
                MyPlayer player;
                MyIdentity identity;
                Color salmon;
                this.m_players.TryGetValue(pair2.Key, out player);
                string str = (player == null) ? "N.A." : player.DisplayName;
                this.m_allIdentities.TryGetValue(pair2.Value, out identity);
                if ((identity == null) || identity.IsDead)
                {
                    salmon = Color.Salmon;
                }
                else
                {
                    salmon = Color.LightYellow;
                }
                Color color = salmon;
                string str2 = (identity == null) ? "N.A." : identity.DisplayName;
                num++;
                string[] textArray1 = new string[] { "  PlayerId: ", pair2.Key.ToString(), ", Name: ", str, "; IdentityId: ", pair2.Value.ToString(), ", Name: ", str2 };
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), string.Concat(textArray1), color, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            num++;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "All identities:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (KeyValuePair<long, MyIdentity> pair3 in this.m_allIdentities)
            {
                bool isDead = pair3.Value.IsDead;
                Color color = isDead ? Color.Salmon : Color.LightYellow;
                num++;
                string[] textArray2 = new string[] { "  IdentityId: ", pair3.Key.ToString(), ", Name: ", pair3.Value.DisplayName, ", State: ", isDead ? "DEAD" : "ALIVE" };
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), string.Concat(textArray2), color, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            num++;
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), "Control:", Color.GreenYellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            foreach (KeyValuePair<long, MyPlayer.PlayerId> pair4 in this.m_controlledEntities)
            {
                MyEntity entity;
                MyPlayer player2;
                string text1;
                MyEntities.TryGetEntityById(pair4.Key, out entity, false);
                Color color = (entity == null) ? Color.Salmon : Color.LightYellow;
                string str3 = (entity == null) ? "Unknown entity" : entity.ToString();
                if (entity != null)
                {
                    text1 = entity.EntityId.ToString();
                }
                else
                {
                    text1 = "N.A.";
                }
                string str4 = text1;
                this.m_players.TryGetValue(pair4.Value, out player2);
                string str5 = (player2 == null) ? "N.A." : player2.DisplayName;
                num++;
                string[] textArray3 = new string[9];
                textArray3[0] = "  ";
                textArray3[1] = str3;
                textArray3[2] = " controlled by ";
                textArray3[3] = str5;
                textArray3[4] = " (entityId = ";
                textArray3[5] = str4;
                textArray3[6] = ", playerId = ";
                textArray3[7] = pair4.Value.ToString();
                textArray3[8] = ")";
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, num * 13f), string.Concat(textArray3), color, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            if (MySession.Static.ControlledEntity != null)
            {
                MyShipController controlledEntity = MySession.Static.ControlledEntity as MyShipController;
                if (controlledEntity != null)
                {
                    MyCubeGrid parent = controlledEntity.Parent as MyCubeGrid;
                    if (parent != null)
                    {
                        parent.GridSystems.ControlSystem.DebugDraw(++num * 13f);
                    }
                }
            }
        }

        private void EntityClosing(MyEntity entity)
        {
            entity.OnClosing -= this.m_entityClosingHandler;
            if (!(entity is Sandbox.Game.Entities.IMyControllableEntity))
            {
                this.m_controlledEntities.Remove(entity.EntityId, true);
                this.m_previousControlledEntities.Remove(entity.EntityId);
                if (Sync.IsServer)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.OnControlReleased), entity.EntityId, targetEndpoint, position);
                }
            }
        }

        public void ExtendControl(Sandbox.Game.Entities.IMyControllableEntity baseEntity, MyEntity entityGettingControl)
        {
            MyEntityController controller = baseEntity.ControllerInfo.Controller;
            if (controller != null)
            {
                this.TrySetControlledEntity(controller.Player.Id, entityGettingControl);
            }
            else
            {
                MyRemoteControl control1 = baseEntity as MyRemoteControl;
            }
        }

        public MyPlayer.PlayerId FindFreePlayerId(ulong steamId)
        {
            MyPlayer.PlayerId key = new MyPlayer.PlayerId(steamId);
            while (this.m_playerIdentityIds.ContainsKey(key))
            {
                key += 1;
            }
            return key;
        }

        private long FindLocalIdentityId(MyObjectBuilder_Checkpoint checkpoint)
        {
            long playerId = 0L;
            playerId = this.TryGetIdentityId(Sync.MyId, 0);
            if (playerId == 0)
            {
                if ((checkpoint.Players != null) && checkpoint.Players.Dictionary.ContainsKey(Sync.MyId))
                {
                    playerId = (checkpoint.Players[Sync.MyId].PlayerId != 0) ? checkpoint.Players[Sync.MyId].PlayerId : playerId;
                }
                if (checkpoint.AllPlayers != null)
                {
                    foreach (MyObjectBuilder_Checkpoint.PlayerItem item in checkpoint.AllPlayers)
                    {
                        if ((item.SteamId == Sync.MyId) && !item.IsDead)
                        {
                            playerId = item.PlayerId;
                        }
                        else
                        {
                            if (item.SteamId != Sync.MyId)
                            {
                                continue;
                            }
                            if (playerId != item.PlayerId)
                            {
                                continue;
                            }
                            if (!item.IsDead)
                            {
                                continue;
                            }
                            playerId = 0L;
                        }
                        break;
                    }
                }
            }
            return playerId;
        }

        public ICollection<MyIdentity> GetAllIdentities() => 
            this.m_allIdentities.Values;

        public IOrderedEnumerable<KeyValuePair<long, MyIdentity>> GetAllIdentitiesOrderByName() => 
            (from pair in this.m_allIdentities
                orderby pair.Value.DisplayName
                select pair);

        public ICollection<MyPlayer.PlayerId> GetAllPlayers() => 
            this.m_playerIdentityIds.Keys;

        private string GetControlledEntity(MyPlayer player) => 
            ((player.Controller.ControlledEntity == null) ? "<empty>" : player.Controller.ControlledEntity.Entity.ToString());

        public MyPlayer GetControllingPlayer(MyEntity entity)
        {
            MyPlayer player;
            MyPlayer.PlayerId id;
            if (!this.m_controlledEntities.TryGetValue(entity.EntityId, out id) || !this.m_players.TryGetValue(id, out player))
            {
                return null;
            }
            return player;
        }

        public MyEntityController GetEntityController(MyEntity entity)
        {
            MyPlayer controllingPlayer = this.GetControllingPlayer(entity);
            return controllingPlayer?.Controller;
        }

        public HashSet<long> GetNPCIdentities() => 
            this.m_npcIdentities;

        public int GetOnlinePlayerCount() => 
            this.m_players.Values.Count;

        public ICollection<MyPlayer> GetOnlinePlayers() => 
            this.m_players.Values;

        public MyPlayer GetPlayerById(MyPlayer.PlayerId id)
        {
            MyPlayer player = null;
            this.m_players.TryGetValue(id, out player);
            return player;
        }

        public MyPlayer GetPlayerByName(string name)
        {
            using (IEnumerator<KeyValuePair<MyPlayer.PlayerId, MyPlayer>> enumerator = this.m_players.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyPlayer.PlayerId, MyPlayer> current = enumerator.Current;
                    if (current.Value.DisplayName.Equals(name))
                    {
                        return current.Value;
                    }
                }
            }
            return null;
        }

        private string GetPlayerCharacter(MyPlayer player) => 
            ((player.Identity.Character == null) ? "<empty>" : player.Identity.Character.Entity.ToString());

        public List<MyPlayer> GetPlayersStartingNameWith(string prefix)
        {
            List<MyPlayer> list = new List<MyPlayer>();
            foreach (KeyValuePair<MyPlayer.PlayerId, MyPlayer> pair in this.m_players)
            {
                string displayName = pair.Value.DisplayName;
                if (prefix.Length != 0)
                {
                    if (displayName.Length < prefix.Length)
                    {
                        continue;
                    }
                    if (!prefix.Equals(displayName.Substring(0, prefix.Length)))
                    {
                        continue;
                    }
                }
                list.Add(pair.Value);
            }
            return list;
        }

        public MyPlayer GetPreviousControllingPlayer(MyEntity entity)
        {
            MyPlayer player;
            MyPlayer.PlayerId id;
            if (!this.m_previousControlledEntities.TryGetValue(entity.EntityId, out id) || !this.m_players.TryGetValue(id, out player))
            {
                return null;
            }
            return player;
        }

        public bool HasExtendedControl(Sandbox.Game.Entities.IMyControllableEntity baseEntity, MyEntity secondEntity) => 
            ReferenceEquals(baseEntity.ControllerInfo.Controller, this.GetEntityController(secondEntity));

        public bool HasIdentity(long identityId) => 
            this.m_allIdentities.ContainsKey(identityId);

        private void Identity_CharacterChanged(MyCharacter oldCharacter, MyCharacter newCharacter)
        {
            if (oldCharacter != null)
            {
                oldCharacter.CharacterDied -= new Action<MyCharacter>(this.Character_CharacterDied);
            }
            if (newCharacter != null)
            {
                newCharacter.CharacterDied += new Action<MyCharacter>(this.Character_CharacterDied);
            }
        }

        public bool IdentityIsNpc(long identityId) => 
            this.m_npcIdentities.Contains(identityId);

        public MyPlayer InitNewPlayer(MyPlayer.PlayerId id, MyObjectBuilder_Player playerOb)
        {
            MyNetworkClient client;
            Sync.Clients.TryGetClient(id.SteamId, out client);
            return ((client != null) ? this.CreateNewPlayerInternal(client, id, playerOb) : null);
        }

        public bool IsPlayerOnline(ref MyPlayer.PlayerId playerId) => 
            this.m_players.ContainsKey(playerId);

        public bool IsPlayerOnline(long identityId)
        {
            MyPlayer.PlayerId id;
            return (MySession.Static.Players.TryGetPlayerId(identityId, out id) ? MySession.Static.Players.IsPlayerOnline(ref id) : false);
        }

        public void KillPlayer(MyPlayer player)
        {
            this.SetPlayerDead(player, true, MySession.Static.Settings.PermanentDeath.Value);
        }

        public unsafe void LoadConnectedPlayers(MyObjectBuilder_Checkpoint checkpoint, MyPlayer.PlayerId? savingPlayerId = new MyPlayer.PlayerId?())
        {
            MyPlayer.PlayerId id2;
            MyPlayer.PlayerId? nullable;
            if ((checkpoint.AllPlayers != null) && (checkpoint.AllPlayers.Count != 0))
            {
                foreach (MyObjectBuilder_Checkpoint.PlayerItem item in checkpoint.AllPlayers)
                {
                    long playerId = item.PlayerId;
                    MyObjectBuilder_Player player1 = new MyObjectBuilder_Player();
                    player1.Connected = true;
                    player1.DisplayName = item.Name;
                    player1.IdentityId = playerId;
                    MyObjectBuilder_Player playerOb = player1;
                    MyPlayer.PlayerId id = new MyPlayer.PlayerId(item.SteamId, 0);
                    if (savingPlayerId != null)
                    {
                        id2 = id;
                        nullable = savingPlayerId;
                        if ((nullable != null) ? (id2 == nullable.GetValueOrDefault()) : false)
                        {
                            id = new MyPlayer.PlayerId(Sync.MyId);
                            this.ChangeDisplayNameOfPlayerAndIdentity(playerOb, MyGameService.UserName);
                        }
                    }
                    this.LoadPlayerInternal(ref id, playerOb, true);
                }
            }
            else if ((checkpoint.ConnectedPlayers != null) && (checkpoint.ConnectedPlayers.Dictionary.Count != 0))
            {
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> pair in checkpoint.ConnectedPlayers.Dictionary)
                {
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(pair.Key.ClientId, pair.Key.SerialId);
                    if (savingPlayerId != null)
                    {
                        id2 = playerId;
                        nullable = savingPlayerId;
                        if ((nullable != null) ? (id2 == nullable.GetValueOrDefault()) : false)
                        {
                            playerId = new MyPlayer.PlayerId(Sync.MyId);
                            this.ChangeDisplayNameOfPlayerAndIdentity(pair.Value, MyGameService.UserName);
                        }
                    }
                    pair.Value.Connected = true;
                    this.LoadPlayerInternal(ref playerId, pair.Value, false);
                }
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, long> pair2 in checkpoint.DisconnectedPlayers.Dictionary)
                {
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(pair2.Key.ClientId, pair2.Key.SerialId);
                    MyObjectBuilder_Player player4 = new MyObjectBuilder_Player();
                    player4.Connected = false;
                    player4.IdentityId = pair2.Value;
                    player4.DisplayName = null;
                    MyObjectBuilder_Player playerOb = player4;
                    if (savingPlayerId != null)
                    {
                        id2 = playerId;
                        nullable = savingPlayerId;
                        if ((nullable != null) ? (id2 == nullable.GetValueOrDefault()) : false)
                        {
                            playerId = new MyPlayer.PlayerId(Sync.MyId);
                            this.ChangeDisplayNameOfPlayerAndIdentity(playerOb, MyGameService.UserName);
                        }
                    }
                    this.LoadPlayerInternal(ref playerId, playerOb, false);
                }
            }
            else if (checkpoint.AllPlayersData != null)
            {
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> pair3 in checkpoint.AllPlayersData.Dictionary)
                {
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(pair3.Key.ClientId, pair3.Key.SerialId);
                    if ((savingPlayerId != null) && (playerId.SteamId == savingPlayerId.Value.SteamId))
                    {
                        MyPlayer.PlayerId* idPtr1 = (MyPlayer.PlayerId*) ref playerId;
                        idPtr1 = (MyPlayer.PlayerId*) new MyPlayer.PlayerId(Sync.MyId, playerId.SerialId);
                        if (playerId.SerialId == 0)
                        {
                            this.ChangeDisplayNameOfPlayerAndIdentity(pair3.Value, MyGameService.UserName);
                        }
                    }
                    this.LoadPlayerInternal(ref playerId, pair3.Value, false);
                    MyPlayer player3 = null;
                    if (this.m_players.TryGetValue(playerId, out player3))
                    {
                        List<Vector3> list = null;
                        if ((checkpoint.AllPlayersColors != null) && checkpoint.AllPlayersColors.Dictionary.TryGetValue(pair3.Key, out list))
                        {
                            player3.SetBuildColorSlots(list);
                        }
                        else if (((checkpoint.CharacterToolbar != null) && (checkpoint.CharacterToolbar.ColorMaskHSVList != null)) && (checkpoint.CharacterToolbar.ColorMaskHSVList.Count > 0))
                        {
                            player3.SetBuildColorSlots(checkpoint.CharacterToolbar.ColorMaskHSVList);
                        }
                    }
                }
            }
            if ((MyCubeBuilder.AllPlayersColors != null) && (checkpoint.AllPlayersColors != null))
            {
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, List<Vector3>> pair4 in checkpoint.AllPlayersColors.Dictionary)
                {
                    MyPlayer.PlayerId key = new MyPlayer.PlayerId(pair4.Key.ClientId, pair4.Key.SerialId);
                    if (!MyCubeBuilder.AllPlayersColors.ContainsKey(key))
                    {
                        MyCubeBuilder.AllPlayersColors.Add(key, pair4.Value);
                    }
                }
            }
        }

        public unsafe void LoadControlledEntities(SerializableDictionary<long, MyObjectBuilder_Checkpoint.PlayerId> controlledEntities, long controlledObject, MyPlayer.PlayerId? savingPlayerId = new MyPlayer.PlayerId?())
        {
            if (controlledEntities != null)
            {
                foreach (KeyValuePair<long, MyObjectBuilder_Checkpoint.PlayerId> pair in controlledEntities.Dictionary)
                {
                    MyPlayer.PlayerId id = new MyPlayer.PlayerId(pair.Value.ClientId, pair.Value.SerialId);
                    if ((savingPlayerId != null) && (id.SteamId == savingPlayerId.Value.SteamId))
                    {
                        MyPlayer.PlayerId* idPtr1 = (MyPlayer.PlayerId*) ref id;
                        idPtr1 = (MyPlayer.PlayerId*) new MyPlayer.PlayerId(Sync.MyId, id.SerialId);
                    }
                    MyPlayer playerById = Sync.Players.GetPlayerById(id);
                    if (!Sync.IsServer)
                    {
                        m_controlledEntitiesClientCache[pair.Key] = id;
                    }
                    if (playerById != null)
                    {
                        this.TryTakeControl(playerById, pair.Key);
                    }
                }
            }
        }

        public void LoadDisconnectedPlayers(Dictionary<MyObjectBuilder_Checkpoint.PlayerId, long> dictionary)
        {
            foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, long> pair in dictionary)
            {
                MyPlayer.PlayerId key = new MyPlayer.PlayerId(pair.Key.ClientId, pair.Key.SerialId);
                this.m_playerIdentityIds.TryAdd(key, pair.Value);
                this.m_identityPlayerIds.Add(pair.Value, key);
            }
        }

        public void LoadIdentities(List<MyObjectBuilder_Identity> list)
        {
            if (list != null)
            {
                foreach (MyObjectBuilder_Identity identity in list)
                {
                    this.CreateNewIdentity(identity);
                }
            }
        }

        public void LoadIdentities(MyObjectBuilder_Checkpoint checkpoint, MyPlayer.PlayerId? savingPlayerId = new MyPlayer.PlayerId?())
        {
            if (checkpoint.NonPlayerIdentities != null)
            {
                this.LoadNpcIdentities(checkpoint.NonPlayerIdentities);
            }
            if (checkpoint.AllPlayers.Count != 0)
            {
                this.LoadIdentitiesObsolete(checkpoint.AllPlayers, savingPlayerId);
            }
            else
            {
                this.LoadIdentities(checkpoint.Identities);
            }
        }

        private void LoadIdentitiesObsolete(List<MyObjectBuilder_Checkpoint.PlayerItem> playersFromSession, MyPlayer.PlayerId? savingPlayerId = new MyPlayer.PlayerId?())
        {
            foreach (MyObjectBuilder_Checkpoint.PlayerItem item in playersFromSession)
            {
                Vector3? colorMask = null;
                MyIdentity identity = this.CreateNewIdentity(item.Name, item.PlayerId, item.Model, colorMask);
                MyPlayer.PlayerId key = new MyPlayer.PlayerId(item.SteamId);
                if ((savingPlayerId != null) && (key == savingPlayerId.Value))
                {
                    key = new MyPlayer.PlayerId(Sync.MyId);
                }
                if (!item.IsDead && !this.m_playerIdentityIds.ContainsKey(key))
                {
                    this.m_playerIdentityIds.TryAdd(key, identity.IdentityId);
                    this.m_identityPlayerIds.Add(identity.IdentityId, key);
                    identity.SetDead(false);
                }
            }
        }

        private void LoadNpcIdentities(List<long> list)
        {
            foreach (long num in list)
            {
                this.MarkIdentityAsNPC(num);
            }
        }

        private void LoadPlayerInternal(ref MyPlayer.PlayerId playerId, MyObjectBuilder_Player playerOb, bool obsolete = false)
        {
            MyIdentity identity = this.TryGetIdentity(playerOb.IdentityId);
            if ((identity != null) && (!obsolete || !identity.IsDead))
            {
                if (Sync.IsServer && (Sync.MyId != playerId.SteamId))
                {
                    playerOb.Connected = Sync.Clients.HasClient(playerId.SteamId);
                }
                if (!playerOb.Connected)
                {
                    if (!this.m_playerIdentityIds.ContainsKey(playerId))
                    {
                        this.m_playerIdentityIds.TryAdd(playerId, playerOb.IdentityId);
                        this.m_identityPlayerIds.Add(playerOb.IdentityId, playerId);
                    }
                    identity.SetDead(true);
                }
                else if (this.InitNewPlayer(playerId, playerOb).IsLocalPlayer)
                {
                    Action<int> localPlayerLoaded = Sync.Players.LocalPlayerLoaded;
                    if (localPlayerLoaded != null)
                    {
                        localPlayerLoaded(playerId.SerialId);
                    }
                }
            }
        }

        public void LoadPlayers(List<AllPlayerData> allPlayersData)
        {
            if (allPlayersData != null)
            {
                foreach (AllPlayerData data in allPlayersData)
                {
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(data.SteamId, data.SerialId);
                    this.LoadPlayerInternal(ref playerId, data.Player, false);
                }
            }
        }

        public void MarkIdentityAsNPC(long identityId)
        {
            this.m_npcIdentities.Add(identityId);
        }

        private void Multiplayer_ClientRemoved(ulong steamId)
        {
            if (Sync.IsServer)
            {
                this.m_tmpRemovedPlayers.Clear();
                foreach (KeyValuePair<MyPlayer.PlayerId, MyPlayer> pair in this.m_players)
                {
                    if (pair.Key.SteamId == steamId)
                    {
                        this.m_tmpRemovedPlayers.Add(pair.Value);
                    }
                }
                foreach (MyPlayer player in this.m_tmpRemovedPlayers)
                {
                    this.RemovePlayer(player, false);
                }
                this.m_tmpRemovedPlayers.Clear();
            }
        }

        [Event(null, 0x238), Reliable, Broadcast]
        private static void OnControlChangedSuccess(ulong clientSteamId, int playerSerialId, long entityId, bool justUpdateClientCache)
        {
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
            MyEntity entity = null;
            if (m_controlledEntitiesClientCache != null)
            {
                if (id.IsValid)
                {
                    m_controlledEntitiesClientCache[entityId] = id;
                }
                else if (m_controlledEntitiesClientCache.ContainsKey(entityId))
                {
                    m_controlledEntitiesClientCache.Remove(entityId);
                }
            }
            if (!justUpdateClientCache && MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                Sync.Players.SetControlledEntityInternal(id, entity, true);
            }
        }

        [Event(null, 0x269), Reliable, Server, Broadcast]
        private static void OnControlReleased(long entityId)
        {
            if ((!Sync.IsServer && (m_controlledEntitiesClientCache != null)) && m_controlledEntitiesClientCache.ContainsKey(entityId))
            {
                m_controlledEntitiesClientCache.Remove(entityId);
            }
            if (!MyEventContext.Current.IsLocallyInvoked)
            {
                MyEntity entity = null;
                if (MyEntities.TryGetEntityById(entityId, out entity, false))
                {
                    Sync.Players.RemoveControlledEntityInternal(entity, true);
                }
            }
        }

        [Event(null, 0x27b), Reliable, Broadcast]
        private static void OnIdentityCreated(bool isNpc, long identityId, string displayName)
        {
            if (isNpc)
            {
                Sync.Players.CreateNewNpcIdentity(displayName, identityId);
            }
            else
            {
                Vector3? colorMask = null;
                Sync.Players.CreateNewIdentity(displayName, identityId, null, colorMask);
            }
        }

        [Event(null, 0x3bb), Reliable, Broadcast]
        private static void OnIdentityFirstSpawn(long identidyId)
        {
            MyIdentity identity = Sync.Players.TryGetIdentity(identidyId);
            if (identity != null)
            {
                identity.PerformFirstSpawn();
            }
        }

        [Event(null, 0x288), Reliable, Server]
        private static void OnIdentityRemovedRequest(long identityId, ulong steamId, int serialId)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (steamId != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else if (Sync.Players.RemoveIdentityInternal(identityId, new MyPlayer.PlayerId(steamId, serialId)))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, ulong, int>(s => new Action<long, ulong, int>(MyPlayerCollection.OnIdentityRemovedSuccess), identityId, steamId, serialId, targetEndpoint, position);
            }
        }

        [Event(null, 0x299), Reliable, Broadcast]
        private static void OnIdentityRemovedSuccess(long identityId, ulong steamId, int serialId)
        {
            Sync.Players.RemoveIdentityInternal(identityId, new MyPlayer.PlayerId(steamId, serialId));
        }

        public void OnInitialPlayerCreated(ulong clientSteamId, int playerSerialId, long identityId, string displayName, List<Vector3> buildColors, bool realPlayer, bool newIdentity)
        {
            if (newIdentity)
            {
                OnIdentityCreated(false, identityId, displayName);
            }
            OnPlayerCreated(clientSteamId, playerSerialId, identityId, displayName, buildColors, realPlayer);
            if (clientSteamId == Sync.MyId)
            {
                MyMultiplayer.Static.StartProcessingClientMessages();
            }
        }

        [Event(null, 780), Reliable, Client]
        private static void OnNewPlayerFailure(ulong clientSteamId, int playerSerialId)
        {
            if (clientSteamId == Sync.MyId)
            {
                MyPlayer.PlayerId id = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
                if (Sync.Players.NewPlayerRequestFailed != null)
                {
                    Sync.Players.NewPlayerRequestFailed(id.SerialId);
                }
            }
        }

        [Event(null, 0x2d1), Reliable, Server]
        private static void OnNewPlayerRequest(ulong clientSteamId, int playerSerialId, string displayName, [Serialize(MyObjectFlags.DefaultZero)] string characterModel, bool realPlayer = false, bool initialPlayer = false)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (clientSteamId != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyPlayer.PlayerId key = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
                if (!Sync.Players.m_players.ContainsKey(key))
                {
                    Vector3D? nullable;
                    if (Sync.Players.PlayerRequesting != null)
                    {
                        PlayerRequestArgs args = new PlayerRequestArgs(key);
                        Sync.Players.PlayerRequesting(args);
                        if (args.Cancel)
                        {
                            if (MyEventContext.Current.IsLocallyInvoked)
                            {
                                OnNewPlayerFailure(clientSteamId, playerSerialId);
                                return;
                            }
                            nullable = null;
                            MyMultiplayer.RaiseStaticEvent<ulong, int>(s => new Action<ulong, int>(MyPlayerCollection.OnNewPlayerFailure), clientSteamId, playerSerialId, MyEventContext.Current.Sender, nullable);
                            return;
                        }
                    }
                    MyIdentity objA = Sync.Players.TryGetPlayerIdentity(key);
                    bool newIdentity = ReferenceEquals(objA, null);
                    if (newIdentity)
                    {
                        objA = Sync.Players.RespawnComponent.CreateNewIdentity(displayName, key, characterModel, initialPlayer);
                    }
                    Sync.Players.CreateNewPlayer(objA, key, objA.DisplayName, realPlayer, initialPlayer, newIdentity);
                    if (MyEventContext.Current.IsLocallyInvoked)
                    {
                        OnNewPlayerSuccess(clientSteamId, playerSerialId);
                    }
                    else
                    {
                        nullable = null;
                        MyMultiplayer.RaiseStaticEvent<ulong, int>(s => new Action<ulong, int>(MyPlayerCollection.OnNewPlayerSuccess), clientSteamId, playerSerialId, MyEventContext.Current.Sender, nullable);
                    }
                }
            }
        }

        [Event(null, 0x2fc), Reliable, Client]
        private static void OnNewPlayerSuccess(ulong clientSteamId, int playerSerialId)
        {
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(Sync.MyId, 0);
            MyPlayer.PlayerId id2 = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
            if ((id2 == id) && (!MySession.Static.IsScenario || (MySession.Static.OnlineMode == MyOnlineModeEnum.OFFLINE)))
            {
                RequestLocalRespawn();
            }
            Action<MyPlayer.PlayerId> newPlayerRequestSucceeded = Sync.Players.NewPlayerRequestSucceeded;
            if (newPlayerRequestSucceeded != null)
            {
                newPlayerRequestSucceeded(id2);
            }
        }

        [Event(null, 0x397), Reliable, Server]
        private static void OnNpcIdentityRequest()
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                string name = "NPC " + MyRandom.Instance.Next(0x3e8, 0x270f);
                MyIdentity identity = Sync.Players.CreateNewNpcIdentity(name, 0L);
                if (identity != null)
                {
                    long identityId = identity.IdentityId;
                    if (MyEventContext.Current.IsLocallyInvoked)
                    {
                        OnNpcIdentitySuccess(identityId);
                    }
                    else
                    {
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.OnNpcIdentitySuccess), identityId, MyEventContext.Current.Sender, position);
                    }
                }
            }
        }

        [Event(null, 0x3ae), Reliable, Client]
        private static void OnNpcIdentitySuccess(long identidyId)
        {
            MyIdentity identity = Sync.Players.TryGetIdentity(identidyId);
            if (identity != null)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.NPCIdentityAdded), identity.DisplayName), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        [Event(null, 0x361), Reliable, Server]
        private static void OnPlayerColorChangedRequest(int serialId, int colorIndex, Vector3 newColor)
        {
            ulong steamId = !MyEventContext.Current.IsLocallyInvoked ? MyEventContext.Current.Sender.Value : Sync.MyId;
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(steamId, serialId);
            MyPlayer playerById = Sync.Players.GetPlayerById(id);
            if (playerById != null)
            {
                playerById.SelectedBuildColorSlot = colorIndex;
                playerById.ChangeOrSwitchToColor(newColor);
            }
            else
            {
                List<Vector3> list;
                if (MyCubeBuilder.AllPlayersColors.TryGetValue(id, out list))
                {
                    list[colorIndex] = newColor;
                }
            }
        }

        [Event(null, 890), Reliable, Server]
        private static void OnPlayerColorsChangedRequest(int serialId, [Serialize(MyObjectFlags.DefaultZero)] List<Vector3> newColors)
        {
            ulong steamId = !MyEventContext.Current.IsLocallyInvoked ? MyEventContext.Current.Sender.Value : Sync.MyId;
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(steamId, serialId);
            MyPlayer playerById = Sync.Players.GetPlayerById(id);
            if (playerById != null)
            {
                playerById.SetBuildColorSlots(newColors);
            }
            else
            {
                List<Vector3> list;
                if (MyCubeBuilder.AllPlayersColors.TryGetValue(id, out list))
                {
                    list.Clear();
                    foreach (Vector3 vector in newColors)
                    {
                        list.Add(vector);
                    }
                }
            }
        }

        [Event(null, 0x328), Reliable, Broadcast]
        private static void OnPlayerCreated(ulong clientSteamId, int playerSerialId, long identityId, string displayName, [Serialize(MyObjectFlags.DefaultZero)] List<Vector3> buildColors, bool realPlayer)
        {
            if (Sync.Players.TryGetIdentity(identityId) != null)
            {
                MyNetworkClient client = null;
                Sync.Clients.TryGetClient(clientSteamId, out client);
                if (client != null)
                {
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
                    MyObjectBuilder_Player playerBuilder = new MyObjectBuilder_Player();
                    playerBuilder.DisplayName = displayName;
                    playerBuilder.IdentityId = identityId;
                    playerBuilder.BuildColorSlots = buildColors;
                    playerBuilder.ForceRealPlayer = realPlayer;
                    Sync.Players.CreateNewPlayerInternal(client, playerId, playerBuilder);
                }
            }
        }

        [Event(null, 0x29f), Reliable, Broadcast]
        private static void OnPlayerIdentityChanged(ulong clientSteamId, int playerSerialId, long identityId)
        {
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
            MyPlayer playerById = Sync.Players.GetPlayerById(id);
            if (playerById != null)
            {
                MyIdentity identity = null;
                Sync.Players.m_allIdentities.TryGetValue(identityId, out identity);
                if (identity != null)
                {
                    playerById.Identity = identity;
                }
            }
        }

        [Event(null, 0x355), Reliable, Broadcast]
        private static void OnPlayerRemoved(ulong clientSteamId, int playerSerialId)
        {
            MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(clientSteamId, playerSerialId);
            if (clientSteamId == Sync.MyId)
            {
                Sync.Players.RaiseLocalPlayerRemoved(playerSerialId);
            }
            Sync.Players.RemovePlayerFromDictionary(playerId);
        }

        [Event(null, 0x341), Reliable, Server]
        private static void OnPlayerRemoveRequest(ulong clientSteamId, int playerSerialId, bool removeCharacter)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (clientSteamId != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(clientSteamId, playerSerialId));
                if (playerById != null)
                {
                    Sync.Players.RemovePlayer(playerById, removeCharacter);
                }
            }
        }

        private void OnPlayersChanged(bool added, MyPlayer.PlayerId playerId)
        {
            Action<bool, MyPlayer.PlayerId> playersChanged = this.PlayersChanged;
            if (playersChanged != null)
            {
                playersChanged(added, playerId);
            }
        }

        [Event(null, 0x742), Reliable, Server]
        private static void OnRespawnRequest(RespawnMsg msg)
        {
            Vector3D? spawnPosition = null;
            SerializableDefinitionId? botDefinitionId = null;
            OnRespawnRequest(msg.JoinGame, msg.NewIdentity, msg.RespawnEntityId, msg.RespawnShipId, spawnPosition, botDefinitionId, true, msg.PlayerSerialId, msg.ModelName, msg.Color);
        }

        public static void OnRespawnRequest(bool joinGame, bool newIdentity, long respawnEntityId, string respawnShipId, Vector3D? spawnPosition, SerializableDefinitionId? botDefinitionId, bool realPlayer, int playerSerialId, string modelName, Color color)
        {
            if (Sync.IsServer)
            {
                EndpointId sender;
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    sender = new EndpointId(Sync.MyId);
                }
                else
                {
                    sender = MyEventContext.Current.Sender;
                }
                if (Sync.Players.RespawnComponent != null)
                {
                    Vector3D? nullable;
                    MyPlayer.PlayerId playerId = new MyPlayer.PlayerId(sender.Value, playerSerialId);
                    if (!Sync.Players.RespawnComponent.HandleRespawnRequest(joinGame, newIdentity, respawnEntityId, respawnShipId, playerId, spawnPosition, botDefinitionId, realPlayer, modelName, color))
                    {
                        nullable = null;
                        MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(MyPlayerCollection.OnRespawnRequestFailure), playerSerialId, sender, nullable);
                    }
                    else
                    {
                        EndpointId id3;
                        MyIdentity identity = Sync.Players.TryGetPlayerIdentity(playerId);
                        if (identity != null)
                        {
                            if (!identity.FirstSpawnDone)
                            {
                                id3 = new EndpointId();
                                nullable = null;
                                MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.OnIdentityFirstSpawn), identity.IdentityId, id3, nullable);
                                identity.PerformFirstSpawn();
                            }
                            identity.LogRespawnTime();
                        }
                        MyPlayer playerById = Sync.Players.GetPlayerById(playerId);
                        if ((playerById != null) && (playerById.Controller != null))
                        {
                            MyEntity controlledEntity = playerById.Controller.ControlledEntity as MyEntity;
                            if (controlledEntity != null)
                            {
                                id3 = new EndpointId();
                                nullable = null;
                                MyMultiplayer.RaiseStaticEvent<ulong, int, long, bool>(s => new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess), playerId.SteamId, playerId.SerialId, controlledEntity.EntityId, true, id3, nullable);
                            }
                        }
                    }
                }
            }
        }

        [Event(null, 0x2b0), Reliable, Client]
        private static void OnRespawnRequestFailure(int playerSerialId)
        {
            if (playerSerialId == 0)
            {
                OnRespawnRequestFailureEvent.InvokeIfNotNull<ulong>(Sync.MyId);
                RequestLocalRespawn();
            }
        }

        [Event(null, 0x2ba), Reliable, Server]
        private static void OnSetPlayerDeadRequest(ulong clientSteamId, int playerSerialId, bool isDead, bool resetIdentity)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (clientSteamId != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else if (Sync.Players.SetPlayerDeadInternal(clientSteamId, playerSerialId, isDead, resetIdentity))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int, bool, bool>(s => new Action<ulong, int, bool, bool>(MyPlayerCollection.OnSetPlayerDeadSuccess), clientSteamId, playerSerialId, isDead, resetIdentity, targetEndpoint, position);
            }
        }

        [Event(null, 0x2cb), Reliable, Broadcast]
        private static void OnSetPlayerDeadSuccess(ulong clientSteamId, int playerSerialId, bool isDead, bool resetIdentity)
        {
            Sync.Players.SetPlayerDeadInternal(clientSteamId, playerSerialId, isDead, resetIdentity);
        }

        private void player_IdentityChanged(MyPlayer player, MyIdentity identity)
        {
            long key = this.m_playerIdentityIds[player.Id];
            this.m_identityPlayerIds.Remove(key);
            this.m_playerIdentityIds[player.Id] = identity.IdentityId;
            this.m_identityPlayerIds[identity.IdentityId] = player.Id;
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int, long>(s => new Action<ulong, int, long>(MyPlayerCollection.OnPlayerIdentityChanged), player.Id.SteamId, player.Id.SerialId, identity.IdentityId, targetEndpoint, position);
            }
        }

        private void RaiseLocalPlayerRemoved(int serialId)
        {
            Action<int> localPlayerRemoved = this.LocalPlayerRemoved;
            if (localPlayerRemoved != null)
            {
                localPlayerRemoved(serialId);
            }
        }

        public void ReduceAllControl(Sandbox.Game.Entities.IMyControllableEntity baseEntity)
        {
            MyPlayer.PlayerId id;
            if (this.m_controlledEntities.TryGetValue(baseEntity.Entity.EntityId, out id))
            {
                foreach (KeyValuePair<long, MyPlayer.PlayerId> pair in this.m_controlledEntities)
                {
                    if (pair.Value != id)
                    {
                        continue;
                    }
                    if (pair.Key != baseEntity.Entity.EntityId)
                    {
                        MyEntity entity = null;
                        MyEntities.TryGetEntityById(pair.Key, out entity, true);
                        if (entity != null)
                        {
                            this.RemoveControlledEntityProxy(entity, false);
                        }
                    }
                }
                this.m_controlledEntities.ApplyRemovals();
            }
        }

        public void ReduceControl(Sandbox.Game.Entities.IMyControllableEntity baseEntity, MyEntity entityWhichLoosesControl)
        {
            if (!this.TryReduceControl(baseEntity, entityWhichLoosesControl))
            {
                MyRemoteControl control1 = baseEntity as MyRemoteControl;
            }
        }

        public void RegisterEvents()
        {
            MyClientCollection clients = Sync.Clients;
            clients.ClientRemoved = (Action<ulong>) Delegate.Combine(clients.ClientRemoved, new Action<ulong>(this.Multiplayer_ClientRemoved));
        }

        public void RemoveControlledEntity(MyEntity entity)
        {
            this.RemoveControlledEntityProxy(entity, true);
        }

        private void RemoveControlledEntityInternal(MyEntity entity, bool immediate = true)
        {
            MyPlayer.PlayerId id;
            entity.OnClosing -= this.m_entityClosingHandler;
            if (this.m_controlledEntities.TryGetValue(entity.EntityId, out id))
            {
                this.m_previousControlledEntities[entity.EntityId] = id;
            }
            this.m_controlledEntities.Remove(entity.EntityId, immediate);
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int, long, bool>(s => new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess), 0L, 0, entity.EntityId, true, targetEndpoint, position);
            }
        }

        private void RemoveControlledEntityProxy(MyEntity entity, bool immediateOnServer)
        {
            if (Sync.IsServer)
            {
                this.RemoveControlledEntityInternal(entity, immediateOnServer);
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.OnControlReleased), entity.EntityId, targetEndpoint, position);
        }

        public void RemoveIdentity(long identityId, MyPlayer.PlayerId playerId = new MyPlayer.PlayerId())
        {
            EndpointId id;
            Vector3D? nullable;
            if (!Sync.IsServer)
            {
                id = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<long, ulong, int>(s => new Action<long, ulong, int>(MyPlayerCollection.OnIdentityRemovedRequest), identityId, playerId.SteamId, playerId.SerialId, id, nullable);
            }
            else if (this.RemoveIdentityInternal(identityId, playerId))
            {
                id = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<long, ulong, int>(s => new Action<long, ulong, int>(MyPlayerCollection.OnIdentityRemovedSuccess), identityId, playerId.SteamId, playerId.SerialId, id, nullable);
            }
        }

        private bool RemoveIdentityInternal(long identityId, MyPlayer.PlayerId playerId)
        {
            MyIdentity identity;
            if (playerId.IsValid && this.m_players.ContainsKey(playerId))
            {
                return false;
            }
            if (this.m_allIdentities.TryGetValue(identityId, out identity))
            {
                identity.ChangeCharacter(null);
                identity.CharacterChanged -= new Action<MyCharacter, MyCharacter>(this.Identity_CharacterChanged);
            }
            this.m_allIdentities.Remove<long, MyIdentity>(identityId);
            this.m_npcIdentities.Remove(identityId);
            if (playerId.IsValid)
            {
                long num;
                if (this.m_playerIdentityIds.TryGetValue(playerId, out num))
                {
                    this.m_identityPlayerIds.Remove(num);
                }
                this.m_playerIdentityIds.Remove<MyPlayer.PlayerId, long>(playerId);
            }
            Action identitiesChanged = this.IdentitiesChanged;
            if (identitiesChanged != null)
            {
                identitiesChanged();
            }
            return true;
        }

        public void RemovePlayer(MyPlayer player, bool removeCharacter = true)
        {
            EndpointId id;
            Vector3D? nullable;
            if (!Sync.IsServer)
            {
                if (!player.IsRemotePlayer)
                {
                    id = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<ulong, int, bool>(s => new Action<ulong, int, bool>(MyPlayerCollection.OnPlayerRemoveRequest), player.Id.SteamId, player.Id.SerialId, removeCharacter, id, nullable);
                }
            }
            else
            {
                if ((removeCharacter && (player.Character != null)) && !(player.Character.Parent is MyCryoChamber))
                {
                    player.Character.Close();
                }
                this.KillPlayer(player);
                if (player.IsLocalPlayer)
                {
                    this.RaiseLocalPlayerRemoved(player.Id.SerialId);
                }
                if (this.PlayerRemoved != null)
                {
                    this.PlayerRemoved(player.Id);
                }
                this.RespawnComponent.AfterRemovePlayer(player);
                id = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int>(s => new Action<ulong, int>(MyPlayerCollection.OnPlayerRemoved), player.Id.SteamId, player.Id.SerialId, id, nullable);
                this.RemovePlayerFromDictionary(player.Id);
            }
        }

        private void RemovePlayerFromDictionary(MyPlayer.PlayerId playerId)
        {
            if (this.m_players.ContainsKey(playerId))
            {
                if (Sync.IsServer && (MyVisualScriptLogicProvider.PlayerDisconnected != null))
                {
                    MyVisualScriptLogicProvider.PlayerDisconnected(this.m_players[playerId].Identity.IdentityId);
                }
                this.m_players[playerId].Identity.LastLogoutTime = DateTime.Now;
            }
            this.m_players.Remove<MyPlayer.PlayerId, MyPlayer>(playerId);
            this.OnPlayersChanged(false, playerId);
        }

        public static void RequestLocalRespawn()
        {
            MySandboxGame.Log.WriteLine("RequestRespawn");
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (Sync.Players != null))
            {
                string model = null;
                Color red = Color.Red;
                MyLocalCache.GetCharacterInfoFromInventoryConfig(ref model, ref red);
                Sync.Players.LocalRespawnRequested.InvokeIfNotNull<string, Color>(model, red);
                if (MyMultiplayer.Static != null)
                {
                    MyMultiplayer.Static.InvokeLocalRespawnRequested();
                }
            }
        }

        public void RequestNewNpcIdentity()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MyPlayerCollection.OnNpcIdentityRequest), targetEndpoint, position);
        }

        public void RequestNewPlayer(int serialNumber, string playerName, string characterModel, bool realPlayer, bool initialPlayer)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ulong, int, string, string, bool, bool>(s => new Action<ulong, int, string, string, bool, bool>(MyPlayerCollection.OnNewPlayerRequest), Sync.MyId, serialNumber, playerName, characterModel, realPlayer, initialPlayer, targetEndpoint, position);
        }

        public void RequestPlayerColorChanged(int playerSerialId, int colorIndex, Vector3 newColor)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int, int, Vector3>(s => new Action<int, int, Vector3>(MyPlayerCollection.OnPlayerColorChangedRequest), playerSerialId, colorIndex, newColor, targetEndpoint, position);
        }

        public void RequestPlayerColorsChanged(int playerSerialId, List<Vector3> newColors)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<int, List<Vector3>>(s => new Action<int, List<Vector3>>(MyPlayerCollection.OnPlayerColorsChangedRequest), playerSerialId, newColors, targetEndpoint, position);
        }

        public static void RespawnRequest(bool joinGame, bool newIdentity, long respawnEntityId, string shipPrefabId, int playerSerialId, string modelName, Color color)
        {
            RespawnMsg msg = new RespawnMsg {
                JoinGame = joinGame,
                RespawnEntityId = respawnEntityId,
                NewIdentity = newIdentity,
                RespawnShipId = shipPrefabId,
                PlayerSerialId = playerSerialId,
                ModelName = modelName,
                Color = color
            };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<RespawnMsg>(s => new Action<RespawnMsg>(MyPlayerCollection.OnRespawnRequest), msg, targetEndpoint, position);
        }

        public void RevivePlayer(MyPlayer player)
        {
            this.SetPlayerDead(player, false, false);
        }

        public List<MyObjectBuilder_Identity> SaveIdentities()
        {
            List<MyObjectBuilder_Identity> list = new List<MyObjectBuilder_Identity>();
            foreach (KeyValuePair<long, MyIdentity> pair in this.m_allIdentities)
            {
                MyPlayer.PlayerId id;
                if (((MySession.Static != null) && MySession.Static.Players.TryGetPlayerId(pair.Key, out id)) && (MySession.Static.Players.GetPlayerById(id) != null))
                {
                    pair.Value.LastLogoutTime = DateTime.Now;
                }
                list.Add(pair.Value.GetObjectBuilder());
            }
            return list;
        }

        public List<long> SaveNpcIdentities()
        {
            List<long> list = new List<long>();
            foreach (long num in this.m_npcIdentities)
            {
                list.Add(num);
            }
            return list;
        }

        public List<AllPlayerData> SavePlayers()
        {
            List<AllPlayerData> list = new List<AllPlayerData>();
            foreach (MyPlayer player in this.m_players.Values)
            {
                AllPlayerData item = new AllPlayerData {
                    SteamId = player.Id.SteamId,
                    SerialId = player.Id.SerialId,
                    Player = player.GetObjectBuilder()
                };
                list.Add(item);
            }
            return list;
        }

        public void SavePlayers(MyObjectBuilder_Checkpoint checkpoint)
        {
            MyObjectBuilder_Checkpoint.PlayerId id2;
            checkpoint.ConnectedPlayers = new SerializableDictionary<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player>();
            checkpoint.DisconnectedPlayers = new SerializableDictionary<MyObjectBuilder_Checkpoint.PlayerId, long>();
            checkpoint.AllPlayersData = new SerializableDictionary<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player>();
            checkpoint.AllPlayersColors = new SerializableDictionary<MyObjectBuilder_Checkpoint.PlayerId, List<Vector3>>();
            foreach (MyPlayer player in this.m_players.Values)
            {
                id2 = new MyObjectBuilder_Checkpoint.PlayerId {
                    ClientId = player.Id.SteamId,
                    SerialId = player.Id.SerialId
                };
                MyObjectBuilder_Checkpoint.PlayerId key = id2;
                MyObjectBuilder_Player objectBuilder = player.GetObjectBuilder();
                checkpoint.AllPlayersData.Dictionary.Add(key, objectBuilder);
            }
            foreach (KeyValuePair<MyPlayer.PlayerId, long> pair in this.m_playerIdentityIds)
            {
                if (!this.m_players.ContainsKey(pair.Key))
                {
                    id2 = new MyObjectBuilder_Checkpoint.PlayerId {
                        ClientId = pair.Key.SteamId,
                        SerialId = pair.Key.SerialId
                    };
                    MyObjectBuilder_Checkpoint.PlayerId key = id2;
                    MyIdentity identity = this.TryGetIdentity(pair.Value);
                    MyObjectBuilder_Player player1 = new MyObjectBuilder_Player();
                    MyObjectBuilder_Player player4 = new MyObjectBuilder_Player();
                    player4.DisplayName = identity?.DisplayName;
                    MyObjectBuilder_Player local1 = player4;
                    local1.IdentityId = pair.Value;
                    local1.Connected = false;
                    MyObjectBuilder_Player player3 = local1;
                    if (MyCubeBuilder.AllPlayersColors != null)
                    {
                        MyCubeBuilder.AllPlayersColors.TryGetValue(pair.Key, out player3.BuildColorSlots);
                    }
                    checkpoint.AllPlayersData.Dictionary.Add(key, player3);
                }
            }
            if (MyCubeBuilder.AllPlayersColors != null)
            {
                foreach (KeyValuePair<MyPlayer.PlayerId, List<Vector3>> pair2 in MyCubeBuilder.AllPlayersColors)
                {
                    if (this.m_players.ContainsKey(pair2.Key))
                    {
                        continue;
                    }
                    if (!this.m_playerIdentityIds.ContainsKey(pair2.Key))
                    {
                        id2 = new MyObjectBuilder_Checkpoint.PlayerId {
                            ClientId = pair2.Key.SteamId,
                            SerialId = pair2.Key.SerialId
                        };
                        MyObjectBuilder_Checkpoint.PlayerId key = id2;
                        checkpoint.AllPlayersColors.Dictionary.Add(key, pair2.Value);
                    }
                }
            }
        }

        public void SendDirtyBlockLimit(MyBlockLimits blockLimit, List<EndpointId> playersToSendTo)
        {
            // Invalid method body.
        }

        public void SendDirtyBlockLimits()
        {
            switch (MySession.Static.BlockLimitsEnabled)
            {
                case MyBlockLimitsEnabledEnum.GLOBALLY:
                    foreach (MyPlayer player in this.GetOnlinePlayers())
                    {
                        if (player.Identity == null)
                        {
                            continue;
                        }
                        if (player.IsRealPlayer)
                        {
                            this.m_tmpPlayersLinkedToBlockLimit.Add(new EndpointId(player.Id.SteamId));
                        }
                    }
                    this.SendDirtyBlockLimit(MySession.Static.GlobalBlockLimits, this.m_tmpPlayersLinkedToBlockLimit);
                    this.m_tmpPlayersLinkedToBlockLimit.Clear();
                    return;

                case MyBlockLimitsEnabledEnum.PER_FACTION:
                    foreach (KeyValuePair<long, MyFaction> pair in MySession.Static.Factions)
                    {
                        foreach (MyPlayer player2 in this.GetOnlinePlayers())
                        {
                            if (pair.Value.IsMember(player2.Identity.IdentityId))
                            {
                                this.m_tmpPlayersLinkedToBlockLimit.Add(new EndpointId(player2.Id.SteamId));
                            }
                        }
                        if (this.m_tmpPlayersLinkedToBlockLimit.Count > 0)
                        {
                            this.SendDirtyBlockLimit(pair.Value.BlockLimits, this.m_tmpPlayersLinkedToBlockLimit);
                        }
                        this.m_tmpPlayersLinkedToBlockLimit.Clear();
                    }
                    foreach (MyPlayer player3 in this.GetOnlinePlayers())
                    {
                        if (MySession.Static.Factions.GetPlayerFaction(player3.Identity.IdentityId) == null)
                        {
                            this.m_tmpPlayersLinkedToBlockLimit.Add(new EndpointId(player3.Id.SteamId));
                            this.SendDirtyBlockLimit(player3.Identity.BlockLimits, this.m_tmpPlayersLinkedToBlockLimit);
                        }
                    }
                    return;

                case MyBlockLimitsEnabledEnum.PER_PLAYER:
                    break;

                default:
                    return;
            }
            foreach (MyPlayer player4 in this.GetOnlinePlayers())
            {
                if (player4.Identity == null)
                {
                    continue;
                }
                if (player4.IsRealPlayer)
                {
                    this.m_tmpPlayersLinkedToBlockLimit.Add(new EndpointId(player4.Id.SteamId));
                    this.SendDirtyBlockLimit(player4.Identity.BlockLimits, this.m_tmpPlayersLinkedToBlockLimit);
                    this.m_tmpPlayersLinkedToBlockLimit.Clear();
                }
            }
        }

        public SerializableDictionary<long, MyObjectBuilder_Checkpoint.PlayerId> SerializeControlledEntities()
        {
            SerializableDictionary<long, MyObjectBuilder_Checkpoint.PlayerId> dictionary = new SerializableDictionary<long, MyObjectBuilder_Checkpoint.PlayerId>();
            foreach (KeyValuePair<long, MyPlayer.PlayerId> pair in this.m_controlledEntities)
            {
                MyObjectBuilder_Checkpoint.PlayerId id = new MyObjectBuilder_Checkpoint.PlayerId {
                    ClientId = pair.Value.SteamId,
                    SerialId = pair.Value.SerialId
                };
                dictionary.Dictionary.Add(pair.Key, id);
            }
            return dictionary;
        }

        public void SetControlledEntity(MyPlayer.PlayerId id, MyEntity entity)
        {
            if (Sync.IsServer)
            {
                this.SetControlledEntityInternal(id, entity, true);
            }
        }

        public void SetControlledEntity(ulong steamUserId, MyEntity entity)
        {
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(steamUserId);
            this.SetControlledEntity(id, entity);
        }

        private void SetControlledEntityInternal(MyPlayer.PlayerId id, MyEntity entity, bool sync = true)
        {
            MyPlayer.PlayerId id2;
            if (Sync.IsServer || (m_controlledEntitiesClientCache.TryGetValue(entity.EntityId, out id2) && (id2 == id)))
            {
                Vector3D? nullable;
                MyPlayer playerById = Sync.Players.GetPlayerById(id);
                this.RemoveControlledEntityInternal(entity, true);
                entity.OnClosing += this.m_entityClosingHandler;
                if (((playerById == null) || (playerById.Controller == null)) || !(entity is Sandbox.Game.Entities.IMyControllableEntity))
                {
                    if (playerById != null)
                    {
                        this.m_controlledEntities.Add(entity.EntityId, playerById.Id, true);
                    }
                }
                else
                {
                    if (((entity is MyCharacter) && (playerById.Identity != null)) && !ReferenceEquals(entity, playerById.Identity.Character))
                    {
                        playerById.Identity.ChangeCharacter(entity as MyCharacter);
                    }
                    playerById.Controller.TakeControl((Sandbox.Game.Entities.IMyControllableEntity) entity);
                }
                if ((Sync.IsServer & sync) && (playerById != null))
                {
                    ulong steamId = playerById.Id.SteamId;
                    EndpointId targetEndpoint = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<ulong, int, long, bool>(s => new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess), steamId, playerById.Id.SerialId, entity.EntityId, false, targetEndpoint, nullable);
                }
                if ((MySession.Static.LocalHumanPlayer != null) && (id == MySession.Static.LocalHumanPlayer.Id))
                {
                    IMyCameraController controller = entity as IMyCameraController;
                    if (controller != null)
                    {
                        nullable = null;
                        MySession.Static.SetCameraController(controller.IsInFirstPersonView ? MyCameraControllerEnum.Entity : MyCameraControllerEnum.ThirdPersonSpectator, entity, nullable);
                    }
                }
            }
        }

        public void SetControlledEntityLocally(MyPlayer.PlayerId id, MyEntity entity)
        {
            this.SetControlledEntityInternal(id, entity, false);
        }

        [Event(null, 0x5ea), Reliable, Server]
        public static void SetDampeningEntity(long controlledEntityId)
        {
            Sandbox.Game.Entities.IMyControllableEntity entity = MyEntities.GetEntityByIdOrDefault(controlledEntityId, null, false) as Sandbox.Game.Entities.IMyControllableEntity;
            if ((entity != null) && (entity.Entity != null))
            {
                MatrixD xd = entity.GetHeadMatrix(true, true, false, false);
                Vector3D translation = xd.Translation;
                LineD line = new LineD(translation, translation + (xd.Forward * 1000.0));
                MyIntersectionResultLineTriangleEx? nullable = MyEntities.GetIntersectionWithLine(ref line, (MyEntity) entity, entity.Entity.GetTopMostParent(null), false, false, true, IntersectionFlags.ALL_TRIANGLES, 0f, true);
                if ((nullable == null) || (nullable.Value.Entity == null))
                {
                    entity.RelativeDampeningEntity = null;
                }
                else
                {
                    entity.RelativeDampeningEntity = (MyEntity) nullable.Value.Entity.GetTopMostParent(null);
                }
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient), entity.Entity.EntityId, (entity.RelativeDampeningEntity != null) ? entity.RelativeDampeningEntity.EntityId : 0L, targetEndpoint, position);
            }
        }

        [Event(null, 0x61b), Reliable, BroadcastExcept]
        public static void SetDampeningEntityClient(long controlledEntityId, long dampeningEntityId)
        {
            Sandbox.Game.Entities.IMyControllableEntity entity = MyEntities.GetEntityByIdOrDefault(controlledEntityId, null, false) as Sandbox.Game.Entities.IMyControllableEntity;
            if (entity != null)
            {
                MyEntity entity2 = MyEntities.GetEntityByIdOrDefault(dampeningEntityId, null, false);
                if (entity2 != null)
                {
                    entity.RelativeDampeningEntity = entity2;
                }
                else
                {
                    entity.RelativeDampeningEntity = null;
                }
            }
        }

        [Event(null, 0x3c5), Reliable, Client]
        private static void SetIdentityBlockTypesBuilt(MyBlockLimits.MyTypeLimitData limits)
        {
            MyIdentity identity = MySession.Static.LocalHumanPlayer.Identity;
            if (identity != null)
            {
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    identity.BlockLimits.CallLimitsChanged();
                }
                else
                {
                    identity.BlockLimits.SetTypeLimitsFromServer(limits);
                }
            }
        }

        [Event(null, 980), Reliable, Client]
        private static void SetIdentityGridBlocksBuilt(MyBlockLimits.MyGridLimitData limits, int pcu, int pcuBuilt, int blocksBuilt)
        {
            MyIdentity identity = MySession.Static.LocalHumanPlayer.Identity;
            if (identity != null)
            {
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    identity.BlockLimits.CallLimitsChanged();
                }
                else
                {
                    identity.BlockLimits.SetGridLimitsFromServer(limits, pcu, pcuBuilt, blocksBuilt);
                }
            }
        }

        public void SetPlayerCharacter(MyPlayer player, MyCharacter newCharacter, MyEntity spawnedBy)
        {
            newCharacter.SetPlayer(player, true);
            if ((MyVisualScriptLogicProvider.PlayerSpawned != null) && !newCharacter.IsBot)
            {
                MyVisualScriptLogicProvider.PlayerSpawned(newCharacter.ControllerInfo.Controller.Player.Identity.IdentityId);
            }
            if (spawnedBy != null)
            {
                long entityId = spawnedBy.EntityId;
                Vector3 translation = (Vector3) (newCharacter.WorldMatrix * MatrixD.Invert(spawnedBy.WorldMatrix)).Translation;
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter, long, Vector3>(newCharacter, x => new Action<long, Vector3>(x.SpawnCharacterRelative), entityId, translation, targetEndpoint);
            }
        }

        private void SetPlayerDead(MyPlayer player, bool deadState, bool resetIdentity)
        {
            EndpointId id;
            Vector3D? nullable;
            if (!Sync.IsServer)
            {
                id = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int, bool, bool>(s => new Action<ulong, int, bool, bool>(MyPlayerCollection.OnSetPlayerDeadRequest), player.Id.SteamId, player.Id.SerialId, deadState, resetIdentity, id, nullable);
            }
            else if (this.SetPlayerDeadInternal(player.Id.SteamId, player.Id.SerialId, deadState, resetIdentity))
            {
                id = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<ulong, int, bool, bool>(s => new Action<ulong, int, bool, bool>(MyPlayerCollection.OnSetPlayerDeadSuccess), player.Id.SteamId, player.Id.SerialId, deadState, resetIdentity, id, nullable);
            }
        }

        private bool SetPlayerDeadInternal(ulong playerSteamId, int playerSerialId, bool deadState, bool resetIdentity)
        {
            MyPlayer.PlayerId id = new MyPlayer.PlayerId(playerSteamId, playerSerialId);
            MyPlayer playerById = Sync.Players.GetPlayerById(id);
            if (playerById == null)
            {
                return false;
            }
            if (playerById.Identity == null)
            {
                return false;
            }
            playerById.Identity.SetDead(resetIdentity);
            if (deadState)
            {
                playerById.Controller.TakeControl(null);
                foreach (KeyValuePair<long, MyPlayer.PlayerId> pair in this.m_controlledEntities)
                {
                    if (pair.Value != playerById.Id)
                    {
                        continue;
                    }
                    MyEntity entity = null;
                    MyEntities.TryGetEntityById(pair.Key, out entity, false);
                    if (entity != null)
                    {
                        this.RemoveControlledEntityInternal(entity, false);
                    }
                }
                this.m_controlledEntities.ApplyRemovals();
                if (ReferenceEquals(playerById, Sync.Clients.LocalClient.FirstPlayer))
                {
                    RequestLocalRespawn();
                }
            }
            return true;
        }

        public void SetPlayerToCockpit(MyPlayer player, MyEntity controlledEntity)
        {
            Sync.Players.SetControlledEntityInternal(player.Id, controlledEntity, true);
            if ((ReferenceEquals(player, MySession.Static.LocalHumanPlayer) && (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Entity)) && !MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning)
            {
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.LocalCharacter, position);
            }
        }

        public void SetRespawnComponent(MyRespawnComponentBase respawnComponent)
        {
            this.RespawnComponent = respawnComponent;
        }

        public void TryExtendControl(Sandbox.Game.Entities.IMyControllableEntity baseEntity, MyEntity entityGettingControl)
        {
            MyEntityController controller = baseEntity.ControllerInfo.Controller;
            if (controller != null)
            {
                this.TrySetControlledEntity(controller.Player.Id, entityGettingControl);
            }
        }

        public MyIdentity TryGetIdentity(long identityId)
        {
            MyIdentity identity;
            this.m_allIdentities.TryGetValue(identityId, out identity);
            return identity;
        }

        public long TryGetIdentityId(ulong steamId, int serialId = 0)
        {
            long num = 0L;
            MyPlayer.PlayerId key = new MyPlayer.PlayerId(steamId, serialId);
            this.m_playerIdentityIds.TryGetValue(key, out num);
            return num;
        }

        public string TryGetIdentityNameFromSteamId(ulong steamId)
        {
            MyIdentity identity = this.TryGetIdentity(this.TryGetIdentityId(steamId, 0));
            return ((identity == null) ? string.Empty : identity.DisplayName);
        }

        public bool TryGetPlayerById(MyPlayer.PlayerId id, out MyPlayer player) => 
            this.m_players.TryGetValue(id, out player);

        public bool TryGetPlayerId(long identityId, out MyPlayer.PlayerId result) => 
            this.m_identityPlayerIds.TryGetValue(identityId, out result);

        public MyIdentity TryGetPlayerIdentity(MyPlayer.PlayerId playerId)
        {
            MyIdentity identity = null;
            long identityId = this.TryGetIdentityId(playerId.SteamId, playerId.SerialId);
            if (identityId != 0)
            {
                identity = this.TryGetIdentity(identityId);
            }
            return identity;
        }

        public int TryGetSerialId(long identityId)
        {
            MyPlayer.PlayerId id;
            return (this.TryGetPlayerId(identityId, out id) ? id.SerialId : 0);
        }

        public ulong TryGetSteamId(long identityId)
        {
            MyPlayer.PlayerId id;
            return (this.TryGetPlayerId(identityId, out id) ? id.SteamId : 0UL);
        }

        public bool TryReduceControl(Sandbox.Game.Entities.IMyControllableEntity baseEntity, MyEntity entityWhichLoosesControl)
        {
            MyPlayer.PlayerId id;
            MyEntityController controller = baseEntity.ControllerInfo.Controller;
            if (((controller == null) || !this.m_controlledEntities.TryGetValue(entityWhichLoosesControl.EntityId, out id)) || (controller.Player.Id != id))
            {
                return false;
            }
            this.RemoveControlledEntity(entityWhichLoosesControl);
            return true;
        }

        public bool TrySetControlledEntity(MyPlayer.PlayerId id, MyEntity entity)
        {
            MyPlayer controllingPlayer = this.GetControllingPlayer(entity);
            if (controllingPlayer != null)
            {
                return (controllingPlayer.Id == id);
            }
            this.SetControlledEntity(id, entity);
            return true;
        }

        private void TryTakeControl(MyPlayer player, long controlledEntityId)
        {
            MyEntity entity;
            MyEntities.TryGetEntityById(controlledEntityId, out entity, false);
            if (entity == null)
            {
                if (player.CachedControllerId == null)
                {
                    player.CachedControllerId = new List<long>();
                }
                player.CachedControllerId.Add(controlledEntityId);
            }
            else
            {
                if (Sandbox.Engine.Platform.Game.IsDedicated || !(entity is Sandbox.Game.Entities.IMyControllableEntity))
                {
                    this.m_controlledEntities.Add(controlledEntityId, player.Id, true);
                    if (Sync.IsServer)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<ulong, int, long, bool>(s => new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess), player.Id.SteamId, player.Id.SerialId, controlledEntityId, true, targetEndpoint, position);
                    }
                }
                else
                {
                    player.Controller.TakeControl(entity as Sandbox.Game.Entities.IMyControllableEntity);
                    MyCharacter pilot = entity as MyCharacter;
                    if (pilot == null)
                    {
                        if (entity is MyShipController)
                        {
                            pilot = (entity as MyShipController).Pilot;
                        }
                        else if (entity is MyLargeTurretBase)
                        {
                            pilot = (entity as MyLargeTurretBase).Pilot;
                        }
                    }
                    if (pilot != null)
                    {
                        player.Identity.ChangeCharacter(pilot);
                        pilot.SetPlayer(player, false);
                    }
                }
                if (player.CachedControllerId != null)
                {
                    player.CachedControllerId.Remove(controlledEntityId);
                    if (player.CachedControllerId.Count == 0)
                    {
                        player.CachedControllerId = null;
                    }
                }
            }
        }

        public void UnmarkIdentityAsNPC(long identityId)
        {
            this.m_npcIdentities.Remove(identityId);
        }

        public void UnregisterEvents()
        {
            if (Sync.Clients != null)
            {
                MyClientCollection clients = Sync.Clients;
                clients.ClientRemoved = (Action<ulong>) Delegate.Remove(clients.ClientRemoved, new Action<ulong>(this.Multiplayer_ClientRemoved));
            }
        }

        public static void UpdateControl(MyEntity entity)
        {
            MyPlayer.PlayerId id;
            MyPlayer.PlayerId id2;
            if (((m_controlledEntitiesClientCache != null) && m_controlledEntitiesClientCache.TryGetValue(entity.EntityId, out id)) && (!Sync.Players.m_controlledEntities.TryGetValue(entity.EntityId, out id2) || (id2 != id)))
            {
                Sync.Players.SetControlledEntityInternal(id, entity, true);
            }
        }

        public void UpdatePlayerControllers(long controllerId)
        {
            foreach (KeyValuePair<MyPlayer.PlayerId, MyPlayer> pair in this.m_players)
            {
                if (pair.Value.CachedControllerId == null)
                {
                    continue;
                }
                if (pair.Value.CachedControllerId.Contains(controllerId))
                {
                    this.TryTakeControl(pair.Value, controllerId);
                }
            }
        }

        void IMyPlayerCollection.ExtendControl(VRage.Game.ModAPI.Interfaces.IMyControllableEntity entityWithControl, IMyEntity entityGettingControl)
        {
            Sandbox.Game.Entities.IMyControllableEntity baseEntity = entityWithControl as Sandbox.Game.Entities.IMyControllableEntity;
            MyEntity entity2 = entityGettingControl as MyEntity;
            if ((baseEntity != null) && (entity2 != null))
            {
                this.ExtendControl(baseEntity, entity2);
            }
        }

        void IMyPlayerCollection.GetAllIdentites(List<IMyIdentity> identities, Func<IMyIdentity, bool> collect)
        {
            foreach (KeyValuePair<long, MyIdentity> pair in this.m_allIdentities)
            {
                if ((collect == null) || collect(pair.Value))
                {
                    identities.Add(pair.Value);
                }
            }
        }

        IMyPlayer IMyPlayerCollection.GetPlayerControllingEntity(IMyEntity entity)
        {
            MyEntity entity2 = entity as MyEntity;
            if (entity2 != null)
            {
                MyEntityController entityController = this.GetEntityController(entity2);
                if (entityController != null)
                {
                    return entityController.Player;
                }
            }
            return null;
        }

        void IMyPlayerCollection.GetPlayers(List<IMyPlayer> players, Func<IMyPlayer, bool> collect)
        {
            foreach (KeyValuePair<MyPlayer.PlayerId, MyPlayer> pair in this.m_players)
            {
                if ((collect == null) || collect(pair.Value))
                {
                    players.Add(pair.Value);
                }
            }
        }

        bool IMyPlayerCollection.HasExtendedControl(VRage.Game.ModAPI.Interfaces.IMyControllableEntity firstEntity, IMyEntity secondEntity)
        {
            Sandbox.Game.Entities.IMyControllableEntity baseEntity = firstEntity as Sandbox.Game.Entities.IMyControllableEntity;
            MyEntity entity2 = secondEntity as MyEntity;
            return ((baseEntity != null) && ((entity2 != null) && this.HasExtendedControl(baseEntity, entity2)));
        }

        void IMyPlayerCollection.ReduceControl(VRage.Game.ModAPI.Interfaces.IMyControllableEntity entityWhichKeepsControl, IMyEntity entityWhichLoosesControl)
        {
            Sandbox.Game.Entities.IMyControllableEntity baseEntity = entityWhichKeepsControl as Sandbox.Game.Entities.IMyControllableEntity;
            MyEntity entity2 = entityWhichLoosesControl as MyEntity;
            if ((baseEntity != null) && (entity2 != null))
            {
                this.ReduceControl(baseEntity, entity2);
            }
        }

        void IMyPlayerCollection.RemoveControlledEntity(IMyEntity entity)
        {
            MyEntity entity2 = entity as MyEntity;
            if (entity2 != null)
            {
                this.RemoveControlledEntity(entity2);
            }
        }

        void IMyPlayerCollection.SetControlledEntity(ulong steamUserId, IMyEntity entity)
        {
            MyEntity entity2 = entity as MyEntity;
            if (entity2 != null)
            {
                this.SetControlledEntity(steamUserId, entity2);
            }
        }

        void IMyPlayerCollection.TryExtendControl(VRage.Game.ModAPI.Interfaces.IMyControllableEntity entityWithControl, IMyEntity entityGettingControl)
        {
            Sandbox.Game.Entities.IMyControllableEntity baseEntity = entityWithControl as Sandbox.Game.Entities.IMyControllableEntity;
            MyEntity entity2 = entityGettingControl as MyEntity;
            if ((baseEntity != null) && (entity2 != null))
            {
                this.TryExtendControl(baseEntity, entity2);
            }
        }

        long IMyPlayerCollection.TryGetIdentityId(ulong steamId) => 
            this.TryGetIdentityId(steamId, 0);

        ulong IMyPlayerCollection.TryGetSteamId(long identityId) => 
            this.TryGetSteamId(identityId);

        bool IMyPlayerCollection.TryReduceControl(VRage.Game.ModAPI.Interfaces.IMyControllableEntity entityWhichKeepsControl, IMyEntity entityWhichLoosesControl)
        {
            Sandbox.Game.Entities.IMyControllableEntity baseEntity = entityWhichKeepsControl as Sandbox.Game.Entities.IMyControllableEntity;
            MyEntity entity2 = entityWhichLoosesControl as MyEntity;
            return ((baseEntity != null) && ((entity2 != null) && this.TryReduceControl(baseEntity, entity2)));
        }

        [Conditional("DEBUG")]
        public void WriteDebugInfo()
        {
            StackFrame frame = new StackTrace().GetFrame(1);
            using (IEnumerator<KeyValuePair<MyPlayer.PlayerId, MyPlayer>> enumerator = this.m_players.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<MyPlayer.PlayerId, MyPlayer> player;
                    bool isLocalPlayer = player.Value.IsLocalPlayer;
                    frame.GetMethod().Name + (isLocalPlayer ? "; Control: [L] " : "; Control: ") + player.Value.Id.ToString();
                    (from s in this.m_controlledEntities
                        where s.Value == player.Value.Id
                        select s.Key.ToString("X")).ToArray<string>();
                }
            }
            foreach (MyEntity local2 in MyEntities.GetEntities())
            {
            }
        }

        public MyRespawnComponentBase RespawnComponent { get; set; }

        public DictionaryReader<long, MyPlayer.PlayerId> ControlledEntities =>
            this.m_controlledEntities.Reader;

        long IMyPlayerCollection.Count =>
            ((long) this.m_players.Count);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPlayerCollection.<>c <>9 = new MyPlayerCollection.<>c();
            public static Func<IMyEventOwner, Action<ulong, int, long, bool>> <>9__59_0;
            public static Func<IMyEventOwner, Action<long, ulong, int>> <>9__77_0;
            public static Func<IMyEventOwner, Action<ulong, int, bool, bool>> <>9__81_0;
            public static Func<IMyEventOwner, Action<ulong, int>> <>9__83_1;
            public static Func<IMyEventOwner, Action<ulong, int>> <>9__83_0;
            public static Func<IMyEventOwner, Action<long>> <>9__92_0;
            public static Func<IMyEventOwner, Action<int, int, Vector3>> <>9__97_0;
            public static Func<IMyEventOwner, Action<int, List<Vector3>>> <>9__98_0;
            public static Func<IMyEventOwner, Action<ulong, int, string, string, bool, bool>> <>9__99_0;
            public static Func<IMyEventOwner, Action> <>9__100_0;
            public static Func<IMyEventOwner, Action<ulong, int, long, string, List<Vector3>, bool>> <>9__102_0;
            public static Func<IMyEventOwner, Action<ulong, int>> <>9__104_0;
            public static Func<IMyEventOwner, Action<ulong, int, bool>> <>9__104_1;
            public static Func<IMyEventOwner, Action<long>> <>9__114_0;
            public static Func<MyCharacter, Action<long, Vector3>> <>9__116_0;
            public static Func<KeyValuePair<long, MyIdentity>, string> <>9__125_0;
            public static Func<IMyEventOwner, Action<MyBlockLimits.MyTypeLimitData>> <>9__130_0;
            public static Func<IMyEventOwner, Action<MyBlockLimits.MyGridLimitData, int, int, int>> <>9__130_1;
            public static Func<IMyEventOwner, Action<long, string>> <>9__130_2;
            public static Func<IMyEventOwner, Action<MyBlockLimits.MyGridLimitData, int, int, int>> <>9__130_3;
            public static Func<IMyEventOwner, Action<long, long>> <>9__137_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__138_0;
            public static Func<IMyEventOwner, Action<long, ulong, int>> <>9__144_0;
            public static Func<IMyEventOwner, Action<long, ulong, int>> <>9__144_1;
            public static Func<IMyEventOwner, Action<MyPlayerCollection.RespawnMsg>> <>9__160_0;
            public static Func<IMyEventOwner, Action<ulong, int, bool, bool>> <>9__163_0;
            public static Func<IMyEventOwner, Action<ulong, int, bool, bool>> <>9__163_1;
            public static Func<IMyEventOwner, Action<long>> <>9__166_1;
            public static Func<IMyEventOwner, Action<ulong, int, long, bool>> <>9__166_2;
            public static Func<IMyEventOwner, Action<int>> <>9__166_0;
            public static Func<IMyEventOwner, Action<ulong, int, long, bool>> <>9__169_0;
            public static Func<IMyEventOwner, Action<ulong, int, long, bool>> <>9__170_1;
            public static Func<IMyEventOwner, Action<ulong, int, long, bool>> <>9__170_0;
            public static Func<IMyEventOwner, Action<ulong, int, long, bool>> <>9__171_0;
            public static Func<IMyEventOwner, Action<long>> <>9__172_0;
            public static Func<IMyEventOwner, Action<bool, long, string>> <>9__177_0;
            public static Func<IMyEventOwner, Action<ulong, int, long>> <>9__183_0;
            public static Func<KeyValuePair<long, MyPlayer.PlayerId>, string> <>9__186_1;

            internal Action<bool, long, string> <AfterCreateIdentity>b__177_0(IMyEventOwner s) => 
                new Action<bool, long, string>(MyPlayerCollection.OnIdentityCreated);

            internal Action<long, long> <ClearDampeningEntity>b__138_0(IMyEventOwner s) => 
                new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient);

            internal Action<ulong, int, long, bool> <controller_ControlledEntityChanged>b__170_0(IMyEventOwner s) => 
                new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess);

            internal Action<ulong, int, long, bool> <controller_ControlledEntityChanged>b__170_1(IMyEventOwner s) => 
                new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess);

            internal Action<ulong, int, long, string, List<Vector3>, bool> <CreateNewPlayer>b__102_0(IMyEventOwner s) => 
                new Action<ulong, int, long, string, List<Vector3>, bool>(MyPlayerCollection.OnPlayerCreated);

            internal Action<long> <EntityClosing>b__172_0(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.OnControlReleased);

            internal string <GetAllIdentitiesOrderByName>b__125_0(KeyValuePair<long, MyIdentity> pair) => 
                pair.Value.DisplayName;

            internal Action<long, ulong, int> <OnIdentityRemovedRequest>b__77_0(IMyEventOwner s) => 
                new Action<long, ulong, int>(MyPlayerCollection.OnIdentityRemovedSuccess);

            internal Action<ulong, int> <OnNewPlayerRequest>b__83_0(IMyEventOwner s) => 
                new Action<ulong, int>(MyPlayerCollection.OnNewPlayerSuccess);

            internal Action<ulong, int> <OnNewPlayerRequest>b__83_1(IMyEventOwner s) => 
                new Action<ulong, int>(MyPlayerCollection.OnNewPlayerFailure);

            internal Action<long> <OnNpcIdentityRequest>b__92_0(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.OnNpcIdentitySuccess);

            internal Action<int> <OnRespawnRequest>b__166_0(IMyEventOwner s) => 
                new Action<int>(MyPlayerCollection.OnRespawnRequestFailure);

            internal Action<long> <OnRespawnRequest>b__166_1(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.OnIdentityFirstSpawn);

            internal Action<ulong, int, long, bool> <OnRespawnRequest>b__166_2(IMyEventOwner s) => 
                new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess);

            internal Action<ulong, int, bool, bool> <OnSetPlayerDeadRequest>b__81_0(IMyEventOwner s) => 
                new Action<ulong, int, bool, bool>(MyPlayerCollection.OnSetPlayerDeadSuccess);

            internal Action<ulong, int, long> <player_IdentityChanged>b__183_0(IMyEventOwner s) => 
                new Action<ulong, int, long>(MyPlayerCollection.OnPlayerIdentityChanged);

            internal Action<ulong, int, long, bool> <RemoveControlledEntityInternal>b__171_0(IMyEventOwner s) => 
                new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess);

            internal Action<long> <RemoveControlledEntityProxy>b__114_0(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.OnControlReleased);

            internal Action<long, ulong, int> <RemoveIdentity>b__144_0(IMyEventOwner s) => 
                new Action<long, ulong, int>(MyPlayerCollection.OnIdentityRemovedSuccess);

            internal Action<long, ulong, int> <RemoveIdentity>b__144_1(IMyEventOwner s) => 
                new Action<long, ulong, int>(MyPlayerCollection.OnIdentityRemovedRequest);

            internal Action<ulong, int> <RemovePlayer>b__104_0(IMyEventOwner s) => 
                new Action<ulong, int>(MyPlayerCollection.OnPlayerRemoved);

            internal Action<ulong, int, bool> <RemovePlayer>b__104_1(IMyEventOwner s) => 
                new Action<ulong, int, bool>(MyPlayerCollection.OnPlayerRemoveRequest);

            internal Action <RequestNewNpcIdentity>b__100_0(IMyEventOwner s) => 
                new Action(MyPlayerCollection.OnNpcIdentityRequest);

            internal Action<ulong, int, string, string, bool, bool> <RequestNewPlayer>b__99_0(IMyEventOwner s) => 
                new Action<ulong, int, string, string, bool, bool>(MyPlayerCollection.OnNewPlayerRequest);

            internal Action<int, int, Vector3> <RequestPlayerColorChanged>b__97_0(IMyEventOwner s) => 
                new Action<int, int, Vector3>(MyPlayerCollection.OnPlayerColorChangedRequest);

            internal Action<int, List<Vector3>> <RequestPlayerColorsChanged>b__98_0(IMyEventOwner s) => 
                new Action<int, List<Vector3>>(MyPlayerCollection.OnPlayerColorsChangedRequest);

            internal Action<MyPlayerCollection.RespawnMsg> <RespawnRequest>b__160_0(IMyEventOwner s) => 
                new Action<MyPlayerCollection.RespawnMsg>(MyPlayerCollection.OnRespawnRequest);

            internal Action<MyBlockLimits.MyTypeLimitData> <SendDirtyBlockLimit>b__130_0(IMyEventOwner x) => 
                new Action<MyBlockLimits.MyTypeLimitData>(MyPlayerCollection.SetIdentityBlockTypesBuilt);

            internal Action<MyBlockLimits.MyGridLimitData, int, int, int> <SendDirtyBlockLimit>b__130_1(IMyEventOwner x) => 
                new Action<MyBlockLimits.MyGridLimitData, int, int, int>(MyPlayerCollection.SetIdentityGridBlocksBuilt);

            internal Action<long, string> <SendDirtyBlockLimit>b__130_2(IMyEventOwner x) => 
                new Action<long, string>(MyBlockLimits.SetGridNameFromServer);

            internal Action<MyBlockLimits.MyGridLimitData, int, int, int> <SendDirtyBlockLimit>b__130_3(IMyEventOwner x) => 
                new Action<MyBlockLimits.MyGridLimitData, int, int, int>(MyPlayerCollection.SetIdentityGridBlocksBuilt);

            internal Action<ulong, int, long, bool> <SetControlledEntityInternal>b__169_0(IMyEventOwner s) => 
                new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess);

            internal Action<long, long> <SetDampeningEntity>b__137_0(IMyEventOwner s) => 
                new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient);

            internal Action<long, Vector3> <SetPlayerCharacter>b__116_0(MyCharacter x) => 
                new Action<long, Vector3>(x.SpawnCharacterRelative);

            internal Action<ulong, int, bool, bool> <SetPlayerDead>b__163_0(IMyEventOwner s) => 
                new Action<ulong, int, bool, bool>(MyPlayerCollection.OnSetPlayerDeadSuccess);

            internal Action<ulong, int, bool, bool> <SetPlayerDead>b__163_1(IMyEventOwner s) => 
                new Action<ulong, int, bool, bool>(MyPlayerCollection.OnSetPlayerDeadRequest);

            internal Action<ulong, int, long, bool> <TryTakeControl>b__59_0(IMyEventOwner s) => 
                new Action<ulong, int, long, bool>(MyPlayerCollection.OnControlChangedSuccess);

            internal string <WriteDebugInfo>b__186_1(KeyValuePair<long, MyPlayer.PlayerId> s) => 
                s.Key.ToString("X");
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct AllPlayerData
        {
            [ProtoMember(0x4a)]
            public ulong SteamId;
            [ProtoMember(0x4c)]
            public int SerialId;
            [ProtoMember(0x4e)]
            public MyObjectBuilder_Player Player;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RespawnMsg
        {
            public bool JoinGame;
            public bool NewIdentity;
            public long RespawnEntityId;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string RespawnShipId;
            public int PlayerSerialId;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string ModelName;
            public VRageMath.Color Color;
        }

        public delegate void RespawnRequestedDelegate(ref MyPlayerCollection.RespawnMsg respawnMsg, MyNetworkClient client);
    }
}

