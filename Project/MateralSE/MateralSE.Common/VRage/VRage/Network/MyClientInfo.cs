namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyClientInfo
    {
        private readonly MyClient m_clients;
        public MyClientStateBase State =>
            this.m_clients.State;
        public Endpoint EndpointId =>
            this.m_clients.State.EndpointId;
        public float PriorityMultiplier =>
            this.m_clients.PriorityMultiplier;
        internal MyClientInfo(MyClient client)
        {
            this.m_clients = client;
        }

        public bool HasReplicable(IMyReplicable replicable) => 
            this.m_clients.Replicables.ContainsKey(replicable);

        public bool IsReplicableReady(IMyReplicable replicable) => 
            this.m_clients.IsReplicableReady(replicable);
    }
}

