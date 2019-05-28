namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.GameServices;
    using VRage.Network;
    using VRage.Replication;
    using VRageMath;

    [StaticEventOwner]
    public static class MyMultiplayer
    {
        public const int CONTROL_CHANNEL = 0;
        public const int GAME_EVENT_CHANNEL = 2;
        public const string HOST_NAME_TAG = "host";
        public const string WORLD_NAME_TAG = "world";
        public const string HOST_STEAM_ID_TAG = "host_steamId";
        public const string WORLD_SIZE_TAG = "worldSize";
        public const string APP_VERSION_TAG = "appVersion";
        public const string GAME_MODE_TAG = "gameMode";
        public const string DATA_HASH_TAG = "dataHash";
        public const string MOD_COUNT_TAG = "mods";
        public const string MOD_ITEM_TAG = "mod";
        public const string VIEW_DISTANCE_TAG = "view";
        public const string INVENTORY_MULTIPLIER_TAG = "inventoryMultiplier";
        public const string BLOCKS_INVENTORY_MULTIPLIER_TAG = "blocksInventoryMultiplier";
        public const string ASSEMBLER_MULTIPLIER_TAG = "assemblerMultiplier";
        public const string REFINERY_MULTIPLIER_TAG = "refineryMultiplier";
        public const string WELDER_MULTIPLIER_TAG = "welderMultiplier";
        public const string GRINDER_MULTIPLIER_TAG = "grinderMultiplier";
        public const string SCENARIO_TAG = "scenario";
        public const string SCENARIO_BRIEFING_TAG = "scenarioBriefing";
        public const string SCENARIO_START_TIME_TAG = "scenarioStartTime";
        public const string EXPERIMENTAL_MODE_TAG = "experimentalMode";
        public const string SESSION_CONFIG_TAG = "sc";
        private static MyReplicationSingle m_replicationOffline;

        public static string GetMultiplayerStats() => 
            ((Static == null) ? string.Empty : Static.ReplicationLayer.GetMultiplayerStat());

        internal static MyReplicationClient GetReplicationClient() => 
            ((Static == null) ? null : (Static.ReplicationLayer as MyReplicationClient));

        internal static MyReplicationServer GetReplicationServer() => 
            ((Static == null) ? null : (Static.ReplicationLayer as MyReplicationServer));

        public static MyMultiplayerHostResult HostLobby(MyLobbyType lobbyType, int maxPlayers, MySyncLayer syncLayer)
        {
            MyMultiplayerHostResult ret = new MyMultiplayerHostResult();
            MyGameService.CreateLobby(lobbyType, (uint) maxPlayers, delegate (IMyLobby lobby, bool succes, string msg) {
                if (!ret.Cancelled)
                {
                    if (succes && (lobby.OwnerId != Sync.MyId))
                    {
                        succes = false;
                        lobby.Leave();
                    }
                    lobby.LobbyType = lobbyType;
                    MyMultiplayerBase multiplayer = null;
                    if (succes)
                    {
                        MyMultiplayerLobby lobby1 = new MyMultiplayerLobby(lobby, syncLayer);
                        Static = lobby1;
                        multiplayer = lobby1;
                        multiplayer.ExperimentalMode = true;
                    }
                    ret.RaiseDone(succes, msg, multiplayer);
                }
            });
            return ret;
        }

        public static void InitOfflineReplicationLayer()
        {
            if (m_replicationOffline == null)
            {
                m_replicationOffline = new MyReplicationSingle(new EndpointId(Sync.MyId));
                m_replicationOffline.RegisterFromGameAssemblies();
            }
        }

        public static MyMultiplayerJoinResult JoinLobby(ulong lobbyId)
        {
            MyMultiplayerJoinResult ret = new MyMultiplayerJoinResult();
            MyGameService.JoinLobby(lobbyId, delegate (bool success, IMyLobby lobby, MyLobbyEnterResponseEnum response) {
                if (!ret.Cancelled)
                {
                    MyMultiplayerLobbyClient client2;
                    if ((success && (response == MyLobbyEnterResponseEnum.Success)) && (lobby.OwnerId == Sync.MyId))
                    {
                        response = MyLobbyEnterResponseEnum.DoesntExist;
                        lobby.Leave();
                    }
                    success &= response == MyLobbyEnterResponseEnum.Success;
                    if (!success)
                    {
                        client2 = null;
                    }
                    else
                    {
                        MyMultiplayerLobbyClient client1 = new MyMultiplayerLobbyClient(lobby, new MySyncLayer(new MyTransportLayer(2)));
                        Static = client1;
                        client2 = client1;
                    }
                    ret.RaiseJoined(success, lobby, response, client2);
                }
            });
            return ret;
        }

        [Event(null, 0x19b), Reliable, Server]
        private static void OnTeleport(ulong userId, Vector3D location)
        {
            if ((Sync.IsValidEventOnServer && !MyEventContext.Current.IsLocallyInvoked) && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                MyEventContext.ValidationFailed();
            }
            else
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(userId));
                if (playerById.Controller.ControlledEntity != null)
                {
                    playerById.Controller.ControlledEntity.Entity.GetTopMostParent(null).PositionComp.SetPosition(location, null, false, true);
                }
            }
        }

        public static void RaiseBlockingEvent<T1, T2, T3, T4, T5, T6>(T1 arg1, T6 arg6, Func<T1, Action<T2, T3, T4, T5>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner where T6: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, T4, T5, T6>(arg1, arg6, action, arg2, arg3, arg4, arg5, targetEndpoint, position);
        }

        public static void RaiseBlockingEvent<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T7 arg7, Func<T1, Action<T2, T3, T4, T5, T6>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner where T7: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, T4, T5, T6, T7>(arg1, arg7, action, arg2, arg3, arg4, arg5, arg6, targetEndpoint, position);
        }

        public static void RaiseEvent<T1>(T1 arg1, Func<T1, Action> action, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, IMyEventOwner>(arg1, null, action, targetEndpoint, position);
        }

        public static void RaiseEvent<T1, T2>(T1 arg1, Func<T1, Action<T2>> action, T2 arg2, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, IMyEventOwner>(arg1, null, action, arg2, targetEndpoint, position);
        }

        public static void RaiseEvent<T1, T2, T3>(T1 arg1, Func<T1, Action<T2, T3>> action, T2 arg2, T3 arg3, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, IMyEventOwner>(arg1, null, action, arg2, arg3, targetEndpoint, position);
        }

        public static void RaiseEvent<T1, T2, T3, T4>(T1 arg1, Func<T1, Action<T2, T3, T4>> action, T2 arg2, T3 arg3, T4 arg4, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, T4, IMyEventOwner>(arg1, null, action, arg2, arg3, arg4, targetEndpoint, position);
        }

        public static void RaiseEvent<T1, T2, T3, T4, T5>(T1 arg1, Func<T1, Action<T2, T3, T4, T5>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, T4, T5, IMyEventOwner>(arg1, null, action, arg2, arg3, arg4, arg5, targetEndpoint, position);
        }

        public static void RaiseEvent<T1, T2, T3, T4, T5, T6>(T1 arg1, Func<T1, Action<T2, T3, T4, T5, T6>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, T4, T5, T6, IMyEventOwner>(arg1, null, action, arg2, arg3, arg4, arg5, arg6, targetEndpoint, position);
        }

        public static void RaiseEvent<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, Func<T1, Action<T2, T3, T4, T5, T6, T7>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, EndpointId targetEndpoint = new EndpointId()) where T1: IMyEventOwner
        {
            Vector3D? position = null;
            ReplicationLayer.RaiseEvent<T1, T2, T3, T4, T5, T6, T7, IMyEventOwner>(arg1, null, action, arg2, arg3, arg4, arg5, arg6, arg7, targetEndpoint, position);
        }

        public static void RaiseStaticEvent(Func<IMyEventOwner, Action> action, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, IMyEventOwner>(null, null, action, targetEndpoint, position);
        }

        public static void RaiseStaticEvent<T2>(Func<IMyEventOwner, Action<T2>> action, T2 arg2, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, T2, IMyEventOwner>(null, null, action, arg2, targetEndpoint, position);
        }

        public static void RaiseStaticEvent<T2, T3>(Func<IMyEventOwner, Action<T2, T3>> action, T2 arg2, T3 arg3, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, T2, T3, IMyEventOwner>(null, null, action, arg2, arg3, targetEndpoint, position);
        }

        public static void RaiseStaticEvent<T2, T3, T4>(Func<IMyEventOwner, Action<T2, T3, T4>> action, T2 arg2, T3 arg3, T4 arg4, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, T2, T3, T4, IMyEventOwner>(null, null, action, arg2, arg3, arg4, targetEndpoint, position);
        }

        public static void RaiseStaticEvent<T2, T3, T4, T5>(Func<IMyEventOwner, Action<T2, T3, T4, T5>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, T2, T3, T4, T5, IMyEventOwner>(null, null, action, arg2, arg3, arg4, arg5, targetEndpoint, position);
        }

        public static void RaiseStaticEvent<T2, T3, T4, T5, T6>(Func<IMyEventOwner, Action<T2, T3, T4, T5, T6>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, T2, T3, T4, T5, T6, IMyEventOwner>(null, null, action, arg2, arg3, arg4, arg5, arg6, targetEndpoint, position);
        }

        public static void RaiseStaticEvent<T2, T3, T4, T5, T6, T7>(Func<IMyEventOwner, Action<T2, T3, T4, T5, T6, T7>> action, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, EndpointId targetEndpoint = new EndpointId(), Vector3D? position = new Vector3D?())
        {
            ReplicationLayer.RaiseEvent<IMyEventOwner, T2, T3, T4, T5, T6, T7, IMyEventOwner>(null, null, action, arg2, arg3, arg4, arg5, arg6, arg7, targetEndpoint, position);
        }

        public static void RemoveForClientIfIncomplete(IMyEventProxy obj)
        {
            MyReplicationServer replicationServer = GetReplicationServer();
            if (replicationServer != null)
            {
                replicationServer.RemoveForClientIfIncomplete(obj);
            }
        }

        public static void ReplicateImmediatelly(IMyReplicable replicable, IMyReplicable dependency = null)
        {
            MyReplicationServer replicationServer = GetReplicationServer();
            if (replicationServer != null)
            {
                replicationServer.ForceReplicable(replicable, dependency);
            }
        }

        public static void TeleportControlledEntity(Vector3D location)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            RaiseStaticEvent<ulong, Vector3D>(x => new Action<ulong, Vector3D>(MyMultiplayer.OnTeleport), MySession.Static.LocalHumanPlayer.Id.SteamId, location, targetEndpoint, position);
        }

        public static MyMultiplayerBase Static
        {
            get => 
                ((MyMultiplayerBase) MyMultiplayerMinimalBase.Instance);
            set => 
                (MyMultiplayerMinimalBase.Instance = value);
        }

        public static MyReplicationLayerBase ReplicationLayer
        {
            get
            {
                if (Static != null)
                {
                    return Static.ReplicationLayer;
                }
                InitOfflineReplicationLayer();
                return m_replicationOffline;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMultiplayer.<>c <>9 = new MyMultiplayer.<>c();
            public static Func<IMyEventOwner, Action<ulong, Vector3D>> <>9__52_0;

            internal Action<ulong, Vector3D> <TeleportControlledEntity>b__52_0(IMyEventOwner x) => 
                new Action<ulong, Vector3D>(MyMultiplayer.OnTeleport);
        }
    }
}

