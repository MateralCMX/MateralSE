namespace Sandbox.Game.Multiplayer
{
    using ProtoBuf;
    using Sandbox.Engine.Multiplayer;
    using System;
    using VRage.GameServices;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    public class MySyncLayer
    {
        internal readonly MyTransportLayer TransportLayer;
        internal readonly MyClientCollection Clients;

        internal MySyncLayer(MyTransportLayer transportLayer)
        {
            this.TransportLayer = transportLayer;
            this.Clients = new MyClientCollection();
        }

        public static bool CheckReceivePermissions(ulong sender, MyMessagePermissions permission)
        {
            bool isServer;
            switch (permission)
            {
                case MyMessagePermissions.FromServer:
                    isServer = Sync.ServerId == sender;
                    break;

                case MyMessagePermissions.ToServer:
                    isServer = Sync.IsServer;
                    break;

                case (MyMessagePermissions.ToServer | MyMessagePermissions.FromServer):
                    isServer = (Sync.ServerId == sender) || Sync.IsServer;
                    break;

                default:
                    isServer = false;
                    break;
            }
            return isServer;
        }

        public static bool CheckSendPermissions(ulong target, MyMessagePermissions permission)
        {
            bool isServer;
            switch (permission)
            {
                case MyMessagePermissions.FromServer:
                    isServer = Sync.IsServer;
                    break;

                case MyMessagePermissions.ToServer:
                    isServer = Sync.ServerId == target;
                    break;

                case (MyMessagePermissions.ToServer | MyMessagePermissions.FromServer):
                    isServer = (Sync.ServerId == target) || Sync.IsServer;
                    break;

                default:
                    isServer = false;
                    break;
            }
            return isServer;
        }

        private static ISerializer<TMsg> CreateBlittable<TMsg>() => 
            BlitSerializer<TMsg>.Default;

        private static ISerializer<TMsg> CreateProto<TMsg>() => 
            DefaultProtoSerializer<TMsg>.Default;

        internal static ISerializer<TMsg> GetSerializer<TMsg>() => 
            (!Attribute.IsDefined(typeof(TMsg), typeof(ProtoContractAttribute)) ? CreateBlittable<TMsg>() : CreateProto<TMsg>());

        private void OnClientJoined(ulong steamUserId)
        {
            if (!this.Clients.HasClient(steamUserId))
            {
                this.Clients.AddClient(steamUserId);
            }
        }

        private void OnClientLeft(ulong steamUserId, MyChatMemberStateChangeEnum leaveReason)
        {
            this.Clients.RemoveClient(steamUserId);
        }

        internal void RegisterClientEvents(MyMultiplayerBase multiplayer)
        {
            multiplayer.ClientJoined += new Action<ulong>(this.OnClientJoined);
            multiplayer.ClientLeft += new Action<ulong, MyChatMemberStateChangeEnum>(this.OnClientLeft);
            foreach (ulong num in multiplayer.Members)
            {
                if (num != Sync.MyId)
                {
                    this.OnClientJoined(num);
                }
            }
        }

        private class DefaultProtoSerializer<T>
        {
            public static readonly ProtoSerializer<T> Default;

            static DefaultProtoSerializer()
            {
                MySyncLayer.DefaultProtoSerializer<T>.Default = new ProtoSerializer<T>(MyObjectBuilderSerializer.Serializer);
            }
        }
    }
}

