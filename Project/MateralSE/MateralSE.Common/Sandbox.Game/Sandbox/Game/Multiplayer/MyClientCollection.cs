namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public class MyClientCollection
    {
        private readonly Dictionary<ulong, MyNetworkClient> m_clients = new Dictionary<ulong, MyNetworkClient>();
        private HashSet<ulong> m_disconnectedClients = new HashSet<ulong>();
        private ulong m_localSteamId;
        public Action<ulong> ClientAdded;
        public Action<ulong> ClientRemoved;

        public MyNetworkClient AddClient(ulong steamId)
        {
            if (this.m_clients.ContainsKey(steamId))
            {
                MyLog.Default.WriteLine("ERROR: Added client already present: " + this.m_clients[steamId].DisplayName);
                return this.m_clients[steamId];
            }
            MyNetworkClient client = new MyNetworkClient(steamId);
            this.m_clients.Add(steamId, client);
            this.m_disconnectedClients.Remove(steamId);
            this.RaiseClientAdded(steamId);
            return client;
        }

        public void Clear()
        {
            this.m_clients.Clear();
            this.m_disconnectedClients.Clear();
        }

        public Dictionary<ulong, MyNetworkClient>.ValueCollection GetClients() => 
            this.m_clients.Values;

        public bool HasClient(ulong steamId) => 
            this.m_clients.ContainsKey(steamId);

        private void RaiseClientAdded(ulong steamId)
        {
            Action<ulong> clientAdded = this.ClientAdded;
            if (clientAdded != null)
            {
                clientAdded(steamId);
            }
        }

        private void RaiseClientRemoved(ulong steamId)
        {
            Action<ulong> clientRemoved = this.ClientRemoved;
            if (clientRemoved != null)
            {
                clientRemoved(steamId);
            }
        }

        public void RemoveClient(ulong steamId)
        {
            MyNetworkClient client;
            this.m_clients.TryGetValue(steamId, out client);
            if (client != null)
            {
                this.m_clients.Remove(steamId);
                this.m_disconnectedClients.Add(steamId);
                this.RaiseClientRemoved(steamId);
            }
            else if (!this.m_disconnectedClients.Contains(steamId))
            {
                MyLog.Default.WriteLine("ERROR: Removed client not present: " + steamId);
            }
        }

        public void SetLocalSteamId(ulong localSteamId, bool createLocalClient = false)
        {
            this.m_localSteamId = localSteamId;
            if (createLocalClient && !this.m_clients.ContainsKey(this.m_localSteamId))
            {
                this.AddClient(this.m_localSteamId);
            }
        }

        public bool TryGetClient(ulong steamId, out MyNetworkClient client)
        {
            client = null;
            return this.m_clients.TryGetValue(steamId, out client);
        }

        public int Count =>
            this.m_clients.Count;

        public MyNetworkClient LocalClient
        {
            get
            {
                MyNetworkClient client = null;
                this.m_clients.TryGetValue(this.m_localSteamId, out client);
                return client;
            }
        }
    }
}

