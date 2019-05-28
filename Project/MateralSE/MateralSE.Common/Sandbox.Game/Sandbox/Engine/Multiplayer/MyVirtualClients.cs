namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Game;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner]
    internal class MyVirtualClients
    {
        private readonly List<MyVirtualClient> m_clients = new List<MyVirtualClient>();

        public void Add(int idx)
        {
            int num = this.m_clients.Count + 1;
            MyPlayer.PlayerId id = Sync.Players.FindFreePlayerId(Sync.MyId);
            MyPlayer.PlayerId id2 = new MyPlayer.PlayerId(id.SteamId, id.SerialId + idx);
            MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(Sync.MyId, 0));
            object[] objArray1 = new object[] { "Virtual ", playerById.DisplayName, " #", num + idx };
            Sync.Players.RequestNewPlayer(id2.SerialId, string.Concat(objArray1), null, true, false);
        }

        public bool Any() => 
            (this.m_clients.Count > 0);

        private static MyClientState CreateClientState() => 
            (Activator.CreateInstance(MyPerGameSettings.ClientStateType) as MyClientState);

        public MyPlayer GetNextControlledPlayer(MyPlayer controllingPlayer)
        {
            if (!this.Any())
            {
                return null;
            }
            for (int i = 0; i < this.m_clients.Count; i++)
            {
                MyVirtualClient client = this.m_clients[i];
                if (ReferenceEquals(Sync.Players.GetPlayerById(client.PlayerId), controllingPlayer))
                {
                    return ((i != (this.m_clients.Count - 1)) ? Sync.Players.GetPlayerById(this.m_clients[i + 1].PlayerId) : null);
                }
            }
            return Sync.Players.GetPlayerById(this.m_clients[0].PlayerId);
        }

        public void Init()
        {
            Sync.Players.NewPlayerRequestSucceeded += new Action<MyPlayer.PlayerId>(this.OnNewPlayerSuccess);
        }

        private void OnNewPlayerSuccess(MyPlayer.PlayerId playerId)
        {
            if (((playerId.SteamId == Sync.MyId) && (playerId.SerialId != 0)) && Sync.Players.GetPlayerById(playerId).IsRealPlayer)
            {
                MyPlayerCollection.RespawnRequest(true, true, 0L, string.Empty, playerId.SerialId, null, Color.Red);
                int num = this.m_clients.Count + 1;
                MyVirtualClient item = new MyVirtualClient(new Endpoint(Sync.MyId, (byte) num), CreateClientState(), playerId);
                this.m_clients.Add(item);
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<int>(x => new Action<int>(MyVirtualClients.OnVirtualClientAdded), num, targetEndpoint, position);
            }
        }

        [Event(null, 0x2e), Reliable, Server]
        private static void OnVirtualClientAdded(int index)
        {
            Endpoint endpoint = new Endpoint(MyEventContext.Current.IsLocallyInvoked ? new EndpointId(Sync.MyId) : MyEventContext.Current.Sender, (byte) index);
            MyReplicationServer replicationLayer = MyMultiplayer.Static.ReplicationLayer as MyReplicationServer;
            replicationLayer.AddClient(endpoint, CreateClientState());
            ClientReadyDataMsg msg = new ClientReadyDataMsg {
                UsePlayoutDelayBufferForCharacter = true,
                UsePlayoutDelayBufferForJetpack = true,
                UsePlayoutDelayBufferForGrids = true
            };
            replicationLayer.OnClientReady(endpoint, ref msg);
        }

        public void Tick()
        {
            using (List<MyVirtualClient>.Enumerator enumerator = this.m_clients.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Tick();
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVirtualClients.<>c <>9 = new MyVirtualClients.<>c();
            public static Func<IMyEventOwner, Action<int>> <>9__3_0;

            internal Action<int> <OnNewPlayerSuccess>b__3_0(IMyEventOwner x) => 
                new Action<int>(MyVirtualClients.OnVirtualClientAdded);
        }
    }
}

