namespace Sandbox.ModAPI
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Physics;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Scripting;

    public static class MyModAPIHelper
    {
        public static void Initialize()
        {
            Game.EnableSimSpeedLocking = true;
            MyAPIGateway.Session = MySession.Static;
            MyAPIGateway.Entities = new MyEntitiesHelper_ModAPI();
            MyAPIGateway.Players = Sync.Players;
            MyAPIGateway.CubeBuilder = MyCubeBuilder.Static;
            MyAPIGateway.IngameScripting = MyIngameScripting.Static;
            MyAPIGateway.TerminalActionsHelper = MyTerminalControlFactoryHelper.Static;
            MyAPIGateway.Utilities = MyAPIUtilities.Static;
            MyAPIGateway.Parallel = MyParallelTask.Static;
            MyAPIGateway.Physics = MyPhysics.Static;
            MyAPIGateway.Multiplayer = MyMultiplayer.Static;
            MyAPIGateway.PrefabManager = MyPrefabManager.Static;
            MyAPIGateway.Input = (VRage.ModAPI.IMyInput) MyInput.Static;
            MyAPIGateway.TerminalControls = MyTerminalControls.Static;
            MyAPIGateway.Gui = new MyGuiModHelpers();
            MyAPIGateway.GridGroups = new MyGridGroupsHelper();
        }

        [StaticEventOwner]
        public class MyMultiplayer : IMyMultiplayer
        {
            public static MyModAPIHelper.MyMultiplayer Static = new MyModAPIHelper.MyMultiplayer();
            private const int UNRELIABLE_MAX_SIZE = 0x400;
            private static Dictionary<ushort, List<Action<byte[]>>> m_registeredListeners = new Dictionary<ushort, List<Action<byte[]>>>();

            private static void HandleMessage(ushort id, byte[] message)
            {
                List<Action<byte[]>> list = null;
                if (m_registeredListeners.TryGetValue(id, out list) && (list != null))
                {
                    using (List<Action<byte[]>>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current(message);
                        }
                    }
                }
            }

            private static void HandleMessageClient(ushort id, byte[] message, ulong recipient)
            {
                if (recipient == Sync.MyId)
                {
                    HandleMessage(id, message);
                }
            }

            public bool IsServerPlayer(IMyNetworkClient player) => 
                ((player is MyNetworkClient) && (player as MyNetworkClient).IsGameServer());

            public void JoinServer(string address)
            {
                IPEndPoint endpoint;
                if ((!Game.IsDedicated || !this.IsServer) && (IPAddressExtensions.TryParseEndpoint(address, out endpoint) && (MySandboxGame.Static != null)))
                {
                    MySandboxGame.Static.Invoke(delegate {
                        MySessionLoader.UnloadAndExitToMenu();
                        MyGameService.OnPingServerResponded += new EventHandler<MyGameServerItem>(MySandboxGame.Static.ServerResponded);
                        MyGameService.OnPingServerFailedToRespond += new EventHandler(MySandboxGame.Static.ServerFailedToRespond);
                        MyGameService.PingServer(endpoint.Address.ToIPv4NetworkOrder(), (ushort) endpoint.Port);
                    }, "UnloadAndExitToMenu");
                }
            }

            [Event(null, 0xe4), Reliable, Server, BroadcastExcept]
            private static void ModMessageBroadcastReliable(ushort id, byte[] message)
            {
                HandleMessage(id, message);
            }

            [Event(null, 240), Reliable, BroadcastExcept]
            private static void ModMessageBroadcastReliableFromServer(ushort id, byte[] message)
            {
                HandleMessage(id, message);
            }

            [Event(null, 0xea), Server, BroadcastExcept]
            private static void ModMessageBroadcastUnreliable(ushort id, byte[] message)
            {
                HandleMessage(id, message);
            }

            [Event(null, 0xf6), BroadcastExcept]
            private static void ModMessageBroadcastUnreliableFromServer(ushort id, byte[] message)
            {
                HandleMessage(id, message);
            }

            [Event(null, 0xd8), Reliable, Server, Client]
            private static void ModMessageClientReliable(ushort id, byte[] message, ulong recipient)
            {
                HandleMessageClient(id, message, recipient);
            }

            [Event(null, 0xde), Server, Client]
            private static void ModMessageClientUnreliable(ushort id, byte[] message, ulong recipient)
            {
                HandleMessageClient(id, message, recipient);
            }

            [Event(null, 0xcc), Reliable, Server]
            private static void ModMessageServerReliable(ushort id, byte[] message, ulong recipient)
            {
                HandleMessageClient(id, message, recipient);
            }

            [Event(null, 210), Server]
            private static void ModMessageServerUnreliable(ushort id, byte[] message, ulong recipient)
            {
                HandleMessageClient(id, message, recipient);
            }

            public void RegisterMessageHandler(ushort id, Action<byte[]> messageHandler)
            {
                if (!ReferenceEquals(Thread.CurrentThread, MySandboxGame.Static.UpdateThread))
                {
                    throw new InvalidOperationException("Modifying message handlers from another thread is not supported!");
                }
                List<Action<byte[]>> list = null;
                if (m_registeredListeners.TryGetValue(id, out list))
                {
                    list.Add(messageHandler);
                }
                else
                {
                    m_registeredListeners[id] = new List<Action<byte[]>>();
                    m_registeredListeners[id].Add(messageHandler);
                }
            }

            [Event(null, 0x11a), Reliable, Server]
            private static void ReplicateEntity_Implmentation(long entityId, ulong steamId)
            {
            }

            public void ReplicateEntityForClient(long entityId, ulong steamId)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long, ulong>(x => new Action<long, ulong>(MyModAPIHelper.MyMultiplayer.ReplicateEntity_Implmentation), entityId, steamId, targetEndpoint, position);
            }

            public void SendEntitiesCreated(List<MyObjectBuilder_EntityBase> objectBuilders)
            {
            }

            public bool SendMessageTo(ushort id, byte[] message, ulong recipient, bool reliable)
            {
                Vector3D? nullable;
                if (!reliable && (message.Length > 0x400))
                {
                    return false;
                }
                if (reliable)
                {
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[], ulong>(s => new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageClientReliable), id, message, recipient, new EndpointId(recipient), nullable);
                }
                else
                {
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[], ulong>(s => new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageClientUnreliable), id, message, recipient, new EndpointId(recipient), nullable);
                }
                return true;
            }

            public bool SendMessageToOthers(ushort id, byte[] message, bool reliable)
            {
                EndpointId id2;
                Vector3D? nullable;
                if (!reliable && (message.Length > 0x400))
                {
                    return false;
                }
                if (this.IsServer)
                {
                    if (reliable)
                    {
                        id2 = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[]>(s => new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastReliableFromServer), id, message, id2, nullable);
                    }
                    else
                    {
                        id2 = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[]>(s => new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastUnreliableFromServer), id, message, id2, nullable);
                    }
                }
                else if (reliable)
                {
                    id2 = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[]>(s => new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastReliable), id, message, id2, nullable);
                }
                else
                {
                    id2 = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[]>(s => new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastUnreliable), id, message, id2, nullable);
                }
                return true;
            }

            public bool SendMessageToServer(ushort id, byte[] message, bool reliable)
            {
                EndpointId id2;
                Vector3D? nullable;
                if (!reliable && (message.Length > 0x400))
                {
                    return false;
                }
                if (reliable)
                {
                    id2 = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[], ulong>(s => new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageServerReliable), id, message, Sync.ServerId, id2, nullable);
                }
                else
                {
                    id2 = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ushort, byte[], ulong>(s => new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageServerUnreliable), id, message, Sync.ServerId, id2, nullable);
                }
                return true;
            }

            public void UnregisterMessageHandler(ushort id, Action<byte[]> messageHandler)
            {
                if (!ReferenceEquals(Thread.CurrentThread, MySandboxGame.Static.UpdateThread))
                {
                    throw new InvalidOperationException("Modifying message handlers from another thread is not supported!");
                }
                List<Action<byte[]>> list = null;
                if (m_registeredListeners.TryGetValue(id, out list))
                {
                    list.Remove(messageHandler);
                }
            }

            public bool MultiplayerActive =>
                Sync.MultiplayerActive;

            public bool IsServer =>
                Sync.IsServer;

            public ulong ServerId =>
                Sync.ServerId;

            public ulong MyId =>
                Sync.MyId;

            public string MyName =>
                Sync.MyName;

            public IMyPlayerCollection Players =>
                Sync.Players;

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyModAPIHelper.MyMultiplayer.<>c <>9 = new MyModAPIHelper.MyMultiplayer.<>c();
                public static Func<IMyEventOwner, Action<ushort, byte[], ulong>> <>9__18_0;
                public static Func<IMyEventOwner, Action<ushort, byte[], ulong>> <>9__18_1;
                public static Func<IMyEventOwner, Action<ushort, byte[]>> <>9__19_0;
                public static Func<IMyEventOwner, Action<ushort, byte[]>> <>9__19_1;
                public static Func<IMyEventOwner, Action<ushort, byte[]>> <>9__19_2;
                public static Func<IMyEventOwner, Action<ushort, byte[]>> <>9__19_3;
                public static Func<IMyEventOwner, Action<ushort, byte[], ulong>> <>9__20_0;
                public static Func<IMyEventOwner, Action<ushort, byte[], ulong>> <>9__20_1;
                public static Func<IMyEventOwner, Action<long, ulong>> <>9__34_0;

                internal Action<long, ulong> <ReplicateEntityForClient>b__34_0(IMyEventOwner x) => 
                    new Action<long, ulong>(MyModAPIHelper.MyMultiplayer.ReplicateEntity_Implmentation);

                internal Action<ushort, byte[], ulong> <SendMessageTo>b__20_0(IMyEventOwner s) => 
                    new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageClientReliable);

                internal Action<ushort, byte[], ulong> <SendMessageTo>b__20_1(IMyEventOwner s) => 
                    new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageClientUnreliable);

                internal Action<ushort, byte[]> <SendMessageToOthers>b__19_0(IMyEventOwner s) => 
                    new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastReliableFromServer);

                internal Action<ushort, byte[]> <SendMessageToOthers>b__19_1(IMyEventOwner s) => 
                    new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastUnreliableFromServer);

                internal Action<ushort, byte[]> <SendMessageToOthers>b__19_2(IMyEventOwner s) => 
                    new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastReliable);

                internal Action<ushort, byte[]> <SendMessageToOthers>b__19_3(IMyEventOwner s) => 
                    new Action<ushort, byte[]>(MyModAPIHelper.MyMultiplayer.ModMessageBroadcastUnreliable);

                internal Action<ushort, byte[], ulong> <SendMessageToServer>b__18_0(IMyEventOwner s) => 
                    new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageServerReliable);

                internal Action<ushort, byte[], ulong> <SendMessageToServer>b__18_1(IMyEventOwner s) => 
                    new Action<ushort, byte[], ulong>(MyModAPIHelper.MyMultiplayer.ModMessageServerUnreliable);
            }
        }
    }
}

