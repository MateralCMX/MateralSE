namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Network;

    public static class MyClientStateExtensions
    {
        public static MyNetworkClient GetClient(this MyClientStateBase state)
        {
            MyNetworkClient client;
            if (state == null)
            {
                return null;
            }
            Sync.Clients.TryGetClient(state.EndpointId.Id.Value, out client);
            return client;
        }

        public static MyPlayer GetPlayer(this MyClientStateBase state)
        {
            MyNetworkClient client = state.GetClient();
            return client?.FirstPlayer;
        }
    }
}

